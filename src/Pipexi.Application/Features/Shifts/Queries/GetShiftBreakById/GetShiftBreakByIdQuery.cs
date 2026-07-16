using System.Net;
using MediatR;
using Workforce.Application.Abstractions.Persistence;
using Workforce.Application.Common.Models;
using Workforce.Application.Features.Shifts.Dtos;
using Workforce.Shared.Errors;
using Workforce.Shared.Results;

namespace Workforce.Application.Features.Shifts.Queries.GetShiftBreakById;

public sealed record GetShiftBreakByIdQuery(Guid Id) : IQuery<Result<ShiftBreakDto>>
{
    public sealed class Handler : IRequestHandler<GetShiftBreakByIdQuery, Result<ShiftBreakDto>>
    {
        private readonly IShiftBreakRepository _shiftBreakRepository;

        public Handler(IShiftBreakRepository shiftBreakRepository)
        {
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

            return Result<ShiftBreakDto>.Success(shiftBreak.ToDto());
        }
    }
}
