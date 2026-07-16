namespace Workforce.Domain.Entities;

public sealed class Shift : BaseEntity
{
    private Shift(
        Guid id,
        Guid organizationId,
        Guid? teamId,
        Guid? organizationMemberId,
        Guid locationId,
        string? title,
        DateTimeOffset startAt,
        DateTimeOffset endAt,
        string? notes,
        string status,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt = null)
        : base(id, status, createdAt)
    {
        OrganizationId = organizationId;
        TeamId = teamId;
        OrganizationMemberId = organizationMemberId;
        LocationId = locationId;
        Title = title;
        StartAt = startAt;
        EndAt = endAt;
        Notes = notes;
        UpdatedAt = updatedAt;
    }

    public Guid OrganizationId { get; private set; }
    public Guid? TeamId { get; private set; }
    public Guid? OrganizationMemberId { get; private set; }
    public Guid LocationId { get; private set; }

    public string? Title { get; private set; }
    public DateTimeOffset StartAt { get; private set; }
    public DateTimeOffset EndAt { get; private set; }
    public string? Notes { get; private set; }

    public static Shift Create(
        Guid organizationId,
        Guid? teamId,
        Guid? organizationMemberId,
        Guid locationId,
        string? title,
        DateTimeOffset startAt,
        DateTimeOffset endAt,
        string? notes)
    {
        return new Shift(
            Guid.NewGuid(),
            organizationId,
            teamId,
            organizationMemberId,
            locationId,
            string.IsNullOrWhiteSpace(title) ? null : title.Trim(),
            startAt,
            endAt,
            string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
            "active",
            DateTimeOffset.UtcNow);
    }

    public void UpdateDetails(
        Guid? teamId,
        Guid? organizationMemberId,
        Guid? locationId,
        string? title,
        DateTimeOffset? startAt,
        DateTimeOffset? endAt,
        string? notes,
        string? status)
    {
        if (teamId.HasValue)
        {
            TeamId = teamId.Value;
        }

        if (organizationMemberId.HasValue)
        {
            OrganizationMemberId = organizationMemberId.Value;
        }

        if (locationId.HasValue)
        {
            LocationId = locationId.Value;
        }

        if (title is not null)
        {
            Title = string.IsNullOrWhiteSpace(title) ? null : title.Trim();
        }

        if (startAt.HasValue)
        {
            StartAt = startAt.Value;
        }

        if (endAt.HasValue)
        {
            EndAt = endAt.Value;
        }

        if (notes is not null)
        {
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        }

        if (status is not null)
        {
            SetStatus(status.Trim().ToLowerInvariant());
        }

        if (teamId.HasValue ||
            organizationMemberId.HasValue ||
            locationId.HasValue ||
            title is not null ||
            startAt.HasValue ||
            endAt.HasValue ||
            notes is not null ||
            status is not null)
        {
            Touch();
        }
    }

    public void ClearTeam()
    {
        TeamId = null;
        Touch();
    }

    public void ClearOrganizationMember()
    {
        OrganizationMemberId = null;
        Touch();
    }
}
