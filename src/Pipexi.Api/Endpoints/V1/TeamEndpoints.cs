using MediatR;
using Pipexi.Application.Features.Tasks.Queries.GetTasks;
using Pipexi.Application.Features.Teams.Commands.CreateTeam;
using Pipexi.Application.Features.Teams.Commands.CreateTeamMember;
using Pipexi.Application.Features.Teams.Commands.CreateTeamMemberDayOff;
using Pipexi.Application.Features.Teams.Commands.CreateTeamMemberWithUser;
using Pipexi.Application.Features.Teams.Commands.DeleteTeam;
using Pipexi.Application.Features.Teams.Commands.DeleteTeamMember;
using Pipexi.Application.Features.Teams.Commands.DeleteTeamMemberDayOff;
using Pipexi.Application.Features.Teams.Commands.UpdateTeam;
using Pipexi.Application.Features.Teams.Commands.UpdateTeamMember;
using Pipexi.Application.Features.Teams.Commands.UpdateTeamMemberDayOff;
using Pipexi.Application.Features.Teams.Queries.GetTeamById;
using Pipexi.Application.Features.Teams.Queries.GetTeamDayOffs;
using Pipexi.Application.Features.Teams.Queries.GetTeamMemberById;
using Pipexi.Application.Features.Teams.Queries.GetTeamMemberDayOffById;
using Pipexi.Application.Features.Teams.Queries.GetTeamMemberDayOffs;
using Pipexi.Application.Features.Teams.Queries.GetTeamMemberDetailsById;
using Pipexi.Application.Features.Teams.Queries.GetTeamMemberDetailsByOrganizationMember;
using Pipexi.Application.Features.Teams.Queries.GetTeamMembers;
using Pipexi.Application.Features.Teams.Queries.GetTeamMembersWorkSummary;
using Pipexi.Application.Features.Teams.Queries.GetTeamMemberTasksById;
using Pipexi.Application.Features.Teams.Queries.GetTeams;
using Pipexi.Application.Features.TimeEntries.Queries.GetTimeEntriesByTeamId;
using Pipexi.Contracts.V1.Teams;

namespace Pipexi.Api.Endpoints.V1;

