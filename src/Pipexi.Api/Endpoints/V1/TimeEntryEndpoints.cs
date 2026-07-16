using MediatR;
using Workforce.Application.Features.TimeEntries.Commands.CreateTimeEntry;
using Workforce.Application.Features.TimeEntries.Commands.CreateTimeEntryBreak;
using Workforce.Application.Features.TimeEntries.Commands.DeleteTimeEntry;
using Workforce.Application.Features.TimeEntries.Commands.DeleteTimeEntryBreak;
using Workforce.Application.Features.TimeEntries.Commands.UpdateTimeEntry;
using Workforce.Application.Features.TimeEntries.Commands.UpdateTimeEntryBreak;
using Workforce.Application.Features.TimeEntries.Queries.GetTimeEntries;
using Workforce.Application.Features.TimeEntries.Queries.GetTimeEntryBreakById;
using Workforce.Application.Features.TimeEntries.Queries.GetTimeEntryBreaks;
using Workforce.Application.Features.TimeEntries.Queries.GetTimeEntryById;
using Workforce.Contracts.V1.TimeEntries;

namespace Workforce.Api.Endpoints.V1;

public static class TimeEntryEndpoints
{
    public static IEndpointRouteBuilder MapTimeEntryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/time-entries")
            .WithTags("time-entries")
            .RequireAuthorization();

        group.MapGet("/", ListTimeEntriesAsync);
        group.MapGet("/{id:guid}", GetTimeEntryByIdAsync);
        group.MapPost("/", CreateTimeEntryAsync);
        group.MapPut("/{id:guid}", UpdateTimeEntryAsync);
        group.MapDelete("/{id:guid}", DeleteTimeEntryAsync);

        group.MapGet("/{timeEntryId:guid}/breaks", ListTimeEntryBreaksAsync);
        group.MapGet("/breaks/{id:guid}", GetTimeEntryBreakByIdAsync);
        group.MapPost("/breaks", CreateTimeEntryBreakAsync);
        group.MapPut("/breaks/{id:guid}", UpdateTimeEntryBreakAsync);
        group.MapDelete("/breaks/{id:guid}", DeleteTimeEntryBreakAsync);

        return app;
    }

    private static async Task<IResult> ListTimeEntriesAsync(Guid? organizationId, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetTimeEntriesQuery(organizationId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> GetTimeEntryByIdAsync(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetTimeEntryByIdQuery(id), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> CreateTimeEntryAsync(CreateTimeEntryRequest request, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateTimeEntryCommand(
                request.OrganizationId,
                request.ShiftId,
                request.OrganizationMemberId,
                request.LocationId,
                request.ClockInAt,
                request.ClockOutAt,
                request.EmployeeNote,
                request.ManagerNote,
                request.Breaks?.Select(x => new CreateTimeEntryBreakInput(x.StartAt, x.EndAt, x.IsPaid)).ToList()),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> UpdateTimeEntryAsync(Guid id, UpdateTimeEntryRequest request, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateTimeEntryCommand(
                id,
                request.ShiftId,
                request.OrganizationMemberId,
                request.LocationId,
                request.ClockInAt,
                request.ClockOutAt,
                request.EmployeeNote,
                request.ManagerNote,
                request.Status),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> DeleteTimeEntryAsync(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteTimeEntryCommand(id), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> ListTimeEntryBreaksAsync(Guid timeEntryId, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetTimeEntryBreaksQuery(timeEntryId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> GetTimeEntryBreakByIdAsync(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetTimeEntryBreakByIdQuery(id), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> CreateTimeEntryBreakAsync(CreateTimeEntryBreakRequest request, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateTimeEntryBreakCommand(request.TimeEntryId, request.StartAt, request.EndAt, request.IsPaid),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> UpdateTimeEntryBreakAsync(
        Guid id,
        UpdateTimeEntryBreakRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateTimeEntryBreakCommand(id, request.StartAt, request.EndAt, request.IsPaid, request.Status),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> DeleteTimeEntryBreakAsync(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteTimeEntryBreakCommand(id), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }
}
