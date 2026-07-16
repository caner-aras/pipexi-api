namespace Workforce.Application.Features.Forms.Dtos;

public sealed record FormFieldDto(
    Guid Id,
    Guid FormTemplateId,
    string Type,
    string Label,
    bool IsRequired,
    int SortOrder,
    string? OptionsJson,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
