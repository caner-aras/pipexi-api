using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Organizations.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Organizations.Queries.GetOrganizations;

public sealed record GetOrganizationsQuery() : IQuery<Result<IReadOnlyCollection<OrganizationDto>>>
{
    public sealed class Handler : IRequestHandler<GetOrganizationsQuery, Result<IReadOnlyCollection<OrganizationDto>>>
    {
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IOrganizationMemberRepository _organizationMemberRepository;
        private readonly ICurrentUserContext _currentUserContext;

        public Handler(
            IOrganizationRepository organizationRepository,
            IOrganizationMemberRepository organizationMemberRepository,
            ICurrentUserContext currentUserContext)
        {
            _organizationRepository = organizationRepository;
            _organizationMemberRepository = organizationMemberRepository;
            _currentUserContext = currentUserContext;
        }

        public async Task<Result<IReadOnlyCollection<OrganizationDto>>> Handle(
            GetOrganizationsQuery request,
            CancellationToken cancellationToken)
        {
            _ = request;

            if (_currentUserContext.UserId == Guid.Empty)
            {
                return Result<IReadOnlyCollection<OrganizationDto>>.Failure(
                    new AppError("auth.unauthorized", "Unauthorized."),
                    (int)HttpStatusCode.Unauthorized);
            }

            var memberships = await _organizationMemberRepository.ListByUserIdAsync(
                _currentUserContext.UserId,
                cancellationToken);

            var organizationIds = memberships
                .Select(x => x.OrganizationId)
                .Distinct()
                .ToList();

            if (organizationIds.Count == 0)
            {
                return Result<IReadOnlyCollection<OrganizationDto>>.Success([]);
            }

            var organizations = await _organizationRepository.GetByIdsAsync(
                organizationIds,
                cancellationToken);

            var organizationDtos = organizations
                .Select(x => x.ToDto())
                .ToList();

            return Result<IReadOnlyCollection<OrganizationDto>>.Success(
                organizationDtos);
        }
    }
}
