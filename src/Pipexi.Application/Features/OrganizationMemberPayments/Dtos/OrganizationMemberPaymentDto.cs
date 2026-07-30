namespace Pipexi.Application.Features.OrganizationMemberPayments.Dtos;

public sealed record OrganizationMemberPaymentDto(
    Guid Id,
    Guid OrganizationMemberId,
    decimal Amount,
    string Currency,
    DateTimeOffset PaidAt,
    string Method,
    string? Reference,
    string? Notes,
    DateOnly? PeriodStart,
    DateOnly? PeriodEnd,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
