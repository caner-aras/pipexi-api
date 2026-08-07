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
    Guid? ScopedOrganizationId = null,
    DateOnly? FromDate = null,
    DateOnly? ToDate = null) : IQuery<Result<IReadOnlyCollection<OrganizationMemberPaymentDto>>>
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

            var (fromDate, toDate) = ResolveDateRange(request.FromDate, request.ToDate);
            var fromPaidAt = new DateTimeOffset(fromDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
            var toPaidAtExclusive = new DateTimeOffset(
                toDate.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));

            var payments = await _paymentRepository.ListByOrganizationMemberIdAsync(
                request.OrganizationMemberId,
                fromPaidAt,
                toPaidAtExclusive,
                cancellationToken);

            return Result<IReadOnlyCollection<OrganizationMemberPaymentDto>>.Success(
                payments.Select(x => x.ToDto()).ToList(),
                (int)HttpStatusCode.OK);
        }

        private static (DateOnly FromDate, DateOnly ToDate) ResolveDateRange(
            DateOnly? fromDate,
            DateOnly? toDate)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            // Default: current calendar month through today.
            var resolvedFrom = fromDate ?? new DateOnly(today.Year, today.Month, 1);
            var resolvedTo = toDate ?? today;

            if (resolvedFrom > resolvedTo)
            {
                return (resolvedTo, resolvedFrom);
            }

            return (resolvedFrom, resolvedTo);
        }
    }
}
