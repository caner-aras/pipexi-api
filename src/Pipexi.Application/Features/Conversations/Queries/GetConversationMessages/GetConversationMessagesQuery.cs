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

namespace Pipexi.Application.Features.Conversations.Queries.GetConversationMessages;

public sealed record GetConversationMessagesQuery(
    Guid ConversationId,
    int PageNumber = 1,
    int PageSize = 50)
    : IQuery<Result<PagedConversationMessagesDto>>
{
    public sealed class Handler(
        IConversationRepository conversationRepository,
        IConversationMemberRepository conversationMemberRepository,
        IConversationMessageRepository conversationMessageRepository,
        IOrganizationMemberRepository organizationMemberRepository,
        IUserRepository userRepository,
        ICurrentUserContext currentUserContext)
        : IRequestHandler<GetConversationMessagesQuery, Result<PagedConversationMessagesDto>>
    {
        public async Task<Result<PagedConversationMessagesDto>> Handle(
            GetConversationMessagesQuery request,
            CancellationToken cancellationToken)
        {
            if (currentUserContext.UserId == Guid.Empty || currentUserContext.OrganizationId == Guid.Empty)
            {
                return Result<PagedConversationMessagesDto>.Failure(
                    new AppError("auth.unauthorized", "Authenticated organization membership is required."),
                    (int)HttpStatusCode.Unauthorized);
            }

            var currentMember = await organizationMemberRepository.GetByOrganizationIdAndUserIdAsync(
                currentUserContext.OrganizationId,
                currentUserContext.UserId,
                cancellationToken);

            if (currentMember is null)
            {
                return Result<PagedConversationMessagesDto>.Failure(
                    new AppError("conversations.member_not_found", "Organization membership not found."),
                    (int)HttpStatusCode.Forbidden);
            }

            var conversation = await conversationRepository.GetByIdAsync(request.ConversationId, cancellationToken);
            if (conversation is null || conversation.OrganizationId != currentUserContext.OrganizationId)
            {
                return Result<PagedConversationMessagesDto>.Failure(
                    new AppError("conversations.not_found", "Conversation not found."),
                    (int)HttpStatusCode.NotFound);
            }

            var isMember = await conversationMemberRepository.IsMemberAsync(
                conversation.Id,
                currentMember.Id,
                cancellationToken);

            if (!isMember)
            {
                return Result<PagedConversationMessagesDto>.Failure(
                    new AppError("conversations.forbidden", "You are not a member of this conversation."),
                    (int)HttpStatusCode.Forbidden);
            }

            var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
            var pageSize = request.PageSize is < 1 or > 100 ? 50 : request.PageSize;

            var (items, totalCount) = await conversationMessageRepository.ListByConversationIdPagedAsync(
                conversation.Id,
                pageNumber,
                pageSize,
                cancellationToken);

            var membership = await conversationMemberRepository.GetByConversationAndMemberAsync(
                conversation.Id,
                currentMember.Id,
                cancellationToken);

            if (membership is not null)
            {
                membership.MarkRead(DateTimeOffset.UtcNow);
                await conversationMemberRepository.UpdateAsync(membership, cancellationToken);
            }

            DateTimeOffset? peerLastReadAt = null;
            var conversationMembers = await conversationMemberRepository.ListByConversationIdAsync(
                conversation.Id,
                cancellationToken);

            if (conversation.Type == Conversation.TypeDirect)
            {
                var peerMembership = conversationMembers.FirstOrDefault(x => x.OrganizationMemberId != currentMember.Id);
                peerLastReadAt = peerMembership?.LastReadAt;
            }

            var senderMemberIds = items
                .Select(x => x.SenderOrganizationMemberId)
                .Distinct()
                .ToList();

            var senderMembers = senderMemberIds.Count == 0
                ? Array.Empty<OrganizationMember>()
                : await organizationMemberRepository.GetByIdsAsync(senderMemberIds, cancellationToken);

            var senderMembersById = senderMembers.ToDictionary(x => x.Id);
            var senderUserIds = senderMembers.Select(x => x.UserId).Distinct().ToList();
            var senderUsers = senderUserIds.Count == 0
                ? Array.Empty<User>()
                : await userRepository.ListByIdsAsync(senderUserIds, cancellationToken);
            var senderUsersById = senderUsers.ToDictionary(x => x.Id);
            var currentUser = await userRepository.GetByIdAsync(currentUserContext.UserId, cancellationToken);

            var dtos = items
                .OrderBy(x => x.CreatedAt)
                .Select(message =>
                {
                    var isMine = message.SenderOrganizationMemberId == currentMember.Id;
                    User? senderUser = null;

                    if (isMine)
                    {
                        senderUser = currentUser;
                    }
                    else if (senderMembersById.TryGetValue(message.SenderOrganizationMemberId, out var senderMember))
                    {
                        senderUsersById.TryGetValue(senderMember.UserId, out senderUser);
                    }

                    var canDelete = isMine
                        && !message.IsDeleted
                        && ConversationMappings.IsMessageUnreadByPeers(
                            message.CreatedAt,
                            message.SenderOrganizationMemberId,
                            conversationMembers);
                    var canEdit = isMine && !message.IsDeleted;

                    return message.ToDto(
                        ConversationMappings.BuildMemberDisplayName(senderUser),
                        currentMember.Id,
                        canDelete,
                        canEdit);
                })
                .ToList();

            return Result<PagedConversationMessagesDto>.Success(
                new PagedConversationMessagesDto(
                    dtos,
                    pageNumber,
                    pageSize,
                    totalCount,
                    conversation.Type,
                    peerLastReadAt,
                    currentMember.Id),
                (int)HttpStatusCode.OK);
        }
    }
}

public sealed record PagedConversationMessagesDto(
    IReadOnlyCollection<ConversationMessageDto> Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    string ConversationType,
    DateTimeOffset? PeerLastReadAt,
    Guid CurrentOrganizationMemberId);
