using MediatR;
using Workforce.Application.Abstractions.Identity;
using Workforce.Application.Features.Tasks.Commands.CreateTask;
using Workforce.Application.Features.Tasks.Commands.CreateTaskComment;
using Workforce.Application.Features.Tasks.Commands.DeleteTask;
using Workforce.Application.Features.Tasks.Commands.DeleteTaskComment;
using Workforce.Application.Features.Tasks.Commands.UpdateTask;
using Workforce.Application.Features.Tasks.Commands.UpdateTaskComment;
using Workforce.Application.Features.Tasks.Queries.GetTaskById;
using Workforce.Application.Features.Tasks.Queries.GetTaskCommentById;
using Workforce.Application.Features.Tasks.Queries.GetTaskComments;
using Workforce.Application.Features.Tasks.Queries.GetTasks;
using Workforce.Contracts.V1.Tasks;
using Workforce.Shared.Results;

namespace Workforce.Api.Endpoints.V1;

public static class TaskEndpoints
{
    public static IEndpointRouteBuilder MapTaskEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/tasks")
            .WithTags("tasks")
            .RequireAuthorization();

        group.MapGet("/", ListTasksAsync);
        group.MapGet("/me", ListCurrentUserTasksAsync);
        group.MapGet("/{id:guid}", GetTaskByIdAsync);
        group.MapPost("/", CreateTaskAsync);
        group.MapPut("/{id:guid}", UpdateTaskAsync);
        group.MapDelete("/{id:guid}", DeleteTaskAsync);

        group.MapGet("/{taskId:guid}/comments", ListTaskCommentsAsync);
        group.MapGet("/comments/{id:guid}", GetTaskCommentByIdAsync);
        group.MapPost("/comments", CreateTaskCommentAsync);
        group.MapPut("/comments/{id:guid}", UpdateTaskCommentAsync);
        group.MapDelete("/comments/{id:guid}", DeleteTaskCommentAsync);

        return app;
    }

    private static async Task<IResult> ListTasksAsync(
        Guid? organizationId,
        Guid? userId,
        ICurrentUserContext currentUserContext,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var effectiveUserId = userId ?? currentUserContext.UserId;
        var result = await sender.Send(new GetTasksQuery(OrganizationId: organizationId, UserId: effectiveUserId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> ListCurrentUserTasksAsync(
        Guid? organizationId,
        ISender sender,
        ICurrentUserContext currentUserContext,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetTasksQuery(OrganizationId: organizationId, UserId: currentUserContext.UserId),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> GetTaskByIdAsync(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetTaskByIdQuery(id), cancellationToken);

        if (!result.IsSuccess)
        {
            var failed = Result<IReadOnlyCollection<Workforce.Application.Features.Tasks.Dtos.TaskDto>>.Failure(
                result.Error!,
                result.StatusCode);

            return Results.Json(failed, statusCode: failed.StatusCode);
        }

        var successful = Result<IReadOnlyCollection<Workforce.Application.Features.Tasks.Dtos.TaskDto>>.Success(
            result.Data is null ? [] : [result.Data],
            result.StatusCode);

        return Results.Json(successful, statusCode: successful.StatusCode);
    }

    private static async Task<IResult> CreateTaskAsync(CreateTaskRequest request, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateTaskCommand(
                request.OrganizationId,
                request.ShiftId,
                request.LocationId,
                request.Title,
                request.Description,
                request.AssignedToTeamMemberId,
                request.AssignedToTeamId,
                request.DueAt,
                request.Priority),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> UpdateTaskAsync(Guid id, UpdateTaskRequest request, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateTaskCommand(
                id,
                request.ShiftId,
                request.LocationId,
                request.Title,
                request.Description,
                request.AssignedToTeamMemberId,
                request.AssignedToTeamId,
                request.DueAt,
                request.Priority,
                request.Status),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> DeleteTaskAsync(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteTaskCommand(id), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> ListTaskCommentsAsync(Guid taskId, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetTaskCommentsQuery(taskId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> GetTaskCommentByIdAsync(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetTaskCommentByIdQuery(id), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> CreateTaskCommentAsync(CreateTaskCommentRequest request, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateTaskCommentCommand(request.WorkTaskId, request.UserId, request.Message),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> UpdateTaskCommentAsync(
        Guid id,
        UpdateTaskCommentRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateTaskCommentCommand(id, request.Message, request.Status), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> DeleteTaskCommentAsync(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteTaskCommentCommand(id), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }
}
