using MediatR;
using Pipexi.Application.Features.OrganizationMemberProfiles.Commands.UpsertOrganizationMemberProfile;
using Pipexi.Application.Features.OrganizationMemberProfiles.Queries.GetOrganizationMemberProfile;
using Pipexi.Contracts.V1.OrganizationMemberProfiles;

namespace Pipexi.Api.Endpoints.V1;

public static class OrganizationMemberProfileEndpoints
{
    public static IEndpointRouteBuilder MapOrganizationMemberProfileEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/v1/organizations/{organizationId:guid}/organization-members/{organizationMemberId:guid}/profile")
            .WithTags("organization-member-profiles")
            .RequireAuthorization();

        group.MapGet("/", GetAsync);
        group.MapPut("/", UpsertAsync);

        return app;
    }

    private static async Task<IResult> GetAsync(
        Guid organizationId,
        Guid organizationMemberId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetOrganizationMemberProfileQuery(organizationMemberId, organizationId),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> UpsertAsync(
        Guid organizationId,
        Guid organizationMemberId,
        UpsertOrganizationMemberProfileRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpsertOrganizationMemberProfileCommand(
                organizationMemberId,
                request.DateOfBirth,
                request.Gender,
                request.AddressLine1,
                request.AddressLine2,
                request.City,
                request.State,
                request.PostalCode,
                request.Country,
                request.EmergencyContactName,
                request.EmergencyContactPhone,
                request.NationalId,
                organizationId),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }
}
