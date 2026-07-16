using Workforce.Application.Features.Tasks.Dtos;
using Workforce.Domain.Entities;

namespace Workforce.Application.Features.Tasks;

internal static class TaskMappings
{
    public static TaskDto ToDto(this WorkTask task, IReadOnlyCollection<TaskCommentDto>? comments = null)
    {
        return new TaskDto(
            task.Id,
            task.OrganizationId,
            task.ReporterUserId,
            task.ShiftId,
            task.LocationId,
            task.Title,
            task.Description,
            task.AssignedToTeamMemberId,
            task.AssignedToTeamId,
            task.DueAt,
            task.Priority,
            task.Status,
            task.CreatedAt,
            task.UpdatedAt,
            comments ?? Array.Empty<TaskCommentDto>());
    }

    public static TaskCommentDto ToDto(this TaskComment comment, TaskCommentMemberDto? member = null)
    {
        return new TaskCommentDto(
            comment.Id,
            comment.WorkTaskId,
            comment.TeamMemberId,
            comment.Message,
            comment.Status,
            comment.CreatedAt,
            comment.UpdatedAt,
            member);
    }

    public static TaskCommentMemberDto ToCommentMemberDto(
        this TeamMember teamMember,
        OrganizationMember organizationMember,
        User? user)
    {
        return new TaskCommentMemberDto(
            teamMember.Id,
            teamMember.TeamId,
            organizationMember.Id,
            organizationMember.UserId,
            organizationMember.JobTitle,
            user is null ? null : user.ToTaskCommentMemberUserDto());
    }

    public static TaskCommentMemberUserDto ToTaskCommentMemberUserDto(this User user)
    {
        return new TaskCommentMemberUserDto(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.AvatarUrl);
    }
}
