using MediatR;
using Pipexi.Application.Features.Permissions.Commands.CreatePermission;
using Pipexi.Application.Features.Permissions.Commands.DeletePermission;
using Pipexi.Application.Features.Permissions.Commands.UpdatePermission;
using Pipexi.Application.Features.Permissions.Queries.GetPermissionById;
using Pipexi.Application.Features.Permissions.Queries.GetPermissions;
using Pipexi.Contracts.V1.Permissions;

namespace Pipexi.Api.Endpoints.V1;

public static class PermissionEndpoints
{
    public static IEndpointRouteBuilder MapPermissionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/permissions")
            .WithTags("permissions")
            .RequireAuthorization();

        group.MapGet("/", ListAsync);
        group.MapGet("/{id:guid}", GetByIdAsync);
        group.MapPost("/", CreateAsync);
        group.MapPut("/{id:guid}", UpdateAsync);
        group.MapDelete("/{id:guid}", DeleteAsync);

        return app;
    }

    private static async Task<IResult> ListAsync(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPermissionsQuery(), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> GetByIdAsync(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPermissionByIdQuery(id), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> CreateAsync(CreatePermissionRequest request, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreatePermissionCommand(request.Key), cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> UpdateAsync(
        Guid id,
        UpdatePermissionRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdatePermissionCommand(id, request.Key, request.Status),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> DeleteAsync(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeletePermissionCommand(id), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }
}
