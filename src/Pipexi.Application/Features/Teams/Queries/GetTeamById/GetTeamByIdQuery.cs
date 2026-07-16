using System.Net;
using MediatR;
using Workforce.Application.Abstractions.Persistence;
using Workforce.Application.Common.Models;
using Workforce.Application.Features.OrganizationMembers;
using Workforce.Application.Features.Teams.Dtos;
using Workforce.Shared.Errors;
using Workforce.Shared.Results;

namespace Workforce.Application.Features.Teams.Queries.GetTeamById;

public sealed record GetTeamByIdQuery(Guid Id) : IQuery<Result<TeamDto>>
{
    public sealed class Handler : IRequestHandler<GetTeamByIdQuery, Result<TeamDto>>
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

        public async Task<Result<TeamDto>> Handle(GetTeamByIdQuery request, CancellationToken cancellationToken)
        {
            var team = await _teamRepository.GetByIdAsync(request.Id, cancellationToken);
            if (team is null)
            {
                return Result<TeamDto>.Failure(
                    new AppError("teams.not_found", "Team not found."),
                    (int)HttpStatusCode.NotFound);
            }

            Workforce.Application.Features.OrganizationMembers.Dtos.OrganizationMemberDto? managerMemberDto = null;
            if (team.ManagerMemberId.HasValue)
            {
                var managerMember = await _organizationMemberRepository.GetByIdAsync(team.ManagerMemberId.Value, cancellationToken);
                if (managerMember is not null)
                {
                    var user = await _userRepository.GetByIdAsync(managerMember.UserId, cancellationToken);
                    managerMemberDto = managerMember.ToDto(user?.ToDto());
                }
            }

            return Result<TeamDto>.Success(team.ToDto(managerMemberDto));
        }
    }
}
