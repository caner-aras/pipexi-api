namespace Workforce.Domain.Entities;

public abstract class BaseEntity
{
    protected BaseEntity(Guid id, string status, DateTimeOffset createdAt)
    {
        Id = id;
        Status = status;
        CreatedAt = createdAt;
    }

    public Guid Id { get; protected set; }
    public string Status { get; protected set; }
    public DateTimeOffset CreatedAt { get; protected set; }
    public DateTimeOffset? UpdatedAt { get; protected set; }

    protected void Touch()
    {
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    protected void SetStatus(string status)
    {
        Status = status;
        Touch();
    }

    public void MarkDeleted()
    {
        SetStatus("deleted");
    }
}
