namespace Pipexi.Application.Features.Forms.Dtos;

public sealed record StoredFileDto(
    Guid Id,
    Guid OrganizationId,
    string FileName,
    string ContentType,
    string StoragePath,
    long SizeBytes,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
