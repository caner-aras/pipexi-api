using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Tasks.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Tasks.Commands.UpdateTaskComment;

public sealed record UpdateTaskCommentCommand(
    Guid Id,
    string? Message,
    string? Status) : ICommand<Result<TaskCommentDto>>
{
    public sealed class Handler : IRequestHandler<UpdateTaskCommentCommand, Result<TaskCommentDto>>
    {
        private readonly ITaskCommentRepository _taskCommentRepository;

        public Handler(ITaskCommentRepository taskCommentRepository)
        {
            _taskCommentRepository = taskCommentRepository;
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

            comment.UpdateDetails(request.Message, request.Status);
            await _taskCommentRepository.UpdateAsync(comment, cancellationToken);

            return Result<TaskCommentDto>.Success(comment.ToDto(), (int)HttpStatusCode.OK);
        }
    }
}
