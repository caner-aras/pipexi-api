using Pipexi.Domain.Entities;

namespace Pipexi.Application.Abstractions.Persistence;

public interface IConversationMemberRepository : IRepository<ConversationMember>
{
    Task<bool> IsMemberAsync(
        Guid conversationId,
        Guid organizationMemberId,
        CancellationToken cancellationToken = default);

    Task<ConversationMember?> GetByConversationAndMemberAsync(
        Guid conversationId,
        Guid organizationMemberId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ConversationMember>> ListByConversationIdAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ConversationMember>> ListByConversationIdsAsync(
        IReadOnlyCollection<Guid> conversationIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ConversationMember>> ListByOrganizationMemberIdAsync(
        Guid organizationMemberId,
        CancellationToken cancellationToken = default);
}
