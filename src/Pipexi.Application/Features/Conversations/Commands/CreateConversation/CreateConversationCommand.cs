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

public sealed record CreateConversationCommand(
    string? Type,
    Guid? PeerOrganizationMemberId,
    string? Title,
    IReadOnlyCollection<Guid>? OrganizationMemberIds)
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

            var type = string.IsNullOrWhiteSpace(request.Type)
                ? Conversation.TypeDirect
                : request.Type.Trim().ToLowerInvariant();

            return type switch
            {
                Conversation.TypeDirect => await CreateDirectAsync(
                    request,
                    currentMember,
                    cancellationToken),
                Conversation.TypeGroup => await CreateGroupAsync(
                    request,
                    currentMember,
                    cancellationToken),
                _ => Result<ConversationDto>.Failure(
                    new AppError("conversations.invalid_type", "Conversation type must be direct or group."),
                    (int)HttpStatusCode.BadRequest)
            };
        }

        private async Task<Result<ConversationDto>> CreateDirectAsync(
            CreateConversationCommand request,
            OrganizationMember currentMember,
            CancellationToken cancellationToken)
        {
            if (!request.PeerOrganizationMemberId.HasValue || request.PeerOrganizationMemberId.Value == Guid.Empty)
            {
                return Result<ConversationDto>.Failure(
                    new AppError("conversations.peer_required", "Peer organization member is required for direct chat."),
                    (int)HttpStatusCode.BadRequest);
            }

            var peerId = request.PeerOrganizationMemberId.Value;
            if (peerId == currentMember.Id)
            {
                return Result<ConversationDto>.Failure(
                    new AppError("conversations.self_dm", "Cannot create a conversation with yourself."),
                    (int)HttpStatusCode.BadRequest);
            }

            var peerMember = await organizationMemberRepository.GetByIdAsync(peerId, cancellationToken);
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
            var created = false;
            if (existing is not null)
            {
                conversation = existing;
                var membership = await conversationMemberRepository.GetByConversationAndMemberAsync(
                    conversation.Id,
                    currentMember.Id,
                    cancellationToken);
                if (membership is null)
                {
                    await conversationMemberRepository.AddAsync(
                        ConversationMember.Create(conversation.Id, currentMember.Id),
                        cancellationToken);
                }
                else if (!membership.IsActive)
                {
                    membership.Reactivate();
                    await conversationMemberRepository.UpdateAsync(membership, cancellationToken);
                }
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
                created = true;
            }

            var dto = await BuildDtoAsync(
                conversation,
                currentMember.Id,
                cancellationToken);

            return Result<ConversationDto>.Success(
                dto,
                created ? (int)HttpStatusCode.Created : (int)HttpStatusCode.OK);
        }

        private async Task<Result<ConversationDto>> CreateGroupAsync(
            CreateConversationCommand request,
            OrganizationMember currentMember,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return Result<ConversationDto>.Failure(
                    new AppError("conversations.title_required", "Group title is required."),
                    (int)HttpStatusCode.BadRequest);
            }

            var peerIds = (request.OrganizationMemberIds ?? Array.Empty<Guid>())
                .Where(x => x != Guid.Empty && x != currentMember.Id)
                .Distinct()
                .ToList();

            if (peerIds.Count < 2)
            {
                return Result<ConversationDto>.Failure(
                    new AppError(
                        "conversations.group_members_required",
                        "Group chat requires at least two other members."),
                    (int)HttpStatusCode.BadRequest);
            }

            var peerMembers = await organizationMemberRepository.GetByIdsAsync(peerIds, cancellationToken);
            if (peerMembers.Count != peerIds.Count
                || peerMembers.Any(x => x.OrganizationId != currentUserContext.OrganizationId))
            {
                return Result<ConversationDto>.Failure(
                    new AppError(
                        "conversations.group_members_invalid",
                        "One or more group members were not found in this organization."),
                    (int)HttpStatusCode.BadRequest);
            }

            var conversation = Conversation.CreateGroup(
                currentUserContext.OrganizationId,
                request.Title);

            await conversationRepository.AddAsync(conversation, cancellationToken);

            var memberships = new List<ConversationMember>
            {
                ConversationMember.Create(conversation.Id, currentMember.Id)
            };
            memberships.AddRange(peerIds.Select(id => ConversationMember.Create(conversation.Id, id)));

            await conversationMemberRepository.AddRangeAsync(memberships, cancellationToken);

            var dto = await BuildDtoAsync(conversation, currentMember.Id, cancellationToken);
            return Result<ConversationDto>.Success(dto, (int)HttpStatusCode.Created);
        }

        private async Task<ConversationDto> BuildDtoAsync(
            Conversation conversation,
            Guid currentOrganizationMemberId,
            CancellationToken cancellationToken)
        {
            var members = await conversationMemberRepository.ListByConversationIdAsync(
                conversation.Id,
                cancellationToken);
            var activeMembers = members.Where(x => x.IsActive).ToList();

            var myMembership = activeMembers.FirstOrDefault(x => x.OrganizationMemberId == currentOrganizationMemberId);
            var latest = await conversationMessageRepository.GetLatestByConversationIdAsync(
                conversation.Id,
                myMembership?.ClearedAt,
                cancellationToken);

            Guid? peerOrganizationMemberId = null;
            string displayName;
            string? peerAvatarUrl = null;

            if (conversation.Type == Conversation.TypeGroup)
            {
                displayName = conversation.Title?.Trim() ?? "Group";
            }
            else
            {
                var peerMembership = activeMembers.FirstOrDefault(x => x.OrganizationMemberId != currentOrganizationMemberId);
                peerOrganizationMemberId = peerMembership?.OrganizationMemberId;
                displayName = "Member";

                if (peerMembership is not null)
                {
                    var peerMember = await organizationMemberRepository.GetByIdAsync(
                        peerMembership.OrganizationMemberId,
                        cancellationToken);
                    if (peerMember is not null)
                    {
                        var peerUser = await userRepository.GetByIdAsync(peerMember.UserId, cancellationToken);
                        if (peerUser is not null)
                        {
                            displayName = $"{peerUser.FirstName} {peerUser.LastName}".Trim();
                            if (string.IsNullOrWhiteSpace(displayName))
                            {
                                displayName = peerUser.Email;
                            }

                            peerAvatarUrl = peerUser.AvatarUrl;
                        }
                    }
                }
            }

            var readAfter = myMembership?.LastReadAt ?? myMembership?.CreatedAt ?? conversation.CreatedAt;
            var unreadCount = await conversationMessageRepository.CountUnreadAsync(
                conversation.Id,
                currentOrganizationMemberId,
                readAfter,
                cancellationToken);

            return new ConversationDto(
                conversation.Id,
                conversation.OrganizationId,
                conversation.Type,
                conversation.Title,
                peerOrganizationMemberId,
                displayName,
                peerAvatarUrl,
                latest is null || latest.IsDeleted ? null : latest.Body,
                latest?.CreatedAt,
                unreadCount,
                activeMembers.Count,
                conversation.CreatedAt,
                conversation.UpdatedAt);
        }
    }
}
