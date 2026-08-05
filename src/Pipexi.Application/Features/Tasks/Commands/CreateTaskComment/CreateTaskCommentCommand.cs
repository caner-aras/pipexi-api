using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Tasks.Dtos;
using Pipexi.Domain.Entities;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Tasks.Commands.CreateTaskComment;

public sealed record CreateTaskCommentCommand(
    Guid WorkTaskId,
    Guid UserId,
    string Message, Guid? ScopedOrganizationId = null) : ICommand<Result<TaskCommentDto>>
{
    public sealed class Handler : IRequestHandler<CreateTaskCommentCommand, Result<TaskCommentDto>>
    {
        private readonly IWorkTaskRepository _workTaskRepository;
        private readonly IOrganizationMemberRepository _organizationMemberRepository;
        private readonly ITeamMemberRepository _teamMemberRepository;
        private readonly ITeamRepository _teamRepository;
        private readonly ITaskCommentRepository _taskCommentRepository;
        private readonly IUserRepository _userRepository;
        private readonly IOrganizationAccessService _organizationAccess;

        public Handler(
            IWorkTaskRepository workTaskRepository,
            IOrganizationMemberRepository organizationMemberRepository,
            ITeamMemberRepository teamMemberRepository,
            ITeamRepository teamRepository,
            ITaskCommentRepository taskCommentRepository,
            IUserRepository userRepository,
            IOrganizationAccessService organizationAccess)
        {
            _organizationAccess = organizationAccess;
            _workTaskRepository = workTaskRepository;
            _organizationMemberRepository = organizationMemberRepository;
            _teamMemberRepository = teamMemberRepository;
            _teamRepository = teamRepository;
            _taskCommentRepository = taskCommentRepository;
            _userRepository = userRepository;
        }

        public async Task<Result<TaskCommentDto>> Handle(CreateTaskCommentCommand request, CancellationToken cancellationToken)
        {
            var task = await _workTaskRepository.GetByIdAsync(request.WorkTaskId, cancellationToken);
            if (task is null)
            {
                return Result<TaskCommentDto>.Failure(
                    new AppError("task_comments.invalid_task", "Task not found."),
                    (int)HttpStatusCode.BadRequest);
            }

            var accessDenied = await _organizationAccess.ValidateResourceAccessAsync<TaskCommentDto>(
                task.OrganizationId, request.ScopedOrganizationId, cancellationToken);
            if (accessDenied is not null) return accessDenied;
            var organizationMember = await _organizationMemberRepository.GetByOrganizationIdAndUserIdAsync(
                task.OrganizationId,
                request.UserId,
                cancellationToken);
            if (organizationMember is null)
            {
                return Result<TaskCommentDto>.Failure(
                    new AppError("task_comments.invalid_member", "Team member not found for task organization."),
                    (int)HttpStatusCode.BadRequest);
            }

            var teamMembers = await _teamMemberRepository.ListByOrganizationMemberIdAsync(organizationMember.Id, cancellationToken);
            var teamMember = teamMembers.FirstOrDefault();
            if (teamMember is null)
            {
                return Result<TaskCommentDto>.Failure(
                    new AppError("task_comments.invalid_member", "Team member not found for task organization."),
                    (int)HttpStatusCode.BadRequest);
            }

            var team = await _teamRepository.GetByIdAsync(teamMember.TeamId, cancellationToken);
            if (team is null || team.OrganizationId != task.OrganizationId)
            {
                return Result<TaskCommentDto>.Failure(
                    new AppError("task_comments.invalid_member", "Team member not found for task organization."),
                    (int)HttpStatusCode.BadRequest);
            }

            var comment = TaskComment.Create(request.WorkTaskId, teamMember.Id, request.Message);

            var commenterUser = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            var commenterName = commenterUser != null
                ? $"{commenterUser.FirstName} {commenterUser.LastName}".Trim()
                : "Someone";

            comment.AddDomainEvent(new Pipexi.Domain.Events.Tasks.TaskCommentAddedEvent(
                task.Id,
                comment.Id,
                request.UserId,
                commenterName,
                comment.Message));

            await _taskCommentRepository.AddAsync(comment, cancellationToken);

            return Result<TaskCommentDto>.Success(comment.ToDto(), (int)HttpStatusCode.Created);
        }
    }
}
