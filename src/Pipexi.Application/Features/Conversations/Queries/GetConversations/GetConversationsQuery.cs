using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Conversations.Dtos;
using Pipexi.Domain.Entities;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Conversations.Queries.GetConversations;

public sealed record GetConversationsQuery(Guid? OrganizationId = null)
    : IQuery<Result<IReadOnlyCollection<ConversationDto>>>
{
    public sealed class Handler(
        IConversationRepository conversationRepository,
        IConversationMemberRepository conversationMemberRepository,
        IConversationMessageRepository conversationMessageRepository,
        IOrganizationMemberRepository organizationMemberRepository,
        IUserRepository userRepository,
        ICurrentUserContext currentUserContext)
        : IRequestHandler<GetConversationsQuery, Result<IReadOnlyCollection<ConversationDto>>>
    {
        public async Task<Result<IReadOnlyCollection<ConversationDto>>> Handle(
            GetConversationsQuery request,
            CancellationToken cancellationToken)
        {
            var organizationId = request.OrganizationId ?? currentUserContext.OrganizationId;
            if (organizationId == Guid.Empty || currentUserContext.UserId == Guid.Empty)
            {
                return Result<IReadOnlyCollection<ConversationDto>>.Failure(
                    new AppError("auth.unauthorized", "Authenticated organization membership is required."),
                    (int)HttpStatusCode.Unauthorized);
            }

            var currentMember = await organizationMemberRepository.GetByOrganizationIdAndUserIdAsync(
                organizationId,
                currentUserContext.UserId,
                cancellationToken);

            if (currentMember is null)
            {
                return Result<IReadOnlyCollection<ConversationDto>>.Failure(
                    new AppError("conversations.member_not_found", "Organization membership not found."),
                    (int)HttpStatusCode.Forbidden);
            }

            var conversations = await conversationRepository.ListByOrganizationMemberIdAsync(
                organizationId,
                currentMember.Id,
                cancellationToken);

            if (conversations.Count == 0)
            {
                return Result<IReadOnlyCollection<ConversationDto>>.Success(
                    Array.Empty<ConversationDto>(),
                    (int)HttpStatusCode.OK);
            }

            var conversationIds = conversations.Select(x => x.Id).ToList();
            var members = await conversationMemberRepository.ListByConversationIdsAsync(
                conversationIds,
                cancellationToken);

            var peerMemberIds = members
                .Where(x => x.OrganizationMemberId != currentMember.Id)
                .Select(x => x.OrganizationMemberId)
                .Distinct()
                .ToList();

            var peerMembers = await organizationMemberRepository.GetByIdsAsync(peerMemberIds, cancellationToken);
            var peerMembersById = peerMembers.ToDictionary(x => x.Id);
            var peerUsers = await userRepository.ListByIdsAsync(
                peerMembers.Select(x => x.UserId).Distinct().ToList(),
                cancellationToken);
            var peerUsersById = peerUsers.ToDictionary(x => x.Id);

            var membersByConversation = members
                .GroupBy(x => x.ConversationId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var dtos = new List<ConversationDto>(conversations.Count);
            foreach (var conversation in conversations)
            {
                if (!membersByConversation.TryGetValue(conversation.Id, out var conversationMembers))
                {
                    continue;
                }

                var myMembership = conversationMembers.FirstOrDefault(
                    x => x.OrganizationMemberId == currentMember.Id);
                if (myMembership is null)
                {
                    continue;
                }

                Guid? peerOrganizationMemberId = null;
                string displayName;
                string? peerAvatarUrl = null;

                if (conversation.Type == Conversation.TypeGroup)
                {
                    displayName = string.IsNullOrWhiteSpace(conversation.Title)
                        ? "Group"
                        : conversation.Title.Trim();
                }
                else
                {
                    var peerMembership = conversationMembers.FirstOrDefault(
                        x => x.OrganizationMemberId != currentMember.Id);
                    if (peerMembership is null)
                    {
                        continue;
                    }

                    peerOrganizationMemberId = peerMembership.OrganizationMemberId;
                    peerMembersById.TryGetValue(peerMembership.OrganizationMemberId, out var peerMember);
                    displayName = "Member";

                    if (peerMember is not null && peerUsersById.TryGetValue(peerMember.UserId, out var user))
                    {
                        displayName = $"{user.FirstName} {user.LastName}".Trim();
                        if (string.IsNullOrWhiteSpace(displayName))
                        {
                            displayName = user.Email;
                        }

                        peerAvatarUrl = user.AvatarUrl;
                    }
                }

                var latest = await conversationMessageRepository.GetLatestByConversationIdAsync(
                    conversation.Id,
                    cancellationToken);

                var readAfter = myMembership.LastReadAt ?? myMembership.CreatedAt;
                var unreadCount = await conversationMessageRepository.CountUnreadAsync(
                    conversation.Id,
                    currentMember.Id,
                    readAfter,
                    cancellationToken);

                dtos.Add(new ConversationDto(
                    conversation.Id,
                    conversation.OrganizationId,
                    conversation.Type,
                    conversation.Title,
                    peerOrganizationMemberId,
                    displayName,
                    peerAvatarUrl,
                    latest?.Body,
                    latest?.CreatedAt,
                    unreadCount,
                    conversationMembers.Count,
                    conversation.CreatedAt,
                    conversation.UpdatedAt));
            }

            return Result<IReadOnlyCollection<ConversationDto>>.Success(dtos, (int)HttpStatusCode.OK);
        }
    }
}
