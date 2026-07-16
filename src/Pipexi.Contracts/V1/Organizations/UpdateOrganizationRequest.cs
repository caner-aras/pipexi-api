namespace Workforce.Contracts.V1.Organizations;

public sealed record UpdateOrganizationRequest(
    string? Name,
    string? Slug,
    string? Timezone,
    string? Status);
