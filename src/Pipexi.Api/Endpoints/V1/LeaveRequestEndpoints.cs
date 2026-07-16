using MediatR;
using Workforce.Application.Features.LeaveRequests.Commands.CreateLeaveRequest;
using Workforce.Application.Features.LeaveRequests.Commands.DeleteLeaveRequest;
using Workforce.Application.Features.LeaveRequests.Commands.UpdateLeaveRequest;
using Workforce.Application.Features.LeaveRequests.Queries.GetLeaveRequestById;
using Workforce.Application.Features.LeaveRequests.Queries.GetLeaveRequests;
using Workforce.Contracts.V1.LeaveRequests;

namespace Workforce.Api.Endpoints.V1;

public static class LeaveRequestEndpoints
{
    public static IEndpointRouteBuilder MapLeaveRequestEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/leave-requests")
            .WithTags("leave-requests")
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
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetLeaveRequestsQuery(organizationId, organizationMemberId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> GetByIdAsync(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetLeaveRequestByIdQuery(id), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> CreateAsync(
        CreateLeaveRequestRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateLeaveRequestCommand(
                request.OrganizationId,
                request.OrganizationMemberId,
                request.LeaveType,
                request.StartDate,
                request.EndDate,
                request.Reason),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> UpdateAsync(
        Guid id,
        UpdateLeaveRequestRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateLeaveRequestCommand(
                id,
                request.LeaveType,
                request.StartDate,
                request.EndDate,
                request.Reason,
                request.Status),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> DeleteAsync(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteLeaveRequestCommand(id), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }
}
