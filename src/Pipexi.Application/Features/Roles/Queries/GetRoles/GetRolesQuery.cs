using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Roles.Dtos;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Roles.Queries.GetRoles;

public sealed record GetRolesQuery(Guid? OrganizationId) : IQuery<Result<IReadOnlyCollection<RoleDto>>>
{
    public sealed class Handler : IRequestHandler<GetRolesQuery, Result<IReadOnlyCollection<RoleDto>>>
    {
        private readonly IRoleRepository _roleRepository;

        public Handler(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        public async Task<Result<IReadOnlyCollection<RoleDto>>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
        {
            var items = request.OrganizationId.HasValue
                ? await _roleRepository.ListByOrganizationIdAsync(request.OrganizationId.Value, cancellationToken)
                : await _roleRepository.GetAllAsync(cancellationToken);

            return Result<IReadOnlyCollection<RoleDto>>.Success(items.Select(x => x.ToDto()).ToList());
        }
    }
}
