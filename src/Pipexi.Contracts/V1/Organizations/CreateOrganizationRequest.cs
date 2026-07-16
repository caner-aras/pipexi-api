namespace Workforce.Contracts.V1.Organizations;

public sealed record CreateOrganizationRequest(string Name, string Slug, string Timezone);
