using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Conversations;
using Pipexi.Application.Features.Conversations.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Conversations.Commands.DeleteConversationMessage;

public sealed record DeleteConversationMessageCommand(Guid ConversationId, Guid MessageId)
    : ICommand<Result<ConversationMessageDto>>
{
    public sealed class Handler(
        IConversationRepository conversationRepository,
        IConversationMemberRepository conversationMemberRepository,
        IConversationMessageRepository conversationMessageRepository,
        IOrganizationMemberRepository organizationMemberRepository,
        IUserRepository userRepository,
        ICurrentUserContext currentUserContext)
        : IRequestHandler<DeleteConversationMessageCommand, Result<ConversationMessageDto>>
    {
        public async Task<Result<ConversationMessageDto>> Handle(
            DeleteConversationMessageCommand request,
            CancellationToken cancellationToken)
        {
            if (currentUserContext.UserId == Guid.Empty || currentUserContext.OrganizationId == Guid.Empty)
            {
                return Result<ConversationMessageDto>.Failure(
                    new AppError("auth.unauthorized", "Authenticated organization membership is required."),
                    (int)HttpStatusCode.Unauthorized);
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

            if (message.SenderOrganizationMemberId != currentMember.Id)
            {
                return Result<ConversationMessageDto>.Failure(
                    new AppError("conversations.message_forbidden", "You can only delete your own messages."),
                    (int)HttpStatusCode.Forbidden);
            }

            var members = await conversationMemberRepository.ListByConversationIdAsync(
                conversation.Id,
                cancellationToken);

            if (!ConversationMappings.IsMessageUnreadByPeers(
                    message.CreatedAt,
                    message.SenderOrganizationMemberId,
                    members))
            {
                return Result<ConversationMessageDto>.Failure(
                    new AppError(
                        "conversations.message_already_read",
                        "This message was already read and cannot be deleted."),
                    (int)HttpStatusCode.Conflict);
            }

            message.SoftDeleteContent();
            await conversationMessageRepository.UpdateAsync(message, cancellationToken);

            conversation.MarkActivity();
            await conversationRepository.UpdateAsync(conversation, cancellationToken);

            var senderUser = await userRepository.GetByIdAsync(currentUserContext.UserId, cancellationToken);

            return Result<ConversationMessageDto>.Success(
                message.ToDto(
                    ConversationMappings.BuildMemberDisplayName(senderUser),
                    currentMember.Id,
                    canDelete: false,
                    canEdit: false),
                (int)HttpStatusCode.OK);
        }
    }
}
