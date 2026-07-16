namespace Workforce.Contracts.V1.Shifts;

public sealed record UpdateShiftRequest(
    Guid? TeamId,
    Guid? OrganizationMemberId,
    Guid? LocationId,
    string? Title,
    DateTimeOffset? StartAt,
    DateTimeOffset? EndAt,
    string? Notes,
    string? Status,
    IReadOnlyCollection<Guid>? RequiredFormTemplateIds);
