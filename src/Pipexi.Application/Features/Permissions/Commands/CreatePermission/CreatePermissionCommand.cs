using System.Net;
using MediatR;
using Workforce.Application.Abstractions.Persistence;
using Workforce.Application.Common.Models;
using Workforce.Application.Features.Permissions.Dtos;
using Workforce.Domain.Entities;
using Workforce.Shared.Errors;
using Workforce.Shared.Results;

namespace Workforce.Application.Features.Permissions.Commands.CreatePermission;

public sealed record CreatePermissionCommand(string Key)
    : ICommand<Result<PermissionDto>>
{
    public sealed class Handler : IRequestHandler<CreatePermissionCommand, Result<PermissionDto>>
    {
        private readonly IPermissionRepository _permissionRepository;

        public Handler(IPermissionRepository permissionRepository)
        {
            _permissionRepository = permissionRepository;
        }

        public async Task<Result<PermissionDto>> Handle(CreatePermissionCommand request, CancellationToken cancellationToken)
        {
            var exists = await _permissionRepository.KeyExistsAsync(request.Key, cancellationToken: cancellationToken);
            if (exists)
            {
                return Result<PermissionDto>.Failure(
                    new AppError("permissions.key_conflict", "Permission key already exists."),
                    (int)HttpStatusCode.Conflict);
            }

            var permission = Permission.Create(request.Key);
            await _permissionRepository.AddAsync(permission, cancellationToken);

            return Result<PermissionDto>.Success(permission.ToDto(), (int)HttpStatusCode.Created);
        }
    }
}
