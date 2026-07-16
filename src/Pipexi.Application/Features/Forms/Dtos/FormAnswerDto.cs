namespace Workforce.Application.Features.Forms.Dtos;

public sealed record FormAnswerDto(
    Guid Id,
    Guid FormSubmissionId,
    Guid FormFieldId,
    FormFieldDto? FormField,
    string? Value,
    Guid? FileId,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    StoredFileDto? File);
