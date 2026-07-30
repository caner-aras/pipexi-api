using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Features.OrganizationMembers.Commands.CreateOrganizationMember;
using Pipexi.Application.Features.OrganizationMembers.Commands.DeleteOrganizationMember;
using Pipexi.Application.Features.OrganizationMembers.Commands.UpdateOrganizationMember;
using Pipexi.Application.Features.OrganizationMembers.Queries.GetOrganizationMemberById;
using Pipexi.Application.Features.OrganizationMembers.Queries.GetOrganizationMembers;
using Pipexi.Application.Features.Shifts.Queries.GetShifts;
using Pipexi.Application.Features.Tasks.Queries.GetTaskCommentsByOrganizationMemberId;
using Pipexi.Application.Features.Tasks.Queries.GetTasks;
using Pipexi.Application.Features.TimeEntries.Queries.GetTimeEntries;
using Pipexi.Contracts.V1.OrganizationMembers;

namespace Pipexi.Api.Endpoints.V1;

public static class OrganizationMemberEndpoints
{
    public static IEndpointRouteBuilder MapOrganizationMemberEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/organization-members")
            .WithTags("organization-members")
            .RequireAuthorization();

        group.MapGet("/", ListAsync);
        group.MapGet("/{id:guid}", GetByIdAsync);
        group.MapPost("/", CreateAsync);
        group.MapPut("/{id:guid}", UpdateAsync);
        group.MapDelete("/{id:guid}", DeleteAsync);
        group.MapGet("/{organizationMemberId:guid}/shifts", ListMemberShiftsAsync);
        group.MapGet("/{organizationMemberId:guid}/time-entries", ListMemberTimeEntriesAsync);
        group.MapGet("/{organizationMemberId:guid}/tasks", ListMemberTasksAsync);
        group.MapGet("/{organizationMemberId:guid}/task-comments", ListMemberTaskCommentsAsync);

        return app;
    }

    private static async Task<IResult> ListAsync(Guid? organizationId, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetOrganizationMembersQuery(organizationId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> GetByIdAsync(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetOrganizationMemberByIdQuery(id), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> CreateAsync(
        CreateOrganizationMemberRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new CreateOrganizationMemberCommand(
            request.OrganizationId,
            request.UserId,
            request.RoleId,
            request.JobTitle);

        var result = await sender.Send(command, cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> UpdateAsync(
        Guid id,
        UpdateOrganizationMemberRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new UpdateOrganizationMemberCommand(id, request.RoleId, request.JobTitle, request.Status);
        var result = await sender.Send(command, cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> DeleteAsync(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteOrganizationMemberCommand(id), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> ListMemberShiftsAsync(
        Guid organizationMemberId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetShiftsQuery(null, organizationMemberId), cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> ListMemberTimeEntriesAsync(
        Guid organizationMemberId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetTimeEntriesQuery(null, organizationMemberId), cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> ListMemberTasksAsync(
        Guid organizationMemberId,
        IOrganizationMemberRepository organizationMemberRepository,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var organizationMember = await organizationMemberRepository.GetByIdAsync(organizationMemberId, cancellationToken);
        if (organizationMember is null)
        {
            return Results.NotFound();
        }

        var result = await sender.Send(
            new GetTasksQuery(OrganizationId: organizationMember.OrganizationId, UserId: organizationMember.UserId),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> ListMemberTaskCommentsAsync(
        Guid organizationMemberId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetTaskCommentsByOrganizationMemberIdQuery(organizationMemberId),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }
}
