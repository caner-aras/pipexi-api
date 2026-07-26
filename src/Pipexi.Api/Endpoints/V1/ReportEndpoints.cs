using MediatR;
using Pipexi.Application.Features.Reports.Queries.GetReportSummary;

namespace Pipexi.Api.Endpoints.V1;

public static class ReportEndpoints
{
    public static IEndpointRouteBuilder MapReportEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/report")
            .WithTags("report")
            .RequireAuthorization();

        group.MapGet("/summary", GetSummaryAsync);

        return app;
    }

    private static async Task<IResult> GetSummaryAsync(
        Guid organizationId,
        int? trendDays,
        int? futureDays,
        ISender sender,
        CancellationToken cancellationToken)
    {
        int queryTrendDays = trendDays ?? 0;
        int queryFutureDays = futureDays ?? 14;

        var result = await sender.Send(
            new GetReportSummaryQuery(organizationId, queryTrendDays, queryFutureDays),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }
}