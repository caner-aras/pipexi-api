namespace Pipexi.Application.Features.Forms.Dtos;

public sealed record ShiftFormTemplateDto(
    Guid Id,
    Guid OrganizationId,
    string Name,
    string? Description,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    bool IsFilled);