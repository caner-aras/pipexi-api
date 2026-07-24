using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Shifts.Dtos;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Shifts.Queries.GetShiftBreaks;

public sealed record GetShiftBreaksQuery(Guid ShiftId) : IQuery<Result<IReadOnlyCollection<ShiftBreakDto>>>
{
    public sealed class Handler : IRequestHandler<GetShiftBreaksQuery, Result<IReadOnlyCollection<ShiftBreakDto>>>
    {
        private readonly IShiftBreakRepository _shiftBreakRepository;

        public Handler(IShiftBreakRepository shiftBreakRepository)
        {
            _shiftBreakRepository = shiftBreakRepository;
        }

        public async Task<Result<IReadOnlyCollection<ShiftBreakDto>>> Handle(GetShiftBreaksQuery request, CancellationToken cancellationToken)
        {
            var items = await _shiftBreakRepository.ListByShiftIdAsync(request.ShiftId, cancellationToken);
            return Result<IReadOnlyCollection<ShiftBreakDto>>.Success(items.Select(x => x.ToDto()).ToList());
        }
    }
}
