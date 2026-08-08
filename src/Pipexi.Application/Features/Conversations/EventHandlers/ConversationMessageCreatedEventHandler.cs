using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pipexi.Application.Abstractions.Notifications;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Domain.Entities;
using Pipexi.Domain.Events.Conversations;

namespace Pipexi.Application.Features.Conversations.EventHandlers;

public sealed class ConversationMessageCreatedEventHandler : INotificationHandler<ConversationMessageCreatedEvent>
{
    /// <summary>
    /// Gives open chat clients time to mark the conversation read via realtime
    /// before we decide whether to send a push. Must NOT run on the request thread —
    /// domain events are awaited inside SaveChanges and would block the API response.
    /// </summary>
    private static readonly TimeSpan ReadCatchUpDelay = TimeSpan.FromSeconds(3);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ConversationMessageCreatedEventHandler> _logger;

    public ConversationMessageCreatedEventHandler(
        IServiceScopeFactory scopeFactory,
        ILogger<ConversationMessageCreatedEventHandler> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public Task Handle(ConversationMessageCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Queueing chat push check for ConversationId: {ConversationId}, SenderMemberId: {SenderMemberId}",
            notification.ConversationId,
            notification.SenderOrganizationMemberId);

        // Fire-and-forget so CreateMessage HTTP response is not blocked by the delay.
        _ = ProcessInBackgroundAsync(notification);

        return Task.CompletedTask;
    }

    private async Task ProcessInBackgroundAsync(ConversationMessageCreatedEvent notification)
    {
        try
        {
            await Task.Delay(ReadCatchUpDelay);

            await using var scope = _scopeFactory.CreateAsyncScope();
            var sp = scope.ServiceProvider;

            var conversationRepository = sp.GetRequiredService<IConversationRepository>();
            var conversationMemberRepository = sp.GetRequiredService<IConversationMemberRepository>();
            var organizationMemberRepository = sp.GetRequiredService<IOrganizationMemberRepository>();
            var userDeviceRepository = sp.GetRequiredService<IUserDeviceRepository>();
            var notificationRepository = sp.GetRequiredService<INotificationRepository>();
            var pushNotificationService = sp.GetRequiredService<IPushNotificationService>();
            var unitOfWork = sp.GetRequiredService<IUnitOfWork>();

            var conversation = await conversationRepository.GetByIdAsync(notification.ConversationId);
            if (conversation is null) return;

            var members = await conversationMemberRepository.ListByConversationIdAsync(
                notification.ConversationId);
            var recipientMembers = members
                .Where(m => m.OrganizationMemberId != notification.SenderOrganizationMemberId)
                .ToList();

            var notificationType = $"chat:{conversation.Id}";
            var title = $"New message from {notification.SenderName}";
            var body = notification.Body;
            var shouldSave = false;

            foreach (var recipientMember in recipientMembers)
            {
                // Read = LastReadAt >= message time → recipient already caught up (app open on this chat).
                var isRead = recipientMember.LastReadAt.HasValue
                    && recipientMember.LastReadAt.Value >= notification.MessageCreatedAt;

                if (isRead)
                {
                    _logger.LogInformation(
                        "Skipping chat push for OrgMemberId {OrgMemberId}: conversation already read (LastReadAt={LastReadAt})",
                        recipientMember.OrganizationMemberId,
                        recipientMember.LastReadAt);
                    continue;
                }

                var orgMember = await organizationMemberRepository.GetByIdAsync(
                    recipientMember.OrganizationMemberId);
                if (orgMember is null) continue;

                var dbNotification = Notification.Create(
                    conversation.OrganizationId,
                    orgMember.Id,
                    notificationType,
                    title,
                    body,
                    isRead: false,
                    scheduledTime: null);

                await notificationRepository.AddAsync(dbNotification);
                shouldSave = true;

                var devices = await userDeviceRepository.GetByUserIdAsync(orgMember.UserId);
                var tokens = devices.Select(d => d.FcmToken).ToList();

                _logger.LogInformation(
                    "Sending chat push to OrgMemberId {OrgMemberId} ({Count} devices)",
                    orgMember.Id,
                    tokens.Count);

                if (tokens.Count > 0)
                {
                    var data = new Dictionary<string, string>
                    {
                        { "type", "chat_message" },
                        { "conversationId", conversation.Id.ToString() },
                        { "organizationId", conversation.OrganizationId.ToString() }
                    };

                    await pushNotificationService.SendPushNotificationAsync(
                        tokens,
                        title,
                        body,
                        data);
                }
            }

            if (shouldSave)
            {
                await unitOfWork.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing chat push for ConversationId {ConversationId}",
                notification.ConversationId);
        }
    }
}
