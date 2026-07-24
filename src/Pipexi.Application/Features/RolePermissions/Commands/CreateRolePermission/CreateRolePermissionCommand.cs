using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.RolePermissions.Dtos;
using Pipexi.Domain.Entities;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.RolePermissions.Commands.CreateRolePermission;

public sealed record CreateRolePermissionCommand(Guid RoleId, Guid PermissionId) : ICommand<Result<RolePermissionDto>>
{
    public sealed class Handler : IRequestHandler<CreateRolePermissionCommand, Result<RolePermissionDto>>
    {
        private readonly IRolePermissionRepository _rolePermissionRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IPermissionRepository _permissionRepository;

        public Handler(
            IRolePermissionRepository rolePermissionRepository,
            IRoleRepository roleRepository,
            IPermissionRepository permissionRepository)
        {
            _rolePermissionRepository = rolePermissionRepository;
            _roleRepository = roleRepository;
            _permissionRepository = permissionRepository;
        }

        public async Task<Result<RolePermissionDto>> Handle(CreateRolePermissionCommand request, CancellationToken cancellationToken)
        {
            var role = await _roleRepository.GetByIdAsync(request.RoleId, cancellationToken);
            if (role is null)
            {
                return Result<RolePermissionDto>.Failure(
                    new AppError("roles.not_found", "Role not found."),
                    (int)HttpStatusCode.BadRequest);
            }

            var permission = await _permissionRepository.GetByIdAsync(request.PermissionId, cancellationToken);
            if (permission is null)
            {
                return Result<RolePermissionDto>.Failure(
                    new AppError("permissions.not_found", "Permission not found."),
                    (int)HttpStatusCode.BadRequest);
            }

            var exists = await _rolePermissionRepository.ExistsAsync(
                request.RoleId,
                request.PermissionId,
                cancellationToken: cancellationToken);

            if (exists)
            {
                return Result<RolePermissionDto>.Failure(
                    new AppError("role_permissions.conflict", "Role permission already exists."),
                    (int)HttpStatusCode.Conflict);
            }

            var rolePermission = RolePermission.Create(request.RoleId, request.PermissionId);
            await _rolePermissionRepository.AddAsync(rolePermission, cancellationToken);

            return Result<RolePermissionDto>.Success(rolePermission.ToDto(), (int)HttpStatusCode.Created);
        }
    }
}
