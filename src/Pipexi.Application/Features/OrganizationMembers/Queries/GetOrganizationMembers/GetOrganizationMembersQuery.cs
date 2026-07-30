using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.OrganizationMembers.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.OrganizationMembers.Queries.GetOrganizationMembers;

public sealed record GetOrganizationMembersQuery(Guid? OrganizationId) : IQuery<Result<IReadOnlyCollection<OrganizationMemberDto>>>
{
    public sealed class Handler : IRequestHandler<GetOrganizationMembersQuery, Result<IReadOnlyCollection<OrganizationMemberDto>>>
    {
        private readonly IOrganizationMemberRepository _organizationMemberRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICurrentUserContext _currentUserContext;

        public Handler(
            IOrganizationMemberRepository organizationMemberRepository,
            IUserRepository userRepository,
            ICurrentUserContext currentUserContext)
        {
            _organizationMemberRepository = organizationMemberRepository;
            _userRepository = userRepository;
            _currentUserContext = currentUserContext;
        }

        public async Task<Result<IReadOnlyCollection<OrganizationMemberDto>>> Handle(
            GetOrganizationMembersQuery request,
            CancellationToken cancellationToken)
        {
            var organizationId = request.OrganizationId ?? _currentUserContext.OrganizationId;
            if (organizationId == Guid.Empty)
            {
                return Result<IReadOnlyCollection<OrganizationMemberDto>>.Failure(
                    new AppError("auth.organization_required", "Organization is required."),
                    (int)HttpStatusCode.Forbidden);
            }

            var items = await _organizationMemberRepository.ListByOrganizationIdAsync(organizationId, cancellationToken);

            var users = await _userRepository.ListByIdsAsync(
                items.Select(x => x.UserId).Distinct().ToList(),
                cancellationToken);

            var userMap = users
                .Select(x => x.ToDto())
                .ToDictionary(x => x.Id, x => x);

            return Result<IReadOnlyCollection<OrganizationMemberDto>>.Success(
                items.Select(x => x.ToDto(userMap.GetValueOrDefault(x.UserId))).ToList());
        }
    }
}
