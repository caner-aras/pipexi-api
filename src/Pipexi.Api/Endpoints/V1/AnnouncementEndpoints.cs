using MediatR;
using Pipexi.Application.Features.Announcements.Commands.CreateAnnouncement;
using Pipexi.Application.Features.Announcements.Commands.DeleteAnnouncement;
using Pipexi.Application.Features.Announcements.Commands.UpdateAnnouncement;
using Pipexi.Application.Features.Announcements.Queries.GetAnnouncementById;
using Pipexi.Application.Features.Announcements.Queries.GetAnnouncements;
using Pipexi.Application.Features.Announcements.Queries.GetMyAnnouncements;
using Pipexi.Contracts.V1.Announcements;

namespace Pipexi.Api.Endpoints.V1;

public static class AnnouncementEndpoints
{
    public static IEndpointRouteBuilder MapAnnouncementEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/announcements")
            .WithTags("announcements")
            .RequireAuthorization();

        group.MapGet("/", ListAsync);
        group.MapGet("/me", ListMineAsync);
        group.MapGet("/{id:guid}", GetByIdAsync);
        group.MapPost("/", CreateAsync);
        group.MapPut("/{id:guid}", UpdateAsync);
        group.MapDelete("/{id:guid}", DeleteAsync);

        return app;
    }

    private static async Task<IResult> ListAsync(
        Guid? organizationId,
        string? audienceType,
        Guid? audienceId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAnnouncementsQuery(organizationId, audienceType, audienceId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> ListMineAsync(
        Guid? organizationId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetMyAnnouncementsQuery(organizationId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> GetByIdAsync(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAnnouncementByIdQuery(id), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> CreateAsync(
        CreateAnnouncementRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateAnnouncementCommand(
                request.OrganizationId,
                request.Title,
                request.Body,
                request.AudienceType,
                request.AudienceId,
                request.PublishedAt),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> UpdateAsync(
        Guid id,
        UpdateAnnouncementRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateAnnouncementCommand(
                id,
                request.Title,
                request.Body,
                request.AudienceType,
                request.AudienceId,
                request.PublishedAt,
                request.Status),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> DeleteAsync(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteAnnouncementCommand(id), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }
}
