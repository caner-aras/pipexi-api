using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.RolePermissions.Commands.DeleteRolePermission;

public sealed record DeleteRolePermissionCommand(Guid Id) : ICommand<Result<object?>>
{
    public sealed class Handler : IRequestHandler<DeleteRolePermissionCommand, Result<object?>>
    {
        private readonly IRolePermissionRepository _rolePermissionRepository;

        public Handler(IRolePermissionRepository rolePermissionRepository)
        {
            _rolePermissionRepository = rolePermissionRepository;
        }

        public async Task<Result<object?>> Handle(DeleteRolePermissionCommand request, CancellationToken cancellationToken)
        {
            var rolePermission = await _rolePermissionRepository.GetByIdAsync(request.Id, cancellationToken);
            if (rolePermission is null)
            {
                return Result<object?>.Failure(
                    new AppError("role_permissions.not_found", "Role permission not found."),
                    (int)HttpStatusCode.NotFound);
            }

            await _rolePermissionRepository.DeleteAsync(rolePermission, cancellationToken);
            return Result<object?>.Success(null, (int)HttpStatusCode.OK);
        }
    }
}
