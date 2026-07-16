using MediatR;
using Workforce.Application.Abstractions.Persistence;
using Workforce.Application.Common.Models;
using Workforce.Application.Features.Locations.Dtos;
using Workforce.Shared.Results;

namespace Workforce.Application.Features.Locations.Queries.GetLocations;

public sealed record GetLocationsQuery(Guid? OrganizationId) : IQuery<Result<IReadOnlyCollection<LocationDto>>>
{
    public sealed class Handler : IRequestHandler<GetLocationsQuery, Result<IReadOnlyCollection<LocationDto>>>
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

        public async Task<Result<IReadOnlyCollection<LocationDto>>> Handle(GetLocationsQuery request, CancellationToken cancellationToken)
        {
            var items = request.OrganizationId.HasValue
                ? await _locationRepository.ListByOrganizationIdAsync(request.OrganizationId.Value, cancellationToken)
                : await _locationRepository.GetAllAsync(cancellationToken);

            var locationIds = items.Select(x => x.Id).ToList();
            var workingHours = await _locationWorkingHourRepository.ListByLocationIdsAsync(locationIds, cancellationToken);
            var workingHoursMap = workingHours
                .GroupBy(x => x.LocationId)
                .ToDictionary(
                    g => g.Key,
                    g => (IReadOnlyCollection<LocationWorkingHourDto>)g.OrderBy(x => x.DayOfWeek).Select(x => x.ToDto()).ToList());

            return Result<IReadOnlyCollection<LocationDto>>.Success(
                items.Select(x => x.ToDto(workingHoursMap.GetValueOrDefault(x.Id))).ToList());
        }
    }
}
