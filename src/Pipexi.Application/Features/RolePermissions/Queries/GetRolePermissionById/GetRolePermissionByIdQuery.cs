using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.RolePermissions.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.RolePermissions.Queries.GetRolePermissionById;

public sealed record GetRolePermissionByIdQuery(Guid Id) : IQuery<Result<RolePermissionDto>>
{
    public sealed class Handler : IRequestHandler<GetRolePermissionByIdQuery, Result<RolePermissionDto>>
    {
        private readonly IRolePermissionRepository _rolePermissionRepository;

        public Handler(IRolePermissionRepository rolePermissionRepository)
        {
            _rolePermissionRepository = rolePermissionRepository;
        }

        public async Task<Result<RolePermissionDto>> Handle(GetRolePermissionByIdQuery request, CancellationToken cancellationToken)
        {
            var rolePermission = await _rolePermissionRepository.GetByIdAsync(request.Id, cancellationToken);
            if (rolePermission is null)
            {
                return Result<RolePermissionDto>.Failure(
                    new AppError("role_permissions.not_found", "Role permission not found."),
                    (int)HttpStatusCode.NotFound);
            }

            return Result<RolePermissionDto>.Success(rolePermission.ToDto());
        }
    }
}
