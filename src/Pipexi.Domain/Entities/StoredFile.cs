namespace Pipexi.Domain.Entities;

public sealed class StoredFile : BaseEntity
{
    private StoredFile(
        Guid id,
        Guid organizationId,
        string fileName,
        string contentType,
        string storagePath,
        long sizeBytes,
        string status,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt = null)
        : base(id, status, createdAt)
    {
        OrganizationId = organizationId;
        FileName = fileName;
        ContentType = contentType;
        StoragePath = storagePath;
        SizeBytes = sizeBytes;
        UpdatedAt = updatedAt;
    }

    public Guid OrganizationId { get; private set; }
    public string FileName { get; private set; }
    public string ContentType { get; private set; }
    public string StoragePath { get; private set; }
    public long SizeBytes { get; private set; }

    public static StoredFile Create(
        Guid organizationId,
        string fileName,
        string contentType,
        string storagePath,
        long sizeBytes)
    {
        return new StoredFile(
            Guid.NewGuid(),
            organizationId,
            fileName.Trim(),
            contentType.Trim().ToLowerInvariant(),
            storagePath.Trim(),
            sizeBytes,
            "active",
            DateTimeOffset.UtcNow);
    }

    public void UpdateDetails(
        string? fileName,
        string? contentType,
        string? storagePath,
        long? sizeBytes,
        string? status)
    {
        if (fileName is not null)
        {
            FileName = fileName.Trim();
        }

        if (contentType is not null)
        {
            ContentType = contentType.Trim().ToLowerInvariant();
        }

        if (storagePath is not null)
        {
            StoragePath = storagePath.Trim();
        }

        if (sizeBytes.HasValue)
        {
            SizeBytes = sizeBytes.Value;
        }

        if (status is not null)
        {
            SetStatus(status.Trim().ToLowerInvariant());
        }

        if (fileName is not null ||
            contentType is not null ||
            storagePath is not null ||
            sizeBytes.HasValue ||
            status is not null)
        {
            Touch();
        }
    }
}
