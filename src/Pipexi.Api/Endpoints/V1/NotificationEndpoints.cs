using MediatR;
using Pipexi.Application.Features.Notifications.Commands.CreateNotification;
using Pipexi.Application.Features.Notifications.Commands.DeleteNotification;
using Pipexi.Application.Features.Notifications.Commands.UpdateNotification;
using Pipexi.Application.Features.Notifications.Queries.GetNotificationById;
using Pipexi.Application.Features.Notifications.Queries.GetNotifications;
using Pipexi.Contracts.V1.Notifications;

namespace Pipexi.Api.Endpoints.V1;

public static class NotificationEndpoints
{
    public static IEndpointRouteBuilder MapNotificationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/notifications")
            .WithTags("notifications")
            .RequireAuthorization();

        group.MapGet("/", ListAsync);
        group.MapGet("/{id:guid}", GetByIdAsync);
        group.MapPost("/", CreateAsync);
        group.MapPut("/{id:guid}", UpdateAsync);
        group.MapDelete("/{id:guid}", DeleteAsync);

        return app;
    }

    private static async Task<IResult> ListAsync(
        Guid? organizationId,
        Guid? organizationMemberId,
        bool? isRead,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetNotificationsQuery(organizationId, organizationMemberId, isRead), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> GetByIdAsync(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetNotificationByIdQuery(id), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> CreateAsync(
        CreateNotificationRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateNotificationCommand(
                request.OrganizationId,
                request.OrganizationMemberId,
                request.Type,
                request.Title,
                request.Body,
                request.IsRead,
                request.ScheduledTime),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> UpdateAsync(
        Guid id,
        UpdateNotificationRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateNotificationCommand(
                id,
                request.Type,
                request.Title,
                request.Body,
                request.IsRead,
                request.ScheduledTime,
                request.Status),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> DeleteAsync(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteNotificationCommand(id), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }
}
