namespace Pipexi.Application.Features.Tasks.Dtos;

public sealed record TaskCommentMemberUserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string? AvatarUrl);

public sealed record TaskCommentMemberDto(
    Guid TeamMemberId,
    Guid TeamId,
    Guid OrganizationMemberId,
    Guid UserId,
    string? JobTitle,
    TaskCommentMemberUserDto? User);

public sealed record TaskCommentDto(
    Guid Id,
    Guid WorkTaskId,
    Guid TeamMemberId,
    string Message,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    TaskCommentMemberDto? Member = null);
