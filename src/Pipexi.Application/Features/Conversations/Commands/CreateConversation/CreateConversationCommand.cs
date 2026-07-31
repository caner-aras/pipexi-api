using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Conversations.Dtos;
using Pipexi.Domain.Entities;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Conversations.Commands.CreateConversation;

public sealed record CreateConversationCommand(Guid PeerOrganizationMemberId)
    : ICommand<Result<ConversationDto>>
{
    public sealed class Handler(
        IConversationRepository conversationRepository,
        IConversationMemberRepository conversationMemberRepository,
        IConversationMessageRepository conversationMessageRepository,
        IOrganizationMemberRepository organizationMemberRepository,
        IUserRepository userRepository,
        ICurrentUserContext currentUserContext)
        : IRequestHandler<CreateConversationCommand, Result<ConversationDto>>
    {
        public async Task<Result<ConversationDto>> Handle(
            CreateConversationCommand request,
            CancellationToken cancellationToken)
        {
            if (currentUserContext.UserId == Guid.Empty || currentUserContext.OrganizationId == Guid.Empty)
            {
                return Result<ConversationDto>.Failure(
                    new AppError("auth.unauthorized", "Authenticated organization membership is required."),
                    (int)HttpStatusCode.Unauthorized);
            }

            var currentMember = await organizationMemberRepository.GetByOrganizationIdAndUserIdAsync(
                currentUserContext.OrganizationId,
                currentUserContext.UserId,
                cancellationToken);

            if (currentMember is null)
            {
                return Result<ConversationDto>.Failure(
                    new AppError("conversations.member_not_found", "Organization membership not found."),
                    (int)HttpStatusCode.Forbidden);
            }

            if (request.PeerOrganizationMemberId == currentMember.Id)
            {
                return Result<ConversationDto>.Failure(
                    new AppError("conversations.self_dm", "Cannot create a conversation with yourself."),
                    (int)HttpStatusCode.BadRequest);
            }

            var peerMember = await organizationMemberRepository.GetByIdAsync(
                request.PeerOrganizationMemberId,
                cancellationToken);

            if (peerMember is null || peerMember.OrganizationId != currentUserContext.OrganizationId)
            {
                return Result<ConversationDto>.Failure(
                    new AppError("conversations.peer_not_found", "Peer organization member not found in this organization."),
                    (int)HttpStatusCode.NotFound);
            }

            var pairKey = Conversation.BuildDirectMemberPairKey(currentMember.Id, peerMember.Id);
            var existing = await conversationRepository.GetDirectByOrganizationAndPairKeyAsync(
                currentUserContext.OrganizationId,
                pairKey,
                cancellationToken);

            Conversation conversation;
            if (existing is not null)
            {
                conversation = existing;
            }
            else
            {
                conversation = Conversation.CreateDirect(
                    currentUserContext.OrganizationId,
                    currentMember.Id,
                    peerMember.Id);

                await conversationRepository.AddAsync(conversation, cancellationToken);
                await conversationMemberRepository.AddRangeAsync(
                    [
                        ConversationMember.Create(conversation.Id, currentMember.Id),
                        ConversationMember.Create(conversation.Id, peerMember.Id)
                    ],
                    cancellationToken);
            }

            var peerUser = await userRepository.GetByIdAsync(peerMember.UserId, cancellationToken);
            var peerDisplayName = peerUser is null
                ? "Member"
                : $"{peerUser.FirstName} {peerUser.LastName}".Trim();

            var latest = await conversationMessageRepository.GetLatestByConversationIdAsync(
                conversation.Id,
                cancellationToken);

            return Result<ConversationDto>.Success(
                new ConversationDto(
                    conversation.Id,
                    conversation.OrganizationId,
                    conversation.Type,
                    conversation.Title,
                    peerMember.Id,
                    string.IsNullOrWhiteSpace(peerDisplayName) ? (peerUser?.Email ?? "Member") : peerDisplayName,
                    peerUser?.AvatarUrl,
                    latest?.Body,
                    latest?.CreatedAt,
                    conversation.CreatedAt,
                    conversation.UpdatedAt),
                existing is null ? (int)HttpStatusCode.Created : (int)HttpStatusCode.OK);
        }
    }
}
