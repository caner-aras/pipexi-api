using MediatR;
using Pipexi.Application.Features.OrganizationMemberPayments.Commands.CreateOrganizationMemberPayment;
using Pipexi.Application.Features.OrganizationMemberPayments.Commands.DeleteOrganizationMemberPayment;
using Pipexi.Application.Features.OrganizationMemberPayments.Commands.UpdateOrganizationMemberPayment;
using Pipexi.Application.Features.OrganizationMemberPayments.Queries.GetOrganizationMemberPaymentById;
using Pipexi.Application.Features.OrganizationMemberPayments.Queries.GetOrganizationMemberPayments;
using Pipexi.Contracts.V1.OrganizationMemberPayments;

namespace Pipexi.Api.Endpoints.V1;

public static class OrganizationMemberPaymentEndpoints
{
    public static IEndpointRouteBuilder MapOrganizationMemberPaymentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/v1/organizations/{organizationId:guid}/organization-members/{organizationMemberId:guid}/payments")
            .WithTags("organization-member-payments")
            .RequireAuthorization();

        group.MapGet("/", ListAsync);
        group.MapPost("/", CreateAsync);
        group.MapGet("/{paymentId:guid}", GetByIdAsync);
        group.MapPut("/{paymentId:guid}", UpdateAsync);
        group.MapDelete("/{paymentId:guid}", DeleteAsync);

        return app;
    }

    private static async Task<IResult> ListAsync(
        Guid organizationId,
        Guid organizationMemberId,
        DateOnly? fromDate,
        DateOnly? toDate,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetOrganizationMemberPaymentsQuery(
                organizationMemberId,
                organizationId,
                fromDate,
                toDate),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> CreateAsync(
        Guid organizationId,
        Guid organizationMemberId,
        CreateOrganizationMemberPaymentRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateOrganizationMemberPaymentCommand(
                organizationMemberId,
                request.Amount,
                request.Currency,
                request.PaidAt,
                request.Method,
                request.Reference,
                request.Notes,
                request.PeriodStart,
                request.PeriodEnd,
                organizationId),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> GetByIdAsync(
        Guid organizationId,
        Guid organizationMemberId,
        Guid paymentId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetOrganizationMemberPaymentByIdQuery(organizationMemberId, paymentId, organizationId),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> UpdateAsync(
        Guid organizationId,
        Guid organizationMemberId,
        Guid paymentId,
        UpdateOrganizationMemberPaymentRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateOrganizationMemberPaymentCommand(
                organizationMemberId,
                paymentId,
                request.Amount,
                request.Currency,
                request.PaidAt,
                request.Method,
                request.Reference,
                request.Notes,
                request.PeriodStart,
                request.PeriodEnd,
                organizationId),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> DeleteAsync(
        Guid organizationId,
        Guid organizationMemberId,
        Guid paymentId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new DeleteOrganizationMemberPaymentCommand(organizationMemberId, paymentId, organizationId),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }
}
