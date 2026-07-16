namespace Workforce.Contracts.V1.Forms;

public sealed record CreateFormAnswerRequest(
    Guid FormSubmissionId,
    Guid FormFieldId,
    string? Value,
    Guid? FileId);
