using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.OrganizationMembers;
using Pipexi.Application.Features.Teams.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Teams.Commands.UpdateTeam;

public sealed record UpdateTeamCommand(
    Guid Id,
    string? Name,
    Guid? ManagerMemberId,
    string? Status) : ICommand<Result<TeamDto>>
{
    public sealed class Handler : IRequestHandler<UpdateTeamCommand, Result<TeamDto>>
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

        public async Task<Result<TeamDto>> Handle(UpdateTeamCommand request, CancellationToken cancellationToken)
        {
            var team = await _teamRepository.GetByIdAsync(request.Id, cancellationToken);
            if (team is null)
            {
                return Result<TeamDto>.Failure(
                    new AppError("teams.not_found", "Team not found."),
                    (int)HttpStatusCode.NotFound);
            }

            if (request.ManagerMemberId.HasValue)
            {
                var managerMember = await _organizationMemberRepository.GetByIdAsync(request.ManagerMemberId.Value, cancellationToken);
                if (managerMember is null || managerMember.OrganizationId != team.OrganizationId)
                {
                    return Result<TeamDto>.Failure(
                        new AppError("teams.invalid_manager_member", "Manager member not found for organization."),
                        (int)HttpStatusCode.BadRequest);
                }
            }

            var candidateName = request.Name ?? team.Name;
            var exists = await _teamRepository.NameExistsAsync(
                team.OrganizationId,
                candidateName,
                team.Id,
                cancellationToken);

            if (exists)
            {
                return Result<TeamDto>.Failure(
                    new AppError("teams.name_conflict", "Team name already exists in this organization."),
                    (int)HttpStatusCode.Conflict);
            }

            team.UpdateDetails(request.Name, request.ManagerMemberId, request.Status);
            await _teamRepository.UpdateAsync(team, cancellationToken);

            Pipexi.Application.Features.OrganizationMembers.Dtos.OrganizationMemberDto? managerMemberDto = null;
            if (team.ManagerMemberId.HasValue)
            {
                var managerMember = await _organizationMemberRepository.GetByIdAsync(team.ManagerMemberId.Value, cancellationToken);
                if (managerMember is not null)
                {
                    var user = await _userRepository.GetByIdAsync(managerMember.UserId, cancellationToken);
                    managerMemberDto = managerMember.ToDto(user?.ToDto());
                }
            }

            return Result<TeamDto>.Success(team.ToDto(managerMemberDto), (int)HttpStatusCode.OK);
        }
    }
}
