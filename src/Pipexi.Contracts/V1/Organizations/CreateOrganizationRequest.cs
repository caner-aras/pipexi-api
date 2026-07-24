namespace Pipexi.Contracts.V1.Organizations;

public sealed record CreateOrganizationRequest(string Name, string Slug, string Timezone, string? Currency = null);
