using MediatR;
using Microsoft.Extensions.Logging;
using Pipexi.Application.Abstractions.Notifications;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Domain.Entities;
using Pipexi.Domain.Events.Conversations;

namespace Pipexi.Application.Features.Conversations.EventHandlers;

public sealed class ConversationMessageCreatedEventHandler : INotificationHandler<ConversationMessageCreatedEvent>
{
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
        _logger.LogInformation("Handling ConversationMessageCreatedEvent for ConversationId: {ConversationId}, SenderMemberId: {SenderMemberId}",
            notification.ConversationId, notification.SenderOrganizationMemberId);

        try
        {
            var conversation = await _conversationRepository.GetByIdAsync(notification.ConversationId, cancellationToken);
            if (conversation is null) return;

            var members = await _conversationMemberRepository.ListByConversationIdAsync(notification.ConversationId, cancellationToken);
            var recipientMembers = members.Where(m => m.OrganizationMemberId != notification.SenderOrganizationMemberId).ToList();

            var notificationType = $"chat:{conversation.Id}";
            var cooldownCutoff = DateTimeOffset.UtcNow.AddMinutes(-15);

            foreach (var recipientMember in recipientMembers)
            {
                var orgMember = await _organizationMemberRepository.GetByIdAsync(recipientMember.OrganizationMemberId, cancellationToken);
                if (orgMember is null) continue;

                // 15-minute Cooldown Check: Skip if recipient got a chat notification for this conversation in the last 15 minutes
                var existingNotifications = await _notificationRepository.ListByOrganizationMemberIdAsync(orgMember.Id, cancellationToken);
                var recentNotification = existingNotifications.FirstOrDefault(n => n.Type == notificationType && n.CreatedAt >= cooldownCutoff);

                if (recentNotification is not null)
                {
                    _logger.LogInformation("Skipping chat push notification for OrgMemberId {OrgMemberId} due to 15-min cooldown", orgMember.Id);
                    continue;
                }

                var title = $"New message from {notification.SenderName}";
                var body = notification.Body;

                // A. Save to notifications table
                var dbNotification = Notification.Create(
                    conversation.OrganizationId,
                    orgMember.Id,
                    notificationType,
                    title,
                    body,
                    isRead: false,
                    scheduledTime: null);

                await _notificationRepository.AddAsync(dbNotification, cancellationToken);

                // B. Send FCM push notification
                var devices = await _userDeviceRepository.GetByUserIdAsync(orgMember.UserId, cancellationToken);
                var tokens = devices.Select(d => d.FcmToken).ToList();

                _logger.LogInformation("Found {Count} active devices for chat recipient UserId: {UserId}", tokens.Count, orgMember.UserId);

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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling ConversationMessageCreatedEvent for ConversationId {ConversationId}", notification.ConversationId);
        }
    }
}
