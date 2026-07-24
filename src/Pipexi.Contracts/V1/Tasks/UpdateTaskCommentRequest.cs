namespace Pipexi.Contracts.V1.Tasks;

public sealed record UpdateTaskCommentRequest(
    string? Message,
    string? Status);
