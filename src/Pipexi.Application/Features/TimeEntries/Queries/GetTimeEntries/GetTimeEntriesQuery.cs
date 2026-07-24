using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.TimeEntries.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.TimeEntries.Queries.GetTimeEntries;

public sealed record GetTimeEntriesQuery(Guid? OrganizationId, Guid? OrganizationMemberId = null) : IQuery<Result<IReadOnlyCollection<TimeEntryDto>>>
{
    public sealed class Handler : IRequestHandler<GetTimeEntriesQuery, Result<IReadOnlyCollection<TimeEntryDto>>>
    {
        private readonly ITimeEntryRepository _timeEntryRepository;
        private readonly ITimeEntryBreakRepository _timeEntryBreakRepository;
        private readonly ICurrentUserContext _currentUserContext;

        public Handler(
            ITimeEntryRepository timeEntryRepository,
            ITimeEntryBreakRepository timeEntryBreakRepository,
            ICurrentUserContext currentUserContext)
        {
            _timeEntryRepository = timeEntryRepository;
            _timeEntryBreakRepository = timeEntryBreakRepository;
            _currentUserContext = currentUserContext;
        }

        public async Task<Result<IReadOnlyCollection<TimeEntryDto>>> Handle(GetTimeEntriesQuery request, CancellationToken cancellationToken)
        {
            var organizationId = request.OrganizationId ?? _currentUserContext.OrganizationId;
            if (organizationId == Guid.Empty)
            {
                return Result<IReadOnlyCollection<TimeEntryDto>>.Failure(
                    new AppError("auth.unauthorized", "Unauthorized."),
                    (int)HttpStatusCode.Unauthorized);
            }

            IReadOnlyCollection<Pipexi.Domain.Entities.TimeEntry> timeEntries;
            if (request.OrganizationMemberId.HasValue)
            {
                timeEntries = await _timeEntryRepository.ListByOrganizationMemberIdAsync(request.OrganizationMemberId.Value, cancellationToken);
                timeEntries = timeEntries.Where(x => x.OrganizationId == organizationId).ToList();
            }
            else
            {
                timeEntries = await _timeEntryRepository.ListByOrganizationIdAsync(organizationId, cancellationToken);
            }

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
