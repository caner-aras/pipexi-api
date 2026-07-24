using Pipexi.Application.Abstractions.Persistence;

namespace Pipexi.Persistence.UnitOfWork;

public sealed class InMemoryUnitOfWork : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return Task.FromResult(0);
    }
}
