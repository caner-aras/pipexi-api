namespace Pipexi.Application.Features.Reports.Dtos;

public sealed record ShiftFormsStatusDto(
    Guid ShiftId,
    Guid? OrganizationMemberId,
    string MemberName,
    string? MemberAvatarUrl,
    string TeamName,
    DateTimeOffset StartAt,
    DateTimeOffset EndAt,
    bool IsMissingForms
);
