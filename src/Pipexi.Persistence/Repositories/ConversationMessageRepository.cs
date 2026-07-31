using Microsoft.EntityFrameworkCore;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Domain.Entities;
using Pipexi.Persistence.Context;

namespace Pipexi.Persistence.Repositories;

public sealed class ConversationMessageRepository(ApplicationDbContext dbContext)
    : Repository<ConversationMessage>(dbContext), IConversationMessageRepository
{
    public async Task<(IReadOnlyCollection<ConversationMessage> Items, int TotalCount)> ListByConversationIdPagedAsync(
        Guid conversationId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.Where(x => x.ConversationId == conversationId);
        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<ConversationMessage?> GetLatestByConversationIdAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(x => x.ConversationId == conversationId)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
