using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Locations.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Locations.Queries.GetLocationById;

public sealed record GetLocationByIdQuery(Guid Id) : IQuery<Result<LocationDto>>
{
    public sealed class Handler : IRequestHandler<GetLocationByIdQuery, Result<LocationDto>>
    {
        private readonly ILocationRepository _locationRepository;
        private readonly ILocationWorkingHourRepository _locationWorkingHourRepository;

        public Handler(
            ILocationRepository locationRepository,
            ILocationWorkingHourRepository locationWorkingHourRepository)
        {
            _locationRepository = locationRepository;
            _locationWorkingHourRepository = locationWorkingHourRepository;
        }

        public async Task<Result<LocationDto>> Handle(GetLocationByIdQuery request, CancellationToken cancellationToken)
        {
            var location = await _locationRepository.GetByIdAsync(request.Id, cancellationToken);
            if (location is null)
            {
                return Result<LocationDto>.Failure(
                    new AppError("locations.not_found", "Location not found."),
                    (int)HttpStatusCode.NotFound);
            }

            var workingHours = await _locationWorkingHourRepository.ListByLocationIdAsync(location.Id, cancellationToken);

            return Result<LocationDto>.Success(location.ToDto(workingHours.Select(x => x.ToDto()).ToList()));
        }
    }
}
