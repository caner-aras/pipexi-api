namespace Pipexi.Contracts.V1.Shifts;

public sealed record CreateShiftRequest(
    Guid OrganizationId,
    Guid? TeamId,
    Guid? OrganizationMemberId,
    Guid LocationId,
    string? Title,
    DateTimeOffset StartAt,
    DateTimeOffset EndAt,
    string? Notes,
    IReadOnlyCollection<CreateShiftPlannedBreakRequest>? Breaks,
    IReadOnlyCollection<Guid>? RequiredFormTemplateIds,
    string? Repeat,
    int? RepeatTimes,
    IReadOnlyCollection<int>? RepeatOn,
    int? DayOfMonth);

public sealed record CreateShiftPlannedBreakRequest(
    DateTimeOffset StartAt,
    DateTimeOffset EndAt,
    bool IsPaid);
