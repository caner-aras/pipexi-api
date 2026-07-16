using MediatR;
using Workforce.Application.Features.Announcements.Commands.CreateAnnouncement;
using Workforce.Application.Features.Announcements.Commands.DeleteAnnouncement;
using Workforce.Application.Features.Announcements.Commands.UpdateAnnouncement;
using Workforce.Application.Features.Announcements.Queries.GetAnnouncementById;
using Workforce.Application.Features.Announcements.Queries.GetAnnouncements;
using Workforce.Contracts.V1.Announcements;

namespace Workforce.Api.Endpoints.V1;

public static class AnnouncementEndpoints
{
    public static IEndpointRouteBuilder MapAnnouncementEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/announcements")
            .WithTags("announcements")
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
        string? audienceType,
        Guid? audienceId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAnnouncementsQuery(organizationId, audienceType, audienceId), cancellationToken);
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
