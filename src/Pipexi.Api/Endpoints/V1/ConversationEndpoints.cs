using MediatR;
using Pipexi.Application.Features.Conversations.Commands.CreateConversation;
using Pipexi.Application.Features.Conversations.Commands.CreateConversationMessage;
using Pipexi.Application.Features.Conversations.Commands.MarkConversationRead;
using Pipexi.Application.Features.Conversations.Queries.GetConversationMessages;
using Pipexi.Application.Features.Conversations.Queries.GetConversations;
using Pipexi.Application.Features.Conversations.Queries.GetConversationUnreadCount;
using Pipexi.Contracts.V1.Conversations;

namespace Pipexi.Api.Endpoints.V1;

public static class ConversationEndpoints
{
    public static IEndpointRouteBuilder MapConversationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/conversations")
            .WithTags("conversations")
            .RequireAuthorization();

        group.MapGet("/", ListAsync);
        group.MapGet("/unread-count", UnreadCountAsync);
        group.MapPost("/", CreateAsync);
        group.MapGet("/{id:guid}/messages", ListMessagesAsync);
        group.MapPost("/{id:guid}/messages", CreateMessageAsync);
        group.MapPost("/{id:guid}/read", MarkReadAsync);

        return app;
    }

    private static async Task<IResult> ListAsync(
        Guid? organizationId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetConversationsQuery(organizationId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> UnreadCountAsync(
        Guid? organizationId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetConversationUnreadCountQuery(organizationId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> CreateAsync(
        CreateConversationRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateConversationCommand(
                request.Type,
                request.OrganizationMemberId,
                request.Title,
                request.OrganizationMemberIds),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> ListMessagesAsync(
        Guid id,
        int? pageNumber,
        int? pageSize,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetConversationMessagesQuery(id, pageNumber ?? 1, pageSize ?? 50),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> CreateMessageAsync(
        Guid id,
        CreateConversationMessageRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateConversationMessageCommand(id, request.Body),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> MarkReadAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new MarkConversationReadCommand(id), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }
}
