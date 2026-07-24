namespace Pipexi.Application.Features.Forms.Dtos;

public sealed record FormTemplateDto(
    Guid Id,
    Guid OrganizationId,
    string Name,
    string? Description,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    IReadOnlyCollection<FormFieldDto> Fields);
