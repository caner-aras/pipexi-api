using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Organizations.Provisioning;
using Pipexi.Application.Features.Roles;
using Pipexi.Application.Features.Roles.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Roles.Queries.GetRoles;

public sealed record GetRolesQuery(Guid? OrganizationId) : IQuery<Result<IReadOnlyCollection<RoleDto>>>
{
    public sealed class Handler : IRequestHandler<GetRolesQuery, Result<IReadOnlyCollection<RoleDto>>>
    {
        private readonly IRoleRepository _roleRepository;
        private readonly ICurrentUserContext _currentUserContext;

        public Handler(IRoleRepository roleRepository, ICurrentUserContext currentUserContext)
        {
            _roleRepository = roleRepository;
            _currentUserContext = currentUserContext;
        }

        public async Task<Result<IReadOnlyCollection<RoleDto>>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
        {
            var organizationId = request.OrganizationId ?? _currentUserContext.OrganizationId;
            if (organizationId == Guid.Empty)
            {
                return Result<IReadOnlyCollection<RoleDto>>.Failure(
                    new AppError("auth.organization_required", "Organization is required."),
                    (int)HttpStatusCode.Forbidden);
            }

            var items = await OrganizationDefaultRoleProvisioner.EnsureDefaultRolesAsync(
                _roleRepository,
                organizationId,
                cancellationToken);

            return Result<IReadOnlyCollection<RoleDto>>.Success(
                items
                    .OrderBy(role => role.Name, StringComparer.OrdinalIgnoreCase)
                    .Select(x => x.ToDto())
                    .ToList());
        }
    }
}
