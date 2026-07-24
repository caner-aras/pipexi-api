using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Roles.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Roles.Queries.GetRoleById;

public sealed record GetRoleByIdQuery(Guid Id) : IQuery<Result<RoleDto>>
{
    public sealed class Handler : IRequestHandler<GetRoleByIdQuery, Result<RoleDto>>
    {
        private readonly IRoleRepository _roleRepository;

        public Handler(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        public async Task<Result<RoleDto>> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
        {
            var role = await _roleRepository.GetByIdAsync(request.Id, cancellationToken);
            if (role is null)
            {
                return Result<RoleDto>.Failure(
                    new AppError("roles.not_found", "Role not found."),
                    (int)HttpStatusCode.NotFound);
            }

            return Result<RoleDto>.Success(role.ToDto());
        }
    }
}
