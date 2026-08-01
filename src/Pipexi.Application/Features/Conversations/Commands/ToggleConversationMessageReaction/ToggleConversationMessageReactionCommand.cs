using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Conversations;
using Pipexi.Application.Features.Conversations.Dtos;
using Pipexi.Domain.Entities;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Conversations.Commands.ToggleConversationMessageReaction;

public sealed record ToggleConversationMessageReactionCommand(
    Guid ConversationId,
    Guid MessageId,
    string Emoji)
    : ICommand<Result<ConversationMessageDto>>
{
    public sealed class Handler(
        IConversationRepository conversationRepository,
        IConversationMemberRepository conversationMemberRepository,
        IConversationMessageRepository conversationMessageRepository,
        IOrganizationMemberRepository organizationMemberRepository,
        IUserRepository userRepository,
        ICurrentUserContext currentUserContext)
        : IRequestHandler<ToggleConversationMessageReactionCommand, Result<ConversationMessageDto>>
    {
        public async Task<Result<ConversationMessageDto>> Handle(
            ToggleConversationMessageReactionCommand request,
            CancellationToken cancellationToken)
        {
            if (currentUserContext.UserId == Guid.Empty || currentUserContext.OrganizationId == Guid.Empty)
            {
                return Result<ConversationMessageDto>.Failure(
                    new AppError("auth.unauthorized", "Authenticated organization membership is required."),
                    (int)HttpStatusCode.Unauthorized);
            }

            if (!ConversationMessage.IsAllowedEmoji(request.Emoji))
            {
                return Result<ConversationMessageDto>.Failure(
                    new AppError("conversations.invalid_reaction", "Unsupported reaction emoji."),
                    (int)HttpStatusCode.BadRequest);
            }

            var currentMember = await organizationMemberRepository.GetByOrganizationIdAndUserIdAsync(
                currentUserContext.OrganizationId,
                currentUserContext.UserId,
                cancellationToken);

            if (currentMember is null)
            {
                return Result<ConversationMessageDto>.Failure(
                    new AppError("conversations.member_not_found", "Organization membership not found."),
                    (int)HttpStatusCode.Forbidden);
            }

            var conversation = await conversationRepository.GetByIdAsync(request.ConversationId, cancellationToken);
            if (conversation is null || conversation.OrganizationId != currentUserContext.OrganizationId)
            {
                return Result<ConversationMessageDto>.Failure(
                    new AppError("conversations.not_found", "Conversation not found."),
                    (int)HttpStatusCode.NotFound);
            }

            var isMember = await conversationMemberRepository.IsMemberAsync(
                conversation.Id,
                currentMember.Id,
                cancellationToken);

            if (!isMember)
            {
                return Result<ConversationMessageDto>.Failure(
                    new AppError("conversations.forbidden", "You are not a member of this conversation."),
                    (int)HttpStatusCode.Forbidden);
            }

            var message = await conversationMessageRepository.GetByIdAsync(request.MessageId, cancellationToken);
            if (message is null || message.ConversationId != conversation.Id)
            {
                return Result<ConversationMessageDto>.Failure(
                    new AppError("conversations.message_not_found", "Message not found."),
                    (int)HttpStatusCode.NotFound);
            }

            if (message.IsDeleted)
            {
                return Result<ConversationMessageDto>.Failure(
                    new AppError("conversations.message_deleted", "Cannot react to a deleted message."),
                    (int)HttpStatusCode.Conflict);
            }

            if (!message.ToggleReaction(currentMember.Id, request.Emoji))
            {
                return Result<ConversationMessageDto>.Failure(
                    new AppError("conversations.invalid_reaction", "Could not update reaction."),
                    (int)HttpStatusCode.BadRequest);
            }

            await conversationMessageRepository.UpdateAsync(message, cancellationToken);

            var members = await conversationMemberRepository.ListByConversationIdAsync(
                conversation.Id,
                cancellationToken);
            var canDelete = message.SenderOrganizationMemberId == currentMember.Id
                && ConversationMappings.IsMessageUnreadByPeers(
                    message.CreatedAt,
                    message.SenderOrganizationMemberId,
                    members);

            string senderDisplayName;
            if (message.SenderOrganizationMemberId == currentMember.Id)
            {
                var currentUser = await userRepository.GetByIdAsync(currentUserContext.UserId, cancellationToken);
                senderDisplayName = ConversationMappings.BuildMemberDisplayName(currentUser);
            }
            else
            {
                var senderMember = await organizationMemberRepository.GetByIdAsync(
                    message.SenderOrganizationMemberId,
                    cancellationToken);
                User? senderUser = null;
                if (senderMember is not null)
                {
                    senderUser = await userRepository.GetByIdAsync(senderMember.UserId, cancellationToken);
                }

                senderDisplayName = ConversationMappings.BuildMemberDisplayName(senderUser);
            }

            return Result<ConversationMessageDto>.Success(
                message.ToDto(senderDisplayName, currentMember.Id, canDelete),
                (int)HttpStatusCode.OK);
        }
    }
}
