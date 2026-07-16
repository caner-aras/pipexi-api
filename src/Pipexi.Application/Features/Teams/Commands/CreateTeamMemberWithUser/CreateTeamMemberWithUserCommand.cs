using System.Net;
using MediatR;
using Workforce.Application.Abstractions.Persistence;
using Workforce.Application.Common.Models;
using Workforce.Application.Features.OrganizationMembers;
using Workforce.Application.Features.Teams.Dtos;
using Workforce.Domain.Entities;
using Workforce.Shared.Errors;
using Workforce.Shared.Results;

namespace Workforce.Application.Features.Teams.Commands.CreateTeamMemberWithUser;

public sealed record CreateTeamMemberWithUserCommand(
    Guid TeamId,
    string Email,
    string FirstName,
    string LastName,
    Guid RoleId,
    string? JobTitle,
    string? Phone,
    string? AvatarUrl,
    string? AuthProviderId = null) : ICommand<Result<TeamMemberDto>>
{
    public sealed class Handler : IRequestHandler<CreateTeamMemberWithUserCommand, Result<TeamMemberDto>>
    {
        private readonly ITeamMemberRepository _teamMemberRepository;
        private readonly ITeamRepository _teamRepository;
        private readonly IOrganizationMemberRepository _organizationMemberRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IUserRepository _userRepository;

        public Handler(
            ITeamMemberRepository teamMemberRepository,
            ITeamRepository teamRepository,
            IOrganizationMemberRepository organizationMemberRepository,
            IRoleRepository roleRepository,
            IUserRepository userRepository)
        {
            _teamMemberRepository = teamMemberRepository;
            _teamRepository = teamRepository;
            _organizationMemberRepository = organizationMemberRepository;
            _roleRepository = roleRepository;
            _userRepository = userRepository;
        }

        public async Task<Result<TeamMemberDto>> Handle(CreateTeamMemberWithUserCommand request, CancellationToken cancellationToken)
        {
            var team = await _teamRepository.GetByIdAsync(request.TeamId, cancellationToken);
            if (team is null)
            {
                return Result<TeamMemberDto>.Failure(
                    new AppError("team_members.invalid_team", "Team not found."),
                    (int)HttpStatusCode.BadRequest);
            }

            var role = await _roleRepository.GetByIdAsync(request.RoleId, cancellationToken);
            if (role is null || role.OrganizationId != team.OrganizationId)
            {
                return Result<TeamMemberDto>.Failure(
                    new AppError("team_members.invalid_role", "Role not found for team organization."),
                    (int)HttpStatusCode.BadRequest);
            }

            var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (user is null)
            {
                var authProviderId = string.IsNullOrWhiteSpace(request.AuthProviderId)
                    ? $"local:{Guid.NewGuid():N}"
                    : request.AuthProviderId;

                user = User.Create(
                    Guid.NewGuid(),
                    authProviderId,
                    request.Email,
                    request.FirstName,
                    request.LastName,
                    request.Phone,
                    request.AvatarUrl);

                await _userRepository.AddAsync(user, cancellationToken);
            }

            var organizationMember = await _organizationMemberRepository.GetByOrganizationIdAndUserIdAsync(
                team.OrganizationId,
                user.Id,
                cancellationToken);

            if (organizationMember is null)
            {
                organizationMember = OrganizationMember.Create(
                    team.OrganizationId,
                    user.Id,
                    request.RoleId,
                    request.JobTitle);

                await _organizationMemberRepository.AddAsync(organizationMember, cancellationToken);
            }

            var teamMemberExists = await _teamMemberRepository.ExistsAsync(
                team.Id,
                organizationMember.Id,
                cancellationToken: cancellationToken);

            if (teamMemberExists)
            {
                return Result<TeamMemberDto>.Failure(
                    new AppError("team_members.conflict", "Team member already exists."),
                    (int)HttpStatusCode.Conflict);
            }

            var teamMember = TeamMember.Create(team.Id, organizationMember.Id);
            await _teamMemberRepository.AddAsync(teamMember, cancellationToken);

            return Result<TeamMemberDto>.Success(
                teamMember.ToDto(team.ToDto(), organizationMember.ToDto(user.ToDto())),
                (int)HttpStatusCode.Created);
        }
    }
}
