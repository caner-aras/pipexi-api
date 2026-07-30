using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.TimeEntries.Commands.DeleteTimeEntryBreak;

public sealed record DeleteTimeEntryBreakCommand(Guid Id, Guid? ScopedOrganizationId = null) : ICommand<Result<object?>>
{
    public sealed class Handler : IRequestHandler<DeleteTimeEntryBreakCommand, Result<object?>>
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

        public async Task<Result<object?>> Handle(DeleteTimeEntryBreakCommand request, CancellationToken cancellationToken)
        {
            var timeEntryBreak = await _timeEntryBreakRepository.GetByIdAsync(request.Id, cancellationToken);
            if (timeEntryBreak is null)
            {
                return Result<object?>.Failure(
                    new AppError("time_entry_breaks.not_found", "Time entry break not found."),
                    (int)HttpStatusCode.NotFound);
            }

            var timeEntry = await _timeEntryRepository.GetByIdAsync(timeEntryBreak.TimeEntryId, cancellationToken);
            if (timeEntry is null)
            {
                return Result<object?>.Failure(
                    new AppError("resource.not_found", "Parent resource not found."),
                    (int)HttpStatusCode.BadRequest);
            }

            var accessDenied = await _organizationAccess.ValidateResourceAccessAsync<object?>(
                timeEntry.OrganizationId, request.ScopedOrganizationId, cancellationToken);
            if (accessDenied is not null) return accessDenied;
            await _timeEntryBreakRepository.DeleteAsync(timeEntryBreak, cancellationToken);
            return Result<object?>.Success(null, (int)HttpStatusCode.OK);
        }
    }
}
