using Pipexi.Application.Features.OrganizationMemberPayments.Dtos;
using Pipexi.Domain.Entities;

namespace Pipexi.Application.Features.OrganizationMemberPayments;

internal static class OrganizationMemberPaymentMappings
{
    public static OrganizationMemberPaymentDto ToDto(this OrganizationMemberPayment payment)
    {
        return new OrganizationMemberPaymentDto(
            payment.Id,
            payment.OrganizationMemberId,
            payment.Amount,
            payment.Currency,
            payment.PaidAt,
            payment.Method,
            payment.Reference,
            payment.Notes,
            payment.PeriodStart,
            payment.PeriodEnd,
            payment.Status,
            payment.CreatedAt,
            payment.UpdatedAt);
    }
}
