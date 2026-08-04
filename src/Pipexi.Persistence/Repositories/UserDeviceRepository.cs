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
            .FirstOrDefaultAsync(x => x.FcmToken == fcmToken && x.Status == "active", cancellationToken);
    }

    public async Task<IReadOnlyCollection<UserDevice>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(x => x.UserId == userId && x.Status == "active")
            .ToListAsync(cancellationToken);
    }
}
