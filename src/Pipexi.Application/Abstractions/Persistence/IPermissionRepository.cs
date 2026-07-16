using Workforce.Domain.Entities;

namespace Workforce.Application.Abstractions.Persistence;

public interface IPermissionRepository : IRepository<Permission>
{
    Task<bool> KeyExistsAsync(string key, Guid? excludingPermissionId = null, CancellationToken cancellationToken = default);
}
