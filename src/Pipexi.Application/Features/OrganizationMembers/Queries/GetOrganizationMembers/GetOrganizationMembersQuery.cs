using MediatR;
using Workforce.Application.Abstractions.Persistence;
using Workforce.Application.Common.Models;
using Workforce.Application.Features.OrganizationMembers.Dtos;
using Workforce.Shared.Results;

namespace Workforce.Application.Features.OrganizationMembers.Queries.GetOrganizationMembers;

public sealed record GetOrganizationMembersQuery(Guid? OrganizationId) : IQuery<Result<IReadOnlyCollection<OrganizationMemberDto>>>
{
    public sealed class Handler : IRequestHandler<GetOrganizationMembersQuery, Result<IReadOnlyCollection<OrganizationMemberDto>>>
    {
        private readonly IOrganizationMemberRepository _organizationMemberRepository;
        private readonly IUserRepository _userRepository;

        public Handler(
            IOrganizationMemberRepository organizationMemberRepository,
            IUserRepository userRepository)
        {
            _organizationMemberRepository = organizationMemberRepository;
            _userRepository = userRepository;
        }

        public async Task<Result<IReadOnlyCollection<OrganizationMemberDto>>> Handle(
            GetOrganizationMembersQuery request,
            CancellationToken cancellationToken)
        {
            var items = request.OrganizationId.HasValue
                ? await _organizationMemberRepository.ListByOrganizationIdAsync(request.OrganizationId.Value, cancellationToken)
                : await _organizationMemberRepository.GetAllAsync(cancellationToken);

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
