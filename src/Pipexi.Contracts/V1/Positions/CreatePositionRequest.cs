namespace Pipexi.Contracts.V1.Positions;

public sealed record CreatePositionRequest(
    Guid OrganizationId,
    string Title,
    decimal DefaultHourlyRate,
    string? Description);
