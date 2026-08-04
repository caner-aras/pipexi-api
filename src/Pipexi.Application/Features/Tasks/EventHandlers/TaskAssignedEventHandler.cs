using MediatR;
using Microsoft.Extensions.Logging;
using Pipexi.Application.Abstractions.Notifications;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Domain.Events.Tasks;

namespace Pipexi.Application.Features.Tasks.EventHandlers;

public sealed class TaskAssignedEventHandler : INotificationHandler<TaskAssignedEvent>
{
    private readonly ITeamMemberRepository _teamMemberRepository;
    private readonly IOrganizationMemberRepository _organizationMemberRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserDeviceRepository _userDeviceRepository;
    private readonly IPushNotificationService _pushNotificationService;
    private readonly IWorkTaskRepository _workTaskRepository;
    private readonly ILogger<TaskAssignedEventHandler> _logger;

    public TaskAssignedEventHandler(
        ITeamMemberRepository teamMemberRepository,
        IOrganizationMemberRepository organizationMemberRepository,
        IUserRepository userRepository,
        IUserDeviceRepository userDeviceRepository,
        IPushNotificationService pushNotificationService,
        IWorkTaskRepository workTaskRepository,
        ILogger<TaskAssignedEventHandler> logger)
    {
        _teamMemberRepository = teamMemberRepository;
        _organizationMemberRepository = organizationMemberRepository;
        _userRepository = userRepository;
        _userDeviceRepository = userDeviceRepository;
        _pushNotificationService = pushNotificationService;
        _workTaskRepository = workTaskRepository;
        _logger = logger;
    }

    public async Task Handle(TaskAssignedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var task = await _workTaskRepository.GetByIdAsync(notification.TaskId, cancellationToken);
            if (task is null)
            {
                _logger.LogWarning("Task {TaskId} not found for TaskAssignedEvent", notification.TaskId);
                return;
            }

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

            // We no longer skip if assigner == assignee, so you can test it by assigning to yourself.

            var assignerUser = await _userRepository.GetByIdAsync(notification.AssignerUserId, cancellationToken);
            var assignerName = assignerUser != null 
                ? $"{assignerUser.FirstName} {assignerUser.LastName}".Trim() 
                : "Someone";

            // Send notification to assignee
            var assigneeDevices = await _userDeviceRepository.GetByUserIdAsync(assignedUserId, cancellationToken);
            var assigneeTokens = assigneeDevices.Select(x => x.FcmToken).ToList();

            if (assigneeTokens.Count > 0)
            {
                var title = "New Task Assigned";
                var body = $"{assignerName} assigned you a new task: {task.Title}";
                
                var data = new Dictionary<string, string>
                {
                    { "type", "task_assigned" },
                    { "taskId", task.Id.ToString() },
                    { "organizationId", task.OrganizationId.ToString() }
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

            if (assignerTokens.Count > 0)
            {
                // We need assignee's name for the message
                var assigneeUser = await _userRepository.GetByIdAsync(assignedUserId, cancellationToken);
                var assigneeName = assigneeUser != null 
                    ? $"{assigneeUser.FirstName} {assigneeUser.LastName}".Trim() 
                    : "Someone";

                var title = "Task Created";
                var body = $"You successfully assigned task '{task.Title}' to {assigneeName}.";
                
                var data = new Dictionary<string, string>
                {
                    { "type", "task_created" },
                    { "taskId", task.Id.ToString() },
                    { "organizationId", task.OrganizationId.ToString() }
                };

                await _pushNotificationService.SendPushNotificationAsync(
                    assignerTokens,
                    title,
                    body,
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
