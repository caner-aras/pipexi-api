using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Organizations.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Organizations.Queries.GetOrganizationById;

public sealed record GetOrganizationByIdQuery(Guid Id) : IQuery<Result<OrganizationDto>>
{
    public sealed class Handler : IRequestHandler<GetOrganizationByIdQuery, Result<OrganizationDto>>
    {
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IOrganizationAccessService _organizationAccess;

        public Handler(
            IOrganizationRepository organizationRepository,
            IOrganizationAccessService organizationAccess)
        {
            _organizationRepository = organizationRepository;
            _organizationAccess = organizationAccess;
        }

        public async Task<Result<OrganizationDto>> Handle(
            GetOrganizationByIdQuery request,
            CancellationToken cancellationToken)
        {
            var organization = await _organizationRepository.GetByIdAsync(request.Id, cancellationToken);
            if (organization is null)
            {
                return Result<OrganizationDto>.Failure(
                    new AppError("organizations.not_found", "Organization not found."),
                    (int)HttpStatusCode.NotFound);
            }

            await _organizationAccess.EnsureMemberAsync(request.Id, cancellationToken);

            return Result<OrganizationDto>.Success(organization.ToDto());
        }
    }
}
