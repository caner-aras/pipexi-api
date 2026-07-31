using Pipexi.Domain.Entities;

namespace Pipexi.Application.Abstractions.Persistence;

public interface IConversationRepository : IRepository<Conversation>
{
    Task<Conversation?> GetDirectByOrganizationAndPairKeyAsync(
        Guid organizationId,
        string directMemberPairKey,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Conversation>> ListByOrganizationMemberIdAsync(
        Guid organizationId,
        Guid organizationMemberId,
        CancellationToken cancellationToken = default);
}
