using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Abstractions.Auth;
using Pipexi.Application.Common;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.OrganizationMembers;
using Pipexi.Application.Features.Teams.Dtos;
using Pipexi.Domain.Entities;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Teams.Commands.CreateTeamMemberWithUser;

public sealed record CreateTeamMemberWithUserCommand(
    Guid TeamId,
    string Email,
    string FirstName,
    string LastName,
    Guid RoleId,
    string? JobTitle,
    string? Phone,
    string? AvatarUrl,
    string? AuthProviderId = null, Guid? ScopedOrganizationId = null) : ICommand<Result<TeamMemberDto>>
{
    public sealed class Handler : IRequestHandler<CreateTeamMemberWithUserCommand, Result<TeamMemberDto>>
    {
        private readonly ITeamMemberRepository _teamMemberRepository;
        private readonly ITeamRepository _teamRepository;
        private readonly IOrganizationMemberRepository _organizationMemberRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IUserRepository _userRepository;
        private readonly IOrganizationAccessService _organizationAccess;
        private readonly ITokenService _tokenService;

        public Handler(
            ITeamMemberRepository teamMemberRepository,
            ITeamRepository teamRepository,
            IOrganizationMemberRepository organizationMemberRepository,
            IRoleRepository roleRepository,
            IUserRepository userRepository,
            IOrganizationAccessService organizationAccess,
            ITokenService tokenService)
        {
            _organizationAccess = organizationAccess;
            _teamMemberRepository = teamMemberRepository;
            _teamRepository = teamRepository;
            _organizationMemberRepository = organizationMemberRepository;
            _roleRepository = roleRepository;
            _userRepository = userRepository;
            _tokenService = tokenService;
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


            var accessDenied = await _organizationAccess.ValidateResourceAccessAsync<TeamMemberDto>(
                team.OrganizationId, request.ScopedOrganizationId, cancellationToken);
            if (accessDenied is not null) return accessDenied;
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
                var inviteResult = await _tokenService.InviteUserAsync(request.Email, cancellationToken);
                if (!inviteResult.IsSuccess)
                {
                    return Result<TeamMemberDto>.Failure(inviteResult.Error!, inviteResult.StatusCode);
                }

                if (!Guid.TryParse(inviteResult.Data!.user_id, out var parsedUserId))
                {
                    return Result<TeamMemberDto>.Failure(
                        new AppError("invite_failed", "Invalid user ID returned from invite service."),
                        500);
                }

                user = User.Create(
                    parsedUserId,
                    "supabase",
                    request.Email,
                    request.FirstName,
                    request.LastName,
                    request.Phone,
                    AvatarUrls.Resolve(parsedUserId, request.AvatarUrl));

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
