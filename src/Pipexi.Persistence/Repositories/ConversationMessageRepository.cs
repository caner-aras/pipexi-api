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
        // Include soft-deleted tombstones so clients can show "This message was deleted".
        var query = DbSet.IgnoreQueryFilters().Where(x => x.ConversationId == conversationId);
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

    public Task<int> CountUnreadAsync(
        Guid conversationId,
        Guid readerOrganizationMemberId,
        DateTimeOffset readAfter,
        CancellationToken cancellationToken = default)
    {
        return DbSet.CountAsync(
            x => x.ConversationId == conversationId
                 && x.SenderOrganizationMemberId != readerOrganizationMemberId
                 && x.CreatedAt > readAfter,
            cancellationToken);
    }

    public async Task<int> CountUnreadForMembershipsAsync(
        IReadOnlyCollection<(Guid ConversationId, Guid ReaderOrganizationMemberId, DateTimeOffset ReadAfter)> memberships,
        CancellationToken cancellationToken = default)
    {
        if (memberships.Count == 0)
        {
            return 0;
        }

        var total = 0;
        foreach (var membership in memberships)
        {
            total += await CountUnreadAsync(
                membership.ConversationId,
                membership.ReaderOrganizationMemberId,
                membership.ReadAfter,
                cancellationToken);
        }

        return total;
    }
}
