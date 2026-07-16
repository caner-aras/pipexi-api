namespace Workforce.Domain.Entities;

public sealed class TeamMember : BaseEntity
{
    private TeamMember(
        Guid id,
        Guid teamId,
        Guid organizationMemberId,
        string status,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt = null)
        : base(id, status, createdAt)
    {
        TeamId = teamId;
        OrganizationMemberId = organizationMemberId;
        UpdatedAt = updatedAt;
    }

    public Guid TeamId { get; private set; }
    public Guid OrganizationMemberId { get; private set; }

    public static TeamMember Create(Guid teamId, Guid organizationMemberId)
    {
        return new TeamMember(
            Guid.NewGuid(),
            teamId,
            organizationMemberId,
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
