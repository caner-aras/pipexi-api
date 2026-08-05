using MediatR;
using Microsoft.Extensions.Logging;
using Pipexi.Application.Abstractions.Notifications;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Domain.Entities;
using Pipexi.Domain.Events.Tasks;

namespace Pipexi.Application.Features.Tasks.EventHandlers;

public sealed class TaskCommentAddedEventHandler : INotificationHandler<TaskCommentAddedEvent>
{
    private readonly IWorkTaskRepository _workTaskRepository;
    private readonly ITeamMemberRepository _teamMemberRepository;
    private readonly IOrganizationMemberRepository _organizationMemberRepository;
    private readonly IUserDeviceRepository _userDeviceRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IPushNotificationService _pushNotificationService;
    private readonly ILogger<TaskCommentAddedEventHandler> _logger;

    public TaskCommentAddedEventHandler(
        IWorkTaskRepository workTaskRepository,
        ITeamMemberRepository teamMemberRepository,
        IOrganizationMemberRepository organizationMemberRepository,
        IUserDeviceRepository userDeviceRepository,
        INotificationRepository notificationRepository,
        IPushNotificationService pushNotificationService,
        ILogger<TaskCommentAddedEventHandler> logger)
    {
        _workTaskRepository = workTaskRepository;
        _teamMemberRepository = teamMemberRepository;
        _organizationMemberRepository = organizationMemberRepository;
        _userDeviceRepository = userDeviceRepository;
        _notificationRepository = notificationRepository;
        _pushNotificationService = pushNotificationService;
        _logger = logger;
    }

    public async Task Handle(TaskCommentAddedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling TaskCommentAddedEvent for TaskId: {TaskId}, CommenterUserId: {CommenterUserId}",
            notification.TaskId, notification.CommenterUserId);

        try
        {
            var task = await _workTaskRepository.GetByIdAsync(notification.TaskId, cancellationToken);
            if (task is null)
            {
                _logger.LogWarning("Task {TaskId} not found when handling TaskCommentAddedEvent", notification.TaskId);
                return;
            }

            var recipients = new List<(Guid OrgMemberId, Guid UserId)>();

            // 1. Task Reporter (if not the commenter)
            if (task.ReporterUserId.HasValue && task.ReporterUserId.Value != notification.CommenterUserId)
            {
                var reporterOrgMember = await _organizationMemberRepository.GetByOrganizationIdAndUserIdAsync(
                    task.OrganizationId, task.ReporterUserId.Value, cancellationToken);

                if (reporterOrgMember is not null)
                {
                    recipients.Add((reporterOrgMember.Id, task.ReporterUserId.Value));
                }
            }

            // 2. Task Assignee (if not the commenter)
            if (task.AssignedToTeamMemberId.HasValue)
            {
                var assignedTeamMember = await _teamMemberRepository.GetByIdAsync(task.AssignedToTeamMemberId.Value, cancellationToken);
                if (assignedTeamMember is not null)
                {
                    var assignedOrgMember = await _organizationMemberRepository.GetByIdAsync(assignedTeamMember.OrganizationMemberId, cancellationToken);
                    if (assignedOrgMember is not null && assignedOrgMember.UserId != notification.CommenterUserId)
                    {
                        if (!recipients.Any(r => r.UserId == assignedOrgMember.UserId))
                        {
                            recipients.Add((assignedOrgMember.Id, assignedOrgMember.UserId));
                        }
                    }
                }
            }

            var title = $"New comment on '{task.Title}'";
            var body = $"{notification.CommenterName}: {notification.Message}";

            foreach (var (orgMemberId, userId) in recipients)
            {
                // A. Save notification entity to database table
                var dbNotification = Notification.Create(
                    task.OrganizationId,
                    orgMemberId,
                    $"task:{task.Id}",
                    title,
                    body,
                    isRead: false,
                    scheduledTime: null);

                await _notificationRepository.AddAsync(dbNotification, cancellationToken);

                // B. Send push notification via FCM
                var devices = await _userDeviceRepository.GetByUserIdAsync(userId, cancellationToken);
                var tokens = devices.Select(d => d.FcmToken).ToList();

                _logger.LogInformation("Found {Count} active devices for recipient UserId: {UserId}", tokens.Count, userId);

                if (tokens.Count > 0)
                {
                    var data = new Dictionary<string, string>
                    {
                        { "type", "task_assigned" },
                        { "taskId", task.Id.ToString() },
                        { "organizationId", task.OrganizationId.ToString() }
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
            _logger.LogError(ex, "Error handling TaskCommentAddedEvent for TaskId {TaskId}", notification.TaskId);
        }
    }
}
