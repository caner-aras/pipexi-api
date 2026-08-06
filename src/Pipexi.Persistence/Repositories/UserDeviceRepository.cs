using Microsoft.EntityFrameworkCore;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Domain.Entities;
using Pipexi.Persistence.Context;

namespace Pipexi.Persistence.Repositories;

public sealed class UserDeviceRepository : Repository<UserDevice>, IUserDeviceRepository
{
    public UserDeviceRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<UserDevice?> GetByTokenAsync(string fcmToken, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(x => x.FcmToken == fcmToken, cancellationToken);
    }

    public async Task<IReadOnlyCollection<UserDevice>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public override Task DeleteAsync(UserDevice entity, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        DbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public override async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity is not null)
        {
            DbSet.Remove(entity);
        }
    }

    public override Task DeleteRangeAsync(IEnumerable<UserDevice> entities, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        DbSet.RemoveRange(entities);
        return Task.CompletedTask;
    }
}
