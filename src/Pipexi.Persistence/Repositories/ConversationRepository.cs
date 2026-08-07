using Microsoft.EntityFrameworkCore;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Domain.Entities;
using Pipexi.Persistence.Context;

namespace Pipexi.Persistence.Repositories;

public sealed class ConversationRepository(ApplicationDbContext dbContext)
    : Repository<Conversation>(dbContext), IConversationRepository
{
    public async Task<Conversation?> GetDirectByOrganizationAndPairKeyAsync(
        Guid organizationId,
        string directMemberPairKey,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(
            x => x.OrganizationId == organizationId
                 && x.Type == Conversation.TypeDirect
                 && x.DirectMemberPairKey == directMemberPairKey,
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<Conversation>> ListByOrganizationMemberIdAsync(
        Guid organizationId,
        Guid organizationMemberId,
        CancellationToken cancellationToken = default)
    {
        return await (
            from conversation in DbSet
            join member in Context.ConversationMembers on conversation.Id equals member.ConversationId
            where conversation.OrganizationId == organizationId
                  && member.OrganizationMemberId == organizationMemberId
                  && member.Status == "active"
            orderby conversation.UpdatedAt descending, conversation.CreatedAt descending
            select conversation
        ).ToListAsync(cancellationToken);
    }
}
