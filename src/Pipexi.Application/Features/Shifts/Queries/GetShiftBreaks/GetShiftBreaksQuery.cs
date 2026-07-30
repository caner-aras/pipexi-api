using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Shifts.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Shifts.Queries.GetShiftBreaks;

public sealed record GetShiftBreaksQuery(Guid ShiftId, Guid? ScopedOrganizationId = null) : IQuery<Result<IReadOnlyCollection<ShiftBreakDto>>>
{
    public sealed class Handler : IRequestHandler<GetShiftBreaksQuery, Result<IReadOnlyCollection<ShiftBreakDto>>>
    {
        private readonly IShiftRepository _shiftRepository;
        private readonly IShiftBreakRepository _shiftBreakRepository;
        private readonly IOrganizationAccessService _organizationAccess;

        public Handler(
            IShiftRepository shiftRepository,
            IShiftBreakRepository shiftBreakRepository,
            IOrganizationAccessService organizationAccess)
        {
            _shiftRepository = shiftRepository;
            _shiftBreakRepository = shiftBreakRepository;
            _organizationAccess = organizationAccess;
        }

        public async Task<Result<IReadOnlyCollection<ShiftBreakDto>>> Handle(GetShiftBreaksQuery request, CancellationToken cancellationToken)
        {
            var shift = await _shiftRepository.GetByIdAsync(request.ShiftId, cancellationToken);
            if (shift is null)
            {
                return Result<IReadOnlyCollection<ShiftBreakDto>>.Failure(
                    new AppError("shifts.not_found", "Shift not found."),
                    (int)HttpStatusCode.NotFound);
            }

            var accessDenied = await _organizationAccess.ValidateResourceAccessAsync<IReadOnlyCollection<ShiftBreakDto>>(
                shift.OrganizationId, request.ScopedOrganizationId, cancellationToken);
            if (accessDenied is not null) return accessDenied;

            var items = await _shiftBreakRepository.ListByShiftIdAsync(request.ShiftId, cancellationToken);
            return Result<IReadOnlyCollection<ShiftBreakDto>>.Success(items.Select(x => x.ToDto()).ToList());
        }
    }
}
