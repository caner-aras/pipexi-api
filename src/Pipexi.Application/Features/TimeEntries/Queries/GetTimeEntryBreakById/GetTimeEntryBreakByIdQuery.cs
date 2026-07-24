using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.TimeEntries.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.TimeEntries.Queries.GetTimeEntryBreakById;

public sealed record GetTimeEntryBreakByIdQuery(Guid Id) : IQuery<Result<TimeEntryBreakDto>>
{
    public sealed class Handler : IRequestHandler<GetTimeEntryBreakByIdQuery, Result<TimeEntryBreakDto>>
    {
        private readonly ITimeEntryBreakRepository _timeEntryBreakRepository;

        public Handler(ITimeEntryBreakRepository timeEntryBreakRepository)
        {
            _timeEntryBreakRepository = timeEntryBreakRepository;
        }

        public async Task<Result<TimeEntryBreakDto>> Handle(GetTimeEntryBreakByIdQuery request, CancellationToken cancellationToken)
        {
            var timeEntryBreak = await _timeEntryBreakRepository.GetByIdAsync(request.Id, cancellationToken);
            if (timeEntryBreak is null)
            {
                return Result<TimeEntryBreakDto>.Failure(
                    new AppError("time_entry_breaks.not_found", "Time entry break not found."),
                    (int)HttpStatusCode.NotFound);
            }

            return Result<TimeEntryBreakDto>.Success(timeEntryBreak.ToDto());
        }
    }
}
