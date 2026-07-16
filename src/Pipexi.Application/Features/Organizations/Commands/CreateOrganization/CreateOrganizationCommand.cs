using System.Net;
using MediatR;
using Workforce.Application.Abstractions.Identity;
using Workforce.Application.Abstractions.Persistence;
using Workforce.Application.Common.Models;
using Workforce.Application.Features.Organizations.Dtos;
using Workforce.Application.Features.Organizations.Provisioning;
using Workforce.Domain.Entities;
using Workforce.Shared.Errors;
using Workforce.Shared.Results;

namespace Workforce.Application.Features.Organizations.Commands.CreateOrganization;

public sealed record CreateOrganizationCommand(
    string Name,
    string Slug,
    string Timezone) : ICommand<Result<OrganizationDto>>
{
    public sealed class Handler : IRequestHandler<CreateOrganizationCommand, Result<OrganizationDto>>
    {
        private const string DefaultOwnersTeamName = "Owners";

        private readonly IOrganizationRepository _organizationRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IOrganizationMemberRepository _organizationMemberRepository;
        private readonly ITeamRepository _teamRepository;
        private readonly ITeamMemberRepository _teamMemberRepository;
        private readonly IPermissionRepository _permissionRepository;
        private readonly IRolePermissionRepository _rolePermissionRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICurrentUserContext _currentUserContext;

        public Handler(
            IOrganizationRepository organizationRepository,
            IRoleRepository roleRepository,
            IOrganizationMemberRepository organizationMemberRepository,
            ITeamRepository teamRepository,
            ITeamMemberRepository teamMemberRepository,
            IPermissionRepository permissionRepository,
            IRolePermissionRepository rolePermissionRepository,
            IUserRepository userRepository,
            ICurrentUserContext currentUserContext)
        {
            _organizationRepository = organizationRepository;
            _roleRepository = roleRepository;
            _organizationMemberRepository = organizationMemberRepository;
            _teamRepository = teamRepository;
            _teamMemberRepository = teamMemberRepository;
            _permissionRepository = permissionRepository;
            _rolePermissionRepository = rolePermissionRepository;
            _userRepository = userRepository;
            _currentUserContext = currentUserContext;
        }

        public async Task<Result<OrganizationDto>> Handle(
            CreateOrganizationCommand request,
            CancellationToken cancellationToken)
        {
            var slugExists = await _organizationRepository.SlugExistsAsync(
                request.Slug,
                cancellationToken: cancellationToken);

            if (slugExists)
            {
                return Result<OrganizationDto>.Failure(
                    new AppError("organizations.slug_conflict", "Organization slug already exists."),
                    (int)HttpStatusCode.Conflict);
            }

            var currentUserId = _currentUserContext.UserId;
            if (currentUserId == Guid.Empty)
            {
                return Result<OrganizationDto>.Failure(
                    new AppError("organizations.unauthorized", "Current user could not be resolved from token."),
                    (int)HttpStatusCode.Unauthorized);
            }

            var userExists = await _userRepository.ExistsAsync(currentUserId, cancellationToken);
            if (!userExists)
            {
                return Result<OrganizationDto>.Failure(
                    new AppError("organizations.user_not_found", "Current user was not found in users table."),
                    (int)HttpStatusCode.BadRequest);
            }

            var organization = Organization.Create(request.Name, request.Slug, request.Timezone);
            await _organizationRepository.AddAsync(organization, cancellationToken);

            var roles = new List<Role>
            {
                Role.Create(organization.Id, OrganizationRoleType.Owner.ToRoleName())
            };

            await _roleRepository.AddRangeAsync(roles, cancellationToken);

            var ownerRole = roles.First(x => x.Name == OrganizationRoleType.Owner.ToRoleName());
            var ownerMembership = OrganizationMember.Create(
                organization.Id,
                currentUserId,
                ownerRole.Id,
                "Organization Owner");

            await _organizationMemberRepository.AddAsync(ownerMembership, cancellationToken);

            var ownersTeam = Team.Create(
                organization.Id,
                DefaultOwnersTeamName,
                ownerMembership.Id);

            await _teamRepository.AddAsync(ownersTeam, cancellationToken);

            var ownerTeamMember = TeamMember.Create(ownersTeam.Id, ownerMembership.Id);
            await _teamMemberRepository.AddAsync(ownerTeamMember, cancellationToken);

            var permissions = await _permissionRepository.GetAllAsync(cancellationToken);
            var ownerRolePermissions = permissions
                .Select(permission => RolePermission.Create(ownerRole.Id, permission.Id))
                .ToList();

            if (ownerRolePermissions.Count > 0)
            {
                await _rolePermissionRepository.AddRangeAsync(ownerRolePermissions, cancellationToken);
            }

            return Result<OrganizationDto>.Success(
                organization.ToDto(),
                (int)HttpStatusCode.Created);
        }
    }
}
