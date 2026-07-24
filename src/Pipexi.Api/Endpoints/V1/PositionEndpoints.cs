using MediatR;
using Pipexi.Application.Features.Positions.Commands.CreatePosition;
using Pipexi.Application.Features.Positions.Commands.DeletePosition;
using Pipexi.Application.Features.Positions.Commands.UpdatePosition;
using Pipexi.Application.Features.Positions.Queries.GetPositionById;
using Pipexi.Application.Features.Positions.Queries.GetPositions;
using Pipexi.Contracts.V1.Positions;

namespace Pipexi.Api.Endpoints.V1;

public static class PositionEndpoints
{
    public static IEndpointRouteBuilder MapPositionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/positions")
            .WithTags("positions")
            .RequireAuthorization();

        group.MapGet("/", ListAsync);
        group.MapGet("/{id:guid}", GetByIdAsync);
        group.MapPost("/", CreateAsync);
        group.MapPut("/{id:guid}", UpdateAsync);
        group.MapDelete("/{id:guid}", DeleteAsync);

        return app;
    }

    private static async Task<IResult> ListAsync(
        Guid organizationId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPositionsQuery(organizationId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPositionByIdQuery(id), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> CreateAsync(
        CreatePositionRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreatePositionCommand(
                request.OrganizationId,
                request.Title,
                request.DefaultHourlyRate,
                request.Description),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> UpdateAsync(
        Guid id,
        UpdatePositionRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdatePositionCommand(
                id,
                request.Title,
                request.DefaultHourlyRate,
                request.Description,
                request.Status),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> DeleteAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeletePositionCommand(id), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }
}
