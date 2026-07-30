using MediatR;
using Pipexi.Application.Features.Shifts.Commands.CreateShift;
using Pipexi.Application.Features.Shifts.Commands.CreateShiftBreak;
using Pipexi.Application.Features.Shifts.Commands.DeleteShift;
using Pipexi.Application.Features.Shifts.Commands.DeleteShiftBreak;
using Pipexi.Application.Features.Shifts.Commands.UpdateShift;
using Pipexi.Application.Features.Shifts.Commands.UpdateShiftBreak;
using Pipexi.Application.Features.Shifts.Queries.GetShiftBreakById;
using Pipexi.Application.Features.Shifts.Queries.GetShiftBreaks;
using Pipexi.Application.Features.Shifts.Queries.GetShiftById;
using Pipexi.Application.Features.Shifts.Queries.GetShifts;
using Pipexi.Application.Features.TimeEntries.Commands.CreateTimeEntry;
using Pipexi.Application.Features.TimeEntries.Commands.CreateTimeEntryBreak;
using Pipexi.Application.Features.TimeEntries.Commands.DeleteTimeEntry;
using Pipexi.Application.Features.TimeEntries.Commands.DeleteTimeEntryBreak;
using Pipexi.Application.Features.TimeEntries.Commands.UpdateTimeEntry;
using Pipexi.Application.Features.TimeEntries.Commands.UpdateTimeEntryBreak;
using Pipexi.Application.Features.TimeEntries.Queries.GetTimeEntries;
using Pipexi.Application.Features.TimeEntries.Queries.GetTimeEntryBreakById;
using Pipexi.Application.Features.TimeEntries.Queries.GetTimeEntryBreaks;
using Pipexi.Application.Features.TimeEntries.Queries.GetTimeEntryById;
using Pipexi.Contracts.V1.Shifts;

namespace Pipexi.Api.Endpoints.V1;

public static class ShiftEndpoints
{
    public static IEndpointRouteBuilder MapShiftEndpoints(this IEndpointRouteBuilder app)
    {
        var organizationTimeEntryGroup = app.MapGroup("/api/v1/organizations/{organizationId:guid}/time-entries")
            .WithTags("time-entries")
            .RequireAuthorization();

        organizationTimeEntryGroup.MapGet("/", ListTimeEntriesInOrganizationAsync);
        organizationTimeEntryGroup.MapPost("/", CreateTimeEntryInOrganizationAsync);
        organizationTimeEntryGroup.MapGet("/{timeEntryId:guid}", GetTimeEntryByIdInOrganizationAsync);
        organizationTimeEntryGroup.MapPut("/{timeEntryId:guid}", UpdateTimeEntryInOrganizationAsync);
        organizationTimeEntryGroup.MapDelete("/{timeEntryId:guid}", DeleteTimeEntryInOrganizationAsync);

        organizationTimeEntryGroup.MapGet("/{timeEntryId:guid}/breaks", ListTimeEntryBreaksInOrganizationAsync);
        organizationTimeEntryGroup.MapPost("/{timeEntryId:guid}/breaks", CreateTimeEntryBreakInOrganizationAsync);
        organizationTimeEntryGroup.MapGet("/{timeEntryId:guid}/breaks/{timeEntryBreakId:guid}", GetTimeEntryBreakByIdInOrganizationAsync);
        organizationTimeEntryGroup.MapPut("/{timeEntryId:guid}/breaks/{timeEntryBreakId:guid}", UpdateTimeEntryBreakInOrganizationAsync);
        organizationTimeEntryGroup.MapDelete("/{timeEntryId:guid}/breaks/{timeEntryBreakId:guid}", DeleteTimeEntryBreakInOrganizationAsync);

        var group = app.MapGroup("/api/v1/shifts")
            .WithTags("shifts")
            .RequireAuthorization();

        group.MapGet("/", ListShiftsAsync);
        group.MapGet("/{id:guid}", GetShiftByIdAsync);
        group.MapPost("/", CreateShiftAsync);
        group.MapPut("/{id:guid}", UpdateShiftAsync);
        group.MapDelete("/{id:guid}", DeleteShiftAsync);

        group.MapGet("/{shiftId:guid}/breaks", ListShiftBreaksAsync);
        group.MapGet("/breaks/{id:guid}", GetShiftBreakByIdAsync);
        group.MapPost("/breaks", CreateShiftBreakAsync);
        group.MapPut("/breaks/{id:guid}", UpdateShiftBreakAsync);
        group.MapDelete("/breaks/{id:guid}", DeleteShiftBreakAsync);

        return app;
    }

