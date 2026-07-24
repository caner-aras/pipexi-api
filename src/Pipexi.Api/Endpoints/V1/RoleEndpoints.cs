using MediatR;
using Pipexi.Application.Features.Roles.Commands.CreateRole;
using Pipexi.Application.Features.Roles.Commands.DeleteRole;
using Pipexi.Application.Features.Roles.Commands.UpdateRole;
using Pipexi.Application.Features.Roles.Queries.GetRoleById;
using Pipexi.Application.Features.Roles.Queries.GetRoles;
using Pipexi.Contracts.V1.Roles;

namespace Pipexi.Api.Endpoints.V1;

public static class RoleEndpoints
{
    public static IEndpointRouteBuilder MapRoleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/roles")
            .WithTags("roles")
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
        var result = await sender.Send(new GetRolesQuery(organizationId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> GetByIdAsync(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetRoleByIdQuery(id), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> CreateAsync(CreateRoleRequest request, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateRoleCommand(request.OrganizationId, request.Name), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> UpdateAsync(
        Guid id,
        UpdateRoleRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateRoleCommand(id, request.Name, request.Status), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> DeleteAsync(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteRoleCommand(id), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }
}
