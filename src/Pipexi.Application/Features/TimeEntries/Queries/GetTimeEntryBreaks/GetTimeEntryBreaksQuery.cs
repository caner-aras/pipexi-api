using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.TimeEntries.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.TimeEntries.Queries.GetTimeEntryBreaks;

public sealed record GetTimeEntryBreaksQuery(Guid TimeEntryId, Guid? ScopedOrganizationId = null) : IQuery<Result<IReadOnlyCollection<TimeEntryBreakDto>>>
{
    public sealed class Handler : IRequestHandler<GetTimeEntryBreaksQuery, Result<IReadOnlyCollection<TimeEntryBreakDto>>>
    {
        private readonly ITimeEntryRepository _timeEntryRepository;
        private readonly ITimeEntryBreakRepository _timeEntryBreakRepository;
        private readonly IOrganizationAccessService _organizationAccess;

        public Handler(
            ITimeEntryRepository timeEntryRepository,
            ITimeEntryBreakRepository timeEntryBreakRepository,
            IOrganizationAccessService organizationAccess)
        {
            _timeEntryRepository = timeEntryRepository;
            _timeEntryBreakRepository = timeEntryBreakRepository;
            _organizationAccess = organizationAccess;
        }

        public async Task<Result<IReadOnlyCollection<TimeEntryBreakDto>>> Handle(GetTimeEntryBreaksQuery request, CancellationToken cancellationToken)
        {
            var timeEntry = await _timeEntryRepository.GetByIdAsync(request.TimeEntryId, cancellationToken);
            if (timeEntry is null)
            {
                return Result<IReadOnlyCollection<TimeEntryBreakDto>>.Failure(
                    new AppError("time_entries.not_found", "Time entry not found."),
                    (int)HttpStatusCode.NotFound);
            }

            var accessDenied = await _organizationAccess.ValidateResourceAccessAsync<IReadOnlyCollection<TimeEntryBreakDto>>(
                timeEntry.OrganizationId, request.ScopedOrganizationId, cancellationToken);
            if (accessDenied is not null) return accessDenied;

            var items = await _timeEntryBreakRepository.ListByTimeEntryIdAsync(request.TimeEntryId, cancellationToken);
            return Result<IReadOnlyCollection<TimeEntryBreakDto>>.Success(items.Select(x => x.ToDto()).ToList());
        }
    }
}
