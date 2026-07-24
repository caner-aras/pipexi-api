using MediatR;
using Pipexi.Application.Features.AuditLogs.Commands.CreateAuditLog;
using Pipexi.Application.Features.AuditLogs.Queries.GetAuditLogById;
using Pipexi.Application.Features.AuditLogs.Queries.GetAuditLogs;
using Pipexi.Contracts.V1.AuditLogs;

namespace Pipexi.Api.Endpoints.V1;

public static class AuditLogEndpoints
{
    public static IEndpointRouteBuilder MapAuditLogEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/audit-logs")
            .WithTags("audit-logs")
            .RequireAuthorization();

        group.MapGet("/", ListAsync);
        group.MapGet("/{id:guid}", GetByIdAsync);
        group.MapPost("/", CreateAsync);

        return app;
    }

    private static async Task<IResult> ListAsync(
        Guid? organizationId,
        string? entityName,
        Guid? entityId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAuditLogsQuery(organizationId, entityName, entityId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> GetByIdAsync(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAuditLogByIdQuery(id), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> CreateAsync(
        CreateAuditLogRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateAuditLogCommand(
                request.OrganizationId,
                request.ActorMemberId,
                request.EntityName,
                request.EntityId,
                request.Action,
                request.BeforeJson,
                request.AfterJson,
                request.CreatedAt),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }
}
