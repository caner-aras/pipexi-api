namespace Workforce.Contracts.V1.Forms;

public sealed record UpdateFormAnswerRequest(
    string? Value,
    Guid? FileId,
    string? Status);
