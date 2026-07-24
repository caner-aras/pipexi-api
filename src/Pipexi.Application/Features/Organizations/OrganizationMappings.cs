using Pipexi.Application.Features.Organizations.Dtos;
using Pipexi.Domain.Entities;

namespace Pipexi.Application.Features.Organizations;

internal static class OrganizationMappings
{
    public static OrganizationDto ToDto(this Organization organization)
    {
        return new OrganizationDto(
            organization.Id,
            organization.Name,
            organization.Slug,
            organization.Timezone,
            organization.Currency,
            organization.Status,
            organization.CreatedAt,
            organization.UpdatedAt);
    }
}
