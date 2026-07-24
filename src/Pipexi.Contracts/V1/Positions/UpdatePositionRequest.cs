namespace Pipexi.Contracts.V1.Positions;

public sealed record UpdatePositionRequest(
    string? Title,
    decimal? DefaultHourlyRate,
    string? Description,
    string? Status);
