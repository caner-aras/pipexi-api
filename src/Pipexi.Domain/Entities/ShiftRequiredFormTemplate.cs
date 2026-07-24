namespace Pipexi.Domain.Entities;

public sealed class ShiftRequiredFormTemplate : BaseEntity
{
    private ShiftRequiredFormTemplate(
        Guid id,
        Guid shiftId,
        Guid formTemplateId,
        string status,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt = null)
        : base(id, status, createdAt)
    {
        ShiftId = shiftId;
        FormTemplateId = formTemplateId;
        UpdatedAt = updatedAt;
    }

    public Guid ShiftId { get; private set; }
    public Guid FormTemplateId { get; private set; }

    public static ShiftRequiredFormTemplate Create(Guid shiftId, Guid formTemplateId)
    {
        return new ShiftRequiredFormTemplate(
            Guid.NewGuid(),
            shiftId,
            formTemplateId,
            "active",
            DateTimeOffset.UtcNow);
    }

    public void UpdateDetails(string? status)
    {
        if (status is not null)
        {
            SetStatus(status.Trim().ToLowerInvariant());
            Touch();
        }
    }
}