public static class TeamEndpoints
{
    public static IEndpointRouteBuilder MapTeamEndpoints(this IEndpointRouteBuilder app)
    {
        var organizationGroup = app.MapGroup("/api/v1/organizations/{organizationId:guid}/teams")
            .WithTags("teams")
            .RequireAuthorization();

        organizationGroup.MapGet("/", ListTeamsInOrganizationAsync);
        organizationGroup.MapPost("/", CreateTeamInOrganizationAsync);
        organizationGroup.MapGet("/{teamId:guid}/members", ListTeamMembersInOrganizationAsync);
        organizationGroup.MapGet("/{teamId:guid}/members/{teamMemberId:guid}", GetTeamMemberInOrganizationByIdAsync);
        organizationGroup.MapGet("/{teamId:guid}/members/{teamMemberId:guid}/details", GetTeamMemberInOrganizationByIdDetailsAsync);
        organizationGroup.MapGet("/{teamId:guid}/members/{teamMemberId:guid}/tasks", ListTeamMemberTasksInOrganizationAsync);
        organizationGroup.MapGet("/{teamId:guid}/tasks", ListTeamTasksInOrganizationAsync);
        organizationGroup.MapGet("/{teamId:guid}/time-entries", ListTeamTimeEntriesInOrganizationAsync);
        organizationGroup.MapGet("/members/work-summary", GetTeamMembersWorkSummaryInOrganizationAsync);
        organizationGroup.MapGet(
            "/organization-members/{organizationMemberId:guid}/details",
            GetTeamMemberDetailsByOrganizationMemberInOrganizationAsync);
        organizationGroup.MapPost("/{teamId:guid}/members", CreateTeamMemberInOrganizationAsync);
        organizationGroup.MapPost("/{teamId:guid}/members/onboard", CreateTeamMemberWithUserInOrganizationAsync);
        organizationGroup.MapPut("/{teamId:guid}/members/{teamMemberId:guid}", UpdateTeamMemberInOrganizationAsync);
        organizationGroup.MapDelete("/{teamId:guid}/members/{teamMemberId:guid}", DeleteTeamMemberInOrganizationAsync);
        organizationGroup.MapGet("/{teamId:guid}/day-offs", ListTeamDayOffsInOrganizationAsync);

        var group = app.MapGroup("/api/v1/teams")
            .WithTags("teams")
            .RequireAuthorization();

        group.MapGet("/", ListTeamsAsync);
        group.MapGet("/{id:guid}", GetTeamByIdAsync);
        group.MapPost("/", CreateTeamAsync);
        group.MapPut("/{id:guid}", UpdateTeamAsync);
        group.MapDelete("/{id:guid}", DeleteTeamAsync);

        group.MapGet("/{teamId:guid}/members", ListTeamMembersAsync);
        group.MapGet("/{teamId:guid}/members/{teamMemberId:guid}", GetTeamMemberByIdInTeamAsync);
        group.MapGet("/{teamId:guid}/members/{teamMemberId:guid}/details", GetTeamMemberByIdInTeamDetailsAsync);
        group.MapGet("/{teamId:guid}/members/{teamMemberId:guid}/tasks", ListTeamMemberTasksInTeamAsync);
        group.MapPost("/{teamId:guid}/members", CreateTeamMemberInTeamAsync);
        group.MapPost("/{teamId:guid}/members/onboard", CreateTeamMemberWithUserInTeamAsync);
        group.MapPut("/{teamId:guid}/members/{teamMemberId:guid}", UpdateTeamMemberInTeamAsync);
        group.MapDelete("/{teamId:guid}/members/{teamMemberId:guid}", DeleteTeamMemberInTeamAsync);
        group.MapGet("/{teamId:guid}/day-offs", ListTeamDayOffsInTeamAsync);
        group.MapGet("/{teamId:guid}/tasks", ListTeamTasksAsync);
        group.MapGet("/{teamId:guid}/time-entries", ListTeamTimeEntriesAsync);
        group.MapGet("/members/work-summary", GetTeamMembersWorkSummaryAsync);
        group.MapGet("/members/{teamMemberId:guid}", GetTeamMemberByIdAsync);
        group.MapGet("/members/{teamMemberId:guid}/details", GetTeamMemberByIdDetailsAsync);
        group.MapGet("/members/{teamMemberId:guid}/tasks", ListTeamMemberTasksAsync);
        group.MapPost("/members", CreateTeamMemberAsync);
        group.MapPut("/members/{teamMemberId:guid}", UpdateTeamMemberAsync);
        group.MapDelete("/members/{teamMemberId:guid}", DeleteTeamMemberAsync);
        group.MapGet("/members/{teamMemberId:guid}/day-offs", ListTeamMemberDayOffsAsync);
        group.MapPost("/members/{teamMemberId:guid}/day-offs", CreateTeamMemberDayOffAsync);
        group.MapGet("/members/{teamMemberId:guid}/day-offs/{dayOffId:guid}", GetTeamMemberDayOffByIdAsync);
        group.MapPut("/members/{teamMemberId:guid}/day-offs/{dayOffId:guid}", UpdateTeamMemberDayOffAsync);
        group.MapDelete("/members/{teamMemberId:guid}/day-offs/{dayOffId:guid}", DeleteTeamMemberDayOffAsync);

        return app;
    }

