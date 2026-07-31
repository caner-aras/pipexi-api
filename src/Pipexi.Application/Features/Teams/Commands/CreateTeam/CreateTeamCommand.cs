using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.OrganizationMembers;
using Pipexi.Application.Features.Teams.Dtos;
using Pipexi.Domain.Entities;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Teams.Commands.CreateTeam;

public sealed record CreateTeamCommand(
    Guid OrganizationId,
    string Name,
    Guid? ManagerMemberId,
    Guid? LocationId = null) : ICommand<Result<TeamDto>>
{
    public sealed class Handler : IRequestHandler<CreateTeamCommand, Result<TeamDto>>
    {
        private readonly ITeamRepository _teamRepository;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IOrganizationMemberRepository _organizationMemberRepository;
        private readonly ILocationRepository _locationRepository;
        private readonly IUserRepository _userRepository;

        public Handler(
            ITeamRepository teamRepository,
            IOrganizationRepository organizationRepository,
            IOrganizationMemberRepository organizationMemberRepository,
            ILocationRepository locationRepository,
            IUserRepository userRepository)
        {
            _teamRepository = teamRepository;
            _organizationRepository = organizationRepository;
            _organizationMemberRepository = organizationMemberRepository;
            _locationRepository = locationRepository;
            _userRepository = userRepository;
        }

        public async Task<Result<TeamDto>> Handle(CreateTeamCommand request, CancellationToken cancellationToken)
        {
            var organization = await _organizationRepository.GetByIdAsync(request.OrganizationId, cancellationToken);
            if (organization is null)
            {
                return Result<TeamDto>.Failure(
                    new AppError("teams.invalid_organization", "Organization not found."),
                    (int)HttpStatusCode.BadRequest);
            }

            if (request.ManagerMemberId.HasValue)
            {
                var managerMember = await _organizationMemberRepository.GetByIdAsync(request.ManagerMemberId.Value, cancellationToken);
                if (managerMember is null || managerMember.OrganizationId != request.OrganizationId)
                {
                    return Result<TeamDto>.Failure(
                        new AppError("teams.invalid_manager_member", "Manager member not found for organization."),
                        (int)HttpStatusCode.BadRequest);
                }
            }

            if (request.LocationId.HasValue)
            {
                var location = await _locationRepository.GetByIdAsync(request.LocationId.Value, cancellationToken);
                if (location is null || location.OrganizationId != request.OrganizationId)
                {
                    return Result<TeamDto>.Failure(
                        new AppError("teams.invalid_location", "Location not found for organization."),
                        (int)HttpStatusCode.BadRequest);
                }
            }

            var exists = await _teamRepository.NameExistsAsync(
                request.OrganizationId,
                request.Name,
                cancellationToken: cancellationToken);

            if (exists)
            {
                return Result<TeamDto>.Failure(
                    new AppError("teams.name_conflict", "Team name already exists in this organization."),
                    (int)HttpStatusCode.Conflict);
            }

            var team = Team.Create(
                request.OrganizationId,
                request.Name,
                request.ManagerMemberId,
                request.LocationId);
            await _teamRepository.AddAsync(team, cancellationToken);

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

            return Result<TeamDto>.Success(team.ToDto(managerMemberDto), (int)HttpStatusCode.Created);
        }
    }
}
