namespace Workforce.Contracts.V1.Forms;

public sealed record CreateFormSubmissionRequest(
    Guid OrganizationId,
    Guid FormTemplateId,
    Guid SubmittedByMemberId,
    Guid? TaskId,
    Guid? ShiftId,
    DateTimeOffset SubmittedAt,
    IReadOnlyCollection<CreateFormSubmissionAnswerRequest>? Answers);

public sealed record CreateFormSubmissionAnswerRequest(
    Guid FormFieldId,
    string? Value,
    Guid? FileId);
