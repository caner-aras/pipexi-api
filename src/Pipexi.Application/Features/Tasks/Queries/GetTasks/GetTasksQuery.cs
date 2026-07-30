using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Organizations.Provisioning;
using Pipexi.Application.Features.Tasks.Dtos;
using Pipexi.Shared.Errors;
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
        private readonly ITeamMemberRepository _teamMemberRepository;
        private readonly ITeamRepository _teamRepository;
        private readonly IOrganizationMemberRepository _organizationMemberRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICurrentUserContext _currentUserContext;

        public Handler(
            IWorkTaskRepository workTaskRepository,
            ITeamMemberRepository teamMemberRepository,
            ITeamRepository teamRepository,
            IOrganizationMemberRepository organizationMemberRepository,
            IRoleRepository roleRepository,
            IUserRepository userRepository,
            ICurrentUserContext currentUserContext)
        {
            _workTaskRepository = workTaskRepository;
            _teamMemberRepository = teamMemberRepository;
            _teamRepository = teamRepository;
            _organizationMemberRepository = organizationMemberRepository;
            _roleRepository = roleRepository;
            _userRepository = userRepository;
            _currentUserContext = currentUserContext;
        }

        public async Task<Result<IReadOnlyCollection<TaskDto>>> Handle(GetTasksQuery request, CancellationToken cancellationToken)
        {
            var organizationId = request.OrganizationId ?? _currentUserContext.OrganizationId;

            IReadOnlyCollection<Pipexi.Domain.Entities.WorkTask> tasks;
            if (request.TeamId.HasValue)
            {
                tasks = await _workTaskRepository.ListByAssignedTeamIdAsync(request.TeamId.Value, cancellationToken);
                if (organizationId != Guid.Empty)
                {
                    tasks = tasks.Where(x => x.OrganizationId == organizationId).ToList();
                }
            }
            else if (organizationId != Guid.Empty)
            {
                tasks = await _workTaskRepository.ListByOrganizationIdAsync(organizationId, cancellationToken);
            }
            else
            {
                return Result<IReadOnlyCollection<TaskDto>>.Failure(
                    new AppError("auth.organization_required", "Organization is required."),
                    (int)HttpStatusCode.Forbidden);
            }

            if (request.UserId.HasValue &&
                !await IsOrganizationOwnerAsync(organizationId == Guid.Empty ? null : organizationId, request.UserId.Value, cancellationToken))
            {
                var organizationMembers = organizationId != Guid.Empty
                    ? await ResolveOrganizationScopedMemberAsync(organizationId, request.UserId.Value, cancellationToken)
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

            var dtos = await TaskHydration.BuildDtosAsync(
                tasks,
                includeComments: false,
                taskCommentRepository: null,
                teamMemberRepository: _teamMemberRepository,
                teamRepository: _teamRepository,
                organizationMemberRepository: _organizationMemberRepository,
                userRepository: _userRepository,
                cancellationToken);

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
