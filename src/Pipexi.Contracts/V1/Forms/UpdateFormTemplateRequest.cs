namespace Pipexi.Contracts.V1.Forms;

public sealed record UpdateFormTemplateRequest(
    string? Name,
    string? Description,
    string? Status);
