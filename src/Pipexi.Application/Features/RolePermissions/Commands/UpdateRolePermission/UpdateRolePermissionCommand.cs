using System.Net;
using MediatR;
using Workforce.Application.Abstractions.Persistence;
using Workforce.Application.Common.Models;
using Workforce.Application.Features.RolePermissions.Dtos;
using Workforce.Shared.Errors;
using Workforce.Shared.Results;

namespace Workforce.Application.Features.RolePermissions.Commands.UpdateRolePermission;

public sealed record UpdateRolePermissionCommand(Guid Id, string? Status) : ICommand<Result<RolePermissionDto>>
{
    public sealed class Handler : IRequestHandler<UpdateRolePermissionCommand, Result<RolePermissionDto>>
    {
        private readonly IRolePermissionRepository _rolePermissionRepository;

        public Handler(IRolePermissionRepository rolePermissionRepository)
        {
            _rolePermissionRepository = rolePermissionRepository;
        }

        public async Task<Result<RolePermissionDto>> Handle(UpdateRolePermissionCommand request, CancellationToken cancellationToken)
        {
            var rolePermission = await _rolePermissionRepository.GetByIdAsync(request.Id, cancellationToken);
            if (rolePermission is null)
            {
                return Result<RolePermissionDto>.Failure(
                    new AppError("role_permissions.not_found", "Role permission not found."),
                    (int)HttpStatusCode.NotFound);
            }

            rolePermission.UpdateDetails(request.Status);
            await _rolePermissionRepository.UpdateAsync(rolePermission, cancellationToken);

            return Result<RolePermissionDto>.Success(rolePermission.ToDto(), (int)HttpStatusCode.OK);
        }
    }
}
