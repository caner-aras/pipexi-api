using Microsoft.EntityFrameworkCore;
using Workforce.Application.Abstractions.Persistence;
using Workforce.Domain.Entities;
using Workforce.Persistence.Context;

namespace Workforce.Persistence.Repositories;

public sealed class PermissionRepository : Repository<Permission>, IPermissionRepository
{
    public PermissionRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<bool> KeyExistsAsync(
        string key,
        Guid? excludingPermissionId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedKey = key.Trim().ToLowerInvariant();

        return await DbSet.AnyAsync(x =>
            x.Key.ToLower() == normalizedKey &&
            (!excludingPermissionId.HasValue || x.Id != excludingPermissionId.Value),
            cancellationToken);
    }
}
