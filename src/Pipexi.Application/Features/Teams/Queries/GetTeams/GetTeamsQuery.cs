using MediatR;
using Workforce.Application.Abstractions.Persistence;
using Workforce.Application.Common.Models;
using Workforce.Application.Features.OrganizationMembers;
using Workforce.Application.Features.OrganizationMembers.Dtos;
using Workforce.Application.Features.Teams.Dtos;
using Workforce.Shared.Results;

namespace Workforce.Application.Features.Teams.Queries.GetTeams;

public sealed record GetTeamsQuery(Guid? OrganizationId) : IQuery<Result<IReadOnlyCollection<TeamDto>>>
{
    public sealed class Handler : IRequestHandler<GetTeamsQuery, Result<IReadOnlyCollection<TeamDto>>>
    {
        private readonly ITeamRepository _teamRepository;
        private readonly IOrganizationMemberRepository _organizationMemberRepository;
        private readonly IUserRepository _userRepository;

        public Handler(
            ITeamRepository teamRepository,
            IOrganizationMemberRepository organizationMemberRepository,
            IUserRepository userRepository)
        {
            _teamRepository = teamRepository;
            _organizationMemberRepository = organizationMemberRepository;
            _userRepository = userRepository;
        }

        public async Task<Result<IReadOnlyCollection<TeamDto>>> Handle(GetTeamsQuery request, CancellationToken cancellationToken)
        {
            var items = request.OrganizationId.HasValue
                ? await _teamRepository.ListByOrganizationIdAsync(request.OrganizationId.Value, cancellationToken)
                : await _teamRepository.GetAllAsync(cancellationToken);

            var managerMemberIds = items
                .Where(x => x.ManagerMemberId.HasValue)
                .Select(x => x.ManagerMemberId!.Value)
                .Distinct()
                .ToList();

            var managerMemberMap = new Dictionary<Guid, OrganizationMemberDto>();
            if (managerMemberIds.Count > 0)
            {
                var managerMembers = await _organizationMemberRepository.GetByIdsAsync(managerMemberIds, cancellationToken);
                var userIds = managerMembers.Select(x => x.UserId).Distinct().ToList();
                var users = userIds.Count > 0
                    ? await _userRepository.ListByIdsAsync(userIds, cancellationToken)
                    : Array.Empty<Workforce.Domain.Entities.User>();
                var userMap = users.ToDictionary(x => x.Id, x => x.ToDto());

                managerMemberMap = managerMembers.ToDictionary(
                    x => x.Id,
                    x => x.ToDto(userMap.GetValueOrDefault(x.UserId)));
            }

            var dtos = items
                .Select(x => x.ToDto(
                    x.ManagerMemberId.HasValue
                        ? managerMemberMap.GetValueOrDefault(x.ManagerMemberId.Value)
                        : null))
                .ToList();

            return Result<IReadOnlyCollection<TeamDto>>.Success(dtos);
        }
    }
}
