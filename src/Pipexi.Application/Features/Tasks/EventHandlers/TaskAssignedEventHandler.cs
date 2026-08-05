using MediatR;
using Microsoft.Extensions.Logging;
using Pipexi.Application.Abstractions.Notifications;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Domain.Entities;
using Pipexi.Domain.Events.Tasks;

namespace Pipexi.Application.Features.Tasks.EventHandlers;

public sealed class TaskAssignedEventHandler : INotificationHandler<TaskAssignedEvent>
{
    private readonly ITeamMemberRepository _teamMemberRepository;
    private readonly IOrganizationMemberRepository _organizationMemberRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserDeviceRepository _userDeviceRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IPushNotificationService _pushNotificationService;
    private readonly ILogger<TaskAssignedEventHandler> _logger;

    public TaskAssignedEventHandler(
        ITeamMemberRepository teamMemberRepository,
        IOrganizationMemberRepository organizationMemberRepository,
        IUserRepository userRepository,
        IUserDeviceRepository userDeviceRepository,
        INotificationRepository notificationRepository,
        IPushNotificationService pushNotificationService,
        ILogger<TaskAssignedEventHandler> logger)
    {
        _teamMemberRepository = teamMemberRepository;
        _organizationMemberRepository = organizationMemberRepository;
        _userRepository = userRepository;
        _userDeviceRepository = userDeviceRepository;
        _notificationRepository = notificationRepository;
        _pushNotificationService = pushNotificationService;
        _logger = logger;
    }

    public async Task Handle(TaskAssignedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling TaskAssignedEvent for TaskId: {TaskId}, AssignerUserId: {AssignerUserId}, AssignedToTeamMemberId: {AssignedToTeamMemberId}",
            notification.TaskId, notification.AssignerUserId, notification.AssignedToTeamMemberId);

        try
        {
            var assignedTeamMember = await _teamMemberRepository.GetByIdAsync(notification.AssignedToTeamMemberId, cancellationToken);
            if (assignedTeamMember is null)
            {
                _logger.LogWarning("Assigned team member {TeamMemberId} not found", notification.AssignedToTeamMemberId);
                return;
            }

            var orgMember = await _organizationMemberRepository.GetByIdAsync(assignedTeamMember.OrganizationMemberId, cancellationToken);
            if (orgMember is null)
            {
                _logger.LogWarning("Assigned organization member {OrgMemberId} not found", assignedTeamMember.OrganizationMemberId);
                return;
            }

            var assignedUserId = orgMember.UserId;

            var assignerUser = await _userRepository.GetByIdAsync(notification.AssignerUserId, cancellationToken);
            var assignerName = assignerUser != null
                ? $"{assignerUser.FirstName} {assignerUser.LastName}".Trim()
                : "Someone";

            var priorityPrefix = (notification.Priority?.ToLowerInvariant()) switch
            {
                "urgent" => "🚨 [URGENT] ",
                "high" => "⚠️ [HIGH] ",
                _ => ""
            };

            var title = $"{priorityPrefix}New Task Assigned";
            var body = $"{assignerName} assigned you a new task: {notification.TaskTitle}";

            // 1. Save Notification entity to database
            var dbNotification = Notification.Create(
                notification.OrganizationId,
                assignedTeamMember.OrganizationMemberId,
                "task_assigned",
                title,
                body,
                isRead: false,
                scheduledTime: null);

            await _notificationRepository.AddAsync(dbNotification, cancellationToken);

            // 2. Send push notification to assignee devices
            var assigneeDevices = await _userDeviceRepository.GetByUserIdAsync(assignedUserId, cancellationToken);
            var assigneeTokens = assigneeDevices.Select(x => x.FcmToken).ToList();

            _logger.LogInformation("Found {Count} active devices for assignee UserId: {UserId}", assigneeTokens.Count, assignedUserId);

            if (assigneeTokens.Count > 0)
            {
                var data = new Dictionary<string, string>
                {
                    { "type", "task_assigned" },
                    { "taskId", notification.TaskId.ToString() },
                    { "organizationId", notification.OrganizationId.ToString() }
                };

                await _pushNotificationService.SendPushNotificationAsync(
                    assigneeTokens,
                    title,
                    body,
                    data,
                    cancellationToken);
            }

            // Send notification to assigner (for testing/feedback)
            var assignerDevices = await _userDeviceRepository.GetByUserIdAsync(notification.AssignerUserId, cancellationToken);
            var assignerTokens = assignerDevices.Select(x => x.FcmToken).ToList();

            _logger.LogInformation("Found {Count} active devices for assigner UserId: {UserId}", assignerTokens.Count, notification.AssignerUserId);

            if (assignerTokens.Count > 0)
            {
                var assigneeUser = await _userRepository.GetByIdAsync(assignedUserId, cancellationToken);
                var assigneeName = assigneeUser != null
                    ? $"{assigneeUser.FirstName} {assigneeUser.LastName}".Trim()
                    : "Someone";

                var assignerTitle = $"{priorityPrefix}Task Created";
                var assignerBody = $"You successfully assigned task '{notification.TaskTitle}' to {assigneeName}.";

                var data = new Dictionary<string, string>
                {
                    { "type", "task_created" },
                    { "taskId", notification.TaskId.ToString() },
                    { "organizationId", notification.OrganizationId.ToString() }
                };

                await _pushNotificationService.SendPushNotificationAsync(
                    assignerTokens,
                    assignerTitle,
                    assignerBody,
                    data,
                    cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling TaskAssignedEvent for TaskId {TaskId}", notification.TaskId);
        }
    }
}
