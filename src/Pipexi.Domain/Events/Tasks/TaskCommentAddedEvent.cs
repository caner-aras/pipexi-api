namespace Pipexi.Domain.Events.Tasks;

public sealed record TaskCommentAddedEvent(
    Guid TaskId,
    Guid CommentId,
    Guid CommenterUserId,
    string CommenterName,
    string Message) : IDomainEvent;
