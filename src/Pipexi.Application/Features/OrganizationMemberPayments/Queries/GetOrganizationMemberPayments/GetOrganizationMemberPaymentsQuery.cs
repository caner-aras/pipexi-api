using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.OrganizationMemberPayments.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.OrganizationMemberPayments.Queries.GetOrganizationMemberPayments;

public sealed record GetOrganizationMemberPaymentsQuery(
    Guid OrganizationMemberId,
    Guid? ScopedOrganizationId = null) : IQuery<Result<IReadOnlyCollection<OrganizationMemberPaymentDto>>>
{
    public sealed class Handler
        : IRequestHandler<GetOrganizationMemberPaymentsQuery, Result<IReadOnlyCollection<OrganizationMemberPaymentDto>>>
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

        public async Task<Result<IReadOnlyCollection<OrganizationMemberPaymentDto>>> Handle(
            GetOrganizationMemberPaymentsQuery request,
            CancellationToken cancellationToken)
        {
            var member = await _organizationMemberRepository.GetByIdAsync(
                request.OrganizationMemberId,
                cancellationToken);

            if (member is null)
            {
                return Result<IReadOnlyCollection<OrganizationMemberPaymentDto>>.Failure(
                    new AppError("organization_members.not_found", "Organization member not found."),
                    (int)HttpStatusCode.NotFound);
            }

            var accessDenied = await _organizationAccess
                .ValidateResourceAccessAsync<IReadOnlyCollection<OrganizationMemberPaymentDto>>(
                    member.OrganizationId,
                    request.ScopedOrganizationId,
                    cancellationToken);
            if (accessDenied is not null)
            {
                return accessDenied;
            }

            var payments = await _paymentRepository.ListByOrganizationMemberIdAsync(
                request.OrganizationMemberId,
                cancellationToken);

            return Result<IReadOnlyCollection<OrganizationMemberPaymentDto>>.Success(
                payments.Select(x => x.ToDto()).ToList(),
                (int)HttpStatusCode.OK);
        }
    }
}
