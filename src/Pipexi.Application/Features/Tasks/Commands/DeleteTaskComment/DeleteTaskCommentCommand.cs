using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Tasks.Commands.DeleteTaskComment;

public sealed record DeleteTaskCommentCommand(Guid Id, Guid? ScopedOrganizationId = null) : ICommand<Result<object?>>
{
    public sealed class Handler : IRequestHandler<DeleteTaskCommentCommand, Result<object?>>
    {
        private readonly ITaskCommentRepository _taskCommentRepository;
        private readonly IWorkTaskRepository _workTaskRepository;
        private readonly IOrganizationAccessService _organizationAccess;

        public Handler(ITaskCommentRepository taskCommentRepository,
            IWorkTaskRepository workTaskRepository,
            IOrganizationAccessService organizationAccess)
        {
            _organizationAccess = organizationAccess;
            _workTaskRepository = workTaskRepository;
            _taskCommentRepository = taskCommentRepository;
        }

        public async Task<Result<object?>> Handle(DeleteTaskCommentCommand request, CancellationToken cancellationToken)
        {
            var comment = await _taskCommentRepository.GetByIdAsync(request.Id, cancellationToken);
            if (comment is null)
            {
                return Result<object?>.Failure(
                    new AppError("task_comments.not_found", "Task comment not found."),
                    (int)HttpStatusCode.NotFound);
            }

            var task = await _workTaskRepository.GetByIdAsync(comment.WorkTaskId, cancellationToken);
            if (task is null)
            {
                return Result<object?>.Failure(
                    new AppError("resource.not_found", "Parent resource not found."),
                    (int)HttpStatusCode.BadRequest);
            }

            var accessDenied = await _organizationAccess.ValidateResourceAccessAsync<object?>(
                task.OrganizationId, request.ScopedOrganizationId, cancellationToken);
            if (accessDenied is not null) return accessDenied;
            await _taskCommentRepository.DeleteAsync(comment, cancellationToken);
            return Result<object?>.Success(null, (int)HttpStatusCode.OK);
        }
    }
}
