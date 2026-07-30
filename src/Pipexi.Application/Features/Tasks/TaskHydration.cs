using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Features.OrganizationMembers;
using Pipexi.Application.Features.OrganizationMembers.Dtos;
using Pipexi.Application.Features.Tasks.Dtos;
using Pipexi.Application.Features.Teams;
using Pipexi.Application.Features.Teams.Dtos;
using Pipexi.Domain.Entities;

namespace Pipexi.Application.Features.Tasks;

internal static class TaskHydration
{
    public static async Task<IReadOnlyCollection<TaskDto>> BuildDtosAsync(
        IReadOnlyCollection<WorkTask> tasks,
        bool includeComments,
        ITaskCommentRepository? taskCommentRepository,
        ITeamMemberRepository teamMemberRepository,
        ITeamRepository teamRepository,
        IOrganizationMemberRepository organizationMemberRepository,
        IUserRepository userRepository,
        CancellationToken cancellationToken)
    {
        if (tasks.Count == 0)
        {
            return [];
        }

        var reporterIds = tasks
            .Where(x => x.ReporterUserId.HasValue)
            .Select(x => x.ReporterUserId!.Value)
            .Distinct()
            .ToList();

        var assigneeTeamMemberIds = tasks
            .Where(x => x.AssignedToTeamMemberId.HasValue)
            .Select(x => x.AssignedToTeamMemberId!.Value)
            .Distinct()
            .ToList();

        var reporters = reporterIds.Count == 0
            ? []
            : await userRepository.ListByIdsAsync(reporterIds, cancellationToken);
        var reporterMap = reporters.ToDictionary(x => x.Id, x => x.ToTaskCommentMemberUserDto());

        var assigneeMap = await BuildAssigneeMapAsync(
            assigneeTeamMemberIds,
            teamMemberRepository,
            teamRepository,
            organizationMemberRepository,
            userRepository,
            cancellationToken);

        Dictionary<Guid, List<TaskComment>> commentsByTaskId = [];
        Dictionary<Guid, TaskCommentMemberDto> commentMemberMap = [];

        if (includeComments)
        {
            if (taskCommentRepository is null)
            {
                throw new InvalidOperationException("Task comment repository is required when including comments.");
            }

            var taskIds = tasks.Select(x => x.Id).ToList();
            var comments = await taskCommentRepository.ListByWorkTaskIdsAsync(taskIds, cancellationToken);
            commentsByTaskId = comments
                .GroupBy(x => x.WorkTaskId)
                .ToDictionary(g => g.Key, g => g.ToList());

            commentMemberMap = await BuildCommentMemberMapAsync(
                comments,
                teamMemberRepository,
                organizationMemberRepository,
                userRepository,
                cancellationToken);
        }

        return tasks
            .Select(task =>
            {
                var comments = includeComments && commentsByTaskId.TryGetValue(task.Id, out var taskComments)
                    ? taskComments.Select(x => x.ToDto(commentMemberMap.GetValueOrDefault(x.TeamMemberId))).ToList()
                    : [];

                return task.ToDto(comments) with
                {
                    Reporter = task.ReporterUserId.HasValue
                        ? reporterMap.GetValueOrDefault(task.ReporterUserId.Value)
                        : null,
                    AssignedToTeamMember = task.AssignedToTeamMemberId.HasValue
                        ? assigneeMap.GetValueOrDefault(task.AssignedToTeamMemberId.Value)
                        : null
                };
            })
            .ToList();
    }

    private static async Task<Dictionary<Guid, TeamMemberDto>> BuildAssigneeMapAsync(
        IReadOnlyCollection<Guid> teamMemberIds,
        ITeamMemberRepository teamMemberRepository,
        ITeamRepository teamRepository,
        IOrganizationMemberRepository organizationMemberRepository,
        IUserRepository userRepository,
        CancellationToken cancellationToken)
    {
        if (teamMemberIds.Count == 0)
        {
            return [];
        }

        var teamMembers = await teamMemberRepository.GetByIdsAsync(teamMemberIds, cancellationToken);
        var teamIds = teamMembers.Select(x => x.TeamId).Distinct().ToList();
        var teams = await teamRepository.GetByIdsAsync(teamIds, cancellationToken);
        var teamMap = teams.ToDictionary(x => x.Id, x => x.ToDto());

        var organizationMemberIds = teamMembers.Select(x => x.OrganizationMemberId).Distinct().ToList();
        var organizationMembers = await organizationMemberRepository.GetByIdsAsync(organizationMemberIds, cancellationToken);
        var userIds = organizationMembers.Select(x => x.UserId).Distinct().ToList();
        var users = await userRepository.ListByIdsAsync(userIds, cancellationToken);
        var userMap = users.ToDictionary(x => x.Id, x => x.ToDto());

        var organizationMemberMap = organizationMembers
            .Select(x => x.ToDto(userMap.GetValueOrDefault(x.UserId)))
            .ToDictionary(x => x.Id, x => x);

        return teamMembers.ToDictionary(
            x => x.Id,
            x => x.ToDto(
                teamMap.GetValueOrDefault(x.TeamId),
                organizationMemberMap.GetValueOrDefault(x.OrganizationMemberId)));
    }

    private static async Task<Dictionary<Guid, TaskCommentMemberDto>> BuildCommentMemberMapAsync(
        IReadOnlyCollection<TaskComment> comments,
        ITeamMemberRepository teamMemberRepository,
        IOrganizationMemberRepository organizationMemberRepository,
        IUserRepository userRepository,
        CancellationToken cancellationToken)
    {
        var teamMemberIds = comments.Select(x => x.TeamMemberId).Distinct().ToList();
        if (teamMemberIds.Count == 0)
        {
            return [];
        }

        var teamMembers = await teamMemberRepository.GetByIdsAsync(teamMemberIds, cancellationToken);
        var organizationMemberIds = teamMembers.Select(x => x.OrganizationMemberId).Distinct().ToList();
        var organizationMembers = await organizationMemberRepository.GetByIdsAsync(organizationMemberIds, cancellationToken);
        var userIds = organizationMembers.Select(x => x.UserId).Distinct().ToList();
        var users = await userRepository.ListByIdsAsync(userIds, cancellationToken);

        var organizationMemberMap = organizationMembers.ToDictionary(x => x.Id);
        var userMap = users.ToDictionary(x => x.Id);
        var result = new Dictionary<Guid, TaskCommentMemberDto>();

        foreach (var teamMember in teamMembers)
        {
            if (!organizationMemberMap.TryGetValue(teamMember.OrganizationMemberId, out var organizationMember))
            {
                continue;
            }

            userMap.TryGetValue(organizationMember.UserId, out var user);
            result[teamMember.Id] = teamMember.ToCommentMemberDto(organizationMember, user);
        }

        return result;
    }
}
