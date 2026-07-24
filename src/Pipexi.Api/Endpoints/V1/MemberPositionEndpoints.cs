using MediatR;
using Pipexi.Application.Features.MemberPositions.Commands.AssignMemberPosition;
using Pipexi.Application.Features.MemberPositions.Queries.GetActiveMemberPosition;
using Pipexi.Application.Features.MemberPositions.Queries.GetMemberPositionHistory;
using Pipexi.Contracts.V1.MemberPositions;

namespace Pipexi.Api.Endpoints.V1;

public static class MemberPositionEndpoints
{
    public static IEndpointRouteBuilder MapMemberPositionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/member-positions")
            .WithTags("member-positions")
            .RequireAuthorization();

        group.MapPost("/assign", AssignAsync);
        group.MapGet("/active/{organizationMemberId:guid}", GetActiveAsync);
        group.MapGet("/history/{organizationMemberId:guid}", GetHistoryAsync);

        return app;
    }

    private static async Task<IResult> AssignAsync(
        AssignMemberPositionRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new AssignMemberPositionCommand(
                request.OrganizationMemberId,
                request.PositionId,
                request.HourlyRate,
                request.StartDate),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> GetActiveAsync(
        Guid organizationMemberId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetActiveMemberPositionQuery(organizationMemberId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> GetHistoryAsync(
        Guid organizationMemberId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetMemberPositionHistoryQuery(organizationMemberId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }
}
