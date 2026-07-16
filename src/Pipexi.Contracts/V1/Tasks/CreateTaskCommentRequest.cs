namespace Workforce.Contracts.V1.Tasks;

public sealed record CreateTaskCommentRequest(
    Guid WorkTaskId,
    Guid UserId,
    string Message);
