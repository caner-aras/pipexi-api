using System.Net;
using MediatR;
using Workforce.Application.Abstractions.Persistence;
using Workforce.Application.Common.Models;
using Workforce.Application.Features.Organizations.Dtos;
using Workforce.Shared.Errors;
using Workforce.Shared.Results;

namespace Workforce.Application.Features.Organizations.Commands.UpdateOrganization;

public sealed record UpdateOrganizationCommand(
    Guid Id,
    string? Name,
    string? Slug,
    string? Timezone,
    string? Status) : ICommand<Result<OrganizationDto>>
{
    public sealed class Handler : IRequestHandler<UpdateOrganizationCommand, Result<OrganizationDto>>
    {
        private readonly IOrganizationRepository _organizationRepository;

        public Handler(IOrganizationRepository organizationRepository)
        {
            _organizationRepository = organizationRepository;
        }

        public async Task<Result<OrganizationDto>> Handle(
            UpdateOrganizationCommand request,
            CancellationToken cancellationToken)
        {
            var organization = await _organizationRepository.GetByIdAsync(request.Id, cancellationToken);
            if (organization is null)
            {
                return Result<OrganizationDto>.Failure(
                    new AppError("organizations.not_found", "Organization not found."),
                    (int)HttpStatusCode.NotFound);
            }

            var candidateSlug = request.Slug ?? organization.Slug;
            var slugExists = await _organizationRepository.SlugExistsAsync(
                candidateSlug,
                organization.Id,
                cancellationToken);

            if (slugExists)
            {
                return Result<OrganizationDto>.Failure(
                    new AppError("organizations.slug_conflict", "Organization slug already exists."),
                    (int)HttpStatusCode.Conflict);
            }

            organization.UpdateDetails(request.Name, request.Slug, request.Timezone, request.Status);
            await _organizationRepository.UpdateAsync(organization, cancellationToken);

            return Result<OrganizationDto>.Success(organization.ToDto(), (int)HttpStatusCode.OK);
        }
    }
}
