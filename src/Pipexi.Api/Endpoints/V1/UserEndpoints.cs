using MediatR;
using Pipexi.Application.Features.Users.Commands.CreateUser;
using Pipexi.Application.Features.Users.Commands.DeleteUser;
using Pipexi.Application.Features.Users.Commands.UpdateUser;
using Pipexi.Application.Features.Users.Queries.GetUserById;
using Pipexi.Application.Features.Users.Queries.GetUsers;
using Pipexi.Contracts.V1.Users;

namespace Pipexi.Api.Endpoints.V1;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/users")
            .WithTags("users")
            .RequireAuthorization();

        group.MapGet("/", ListAsync);
        group.MapGet("/{id:guid}", GetByIdAsync);
        group.MapPost("/", CreateAsync);
        group.MapPut("/{id:guid}", UpdateAsync);
        group.MapDelete("/{id:guid}", DeleteAsync);

        return app;
    }

    private static async Task<IResult> ListAsync(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetUsersQuery(), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> GetByIdAsync(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetUserByIdQuery(id), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> CreateAsync(CreateUserRequest request, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateUserCommand(
                request.AuthProviderId,
                request.Email,
                request.FirstName,
                request.LastName,
                request.Phone,
                request.AvatarUrl),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> UpdateAsync(Guid id, UpdateUserRequest request, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateUserCommand(id, request.FirstName, request.LastName, request.Phone, request.AvatarUrl),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> DeleteAsync(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteUserCommand(id), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }
}
