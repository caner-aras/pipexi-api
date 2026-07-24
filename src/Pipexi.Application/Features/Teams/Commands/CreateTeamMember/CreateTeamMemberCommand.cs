using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.OrganizationMembers;
using Pipexi.Application.Features.Teams.Dtos;
using Pipexi.Domain.Entities;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Teams.Commands.CreateTeamMember;

public sealed record CreateTeamMemberCommand(
    Guid TeamId,
    Guid OrganizationMemberId) : ICommand<Result<TeamMemberDto>>
{
    public sealed class Handler : IRequestHandler<CreateTeamMemberCommand, Result<TeamMemberDto>>
    {
        private readonly ITeamMemberRepository _teamMemberRepository;
        private readonly ITeamRepository _teamRepository;
        private readonly IOrganizationMemberRepository _organizationMemberRepository;
        private readonly IUserRepository _userRepository;

        public Handler(
            ITeamMemberRepository teamMemberRepository,
            ITeamRepository teamRepository,
            IOrganizationMemberRepository organizationMemberRepository,
            IUserRepository userRepository)
        {
            _teamMemberRepository = teamMemberRepository;
            _teamRepository = teamRepository;
            _organizationMemberRepository = organizationMemberRepository;
            _userRepository = userRepository;
        }

        public async Task<Result<TeamMemberDto>> Handle(CreateTeamMemberCommand request, CancellationToken cancellationToken)
        {
            var team = await _teamRepository.GetByIdAsync(request.TeamId, cancellationToken);
            if (team is null)
            {
                return Result<TeamMemberDto>.Failure(
                    new AppError("team_members.invalid_team", "Team not found."),
                    (int)HttpStatusCode.BadRequest);
            }

            var organizationMember = await _organizationMemberRepository.GetByIdAsync(request.OrganizationMemberId, cancellationToken);
            if (organizationMember is null || organizationMember.OrganizationId != team.OrganizationId)
            {
                return Result<TeamMemberDto>.Failure(
                    new AppError("team_members.invalid_organization_member", "Organization member not found for team organization."),
                    (int)HttpStatusCode.BadRequest);
            }

            var exists = await _teamMemberRepository.ExistsAsync(
                request.TeamId,
                request.OrganizationMemberId,
                cancellationToken: cancellationToken);

            if (exists)
            {
                return Result<TeamMemberDto>.Failure(
                    new AppError("team_members.conflict", "Team member already exists."),
                    (int)HttpStatusCode.Conflict);
            }

            var teamMember = TeamMember.Create(request.TeamId, request.OrganizationMemberId);
            await _teamMemberRepository.AddAsync(teamMember, cancellationToken);

            var user = await _userRepository.GetByIdAsync(organizationMember.UserId, cancellationToken);

            return Result<TeamMemberDto>.Success(
                teamMember.ToDto(team.ToDto(), organizationMember.ToDto(user?.ToDto())),
                (int)HttpStatusCode.Created);
        }
    }
}
