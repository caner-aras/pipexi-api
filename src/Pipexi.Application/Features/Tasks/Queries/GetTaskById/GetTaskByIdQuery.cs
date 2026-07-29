using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Tasks;
using Pipexi.Application.Features.Tasks.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Tasks.Queries.GetTaskById;

public sealed record GetTaskByIdQuery(Guid Id) : IQuery<Result<TaskDto>>
{
    public sealed class Handler : IRequestHandler<GetTaskByIdQuery, Result<TaskDto>>
    {
        private readonly IWorkTaskRepository _workTaskRepository;
        private readonly ITaskCommentRepository _taskCommentRepository;
        private readonly IUserRepository _userRepository;

        public Handler(
            IWorkTaskRepository workTaskRepository,
            ITaskCommentRepository taskCommentRepository,
            IUserRepository userRepository)
        {
            _workTaskRepository = workTaskRepository;
            _taskCommentRepository = taskCommentRepository;
            _userRepository = userRepository;
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
            TaskCommentMemberUserDto? reporter = null;
            if (task.ReporterUserId.HasValue)
            {
                var reporterUser = await _userRepository.GetByIdAsync(task.ReporterUserId.Value, cancellationToken);
                reporter = reporterUser?.ToTaskCommentMemberUserDto();
            }

            return Result<TaskDto>.Success(
                task.ToDto(comments.Select(x => x.ToDto()).ToList()) with
                {
                    Reporter = reporter
                });
        }
    }
}
