using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Permissions.Commands.DeletePermission;

public sealed record DeletePermissionCommand(Guid Id) : ICommand<Result<object?>>
{
    public sealed class Handler : IRequestHandler<DeletePermissionCommand, Result<object?>>
    {
        private readonly IPermissionRepository _permissionRepository;

        public Handler(IPermissionRepository permissionRepository)
        {
            _permissionRepository = permissionRepository;
        }

        public async Task<Result<object?>> Handle(DeletePermissionCommand request, CancellationToken cancellationToken)
        {
            var permission = await _permissionRepository.GetByIdAsync(request.Id, cancellationToken);
            if (permission is null)
            {
                return Result<object?>.Failure(
                    new AppError("permissions.not_found", "Permission not found."),
                    (int)HttpStatusCode.NotFound);
            }

            await _permissionRepository.DeleteAsync(permission, cancellationToken);
            return Result<object?>.Success(null, (int)HttpStatusCode.OK);
        }
    }
}
