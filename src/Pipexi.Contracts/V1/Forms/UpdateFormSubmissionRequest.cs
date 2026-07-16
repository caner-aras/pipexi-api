namespace Workforce.Contracts.V1.Forms;

public sealed record UpdateFormSubmissionRequest(
    Guid? TaskId,
    Guid? ShiftId,
    DateTimeOffset? SubmittedAt,
    string? Status,
    IReadOnlyCollection<UpdateFormSubmissionAnswerRequest>? Answers);

public sealed record UpdateFormSubmissionAnswerRequest(
    Guid FormFieldId,
    string? Value,
    Guid? FileId,
    string? Status);
