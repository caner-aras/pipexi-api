using Workforce.Application.Abstractions.Persistence;

namespace Workforce.Persistence.UnitOfWork;

public sealed class InMemoryUnitOfWork : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return Task.FromResult(0);
    }
}
