using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Permissions.Dtos;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Permissions.Queries.GetPermissions;

public sealed record GetPermissionsQuery() : IQuery<Result<IReadOnlyCollection<PermissionDto>>>
{
    public sealed class Handler : IRequestHandler<GetPermissionsQuery, Result<IReadOnlyCollection<PermissionDto>>>
    {
        private readonly IPermissionRepository _permissionRepository;

        public Handler(IPermissionRepository permissionRepository)
        {
            _permissionRepository = permissionRepository;
        }

        public async Task<Result<IReadOnlyCollection<PermissionDto>>> Handle(GetPermissionsQuery request, CancellationToken cancellationToken)
        {
            _ = request;
            var items = await _permissionRepository.GetAllAsync(cancellationToken);
            return Result<IReadOnlyCollection<PermissionDto>>.Success(items.Select(x => x.ToDto()).ToList());
        }
    }
}
