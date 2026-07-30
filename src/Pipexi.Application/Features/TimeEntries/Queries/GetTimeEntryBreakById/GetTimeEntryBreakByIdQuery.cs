using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.TimeEntries.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.TimeEntries.Queries.GetTimeEntryBreakById;

public sealed record GetTimeEntryBreakByIdQuery(Guid Id, Guid? ScopedOrganizationId = null) : IQuery<Result<TimeEntryBreakDto>>
{
    public sealed class Handler : IRequestHandler<GetTimeEntryBreakByIdQuery, Result<TimeEntryBreakDto>>
    {
        private readonly ITimeEntryBreakRepository _timeEntryBreakRepository;
        private readonly ITimeEntryRepository _timeEntryRepository;
        private readonly IOrganizationAccessService _organizationAccess;

        public Handler(ITimeEntryBreakRepository timeEntryBreakRepository,
            ITimeEntryRepository timeEntryRepository,
            IOrganizationAccessService organizationAccess)
        {
            _organizationAccess = organizationAccess;
            _timeEntryRepository = timeEntryRepository;
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

            var timeEntry = await _timeEntryRepository.GetByIdAsync(timeEntryBreak.TimeEntryId, cancellationToken);
            if (timeEntry is null)
            {
                return Result<TimeEntryBreakDto>.Failure(
                    new AppError("resource.not_found", "Parent resource not found."),
                    (int)HttpStatusCode.BadRequest);
            }

            var accessDenied = await _organizationAccess.ValidateResourceAccessAsync<TimeEntryBreakDto>(
                timeEntry.OrganizationId, request.ScopedOrganizationId, cancellationToken);
            if (accessDenied is not null) return accessDenied;
            return Result<TimeEntryBreakDto>.Success(timeEntryBreak.ToDto());
        }
    }
}
