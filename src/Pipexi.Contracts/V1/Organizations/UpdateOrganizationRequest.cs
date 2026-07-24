namespace Pipexi.Contracts.V1.Organizations;

public sealed record UpdateOrganizationRequest(
    string? Name,
    string? Slug,
    string? Timezone,
    string? Currency,
    string? Status);
