using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.RolePermissions.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.RolePermissions.Queries.GetRolePermissions;

public sealed record GetRolePermissionsQuery(Guid? RoleId) : IQuery<Result<IReadOnlyCollection<RolePermissionDto>>>
{
    public sealed class Handler : IRequestHandler<GetRolePermissionsQuery, Result<IReadOnlyCollection<RolePermissionDto>>>
    {
        private readonly IRolePermissionRepository _rolePermissionRepository;

        public Handler(IRolePermissionRepository rolePermissionRepository)
        {
            _rolePermissionRepository = rolePermissionRepository;
        }

        public async Task<Result<IReadOnlyCollection<RolePermissionDto>>> Handle(GetRolePermissionsQuery request, CancellationToken cancellationToken)
        {
            if (!request.RoleId.HasValue || request.RoleId.Value == Guid.Empty)
            {
                return Result<IReadOnlyCollection<RolePermissionDto>>.Failure(
                    new AppError("auth.organization_required", "Organization is required."),
                    (int)HttpStatusCode.Forbidden);
            }

            var items = await _rolePermissionRepository.ListByRoleIdAsync(request.RoleId.Value, cancellationToken);

            return Result<IReadOnlyCollection<RolePermissionDto>>.Success(items.Select(x => x.ToDto()).ToList());
        }
    }
}
