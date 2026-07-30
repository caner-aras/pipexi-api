using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Shifts.Commands.DeleteShiftBreak;

public sealed record DeleteShiftBreakCommand(Guid Id, Guid? ScopedOrganizationId = null) : ICommand<Result<object?>>
{
    public sealed class Handler : IRequestHandler<DeleteShiftBreakCommand, Result<object?>>
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

        public async Task<Result<object?>> Handle(DeleteShiftBreakCommand request, CancellationToken cancellationToken)
        {
            var shiftBreak = await _shiftBreakRepository.GetByIdAsync(request.Id, cancellationToken);
            if (shiftBreak is null)
            {
                return Result<object?>.Failure(
                    new AppError("shift_breaks.not_found", "Shift break not found."),
                    (int)HttpStatusCode.NotFound);
            }

            var shift = await _shiftRepository.GetByIdAsync(shiftBreak.ShiftId, cancellationToken);
            if (shift is null)
            {
                return Result<object?>.Failure(
                    new AppError("shift_breaks.invalid_shift", "Shift not found."),
                    (int)HttpStatusCode.BadRequest);
            }

            var accessDenied = await _organizationAccess.ValidateResourceAccessAsync<object?>(
                shift.OrganizationId, request.ScopedOrganizationId, cancellationToken);
            if (accessDenied is not null) return accessDenied;
            await _shiftBreakRepository.DeleteAsync(shiftBreak, cancellationToken);
            return Result<object?>.Success(null, (int)HttpStatusCode.OK);
        }
    }
}
