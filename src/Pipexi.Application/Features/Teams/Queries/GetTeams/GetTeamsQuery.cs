using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.OrganizationMembers;
using Pipexi.Application.Features.OrganizationMembers.Dtos;
using Pipexi.Application.Features.Teams.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Teams.Queries.GetTeams;

public sealed record GetTeamsQuery(Guid? OrganizationId) : IQuery<Result<IReadOnlyCollection<TeamDto>>>
{
    public sealed class Handler : IRequestHandler<GetTeamsQuery, Result<IReadOnlyCollection<TeamDto>>>
    {
        private readonly ITeamRepository _teamRepository;
        private readonly ITeamMemberRepository _teamMemberRepository;
        private readonly IOrganizationMemberRepository _organizationMemberRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICurrentUserContext _currentUserContext;

        public Handler(
            ITeamRepository teamRepository,
            ITeamMemberRepository teamMemberRepository,
            IOrganizationMemberRepository organizationMemberRepository,
            IUserRepository userRepository,
            ICurrentUserContext currentUserContext)
        {
            _teamRepository = teamRepository;
            _teamMemberRepository = teamMemberRepository;
            _organizationMemberRepository = organizationMemberRepository;
            _userRepository = userRepository;
            _currentUserContext = currentUserContext;
        }

        public async Task<Result<IReadOnlyCollection<TeamDto>>> Handle(GetTeamsQuery request, CancellationToken cancellationToken)
        {
            var organizationId = request.OrganizationId ?? _currentUserContext.OrganizationId;
            if (organizationId == Guid.Empty)
            {
                return Result<IReadOnlyCollection<TeamDto>>.Failure(
                    new AppError("auth.organization_required", "Organization is required."),
                    (int)HttpStatusCode.Forbidden);
            }

            var items = await _teamRepository.ListByOrganizationIdAsync(organizationId, cancellationToken);
            var teamIds = items.Select(x => x.Id).ToList();

            var memberCounts = await _teamMemberRepository.CountByTeamIdsAsync(teamIds, cancellationToken);

            var managerMemberIds = items
                .Where(x => x.ManagerMemberId.HasValue)
                .Select(x => x.ManagerMemberId!.Value)
                .Distinct()
                .ToList();

            var managerTeamMemberIdByTeamId = new Dictionary<Guid, Guid>();
            if (teamIds.Count > 0 && managerMemberIds.Count > 0)
            {
                var managerTeamMembers = await _teamMemberRepository
                    .ListByTeamIdsAndOrganizationMemberIdsAsync(teamIds, managerMemberIds, cancellationToken);

                foreach (var team in items)
                {
                    if (!team.ManagerMemberId.HasValue)
                    {
                        continue;
                    }

                    var managerTeamMember = managerTeamMembers.FirstOrDefault(x =>
                        x.TeamId == team.Id &&
                        x.OrganizationMemberId == team.ManagerMemberId.Value);

                    if (managerTeamMember is not null)
                    {
                        managerTeamMemberIdByTeamId[team.Id] = managerTeamMember.Id;
                    }
                }
            }

            var managerMemberMap = new Dictionary<Guid, OrganizationMemberDto>();
            if (managerMemberIds.Count > 0)
            {
                var managerMembers = await _organizationMemberRepository.GetByIdsAsync(managerMemberIds, cancellationToken);
                var userIds = managerMembers.Select(x => x.UserId).Distinct().ToList();
                var users = userIds.Count > 0
                    ? await _userRepository.ListByIdsAsync(userIds, cancellationToken)
                    : Array.Empty<Pipexi.Domain.Entities.User>();
                var userMap = users.ToDictionary(x => x.Id, x => x.ToDto());

                managerMemberMap = managerMembers.ToDictionary(
                    x => x.Id,
                    x => x.ToDto(userMap.GetValueOrDefault(x.UserId)));
            }

            var dtos = items
                .Select(x =>
                {
                    managerTeamMemberIdByTeamId.TryGetValue(x.Id, out var managerTeamMemberId);

                    return x.ToDto(
                        x.ManagerMemberId.HasValue
                            ? managerMemberMap.GetValueOrDefault(x.ManagerMemberId.Value)
                            : null,
                        memberCounts.GetValueOrDefault(x.Id),
                        managerTeamMemberId == Guid.Empty ? null : managerTeamMemberId);
                })
                .ToList();

            return Result<IReadOnlyCollection<TeamDto>>.Success(dtos);
        }
    }
}
