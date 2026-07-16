using MediatR;
using Workforce.Application.Abstractions.Persistence;
using Workforce.Application.Common.Models;
using Workforce.Application.Features.RolePermissions.Dtos;
using Workforce.Shared.Results;

namespace Workforce.Application.Features.RolePermissions.Queries.GetRolePermissions;

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
            var items = request.RoleId.HasValue
                ? await _rolePermissionRepository.ListByRoleIdAsync(request.RoleId.Value, cancellationToken)
                : await _rolePermissionRepository.GetAllAsync(cancellationToken);

            return Result<IReadOnlyCollection<RolePermissionDto>>.Success(items.Select(x => x.ToDto()).ToList());
        }
    }
}
