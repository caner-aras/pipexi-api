using System.Net;
using MediatR;
using Workforce.Application.Abstractions.Persistence;
using Workforce.Application.Common.Models;
using Workforce.Application.Features.Tasks.Dtos;
using Workforce.Shared.Errors;
using Workforce.Shared.Results;

namespace Workforce.Application.Features.Tasks.Queries.GetTaskById;

public sealed record GetTaskByIdQuery(Guid Id) : IQuery<Result<TaskDto>>
{
    public sealed class Handler : IRequestHandler<GetTaskByIdQuery, Result<TaskDto>>
    {
        private readonly IWorkTaskRepository _workTaskRepository;
        private readonly ITaskCommentRepository _taskCommentRepository;

        public Handler(IWorkTaskRepository workTaskRepository, ITaskCommentRepository taskCommentRepository)
        {
            _workTaskRepository = workTaskRepository;
            _taskCommentRepository = taskCommentRepository;
        }

        public async Task<Result<TaskDto>> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
        {
            var task = await _workTaskRepository.GetByIdAsync(request.Id, cancellationToken);
            if (task is null)
            {
                return Result<TaskDto>.Failure(
                    new AppError("tasks.not_found", "Task not found."),
                    (int)HttpStatusCode.NotFound);
            }

            var comments = await _taskCommentRepository.ListByWorkTaskIdAsync(task.Id, cancellationToken);
            return Result<TaskDto>.Success(task.ToDto(comments.Select(x => x.ToDto()).ToList()));
        }
    }
}
