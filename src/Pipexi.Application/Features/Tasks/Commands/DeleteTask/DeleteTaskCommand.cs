using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Tasks.Commands.DeleteTask;

public sealed record DeleteTaskCommand(Guid Id) : ICommand<Result<object?>>
{
    public sealed class Handler : IRequestHandler<DeleteTaskCommand, Result<object?>>
    {
        private readonly IWorkTaskRepository _workTaskRepository;

        public Handler(IWorkTaskRepository workTaskRepository)
        {
            _workTaskRepository = workTaskRepository;
        }

        public async Task<Result<object?>> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
        {
            var task = await _workTaskRepository.GetByIdAsync(request.Id, cancellationToken);
            if (task is null)
            {
                return Result<object?>.Failure(
                    new AppError("tasks.not_found", "Task not found."),
                    (int)HttpStatusCode.NotFound);
            }

            await _workTaskRepository.DeleteAsync(task, cancellationToken);
            return Result<object?>.Success(null, (int)HttpStatusCode.OK);
        }
    }
}
