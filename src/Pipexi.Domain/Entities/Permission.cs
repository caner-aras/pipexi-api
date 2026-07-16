namespace Workforce.Domain.Entities;

public sealed class Permission : BaseEntity
{
    private Permission(
        Guid id,
        string key,
        string status,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt = null)
        : base(id, status, createdAt)
    {
        Key = key;
        UpdatedAt = updatedAt;
    }

    public string Key { get; private set; }

    public static Permission Create(string key)
    {
        return new Permission(
            Guid.NewGuid(),
            key.Trim().ToLowerInvariant(),
            "active",
            DateTimeOffset.UtcNow);
    }

    public void UpdateDetails(string? key, string? status)
    {
        if (key is not null)
        {
            Key = key.Trim().ToLowerInvariant();
        }

        if (status is not null)
        {
            SetStatus(status.Trim().ToLowerInvariant());
        }

        if (key is not null || status is not null)
        {
            Touch();
        }
    }
}
