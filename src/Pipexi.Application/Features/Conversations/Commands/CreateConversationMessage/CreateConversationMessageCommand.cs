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

namespace Pipexi.Application.Features.Conversations.Commands.CreateConversationMessage;

public sealed record CreateConversationMessageCommand(Guid ConversationId, string Body)
    : ICommand<Result<ConversationMessageDto>>
{
    public sealed class Handler(
        IConversationRepository conversationRepository,
        IConversationMemberRepository conversationMemberRepository,
        IConversationMessageRepository conversationMessageRepository,
        IOrganizationMemberRepository organizationMemberRepository,
        IUserRepository userRepository,
        ICurrentUserContext currentUserContext)
        : IRequestHandler<CreateConversationMessageCommand, Result<ConversationMessageDto>>
    {
        public async Task<Result<ConversationMessageDto>> Handle(
            CreateConversationMessageCommand request,
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

            var message = ConversationMessage.Create(
                conversation.Id,
                currentMember.Id,
                request.Body);

            await conversationMessageRepository.AddAsync(message, cancellationToken);

            conversation.MarkActivity();
            await conversationRepository.UpdateAsync(conversation, cancellationToken);

            var senderUser = await userRepository.GetByIdAsync(currentUserContext.UserId, cancellationToken);
            var senderDisplayName = ConversationMappings.BuildMemberDisplayName(senderUser);

            return Result<ConversationMessageDto>.Success(
                message.ToDto(senderDisplayName, isMine: true),
                (int)HttpStatusCode.Created);
        }
    }
}
