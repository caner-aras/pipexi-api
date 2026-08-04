using Pipexi.Domain.Entities;

namespace Pipexi.Application.Abstractions.Persistence;

public interface IUserDeviceRepository : IRepository<UserDevice>
{
    Task<UserDevice?> GetByTokenAsync(string fcmToken, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<UserDevice>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
