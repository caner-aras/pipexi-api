using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Organizations.Provisioning;
using Pipexi.Application.Features.Tasks.Dtos;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Tasks.Queries.GetTasks;

public sealed record GetTasksQuery(
    Guid? OrganizationId,
    Guid? UserId = null,
    Guid? TeamId = null) : IQuery<Result<IReadOnlyCollection<TaskDto>>>
{
    public sealed class Handler : IRequestHandler<GetTasksQuery, Result<IReadOnlyCollection<TaskDto>>>
    {
        private readonly IWorkTaskRepository _workTaskRepository;
        // private readonly ITaskCommentRepository _taskCommentRepository;
        private readonly ITeamMemberRepository _teamMemberRepository;
        private readonly IOrganizationMemberRepository _organizationMemberRepository;
        private readonly IRoleRepository _roleRepository;

        public Handler(
            IWorkTaskRepository workTaskRepository,
            // ITaskCommentRepository taskCommentRepository,
            ITeamMemberRepository teamMemberRepository,
            IOrganizationMemberRepository organizationMemberRepository,
            IRoleRepository roleRepository)
        {
            _workTaskRepository = workTaskRepository;
            // _taskCommentRepository = taskCommentRepository;
            _teamMemberRepository = teamMemberRepository;
            _organizationMemberRepository = organizationMemberRepository;
            _roleRepository = roleRepository;
        }

        public async Task<Result<IReadOnlyCollection<TaskDto>>> Handle(GetTasksQuery request, CancellationToken cancellationToken)
        {
            IReadOnlyCollection<Pipexi.Domain.Entities.WorkTask> tasks;
            if (request.TeamId.HasValue)
            {
                tasks = await _workTaskRepository.ListByAssignedTeamIdAsync(request.TeamId.Value, cancellationToken);
                if (request.OrganizationId.HasValue)
                {
                    tasks = tasks.Where(x => x.OrganizationId == request.OrganizationId.Value).ToList();
                }
            }
            else
            {
                tasks = request.OrganizationId.HasValue
                    ? await _workTaskRepository.ListByOrganizationIdAsync(request.OrganizationId.Value, cancellationToken)
                    : await _workTaskRepository.GetAllAsync(cancellationToken);
            }

            if (request.UserId.HasValue &&
                !await IsOrganizationOwnerAsync(request.OrganizationId, request.UserId.Value, cancellationToken))
            {
                var organizationMembers = request.OrganizationId.HasValue
                    ? await ResolveOrganizationScopedMemberAsync(request.OrganizationId.Value, request.UserId.Value, cancellationToken)
                    : await _organizationMemberRepository.ListByUserIdAsync(request.UserId.Value, cancellationToken);

                var teamMemberIds = new HashSet<Guid>();
                foreach (var organizationMember in organizationMembers)
                {
                    var memberTeamMembers = await _teamMemberRepository
                        .ListByOrganizationMemberIdAsync(organizationMember.Id, cancellationToken);

                    foreach (var teamMember in memberTeamMembers)
                    {
                        teamMemberIds.Add(teamMember.Id);
                    }
                }

                tasks = tasks
                    .Where(x => x.ReporterUserId == request.UserId.Value ||
                                (x.AssignedToTeamMemberId.HasValue && teamMemberIds.Contains(x.AssignedToTeamMemberId.Value)))
                    .ToList();
            }

            // var commentMap = new Dictionary<Guid, IReadOnlyCollection<TaskCommentDto>>();
            // var taskIds = tasks.Select(x => x.Id).ToList();
            // if (taskIds.Count > 0)
            // {
            //     var comments = await _taskCommentRepository.ListByWorkTaskIdsAsync(taskIds, cancellationToken);
            //     commentMap = comments
            //         .GroupBy(x => x.WorkTaskId)
            //         .ToDictionary(
            //             g => g.Key,
            //             g => (IReadOnlyCollection<TaskCommentDto>)g.OrderBy(x => x.CreatedAt).Select(x => x.ToDto()).ToList());
            // }

            var dtos = tasks
                // .Select(x => x.ToDto(commentMap.GetValueOrDefault(x.Id) ?? Array.Empty<TaskCommentDto>()))
                .Select(x => x.ToDto())
                .ToList();

            return Result<IReadOnlyCollection<TaskDto>>.Success(dtos);
        }

        private async Task<bool> IsOrganizationOwnerAsync(
            Guid? organizationId,
            Guid userId,
            CancellationToken cancellationToken)
        {
            if (!organizationId.HasValue)
            {
                return false;
            }

            var membership = await _organizationMemberRepository
                .GetByOrganizationIdAndUserIdAsync(organizationId.Value, userId, cancellationToken);

            if (membership is null)
            {
                return false;
            }

            var role = await _roleRepository.GetByIdAsync(membership.RoleId, cancellationToken);
            if (role is null)
            {
                return false;
            }

            return string.Equals(
                role.Name,
                OrganizationRoleType.Owner.ToRoleName(),
                StringComparison.OrdinalIgnoreCase);
        }

        private async Task<IReadOnlyCollection<Pipexi.Domain.Entities.OrganizationMember>> ResolveOrganizationScopedMemberAsync(
            Guid organizationId,
            Guid userId,
            CancellationToken cancellationToken)
        {
            var member = await _organizationMemberRepository
                .GetByOrganizationIdAndUserIdAsync(organizationId, userId, cancellationToken);

            return member is null
                ? []
                : [member];
        }
    }
}
