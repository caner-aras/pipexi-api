using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.TimeEntries.Commands.DeleteTimeEntryBreak;

public sealed record DeleteTimeEntryBreakCommand(Guid Id) : ICommand<Result<object?>>
{
    public sealed class Handler : IRequestHandler<DeleteTimeEntryBreakCommand, Result<object?>>
    {
        private readonly ITimeEntryBreakRepository _timeEntryBreakRepository;

        public Handler(ITimeEntryBreakRepository timeEntryBreakRepository)
        {
            _timeEntryBreakRepository = timeEntryBreakRepository;
        }

        public async Task<Result<object?>> Handle(DeleteTimeEntryBreakCommand request, CancellationToken cancellationToken)
        {
            var timeEntryBreak = await _timeEntryBreakRepository.GetByIdAsync(request.Id, cancellationToken);
            if (timeEntryBreak is null)
            {
                return Result<object?>.Failure(
                    new AppError("time_entry_breaks.not_found", "Time entry break not found."),
                    (int)HttpStatusCode.NotFound);
            }

            await _timeEntryBreakRepository.DeleteAsync(timeEntryBreak, cancellationToken);
            return Result<object?>.Success(null, (int)HttpStatusCode.OK);
        }
    }
}
