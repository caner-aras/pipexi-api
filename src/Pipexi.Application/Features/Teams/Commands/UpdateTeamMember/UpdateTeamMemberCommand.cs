using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.OrganizationMembers;
using Pipexi.Application.Features.Teams.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Teams.Commands.UpdateTeamMember;

public sealed record UpdateTeamMemberCommand(Guid Id, string? Status, Guid? ScopedOrganizationId = null) : ICommand<Result<TeamMemberDto>>
{
    public sealed class Handler : IRequestHandler<UpdateTeamMemberCommand, Result<TeamMemberDto>>
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
            _organizationAccess = organizationAccess;
            _teamMemberRepository = teamMemberRepository;
            _teamRepository = teamRepository;
            _organizationMemberRepository = organizationMemberRepository;
            _userRepository = userRepository;
        }

        public async Task<Result<TeamMemberDto>> Handle(UpdateTeamMemberCommand request, CancellationToken cancellationToken)
        {
            var teamMember = await _teamMemberRepository.GetByIdAsync(request.Id, cancellationToken);
            if (teamMember is null)
            {
                return Result<TeamMemberDto>.Failure(
                    new AppError("team_members.not_found", "Team member not found."),
                    (int)HttpStatusCode.NotFound);
            }

            var team = await _teamRepository.GetByIdAsync(teamMember.TeamId, cancellationToken);
            if (team is null)
            {
                return Result<TeamMemberDto>.Failure(
                    new AppError("teams.not_found", "Team not found."),
                    (int)HttpStatusCode.NotFound);
            }

            var accessDenied = await _organizationAccess.ValidateResourceAccessAsync<TeamMemberDto>(
                team.OrganizationId, request.ScopedOrganizationId, cancellationToken);
            if (accessDenied is not null) return accessDenied;
            teamMember.UpdateDetails(request.Status);
            await _teamMemberRepository.UpdateAsync(teamMember, cancellationToken);

            var organizationMember = await _organizationMemberRepository.GetByIdAsync(
                teamMember.OrganizationMemberId,
                cancellationToken);
            var user = organizationMember is null
                ? null
                : await _userRepository.GetByIdAsync(organizationMember.UserId, cancellationToken);

            return Result<TeamMemberDto>.Success(
                teamMember.ToDto(team?.ToDto(), organizationMember?.ToDto(user?.ToDto())),
                (int)HttpStatusCode.OK);
        }
    }
}
