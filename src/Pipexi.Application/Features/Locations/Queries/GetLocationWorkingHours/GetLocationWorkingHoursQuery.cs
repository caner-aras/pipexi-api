using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Locations.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Locations.Queries.GetLocationWorkingHours;

public sealed record GetLocationWorkingHoursQuery(
    Guid OrganizationId,
    Guid LocationId) : IQuery<Result<IReadOnlyCollection<LocationWorkingHourDto>>>
{
    public sealed class Handler : IRequestHandler<GetLocationWorkingHoursQuery, Result<IReadOnlyCollection<LocationWorkingHourDto>>>
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

        public async Task<Result<IReadOnlyCollection<LocationWorkingHourDto>>> Handle(
            GetLocationWorkingHoursQuery request,
            CancellationToken cancellationToken)
        {
            var location = await _locationRepository.GetByIdAsync(request.LocationId, cancellationToken);
            if (location is null || location.OrganizationId != request.OrganizationId)
            {
                return Result<IReadOnlyCollection<LocationWorkingHourDto>>.Failure(
                    new AppError("locations.not_found", "Location not found for organization."),
                    (int)HttpStatusCode.NotFound);
            }

            var items = await _locationWorkingHourRepository.ListByLocationIdAsync(request.LocationId, cancellationToken);
            return Result<IReadOnlyCollection<LocationWorkingHourDto>>.Success(items.Select(x => x.ToDto()).ToList());
        }
    }
}
