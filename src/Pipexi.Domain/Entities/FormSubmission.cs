namespace Workforce.Domain.Entities;

public sealed class FormSubmission : BaseEntity
{
    private FormSubmission(
        Guid id,
        Guid organizationId,
        Guid formTemplateId,
        Guid submittedByMemberId,
        Guid? taskId,
        Guid? shiftId,
        DateTimeOffset submittedAt,
        string status,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt = null)
        : base(id, status, createdAt)
    {
        OrganizationId = organizationId;
        FormTemplateId = formTemplateId;
        SubmittedByMemberId = submittedByMemberId;
        TaskId = taskId;
        ShiftId = shiftId;
        SubmittedAt = submittedAt;
        UpdatedAt = updatedAt;
    }

    public Guid OrganizationId { get; private set; }
    public Guid FormTemplateId { get; private set; }
    public Guid SubmittedByMemberId { get; private set; }
    public Guid? TaskId { get; private set; }
    public Guid? ShiftId { get; private set; }
    public DateTimeOffset SubmittedAt { get; private set; }

    public static FormSubmission Create(
        Guid organizationId,
        Guid formTemplateId,
        Guid submittedByMemberId,
        Guid? taskId,
        Guid? shiftId,
        DateTimeOffset submittedAt)
    {
        return new FormSubmission(
            Guid.NewGuid(),
            organizationId,
            formTemplateId,
            submittedByMemberId,
            taskId,
            shiftId,
            submittedAt,
            "submitted",
            DateTimeOffset.UtcNow);
    }

    public void UpdateDetails(
        Guid? taskId,
        Guid? shiftId,
        DateTimeOffset? submittedAt,
        string? status)
    {
        if (taskId.HasValue)
        {
            TaskId = taskId.Value;
        }

        if (shiftId.HasValue)
        {
            ShiftId = shiftId.Value;
        }

        if (submittedAt.HasValue)
        {
            SubmittedAt = submittedAt.Value;
        }

        if (status is not null)
        {
            SetStatus(status.Trim().ToLowerInvariant());
        }

        if (taskId.HasValue || shiftId.HasValue || submittedAt.HasValue || status is not null)
        {
            Touch();
        }
    }
}
