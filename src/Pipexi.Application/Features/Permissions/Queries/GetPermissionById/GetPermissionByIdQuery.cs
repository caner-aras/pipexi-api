using System.Net;
using MediatR;
using Workforce.Application.Abstractions.Persistence;
using Workforce.Application.Common.Models;
using Workforce.Application.Features.Permissions.Dtos;
using Workforce.Shared.Errors;
using Workforce.Shared.Results;

namespace Workforce.Application.Features.Permissions.Queries.GetPermissionById;

public sealed record GetPermissionByIdQuery(Guid Id) : IQuery<Result<PermissionDto>>
{
    public sealed class Handler : IRequestHandler<GetPermissionByIdQuery, Result<PermissionDto>>
    {
        private readonly IPermissionRepository _permissionRepository;

        public Handler(IPermissionRepository permissionRepository)
        {
            _permissionRepository = permissionRepository;
        }

        public async Task<Result<PermissionDto>> Handle(GetPermissionByIdQuery request, CancellationToken cancellationToken)
        {
            var permission = await _permissionRepository.GetByIdAsync(request.Id, cancellationToken);
            if (permission is null)
            {
                return Result<PermissionDto>.Failure(
                    new AppError("permissions.not_found", "Permission not found."),
                    (int)HttpStatusCode.NotFound);
            }

            return Result<PermissionDto>.Success(permission.ToDto());
        }
    }
}
