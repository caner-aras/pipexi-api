namespace Pipexi.Contracts.V1.OrganizationMemberPayments;

public sealed record UpdateOrganizationMemberPaymentRequest(
    decimal Amount,
    string Currency,
    DateTimeOffset PaidAt,
    string Method,
    string? Reference,
    string? Notes,
    DateOnly? PeriodStart,
    DateOnly? PeriodEnd);
