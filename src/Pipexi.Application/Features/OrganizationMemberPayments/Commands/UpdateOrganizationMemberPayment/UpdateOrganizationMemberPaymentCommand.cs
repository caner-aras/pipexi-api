using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.OrganizationMemberPayments.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.OrganizationMemberPayments.Commands.UpdateOrganizationMemberPayment;

public sealed record UpdateOrganizationMemberPaymentCommand(
    Guid OrganizationMemberId,
    Guid PaymentId,
    decimal Amount,
    string Currency,
    DateTimeOffset PaidAt,
    string Method,
    string? Reference,
    string? Notes,
    DateOnly? PeriodStart,
    DateOnly? PeriodEnd,
    Guid? ScopedOrganizationId = null) : ICommand<Result<OrganizationMemberPaymentDto>>
{
    public sealed class Handler
        : IRequestHandler<UpdateOrganizationMemberPaymentCommand, Result<OrganizationMemberPaymentDto>>
    {
        private readonly IOrganizationMemberRepository _organizationMemberRepository;
        private readonly IOrganizationMemberPaymentRepository _paymentRepository;
        private readonly IOrganizationAccessService _organizationAccess;

        public Handler(
            IOrganizationMemberRepository organizationMemberRepository,
            IOrganizationMemberPaymentRepository paymentRepository,
            IOrganizationAccessService organizationAccess)
        {
            _organizationMemberRepository = organizationMemberRepository;
            _paymentRepository = paymentRepository;
            _organizationAccess = organizationAccess;
        }

        public async Task<Result<OrganizationMemberPaymentDto>> Handle(
            UpdateOrganizationMemberPaymentCommand request,
            CancellationToken cancellationToken)
        {
            var member = await _organizationMemberRepository.GetByIdAsync(
                request.OrganizationMemberId,
                cancellationToken);

            if (member is null)
            {
                return Result<OrganizationMemberPaymentDto>.Failure(
                    new AppError("organization_members.not_found", "Organization member not found."),
                    (int)HttpStatusCode.NotFound);
            }

            var accessDenied = await _organizationAccess.ValidateResourceAccessAsync<OrganizationMemberPaymentDto>(
                member.OrganizationId,
                request.ScopedOrganizationId,
                cancellationToken);
            if (accessDenied is not null)
            {
                return accessDenied;
            }

            var payment = await _paymentRepository.GetByIdAsync(request.PaymentId, cancellationToken);
            if (payment is null || payment.OrganizationMemberId != request.OrganizationMemberId)
            {
                return Result<OrganizationMemberPaymentDto>.Failure(
                    new AppError("organization_member_payments.not_found", "Payment not found."),
                    (int)HttpStatusCode.NotFound);
            }

            payment.UpdateDetails(
                request.Amount,
                request.Currency,
                request.PaidAt,
                request.Method,
                request.Reference ?? string.Empty,
                request.Notes ?? string.Empty,
                request.PeriodStart,
                request.PeriodEnd);

            await _paymentRepository.UpdateAsync(payment, cancellationToken);
            return Result<OrganizationMemberPaymentDto>.Success(payment.ToDto(), (int)HttpStatusCode.OK);
        }
    }
}
