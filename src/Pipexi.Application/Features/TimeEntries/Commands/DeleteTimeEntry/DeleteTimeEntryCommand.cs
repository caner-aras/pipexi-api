using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.TimeEntries.Commands.DeleteTimeEntry;

public sealed record DeleteTimeEntryCommand(Guid Id) : ICommand<Result<object?>>
{
    public sealed class Handler : IRequestHandler<DeleteTimeEntryCommand, Result<object?>>
    {
        private readonly ITimeEntryRepository _timeEntryRepository;

        public Handler(ITimeEntryRepository timeEntryRepository)
        {
            _timeEntryRepository = timeEntryRepository;
        }

        public async Task<Result<object?>> Handle(DeleteTimeEntryCommand request, CancellationToken cancellationToken)
        {
            var timeEntry = await _timeEntryRepository.GetByIdAsync(request.Id, cancellationToken);
            if (timeEntry is null)
            {
                return Result<object?>.Failure(
                    new AppError("time_entries.not_found", "Time entry not found."),
                    (int)HttpStatusCode.NotFound);
            }

            await _timeEntryRepository.DeleteAsync(timeEntry, cancellationToken);
            return Result<object?>.Success(null, (int)HttpStatusCode.OK);
        }
    }
}
