using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Persistence.Context;

namespace Pipexi.Persistence.UnitOfWork;

public sealed class EfUnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _dbContext;

    public EfUnitOfWork(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
