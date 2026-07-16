using Workforce.Application.Features.OrganizationMembers.Dtos;

namespace Workforce.Application.Features.Forms.Dtos;

public sealed record FormSubmissionDto(
    Guid Id,
    Guid OrganizationId,
    Guid FormTemplateId,
    Guid SubmittedByMemberId,
    OrganizationMemberDto? SubmittedByMember,
    Guid? TaskId,
    Guid? ShiftId,
    DateTimeOffset SubmittedAt,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    IReadOnlyCollection<FormAnswerDto> Answers);
