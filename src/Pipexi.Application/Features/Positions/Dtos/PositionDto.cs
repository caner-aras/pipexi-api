namespace Pipexi.Application.Features.Positions.Dtos;

public sealed record PositionDto(
    Guid Id,
    Guid OrganizationId,
    string Title,
    string? Description,
    decimal DefaultHourlyRate,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
