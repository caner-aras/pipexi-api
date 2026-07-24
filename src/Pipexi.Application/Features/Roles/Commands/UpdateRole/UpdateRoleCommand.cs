using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Roles.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Roles.Commands.UpdateRole;

public sealed record UpdateRoleCommand(Guid Id, string? Name, string? Status) : ICommand<Result<RoleDto>>
{
    public sealed class Handler : IRequestHandler<UpdateRoleCommand, Result<RoleDto>>
    {
        private readonly IRoleRepository _roleRepository;

        public Handler(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        public async Task<Result<RoleDto>> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
        {
            var role = await _roleRepository.GetByIdAsync(request.Id, cancellationToken);
            if (role is null)
            {
                return Result<RoleDto>.Failure(
                    new AppError("roles.not_found", "Role not found."),
                    (int)HttpStatusCode.NotFound);
            }

            var candidateName = request.Name ?? role.Name;
            var exists = await _roleRepository.NameExistsAsync(role.OrganizationId, candidateName, role.Id, cancellationToken);
            if (exists)
            {
                return Result<RoleDto>.Failure(
                    new AppError("roles.name_conflict", "Role name already exists in this organization."),
                    (int)HttpStatusCode.Conflict);
            }

            role.UpdateDetails(request.Name, request.Status);
            await _roleRepository.UpdateAsync(role, cancellationToken);

            return Result<RoleDto>.Success(role.ToDto(), (int)HttpStatusCode.OK);
        }
    }
}
