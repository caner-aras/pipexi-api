namespace Pipexi.Application.Abstractions.Storage;

public interface IObjectStorage
{
    Task<string> GenerateUploadUrlAsync(string key, CancellationToken cancellationToken = default);
}
