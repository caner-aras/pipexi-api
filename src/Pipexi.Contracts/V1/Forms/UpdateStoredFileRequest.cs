namespace Workforce.Contracts.V1.Forms;

public sealed record UpdateStoredFileRequest(
    string? FileName,
    string? ContentType,
    string? StoragePath,
    long? SizeBytes,
    string? Status);
