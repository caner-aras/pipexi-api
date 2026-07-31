using Pipexi.Domain.Entities;

namespace Pipexi.Application.Abstractions.Persistence;

public interface IConversationMessageRepository : IRepository<ConversationMessage>
{
    Task<(IReadOnlyCollection<ConversationMessage> Items, int TotalCount)> ListByConversationIdPagedAsync(
        Guid conversationId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<ConversationMessage?> GetLatestByConversationIdAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default);
}
