using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.TimeEntries.Dtos;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.TimeEntries.Queries.GetTimeEntryBreaks;

public sealed record GetTimeEntryBreaksQuery(Guid TimeEntryId) : IQuery<Result<IReadOnlyCollection<TimeEntryBreakDto>>>
{
    public sealed class Handler : IRequestHandler<GetTimeEntryBreaksQuery, Result<IReadOnlyCollection<TimeEntryBreakDto>>>
    {
        private readonly ITimeEntryBreakRepository _timeEntryBreakRepository;

        public Handler(ITimeEntryBreakRepository timeEntryBreakRepository)
        {
            _timeEntryBreakRepository = timeEntryBreakRepository;
        }

        public async Task<Result<IReadOnlyCollection<TimeEntryBreakDto>>> Handle(GetTimeEntryBreaksQuery request, CancellationToken cancellationToken)
        {
            var items = await _timeEntryBreakRepository.ListByTimeEntryIdAsync(request.TimeEntryId, cancellationToken);
            return Result<IReadOnlyCollection<TimeEntryBreakDto>>.Success(items.Select(x => x.ToDto()).ToList());
        }
    }
}