    private static async Task<IResult> ListTeamsInOrganizationAsync(
        Guid organizationId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetTeamsQuery(organizationId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> CreateTeamInOrganizationAsync(
        Guid organizationId,
        CreateTeamInOrganizationRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateTeamCommand(organizationId, request.Name, request.ManagerMemberId, request.LocationId),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> ListTeamMembersInOrganizationAsync(
        Guid organizationId,
        Guid teamId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetTeamMembersQuery(teamId, organizationId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> ListTeamTasksInOrganizationAsync(
        Guid organizationId,
        Guid teamId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetTasksQuery(OrganizationId: organizationId, TeamId: teamId),
            cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> ListTeamTimeEntriesInOrganizationAsync(
        Guid organizationId,
        Guid teamId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetTimeEntriesByTeamIdQuery(teamId, organizationId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> GetTeamMemberDetailsByOrganizationMemberInOrganizationAsync(
        Guid organizationId,
        Guid organizationMemberId,
        DateTimeOffset? fromDate,
        Guid? teamId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetTeamMemberDetailsByOrganizationMemberQuery(
                organizationId,
                organizationMemberId,
                fromDate,
                teamId),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> GetTeamMembersWorkSummaryInOrganizationAsync(
        Guid organizationId,
        Guid teamMemberId,
        DateOnly fromDate,
        DateOnly toDate,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetTeamMembersWorkSummaryQuery(teamMemberId, fromDate, toDate, organizationId),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> CreateTeamMemberInOrganizationAsync(
        Guid organizationId,
        Guid teamId,
        CreateTeamMemberInTeamRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateTeamMemberCommand(teamId, request.OrganizationMemberId, organizationId),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> CreateTeamMemberWithUserInOrganizationAsync(
        Guid organizationId,
        Guid teamId,
        CreateTeamMemberWithUserRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateTeamMemberWithUserCommand(
                teamId,
                request.Email,
                request.FirstName,
                request.LastName,
                request.RoleId,
                request.JobTitle,
                request.Phone,
                request.AvatarUrl,
                request.AuthProviderId,
                organizationId),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> GetTeamMemberInOrganizationByIdAsync(
        Guid organizationId,
        Guid teamId,
        Guid teamMemberId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetTeamMemberByIdQuery(teamMemberId, organizationId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> GetTeamMemberInOrganizationByIdDetailsAsync(
        Guid organizationId,
        Guid teamId,
        Guid teamMemberId,
        DateTimeOffset? fromDate,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetTeamMemberDetailsByIdQuery(teamMemberId, fromDate, organizationId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> ListTeamMemberTasksInOrganizationAsync(
        Guid organizationId,
        Guid teamId,
        Guid teamMemberId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetTeamMemberTasksByIdQuery(teamMemberId, organizationId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> UpdateTeamMemberInOrganizationAsync(
        Guid organizationId,
        Guid teamId,
        Guid teamMemberId,
        UpdateTeamMemberRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateTeamMemberCommand(teamMemberId, request.Status, organizationId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> DeleteTeamMemberInOrganizationAsync(
        Guid organizationId,
        Guid teamId,
        Guid teamMemberId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteTeamMemberCommand(teamMemberId, organizationId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> ListTeamDayOffsInOrganizationAsync(
        Guid organizationId,
        Guid teamId,
        DateTimeOffset fromAt,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetTeamDayOffsQuery(teamId, fromAt, organizationId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> CreateTeamMemberDayOffInOrganizationAsync(
        Guid organizationId,
        Guid teamId,
        Guid teamMemberId,
        UpsertTeamMemberDayOffRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateTeamMemberDayOffCommand(teamMemberId, request.StartAt, request.EndAt, request.Reason, organizationId),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> GetTeamMemberDayOffInOrganizationByIdAsync(
        Guid organizationId,
        Guid teamId,
        Guid teamMemberId,
        Guid dayOffId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetTeamMemberDayOffByIdQuery(dayOffId, teamMemberId, organizationId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> UpdateTeamMemberDayOffInOrganizationAsync(
        Guid organizationId,
        Guid teamId,
        Guid teamMemberId,
        Guid dayOffId,
        UpdateTeamMemberDayOffRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateTeamMemberDayOffCommand(dayOffId, teamMemberId, request.StartAt, request.EndAt, request.Reason, request.Status, organizationId),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> DeleteTeamMemberDayOffInOrganizationAsync(
        Guid organizationId,
        Guid teamId,
        Guid teamMemberId,
        Guid dayOffId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteTeamMemberDayOffCommand(dayOffId, teamMemberId, organizationId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> ListTeamsAsync(Guid? organizationId, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetTeamsQuery(organizationId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> GetTeamByIdAsync(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetTeamByIdQuery(id), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> CreateTeamAsync(CreateTeamRequest request, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateTeamCommand(request.OrganizationId, request.Name, request.ManagerMemberId, request.LocationId),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> UpdateTeamAsync(
        Guid id,
        UpdateTeamRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateTeamCommand(id, request.Name, request.ManagerMemberId, request.Status, request.LocationId, UpdateLocation: true),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> DeleteTeamAsync(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteTeamCommand(id), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> ListTeamMembersAsync(Guid teamId, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetTeamMembersQuery(teamId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> GetTeamMemberByIdInTeamAsync(
        Guid teamId,
        Guid teamMemberId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        _ = teamId;

        var result = await sender.Send(new GetTeamMemberByIdQuery(teamMemberId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> GetTeamMemberByIdInTeamDetailsAsync(
        Guid teamId,
        Guid teamMemberId,
        DateTimeOffset? fromDate,
        ISender sender,
        CancellationToken cancellationToken)
    {
        _ = teamId;

        var result = await sender.Send(new GetTeamMemberDetailsByIdQuery(teamMemberId, fromDate), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> ListTeamMemberTasksInTeamAsync(
        Guid teamId,
        Guid teamMemberId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        _ = teamId;

        var result = await sender.Send(new GetTeamMemberTasksByIdQuery(teamMemberId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> CreateTeamMemberInTeamAsync(
        Guid teamId,
        CreateTeamMemberInTeamRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateTeamMemberCommand(teamId, request.OrganizationMemberId),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> CreateTeamMemberWithUserInTeamAsync(
        Guid teamId,
        CreateTeamMemberWithUserRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateTeamMemberWithUserCommand(
                teamId,
                request.Email,
                request.FirstName,
                request.LastName,
                request.RoleId,
                request.JobTitle,
                request.Phone,
                request.AvatarUrl,
                request.AuthProviderId),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> UpdateTeamMemberInTeamAsync(
        Guid teamId,
        Guid teamMemberId,
        UpdateTeamMemberRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        _ = teamId;

        var result = await sender.Send(new UpdateTeamMemberCommand(teamMemberId, request.Status), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> DeleteTeamMemberInTeamAsync(
        Guid teamId,
        Guid teamMemberId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        _ = teamId;

        var result = await sender.Send(new DeleteTeamMemberCommand(teamMemberId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> ListTeamDayOffsInTeamAsync(
        Guid teamId,
        DateTimeOffset fromAt,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetTeamDayOffsQuery(teamId, fromAt), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> CreateTeamMemberDayOffInTeamAsync(
        Guid teamId,
        Guid teamMemberId,
        UpsertTeamMemberDayOffRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        _ = teamId;

        var result = await sender.Send(
            new CreateTeamMemberDayOffCommand(teamMemberId, request.StartAt, request.EndAt, request.Reason),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> GetTeamMemberDayOffInTeamByIdAsync(
        Guid teamId,
        Guid teamMemberId,
        Guid dayOffId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        _ = teamId;
        _ = teamMemberId;

        var result = await sender.Send(new GetTeamMemberDayOffByIdQuery(dayOffId, teamMemberId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> UpdateTeamMemberDayOffInTeamAsync(
        Guid teamId,
        Guid teamMemberId,
        Guid dayOffId,
        UpdateTeamMemberDayOffRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        _ = teamId;
        _ = teamMemberId;

        var result = await sender.Send(
            new UpdateTeamMemberDayOffCommand(dayOffId, teamMemberId, request.StartAt, request.EndAt, request.Reason, request.Status),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> DeleteTeamMemberDayOffInTeamAsync(
        Guid teamId,
        Guid teamMemberId,
        Guid dayOffId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        _ = teamId;
        _ = teamMemberId;

        var result = await sender.Send(new DeleteTeamMemberDayOffCommand(dayOffId, teamMemberId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> ListTeamTasksAsync(Guid teamId, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetTasksQuery(OrganizationId: null, TeamId: teamId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> ListTeamTimeEntriesAsync(Guid teamId, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetTimeEntriesByTeamIdQuery(teamId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> GetTeamMembersWorkSummaryAsync(
        Guid teamMemberId,
        DateOnly fromDate,
        DateOnly toDate,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetTeamMembersWorkSummaryQuery(teamMemberId, fromDate, toDate),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> GetTeamMemberByIdAsync(Guid teamMemberId, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetTeamMemberByIdQuery(teamMemberId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> GetTeamMemberByIdDetailsAsync(
        Guid teamMemberId,
        DateTimeOffset? fromDate,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetTeamMemberDetailsByIdQuery(teamMemberId, fromDate), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> ListTeamMemberTasksAsync(
        Guid teamMemberId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetTeamMemberTasksByIdQuery(teamMemberId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> CreateTeamMemberAsync(CreateTeamMemberRequest request, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateTeamMemberCommand(request.TeamId, request.OrganizationMemberId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> UpdateTeamMemberAsync(
        Guid teamMemberId,
        UpdateTeamMemberRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateTeamMemberCommand(teamMemberId, request.Status), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> DeleteTeamMemberAsync(Guid teamMemberId, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteTeamMemberCommand(teamMemberId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> ListTeamMemberDayOffsAsync(
        Guid teamMemberId,
        DateTimeOffset fromAt,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetTeamMemberDayOffsQuery(teamMemberId, fromAt), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> CreateTeamMemberDayOffAsync(
        Guid teamMemberId,
        UpsertTeamMemberDayOffRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateTeamMemberDayOffCommand(teamMemberId, request.StartAt, request.EndAt, request.Reason),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> GetTeamMemberDayOffByIdAsync(
        Guid teamMemberId,
        Guid dayOffId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        _ = teamMemberId;

        var result = await sender.Send(new GetTeamMemberDayOffByIdQuery(dayOffId, teamMemberId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> UpdateTeamMemberDayOffAsync(
        Guid teamMemberId,
        Guid dayOffId,
        UpdateTeamMemberDayOffRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        _ = teamMemberId;

        var result = await sender.Send(
            new UpdateTeamMemberDayOffCommand(dayOffId, teamMemberId, request.StartAt, request.EndAt, request.Reason, request.Status),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> DeleteTeamMemberDayOffAsync(
        Guid teamMemberId,
        Guid dayOffId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        _ = teamMemberId;

        var result = await sender.Send(new DeleteTeamMemberDayOffCommand(dayOffId, teamMemberId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private sealed record CreateTeamInOrganizationRequest(string Name, Guid? ManagerMemberId, Guid? LocationId = null);
    private sealed record CreateTeamMemberInTeamRequest(Guid OrganizationMemberId);
    private sealed record UpsertTeamMemberDayOffRequest(DateTimeOffset StartAt, DateTimeOffset EndAt, string? Reason);
    private sealed record UpdateTeamMemberDayOffRequest(DateTimeOffset? StartAt, DateTimeOffset? EndAt, string? Reason, string? Status);
}
