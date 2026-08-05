namespace Pipexi.Domain.Entities;

public sealed class WorkTask : BaseEntity
{
    private WorkTask(
        Guid id,
        Guid organizationId,
        Guid? reporterUserId,
        Guid? shiftId,
        Guid? locationId,
        string title,
        string? description,
        Guid? assignedToTeamMemberId,
        Guid? assignedToTeamId,
        DateTimeOffset? dueAt,
        string priority,
        string status,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt = null)
        : base(id, status, createdAt)
    {
        OrganizationId = organizationId;
        ReporterUserId = reporterUserId;
        ShiftId = shiftId;
        LocationId = locationId;
        Title = title;
        Description = description;
        AssignedToTeamMemberId = assignedToTeamMemberId;
        AssignedToTeamId = assignedToTeamId;
        DueAt = dueAt;
        Priority = priority;
        UpdatedAt = updatedAt;
    }

    public Guid OrganizationId { get; private set; }
    public Guid? ReporterUserId { get; private set; }
    public Guid? ShiftId { get; private set; }
    public Guid? LocationId { get; private set; }

    public string Title { get; private set; }
    public string? Description { get; private set; }

    public Guid? AssignedToTeamMemberId { get; private set; }
    public Guid? AssignedToTeamId { get; private set; }

    public DateTimeOffset? DueAt { get; private set; }
    public string Priority { get; private set; }

    public static WorkTask Create(
        Guid organizationId,
        Guid? reporterUserId,
        Guid? shiftId,
        Guid? locationId,
        string title,
        string? description,
        Guid? assignedToTeamMemberId,
        Guid? assignedToTeamId,
        DateTimeOffset? dueAt,
        string priority)
    {
        var task = new WorkTask(
            Guid.NewGuid(),
            organizationId,
            reporterUserId,
            shiftId,
            locationId,
            title.Trim(),
            string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            assignedToTeamMemberId,
            assignedToTeamId,
            dueAt,
            string.IsNullOrWhiteSpace(priority) ? "medium" : priority.Trim().ToLowerInvariant(),
            "open",
            DateTimeOffset.UtcNow);

        if (assignedToTeamMemberId.HasValue && reporterUserId.HasValue)
        {
            task.AddDomainEvent(new Pipexi.Domain.Events.Tasks.TaskAssignedEvent(
                task.Id,
                assignedToTeamMemberId.Value,
                reporterUserId.Value,
                task.Title,
                task.OrganizationId,
                task.Priority));
        }

        return task;
    }

    public void UpdateDetails(
        Guid? shiftId,
        Guid? locationId,
        string? title,
        string? description,
        Guid? assignedToTeamMemberId,
        Guid? assignedToTeamId,
        DateTimeOffset? dueAt,
        string? priority,
        string? status,
        Guid? updaterUserId = null)
    {
        var oldAssignedToTeamMemberId = AssignedToTeamMemberId;
        if (shiftId.HasValue)
        {
            ShiftId = shiftId.Value;
        }

        if (locationId.HasValue)
        {
            LocationId = locationId.Value;
        }

        if (title is not null)
        {
            Title = title.Trim();
        }

        if (description is not null)
        {
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        }

        if (assignedToTeamMemberId.HasValue)
        {
            AssignedToTeamMemberId = assignedToTeamMemberId.Value;
        }

        if (assignedToTeamId.HasValue)
        {
            AssignedToTeamId = assignedToTeamId.Value;
        }

        if (dueAt.HasValue)
        {
            DueAt = dueAt.Value;
        }

        if (priority is not null)
        {
            Priority = priority.Trim().ToLowerInvariant();
        }

        if (status is not null)
        {
            SetStatus(status.Trim().ToLowerInvariant());
        }

        if (shiftId.HasValue ||
            locationId.HasValue ||
            title is not null ||
            description is not null ||
            assignedToTeamMemberId.HasValue ||
            assignedToTeamId.HasValue ||
            dueAt.HasValue ||
            priority is not null ||
            status is not null)
        {
            Touch();
        }

        if (AssignedToTeamMemberId.HasValue &&
            AssignedToTeamMemberId != oldAssignedToTeamMemberId &&
            updaterUserId.HasValue)
        {
            AddDomainEvent(new Pipexi.Domain.Events.Tasks.TaskAssignedEvent(
                Id,
                AssignedToTeamMemberId.Value,
                updaterUserId.Value,
                Title,
                OrganizationId));
        }
    }
}
