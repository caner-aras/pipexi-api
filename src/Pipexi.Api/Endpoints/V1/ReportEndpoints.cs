using MediatR;
using Pipexi.Application.Features.Reports.Queries.GetReportSummary;
using Pipexi.Application.Features.Reports.Queries.GetShiftFormsStatus;

namespace Pipexi.Api.Endpoints.V1;

public static class ReportEndpoints
{
    public static IEndpointRouteBuilder MapReportEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/report")
            .WithTags("report")
            .RequireAuthorization();

        group.MapGet("/summary", GetSummaryAsync);
        group.MapGet("/shift-forms", GetShiftFormsStatusAsync);
        group.MapGet("/shift-report", GetShiftReportDataAsync);
        group.MapGet("/shift-report/pdf", GetShiftReportPdfAsync);

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

    private static async Task<IResult> GetShiftFormsStatusAsync(
        Guid organizationId,
        int? trendDays,
        int? futureDays,
        ISender sender,
        CancellationToken cancellationToken)
    {
        int queryTrendDays = trendDays ?? 30;
        int queryFutureDays = futureDays ?? 7;

        var result = await sender.Send(
            new GetShiftFormsStatusQuery(organizationId, queryTrendDays, queryFutureDays),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> GetShiftReportPdfAsync(
        Guid organizationId,
        DateTime fromDate,
        DateTime toDate,
        [Microsoft.AspNetCore.Mvc.FromQuery] Guid[]? memberId,
        [Microsoft.AspNetCore.Mvc.FromQuery] bool? includeSummary,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new Pipexi.Application.Features.Reports.Queries.GetShiftReportPdf.GetShiftReportPdfQuery(
                organizationId, fromDate, toDate, memberId, includeSummary ?? false),
            cancellationToken);

        if (result.IsSuccess)
        {
            return Results.File(result.Data, "application/pdf", "ShiftReport.pdf");
        }

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> GetShiftReportDataAsync(
        Guid organizationId,
        DateTime fromDate,
        DateTime toDate,
        [Microsoft.AspNetCore.Mvc.FromQuery] Guid[]? memberId,
        [Microsoft.AspNetCore.Mvc.FromQuery] bool? includeSummary,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new Pipexi.Application.Features.Reports.Queries.GetShiftReportData.GetShiftReportDataQuery(
                organizationId, fromDate, toDate, memberId, includeSummary ?? false),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }
}