namespace Pipexi.Domain.Entities;

public sealed class FormAnswer : BaseEntity
{
    private FormAnswer(
        Guid id,
        Guid formSubmissionId,
        Guid formFieldId,
        string? value,
        Guid? fileId,
        string status,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt = null)
        : base(id, status, createdAt)
    {
        FormSubmissionId = formSubmissionId;
        FormFieldId = formFieldId;
        Value = value;
        FileId = fileId;
        UpdatedAt = updatedAt;
    }

    public Guid FormSubmissionId { get; private set; }
    public Guid FormFieldId { get; private set; }
    public string? Value { get; private set; }
    public Guid? FileId { get; private set; }

    public static FormAnswer Create(Guid formSubmissionId, Guid formFieldId, string? value, Guid? fileId)
    {
        return new FormAnswer(
            Guid.NewGuid(),
            formSubmissionId,
            formFieldId,
            string.IsNullOrWhiteSpace(value) ? null : value.Trim(),
            fileId,
            "active",
            DateTimeOffset.UtcNow);
    }

    public void UpdateDetails(string? value, Guid? fileId, string? status)
    {
        if (value is not null)
        {
            Value = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        if (fileId.HasValue)
        {
            FileId = fileId.Value;
        }

        if (status is not null)
        {
            SetStatus(status.Trim().ToLowerInvariant());
        }

        if (value is not null || fileId.HasValue || status is not null)
        {
            Touch();
        }
    }
}
