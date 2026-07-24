namespace Pipexi.Domain.Entities;

public sealed class OrganizationMember : BaseEntity
{
    private OrganizationMember(
        Guid id,
        Guid organizationId,
        Guid userId,
        Guid roleId,
        string? jobTitle,
        string status,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt = null)
        : base(id, status, createdAt)
    {
        OrganizationId = organizationId;
        UserId = userId;
        RoleId = roleId;
        JobTitle = jobTitle;
        UpdatedAt = updatedAt;
    }

    public Guid OrganizationId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid RoleId { get; private set; }
    public string? JobTitle { get; private set; }

    public static OrganizationMember Create(Guid organizationId, Guid userId, Guid roleId, string? jobTitle)
    {
        return new OrganizationMember(
            Guid.NewGuid(),
            organizationId,
            userId,
            roleId,
            jobTitle?.Trim(),
            "active",
            DateTimeOffset.UtcNow);
    }

    public void UpdateDetails(Guid? roleId, string? jobTitle, string? status)
    {
        if (roleId.HasValue)
        {
            RoleId = roleId.Value;
        }

        if (jobTitle is not null)
        {
            JobTitle = jobTitle.Trim();
        }

        if (status is not null)
        {
            SetStatus(status.Trim().ToLowerInvariant());
        }

        if (roleId.HasValue || jobTitle is not null || status is not null)
        {
            Touch();
        }
    }
}