    private static async Task<IResult> ListTimeEntriesInOrganizationAsync(
        Guid organizationId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetTimeEntriesQuery(organizationId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> CreateTimeEntryInOrganizationAsync(
        Guid organizationId,
        CreateTimeEntryInOrganizationRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateTimeEntryCommand(
                organizationId,
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

    private static async Task<IResult> GetTimeEntryByIdInOrganizationAsync(
        Guid organizationId,
        Guid timeEntryId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetTimeEntryByIdQuery(timeEntryId, organizationId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> UpdateTimeEntryInOrganizationAsync(
        Guid organizationId,
        Guid timeEntryId,
        UpdateTimeEntryInOrganizationRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateTimeEntryCommand(
                timeEntryId,
                request.ShiftId,
                request.OrganizationMemberId,
                request.LocationId,
                request.ClockInAt,
                request.ClockOutAt,
                request.EmployeeNote,
                request.ManagerNote,
                request.Status,
                organizationId),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> DeleteTimeEntryInOrganizationAsync(
        Guid organizationId,
        Guid timeEntryId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteTimeEntryCommand(timeEntryId, organizationId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> ListTimeEntryBreaksInOrganizationAsync(
        Guid organizationId,
        Guid timeEntryId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetTimeEntryBreaksQuery(timeEntryId, organizationId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> CreateTimeEntryBreakInOrganizationAsync(
        Guid organizationId,
        Guid timeEntryId,
        CreateTimeEntryBreakInTimeEntryRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateTimeEntryBreakCommand(timeEntryId, request.StartAt, request.EndAt, request.IsPaid, organizationId),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> GetTimeEntryBreakByIdInOrganizationAsync(
        Guid organizationId,
        Guid timeEntryId,
        Guid timeEntryBreakId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetTimeEntryBreakByIdQuery(timeEntryBreakId, organizationId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> UpdateTimeEntryBreakInOrganizationAsync(
        Guid organizationId,
        Guid timeEntryId,
        Guid timeEntryBreakId,
        UpdateTimeEntryBreakInTimeEntryRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateTimeEntryBreakCommand(timeEntryBreakId, request.StartAt, request.EndAt, request.IsPaid, request.Status, organizationId),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> DeleteTimeEntryBreakInOrganizationAsync(
        Guid organizationId,
        Guid timeEntryId,
        Guid timeEntryBreakId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteTimeEntryBreakCommand(timeEntryBreakId, organizationId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> ListShiftsAsync(Guid? organizationId, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetShiftsQuery(organizationId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> GetShiftByIdAsync(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetShiftByIdQuery(id), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> CreateShiftAsync(CreateShiftRequest request, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateShiftCommand(
                request.OrganizationId,
                request.TeamId,
                request.OrganizationMemberId,
                request.LocationId,
                request.Title,
                request.StartAt,
                request.EndAt,
                request.Notes,
                request.Breaks?.Select(x =>
                    new CreateShiftBreakInput(x.StartAt, x.EndAt, x.IsPaid)).ToList(),
                request.RequiredFormTemplateIds,
                request.Repeat,
                request.RepeatTimes,
                request.RepeatOn,
                request.DayOfMonth),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> UpdateShiftAsync(Guid id, UpdateShiftRequest request, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateShiftCommand(
                id,
                request.TeamId,
                request.OrganizationMemberId,
                request.LocationId,
                request.Title,
                request.StartAt,
                request.EndAt,
                request.Notes,
                request.Status,
                request.RequiredFormTemplateIds),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> DeleteShiftAsync(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteShiftCommand(id), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> ListShiftBreaksAsync(Guid shiftId, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetShiftBreaksQuery(shiftId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> GetShiftBreakByIdAsync(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetShiftBreakByIdQuery(id), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> CreateShiftBreakAsync(CreateShiftBreakRequest request, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateShiftBreakCommand(
                request.ShiftId,
                request.StartAt,
                request.EndAt,
                request.IsPaid),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> UpdateShiftBreakAsync(
        Guid id,
        UpdateShiftBreakRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateShiftBreakCommand(id, request.StartAt, request.EndAt, request.IsPaid, request.Status),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> DeleteShiftBreakAsync(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteShiftBreakCommand(id), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private sealed record CreateTimeEntryInOrganizationRequest(
        Guid ShiftId,
        Guid OrganizationMemberId,
        Guid LocationId,
        DateTimeOffset ClockInAt,
        DateTimeOffset? ClockOutAt,
        string? EmployeeNote,
        string? ManagerNote,
        IReadOnlyCollection<CreateTimeEntryBreakInTimeEntryRequest>? Breaks);

    private sealed record CreateTimeEntryBreakInTimeEntryRequest(
        DateTimeOffset StartAt,
        DateTimeOffset EndAt,
        bool IsPaid);

    private sealed record UpdateTimeEntryInOrganizationRequest(
        Guid? ShiftId,
        Guid? OrganizationMemberId,
        Guid? LocationId,
        DateTimeOffset? ClockInAt,
        DateTimeOffset? ClockOutAt,
        string? EmployeeNote,
        string? ManagerNote,
        string? Status);

    private sealed record UpdateTimeEntryBreakInTimeEntryRequest(
        DateTimeOffset? StartAt,
        DateTimeOffset? EndAt,
        bool? IsPaid,
        string? Status);
}
