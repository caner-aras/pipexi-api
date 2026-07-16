using MediatR;
using Workforce.Application.Features.Locations.Commands.CreateLocation;
using Workforce.Application.Features.Locations.Commands.DeleteLocation;
using Workforce.Application.Features.Locations.Commands.UpdateLocation;
using Workforce.Application.Features.Locations.Queries.GetLocationById;
using Workforce.Application.Features.Locations.Queries.GetLocations;
using Workforce.Contracts.V1.Locations;

namespace Workforce.Api.Endpoints.V1;

public static class LocationEndpoints
{
    public static IEndpointRouteBuilder MapLocationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/locations")
            .WithTags("locations")
            .RequireAuthorization();

        group.MapGet("/", ListAsync);
        group.MapGet("/{id:guid}", GetByIdAsync);
        group.MapPost("/", CreateAsync);
        group.MapPut("/{id:guid}", UpdateAsync);
        group.MapDelete("/{id:guid}", DeleteAsync);

        return app;
    }

    private static async Task<IResult> ListAsync(Guid? organizationId, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetLocationsQuery(organizationId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> GetByIdAsync(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetLocationByIdQuery(id), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> CreateAsync(CreateLocationRequest request, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateLocationCommand(
                request.OrganizationId,
                request.Name,
                request.Address,
                request.Latitude,
                request.Longitude,
                request.GeofenceRadiusMeters,
                request.Timezone),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> UpdateAsync(
        Guid id,
        UpdateLocationRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateLocationCommand(
                id,
                request.Name,
                request.Address,
                request.Latitude,
                request.Longitude,
                request.GeofenceRadiusMeters,
                request.Timezone,
                request.Status),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> DeleteAsync(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteLocationCommand(id), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }
}
