using Workforce.Application.Features.Organizations.Dtos;
using Workforce.Domain.Entities;

namespace Workforce.Application.Features.Organizations;

internal static class OrganizationMappings
{
    public static OrganizationDto ToDto(this Organization organization)
    {
        return new OrganizationDto(
            organization.Id,
            organization.Name,
            organization.Slug,
            organization.Timezone,
            organization.Status,
            organization.CreatedAt,
            organization.UpdatedAt);
    }
}
