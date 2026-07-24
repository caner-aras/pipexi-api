using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.TimeEntries.Dtos;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.TimeEntries.Queries.GetTimeEntriesByTeamId;

public sealed record GetTimeEntriesByTeamIdQuery(Guid TeamId, Guid? OrganizationId = null)
    : IQuery<Result<IReadOnlyCollection<TimeEntryDto>>>
{
    public sealed class Handler : IRequestHandler<GetTimeEntriesByTeamIdQuery, Result<IReadOnlyCollection<TimeEntryDto>>>
    {
        private readonly IShiftRepository _shiftRepository;
        private readonly ITimeEntryRepository _timeEntryRepository;
        private readonly ITimeEntryBreakRepository _timeEntryBreakRepository;

        public Handler(
            IShiftRepository shiftRepository,
            ITimeEntryRepository timeEntryRepository,
            ITimeEntryBreakRepository timeEntryBreakRepository)
        {
            _shiftRepository = shiftRepository;
            _timeEntryRepository = timeEntryRepository;
            _timeEntryBreakRepository = timeEntryBreakRepository;
        }

        public async Task<Result<IReadOnlyCollection<TimeEntryDto>>> Handle(GetTimeEntriesByTeamIdQuery request, CancellationToken cancellationToken)
        {
            var shifts = await _shiftRepository.ListByTeamIdAsync(request.TeamId, cancellationToken);
            if (request.OrganizationId.HasValue)
            {
                shifts = shifts
                    .Where(x => x.OrganizationId == request.OrganizationId.Value)
                    .ToList();
            }

            var shiftIds = shifts.Select(x => x.Id).ToList();
            var timeEntries = await _timeEntryRepository.ListByShiftIdsAsync(shiftIds, cancellationToken);

            var breakMap = new Dictionary<Guid, IReadOnlyCollection<TimeEntryBreakDto>>();
            var timeEntryIds = timeEntries.Select(x => x.Id).ToList();
            if (timeEntryIds.Count > 0)
            {
                var timeEntryBreaks = await _timeEntryBreakRepository.ListByTimeEntryIdsAsync(timeEntryIds, cancellationToken);
                breakMap = timeEntryBreaks
                    .GroupBy(x => x.TimeEntryId)
                    .ToDictionary(
                        g => g.Key,
                        g => (IReadOnlyCollection<TimeEntryBreakDto>)g.OrderBy(x => x.StartAt).Select(x => x.ToDto()).ToList());
            }

            var dtos = timeEntries
                .Select(x => x.ToDto(breakMap.GetValueOrDefault(x.Id) ?? Array.Empty<TimeEntryBreakDto>()))
                .ToList();

            return Result<IReadOnlyCollection<TimeEntryDto>>.Success(dtos);
        }
    }
}
