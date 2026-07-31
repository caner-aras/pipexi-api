namespace Pipexi.Contracts.V1.Conversations;

public sealed record CreateConversationRequest(
    string? Type,
    Guid? OrganizationMemberId,
    string? Title,
    IReadOnlyCollection<Guid>? OrganizationMemberIds);
