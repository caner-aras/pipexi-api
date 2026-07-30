using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.TimeEntries.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.TimeEntries.Queries.GetTimeEntryById;

public sealed record GetTimeEntryByIdQuery(Guid Id, Guid? ScopedOrganizationId = null) : IQuery<Result<TimeEntryDto>>
{
    public sealed class Handler : IRequestHandler<GetTimeEntryByIdQuery, Result<TimeEntryDto>>
    {
        private readonly ITimeEntryRepository _timeEntryRepository;
        private readonly ITimeEntryBreakRepository _timeEntryBreakRepository;
        private readonly IOrganizationAccessService _organizationAccess;

        public Handler(ITimeEntryRepository timeEntryRepository, ITimeEntryBreakRepository timeEntryBreakRepository,
            IOrganizationAccessService organizationAccess)
        {
            _organizationAccess = organizationAccess;
            _timeEntryRepository = timeEntryRepository;
            _timeEntryBreakRepository = timeEntryBreakRepository;
        }

        public async Task<Result<TimeEntryDto>> Handle(GetTimeEntryByIdQuery request, CancellationToken cancellationToken)
        {
            var timeEntry = await _timeEntryRepository.GetByIdAsync(request.Id, cancellationToken);
            if (timeEntry is null)
            {
                return Result<TimeEntryDto>.Failure(
                    new AppError("time_entries.not_found", "Time entry not found."),
                    (int)HttpStatusCode.NotFound);
            }

            var accessDenied = await _organizationAccess.ValidateResourceAccessAsync<TimeEntryDto>(
                timeEntry.OrganizationId, request.ScopedOrganizationId, cancellationToken);
            if (accessDenied is not null) return accessDenied;

            var breaks = await _timeEntryBreakRepository.ListByTimeEntryIdAsync(timeEntry.Id, cancellationToken);
            return Result<TimeEntryDto>.Success(timeEntry.ToDto(breaks.Select(x => x.ToDto()).ToList()));
        }
    }
}
