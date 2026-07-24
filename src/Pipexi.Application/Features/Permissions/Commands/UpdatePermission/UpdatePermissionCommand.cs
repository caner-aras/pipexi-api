using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Permissions.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Permissions.Commands.UpdatePermission;

public sealed record UpdatePermissionCommand(
    Guid Id,
    string? Key,
    string? Status) : ICommand<Result<PermissionDto>>
{
    public sealed class Handler : IRequestHandler<UpdatePermissionCommand, Result<PermissionDto>>
    {
        private readonly IPermissionRepository _permissionRepository;

        public Handler(IPermissionRepository permissionRepository)
        {
            _permissionRepository = permissionRepository;
        }

        public async Task<Result<PermissionDto>> Handle(UpdatePermissionCommand request, CancellationToken cancellationToken)
        {
            var permission = await _permissionRepository.GetByIdAsync(request.Id, cancellationToken);
            if (permission is null)
            {
                return Result<PermissionDto>.Failure(
                    new AppError("permissions.not_found", "Permission not found."),
                    (int)HttpStatusCode.NotFound);
            }

            var candidateKey = request.Key ?? permission.Key;
            var exists = await _permissionRepository.KeyExistsAsync(candidateKey, permission.Id, cancellationToken);
            if (exists)
            {
                return Result<PermissionDto>.Failure(
                    new AppError("permissions.key_conflict", "Permission key already exists."),
                    (int)HttpStatusCode.Conflict);
            }

            permission.UpdateDetails(request.Key, request.Status);
            await _permissionRepository.UpdateAsync(permission, cancellationToken);

            return Result<PermissionDto>.Success(permission.ToDto(), (int)HttpStatusCode.OK);
        }
    }
}
