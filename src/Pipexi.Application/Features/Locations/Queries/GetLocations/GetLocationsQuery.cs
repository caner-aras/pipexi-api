using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Locations.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Locations.Queries.GetLocations;

public sealed record GetLocationsQuery(Guid? OrganizationId) : IQuery<Result<IReadOnlyCollection<LocationDto>>>
{
    public sealed class Handler : IRequestHandler<GetLocationsQuery, Result<IReadOnlyCollection<LocationDto>>>
    {
        private readonly ILocationRepository _locationRepository;
        private readonly ILocationWorkingHourRepository _locationWorkingHourRepository;
        private readonly ICurrentUserContext _currentUserContext;

        public Handler(
            ILocationRepository locationRepository,
            ILocationWorkingHourRepository locationWorkingHourRepository,
            ICurrentUserContext currentUserContext)
        {
            _locationRepository = locationRepository;
            _locationWorkingHourRepository = locationWorkingHourRepository;
            _currentUserContext = currentUserContext;
        }

        public async Task<Result<IReadOnlyCollection<LocationDto>>> Handle(GetLocationsQuery request, CancellationToken cancellationToken)
        {
            var organizationId = request.OrganizationId ?? _currentUserContext.OrganizationId;
            if (organizationId == Guid.Empty)
            {
                return Result<IReadOnlyCollection<LocationDto>>.Failure(
                    new AppError("auth.organization_required", "Organization is required."),
                    (int)HttpStatusCode.Forbidden);
            }

            var items = await _locationRepository.ListByOrganizationIdAsync(organizationId, cancellationToken);

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
