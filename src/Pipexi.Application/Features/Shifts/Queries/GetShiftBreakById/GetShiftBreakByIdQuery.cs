using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Shifts.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Shifts.Queries.GetShiftBreakById;

public sealed record GetShiftBreakByIdQuery(Guid Id, Guid? ScopedOrganizationId = null) : IQuery<Result<ShiftBreakDto>>
{
    public sealed class Handler : IRequestHandler<GetShiftBreakByIdQuery, Result<ShiftBreakDto>>
    {
        private readonly IShiftBreakRepository _shiftBreakRepository;
        private readonly IShiftRepository _shiftRepository;
        private readonly IOrganizationAccessService _organizationAccess;

        public Handler(IShiftBreakRepository shiftBreakRepository,
            IShiftRepository shiftRepository,
            IOrganizationAccessService organizationAccess)
        {
            _organizationAccess = organizationAccess;
            _shiftRepository = shiftRepository;
            _shiftBreakRepository = shiftBreakRepository;
        }

        public async Task<Result<ShiftBreakDto>> Handle(GetShiftBreakByIdQuery request, CancellationToken cancellationToken)
        {
            var shiftBreak = await _shiftBreakRepository.GetByIdAsync(request.Id, cancellationToken);
            if (shiftBreak is null)
            {
                return Result<ShiftBreakDto>.Failure(
                    new AppError("shift_breaks.not_found", "Shift break not found."),
                    (int)HttpStatusCode.NotFound);
            }

            var shift = await _shiftRepository.GetByIdAsync(shiftBreak.ShiftId, cancellationToken);
            if (shift is null)
            {
                return Result<ShiftBreakDto>.Failure(
                    new AppError("shift_breaks.invalid_shift", "Shift not found."),
                    (int)HttpStatusCode.BadRequest);
            }

            var accessDenied = await _organizationAccess.ValidateResourceAccessAsync<ShiftBreakDto>(
                shift.OrganizationId, request.ScopedOrganizationId, cancellationToken);
            if (accessDenied is not null) return accessDenied;
            return Result<ShiftBreakDto>.Success(shiftBreak.ToDto());
        }
    }
}
