using MediatR;
using Pipexi.Application.Features.RolePermissions.Commands.CreateRolePermission;
using Pipexi.Application.Features.RolePermissions.Commands.DeleteRolePermission;
using Pipexi.Application.Features.RolePermissions.Commands.UpdateRolePermission;
using Pipexi.Application.Features.RolePermissions.Queries.GetRolePermissionById;
using Pipexi.Application.Features.RolePermissions.Queries.GetRolePermissions;
using Pipexi.Contracts.V1.RolePermissions;

namespace Pipexi.Api.Endpoints.V1;

public static class RolePermissionEndpoints
{
    public static IEndpointRouteBuilder MapRolePermissionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/role-permissions")
            .WithTags("role-permissions")
            .RequireAuthorization();

        group.MapGet("/", ListAsync);
        group.MapGet("/{id:guid}", GetByIdAsync);
        group.MapPost("/", CreateAsync);
        group.MapPut("/{id:guid}", UpdateAsync);
        group.MapDelete("/{id:guid}", DeleteAsync);

        return app;
    }

    private static async Task<IResult> ListAsync(Guid? roleId, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetRolePermissionsQuery(roleId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> GetByIdAsync(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetRolePermissionByIdQuery(id), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> CreateAsync(
        CreateRolePermissionRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateRolePermissionCommand(request.RoleId, request.PermissionId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> UpdateAsync(
        Guid id,
        UpdateRolePermissionRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateRolePermissionCommand(id, request.Status), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> DeleteAsync(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteRolePermissionCommand(id), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }
}
