using MediatR;
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
    /// (often 1–2s after send) before we decide whether to send a push.
    /// </summary>
    private static readonly TimeSpan ReadCatchUpDelay = TimeSpan.FromSeconds(3);

    private readonly IConversationRepository _conversationRepository;
    private readonly IConversationMemberRepository _conversationMemberRepository;
    private readonly IOrganizationMemberRepository _organizationMemberRepository;
    private readonly IUserDeviceRepository _userDeviceRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IPushNotificationService _pushNotificationService;
    private readonly ILogger<ConversationMessageCreatedEventHandler> _logger;

    public ConversationMessageCreatedEventHandler(
        IConversationRepository conversationRepository,
        IConversationMemberRepository conversationMemberRepository,
        IOrganizationMemberRepository organizationMemberRepository,
        IUserDeviceRepository userDeviceRepository,
        INotificationRepository notificationRepository,
        IPushNotificationService pushNotificationService,
        ILogger<ConversationMessageCreatedEventHandler> logger)
    {
        _conversationRepository = conversationRepository;
        _conversationMemberRepository = conversationMemberRepository;
        _organizationMemberRepository = organizationMemberRepository;
        _userDeviceRepository = userDeviceRepository;
        _notificationRepository = notificationRepository;
        _pushNotificationService = pushNotificationService;
        _logger = logger;
    }

    public async Task Handle(ConversationMessageCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling ConversationMessageCreatedEvent for ConversationId: {ConversationId}, SenderMemberId: {SenderMemberId}",
            notification.ConversationId,
            notification.SenderOrganizationMemberId);

        try
        {
            var conversation = await _conversationRepository.GetByIdAsync(notification.ConversationId, cancellationToken);
            if (conversation is null) return;

            // Allow recipients with the chat open to mark read before we decide on push.
            await Task.Delay(ReadCatchUpDelay, cancellationToken);

            var members = await _conversationMemberRepository.ListByConversationIdAsync(
                notification.ConversationId,
                cancellationToken);
            var recipientMembers = members
                .Where(m => m.OrganizationMemberId != notification.SenderOrganizationMemberId)
                .ToList();

            var notificationType = $"chat:{conversation.Id}";
            var title = $"New message from {notification.SenderName}";
            var body = notification.Body;

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

                var orgMember = await _organizationMemberRepository.GetByIdAsync(
                    recipientMember.OrganizationMemberId,
                    cancellationToken);
                if (orgMember is null) continue;

                var dbNotification = Notification.Create(
                    conversation.OrganizationId,
                    orgMember.Id,
                    notificationType,
                    title,
                    body,
                    isRead: false,
                    scheduledTime: null);

                await _notificationRepository.AddAsync(dbNotification, cancellationToken);

                var devices = await _userDeviceRepository.GetByUserIdAsync(orgMember.UserId, cancellationToken);
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

                    await _pushNotificationService.SendPushNotificationAsync(
                        tokens,
                        title,
                        body,
                        data,
                        cancellationToken);
                }
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error handling ConversationMessageCreatedEvent for ConversationId {ConversationId}",
                notification.ConversationId);
        }
    }
}
