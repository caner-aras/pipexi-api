using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.OrganizationMemberPayments.Dtos;
using Pipexi.Domain.Entities;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.OrganizationMemberPayments.Commands.CreateOrganizationMemberPayment;

public sealed record CreateOrganizationMemberPaymentCommand(
    Guid OrganizationMemberId,
    decimal Amount,
    string? Currency,
    DateTimeOffset PaidAt,
    string Method,
    string? Reference,
    string? Notes,
    DateOnly? PeriodStart,
    DateOnly? PeriodEnd,
    Guid? ScopedOrganizationId = null) : ICommand<Result<OrganizationMemberPaymentDto>>
{
    public sealed class Handler
        : IRequestHandler<CreateOrganizationMemberPaymentCommand, Result<OrganizationMemberPaymentDto>>
    {
        private readonly IOrganizationMemberRepository _organizationMemberRepository;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IOrganizationMemberPaymentRepository _paymentRepository;
        private readonly IOrganizationAccessService _organizationAccess;

        public Handler(
            IOrganizationMemberRepository organizationMemberRepository,
            IOrganizationRepository organizationRepository,
            IOrganizationMemberPaymentRepository paymentRepository,
            IOrganizationAccessService organizationAccess)
        {
            _organizationMemberRepository = organizationMemberRepository;
            _organizationRepository = organizationRepository;
            _paymentRepository = paymentRepository;
            _organizationAccess = organizationAccess;
        }

        public async Task<Result<OrganizationMemberPaymentDto>> Handle(
            CreateOrganizationMemberPaymentCommand request,
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

            var currency = request.Currency;
            if (string.IsNullOrWhiteSpace(currency))
            {
                var organization = await _organizationRepository.GetByIdAsync(
                    member.OrganizationId,
                    cancellationToken);

                if (organization is null)
                {
                    return Result<OrganizationMemberPaymentDto>.Failure(
                        new AppError("organizations.not_found", "Organization not found."),
                        (int)HttpStatusCode.NotFound);
                }

                currency = organization.Currency;
            }

            var payment = OrganizationMemberPayment.Create(
                request.OrganizationMemberId,
                request.Amount,
                currency,
                request.PaidAt,
                request.Method,
                request.Reference,
                request.Notes,
                request.PeriodStart,
                request.PeriodEnd);

            await _paymentRepository.AddAsync(payment, cancellationToken);
            return Result<OrganizationMemberPaymentDto>.Success(payment.ToDto(), (int)HttpStatusCode.Created);
        }
    }
}
