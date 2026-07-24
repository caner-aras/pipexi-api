using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Tasks.Commands.DeleteTaskComment;

public sealed record DeleteTaskCommentCommand(Guid Id) : ICommand<Result<object?>>
{
    public sealed class Handler : IRequestHandler<DeleteTaskCommentCommand, Result<object?>>
    {
        private readonly ITaskCommentRepository _taskCommentRepository;

        public Handler(ITaskCommentRepository taskCommentRepository)
        {
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

            await _taskCommentRepository.DeleteAsync(comment, cancellationToken);
            return Result<object?>.Success(null, (int)HttpStatusCode.OK);
        }
    }
}
