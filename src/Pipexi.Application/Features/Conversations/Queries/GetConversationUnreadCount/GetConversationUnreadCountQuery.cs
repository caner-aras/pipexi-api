using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Conversations.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Conversations.Queries.GetConversationUnreadCount;

public sealed record GetConversationUnreadCountQuery(Guid? OrganizationId = null)
    : IQuery<Result<ConversationUnreadCountDto>>
{
    public sealed class Handler(
        IConversationMemberRepository conversationMemberRepository,
        IConversationMessageRepository conversationMessageRepository,
        IOrganizationMemberRepository organizationMemberRepository,
        ICurrentUserContext currentUserContext)
        : IRequestHandler<GetConversationUnreadCountQuery, Result<ConversationUnreadCountDto>>
    {
        public async Task<Result<ConversationUnreadCountDto>> Handle(
            GetConversationUnreadCountQuery request,
            CancellationToken cancellationToken)
        {
            var organizationId = request.OrganizationId ?? currentUserContext.OrganizationId;
            if (organizationId == Guid.Empty || currentUserContext.UserId == Guid.Empty)
            {
                return Result<ConversationUnreadCountDto>.Failure(
                    new AppError("auth.unauthorized", "Authenticated organization membership is required."),
                    (int)HttpStatusCode.Unauthorized);
            }

            var currentMember = await organizationMemberRepository.GetByOrganizationIdAndUserIdAsync(
                organizationId,
                currentUserContext.UserId,
                cancellationToken);

            if (currentMember is null)
            {
                return Result<ConversationUnreadCountDto>.Failure(
                    new AppError("conversations.member_not_found", "Organization membership not found."),
                    (int)HttpStatusCode.Forbidden);
            }

            var memberships = await conversationMemberRepository.ListByOrganizationMemberIdAsync(
                currentMember.Id,
                cancellationToken);

            var unreadInputs = memberships
                .Select(x => (
                    x.ConversationId,
                    currentMember.Id,
                    x.LastReadAt ?? x.CreatedAt))
                .ToList();

            var unreadCount = await conversationMessageRepository.CountUnreadForMembershipsAsync(
                unreadInputs,
                cancellationToken);

            return Result<ConversationUnreadCountDto>.Success(
                new ConversationUnreadCountDto(unreadCount),
                (int)HttpStatusCode.OK);
        }
    }
}
