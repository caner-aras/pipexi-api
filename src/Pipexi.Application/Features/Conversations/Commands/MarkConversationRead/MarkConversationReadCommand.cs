using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Conversations.Commands.MarkConversationRead;

public sealed record MarkConversationReadCommand(Guid ConversationId)
    : ICommand<Result<bool>>
{
    public sealed class Handler(
        IConversationRepository conversationRepository,
        IConversationMemberRepository conversationMemberRepository,
        IOrganizationMemberRepository organizationMemberRepository,
        ICurrentUserContext currentUserContext)
        : IRequestHandler<MarkConversationReadCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(
            MarkConversationReadCommand request,
            CancellationToken cancellationToken)
        {
            if (currentUserContext.UserId == Guid.Empty || currentUserContext.OrganizationId == Guid.Empty)
            {
                return Result<bool>.Failure(
                    new AppError("auth.unauthorized", "Authenticated organization membership is required."),
                    (int)HttpStatusCode.Unauthorized);
            }

            var currentMember = await organizationMemberRepository.GetByOrganizationIdAndUserIdAsync(
                currentUserContext.OrganizationId,
                currentUserContext.UserId,
                cancellationToken);

            if (currentMember is null)
            {
                return Result<bool>.Failure(
                    new AppError("conversations.member_not_found", "Organization membership not found."),
                    (int)HttpStatusCode.Forbidden);
            }

            var conversation = await conversationRepository.GetByIdAsync(request.ConversationId, cancellationToken);
            if (conversation is null || conversation.OrganizationId != currentUserContext.OrganizationId)
            {
                return Result<bool>.Failure(
                    new AppError("conversations.not_found", "Conversation not found."),
                    (int)HttpStatusCode.NotFound);
            }

            var membership = await conversationMemberRepository.GetByConversationAndMemberAsync(
                conversation.Id,
                currentMember.Id,
                cancellationToken);

            if (membership is null)
            {
                return Result<bool>.Failure(
                    new AppError("conversations.forbidden", "You are not a member of this conversation."),
                    (int)HttpStatusCode.Forbidden);
            }

            membership.MarkRead(DateTimeOffset.UtcNow);
            await conversationMemberRepository.UpdateAsync(membership, cancellationToken);

            return Result<bool>.Success(true, (int)HttpStatusCode.OK);
        }
    }
}
