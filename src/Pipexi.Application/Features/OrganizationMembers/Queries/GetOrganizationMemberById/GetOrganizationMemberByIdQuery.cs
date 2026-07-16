using System.Net;
using MediatR;
using Workforce.Application.Abstractions.Persistence;
using Workforce.Application.Common.Models;
using Workforce.Application.Features.OrganizationMembers.Dtos;
using Workforce.Shared.Errors;
using Workforce.Shared.Results;

namespace Workforce.Application.Features.OrganizationMembers.Queries.GetOrganizationMemberById;

public sealed record GetOrganizationMemberByIdQuery(Guid Id) : IQuery<Result<OrganizationMemberDto>>
{
    public sealed class Handler : IRequestHandler<GetOrganizationMemberByIdQuery, Result<OrganizationMemberDto>>
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

        public async Task<Result<OrganizationMemberDto>> Handle(
            GetOrganizationMemberByIdQuery request,
            CancellationToken cancellationToken)
        {
            var organizationMember = await _organizationMemberRepository.GetByIdAsync(request.Id, cancellationToken);
            if (organizationMember is null)
            {
                return Result<OrganizationMemberDto>.Failure(
                    new AppError("organization_members.not_found", "Organization member not found."),
                    (int)HttpStatusCode.NotFound);
            }

            var user = await _userRepository.GetByIdAsync(organizationMember.UserId, cancellationToken);

            return Result<OrganizationMemberDto>.Success(organizationMember.ToDto(user?.ToDto()));
        }
    }
}
