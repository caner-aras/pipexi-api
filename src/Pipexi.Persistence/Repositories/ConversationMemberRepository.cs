using Microsoft.EntityFrameworkCore;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Domain.Entities;
using Pipexi.Persistence.Context;

namespace Pipexi.Persistence.Repositories;

public sealed class ConversationMemberRepository(ApplicationDbContext dbContext)
    : Repository<ConversationMember>(dbContext), IConversationMemberRepository
{
    public Task<bool> IsMemberAsync(
        Guid conversationId,
        Guid organizationMemberId,
        CancellationToken cancellationToken = default)
    {
        return DbSet.AnyAsync(
            x => x.ConversationId == conversationId && x.OrganizationMemberId == organizationMemberId,
            cancellationToken);
    }

    public Task<ConversationMember?> GetByConversationAndMemberAsync(
        Guid conversationId,
        Guid organizationMemberId,
        CancellationToken cancellationToken = default)
    {
        return DbSet.FirstOrDefaultAsync(
            x => x.ConversationId == conversationId && x.OrganizationMemberId == organizationMemberId,
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<ConversationMember>> ListByConversationIdAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(x => x.ConversationId == conversationId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<ConversationMember>> ListByConversationIdsAsync(
        IReadOnlyCollection<Guid> conversationIds,
        CancellationToken cancellationToken = default)
    {
        if (conversationIds.Count == 0)
        {
            return Array.Empty<ConversationMember>();
        }

        return await DbSet
            .Where(x => conversationIds.Contains(x.ConversationId))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<ConversationMember>> ListByOrganizationMemberIdAsync(
        Guid organizationMemberId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(x => x.OrganizationMemberId == organizationMemberId)
            .ToListAsync(cancellationToken);
    }
}
