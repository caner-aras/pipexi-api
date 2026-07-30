using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Tasks.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Tasks.Commands.UpdateTaskComment;

public sealed record UpdateTaskCommentCommand(
    Guid Id,
    string? Message,
    string? Status,
    Guid? ScopedOrganizationId = null) : ICommand<Result<TaskCommentDto>>
{
    public sealed class Handler : IRequestHandler<UpdateTaskCommentCommand, Result<TaskCommentDto>>
    {
        private readonly ITaskCommentRepository _taskCommentRepository;
        private readonly IWorkTaskRepository _workTaskRepository;
        private readonly IOrganizationAccessService _organizationAccess;

        public Handler(
            ITaskCommentRepository taskCommentRepository,
            IWorkTaskRepository workTaskRepository,
            IOrganizationAccessService organizationAccess)
        {
            _taskCommentRepository = taskCommentRepository;
            _workTaskRepository = workTaskRepository;
            _organizationAccess = organizationAccess;
        }

        public async Task<Result<TaskCommentDto>> Handle(UpdateTaskCommentCommand request, CancellationToken cancellationToken)
        {
            var comment = await _taskCommentRepository.GetByIdAsync(request.Id, cancellationToken);
            if (comment is null)
            {
                return Result<TaskCommentDto>.Failure(
                    new AppError("task_comments.not_found", "Task comment not found."),
                    (int)HttpStatusCode.NotFound);
            }

            var task = await _workTaskRepository.GetByIdAsync(comment.WorkTaskId, cancellationToken);
            if (task is null)
            {
                return Result<TaskCommentDto>.Failure(
                    new AppError("task_comments.invalid_task", "Task not found."),
                    (int)HttpStatusCode.BadRequest);
            }

            var accessDenied = await _organizationAccess.ValidateResourceAccessAsync<TaskCommentDto>(
                task.OrganizationId, request.ScopedOrganizationId, cancellationToken);
            if (accessDenied is not null) return accessDenied;

            comment.UpdateDetails(request.Message, request.Status);
            await _taskCommentRepository.UpdateAsync(comment, cancellationToken);

            return Result<TaskCommentDto>.Success(comment.ToDto(), (int)HttpStatusCode.OK);
        }
    }
}
