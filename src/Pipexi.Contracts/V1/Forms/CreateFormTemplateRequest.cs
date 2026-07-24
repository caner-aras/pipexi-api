namespace Pipexi.Contracts.V1.Forms;

public sealed record CreateFormTemplateRequest(
    Guid OrganizationId,
    string Name,
    string? Description);
