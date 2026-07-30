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

namespace Pipexi.Application.Features.Teams.Queries.GetTeamMembers;

public sealed record GetTeamMembersQuery(Guid TeamId, Guid? ScopedOrganizationId = null) : IQuery<Result<IReadOnlyCollection<TeamMemberDto>>>
{
    public sealed class Handler : IRequestHandler<GetTeamMembersQuery, Result<IReadOnlyCollection<TeamMemberDto>>>
    {
        private readonly ITeamMemberRepository _teamMemberRepository;
        private readonly ITeamRepository _teamRepository;
        private readonly IOrganizationMemberRepository _organizationMemberRepository;
        private readonly IUserRepository _userRepository;
        private readonly IOrganizationAccessService _organizationAccess;

        public Handler(
            ITeamMemberRepository teamMemberRepository,
            ITeamRepository teamRepository,
            IOrganizationMemberRepository organizationMemberRepository,
            IUserRepository userRepository,
            IOrganizationAccessService organizationAccess)
        {
            _teamMemberRepository = teamMemberRepository;
            _teamRepository = teamRepository;
            _organizationMemberRepository = organizationMemberRepository;
            _userRepository = userRepository;
            _organizationAccess = organizationAccess;
        }

        public async Task<Result<IReadOnlyCollection<TeamMemberDto>>> Handle(GetTeamMembersQuery request, CancellationToken cancellationToken)
        {
            var team = await _teamRepository.GetByIdAsync(request.TeamId, cancellationToken);
            if (team is null)
            {
                return Result<IReadOnlyCollection<TeamMemberDto>>.Failure(
                    new AppError("teams.not_found", "Team not found."),
                    (int)HttpStatusCode.NotFound);
            }

            var accessDenied = await _organizationAccess.ValidateResourceAccessAsync<IReadOnlyCollection<TeamMemberDto>>(
                team.OrganizationId, request.ScopedOrganizationId, cancellationToken);
            if (accessDenied is not null) return accessDenied;

            var teamDto = team.ToDto();

            IReadOnlyDictionary<Guid, OrganizationMemberDto> organizationMemberMap =
                new Dictionary<Guid, OrganizationMemberDto>();

            var orgMembers = await _organizationMemberRepository.ListByOrganizationIdAsync(
                team.OrganizationId,
                cancellationToken);

                var users = await _userRepository.ListByIdsAsync(
                    orgMembers.Select(x => x.UserId).Distinct().ToList(),
                    cancellationToken);

                var userMap = users
                    .Select(x => x.ToDto())
                    .ToDictionary(x => x.Id, x => x);

                organizationMemberMap = orgMembers
                    .Select(x => x.ToDto(userMap.GetValueOrDefault(x.UserId)))
                    .ToDictionary(x => x.Id, x => x);

            var items = await _teamMemberRepository.ListByTeamIdAsync(request.TeamId, cancellationToken);
            var dtos = items
                .Select(x =>
                    x.ToDto(
                        teamDto,
                        organizationMemberMap.GetValueOrDefault(x.OrganizationMemberId)))
                .ToList();

            return Result<IReadOnlyCollection<TeamMemberDto>>.Success(dtos);
        }
    }
}
