using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Roles.Dtos;
using Pipexi.Domain.Entities;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Roles.Commands.CreateRole;

public sealed record CreateRoleCommand(Guid OrganizationId, string Name) : ICommand<Result<RoleDto>>
{
    public sealed class Handler : IRequestHandler<CreateRoleCommand, Result<RoleDto>>
    {
        private readonly IRoleRepository _roleRepository;

        public Handler(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        public async Task<Result<RoleDto>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
        {
            var exists = await _roleRepository.NameExistsAsync(
                request.OrganizationId,
                request.Name,
                cancellationToken: cancellationToken);
            if (exists)
            {
                return Result<RoleDto>.Failure(
                    new AppError("roles.name_conflict", "Role name already exists in this organization."),
                    (int)HttpStatusCode.Conflict);
            }

            var role = Role.Create(request.OrganizationId, request.Name);
            await _roleRepository.AddAsync(role, cancellationToken);

            return Result<RoleDto>.Success(role.ToDto(), (int)HttpStatusCode.Created);
        }
    }
}
