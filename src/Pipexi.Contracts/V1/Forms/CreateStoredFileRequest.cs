namespace Workforce.Contracts.V1.Forms;

public sealed record CreateStoredFileRequest(
    Guid OrganizationId,
    string FileName,
    string ContentType,
    string StoragePath,
    long SizeBytes);
