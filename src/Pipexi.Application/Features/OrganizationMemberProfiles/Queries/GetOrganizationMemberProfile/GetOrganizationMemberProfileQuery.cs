using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.OrganizationMemberProfiles.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.OrganizationMemberProfiles.Queries.GetOrganizationMemberProfile;

public sealed record GetOrganizationMemberProfileQuery(
    Guid OrganizationMemberId,
    Guid? ScopedOrganizationId = null) : IQuery<Result<OrganizationMemberProfileDto>>
{
    public sealed class Handler : IRequestHandler<GetOrganizationMemberProfileQuery, Result<OrganizationMemberProfileDto>>
    {
        private readonly IOrganizationMemberRepository _organizationMemberRepository;
        private readonly IOrganizationMemberProfileRepository _profileRepository;
        private readonly IOrganizationAccessService _organizationAccess;

        public Handler(
            IOrganizationMemberRepository organizationMemberRepository,
            IOrganizationMemberProfileRepository profileRepository,
            IOrganizationAccessService organizationAccess)
        {
            _organizationMemberRepository = organizationMemberRepository;
            _profileRepository = profileRepository;
            _organizationAccess = organizationAccess;
        }

        public async Task<Result<OrganizationMemberProfileDto>> Handle(
            GetOrganizationMemberProfileQuery request,
            CancellationToken cancellationToken)
        {
            var member = await _organizationMemberRepository.GetByIdAsync(
                request.OrganizationMemberId,
                cancellationToken);

            if (member is null)
            {
                return Result<OrganizationMemberProfileDto>.Failure(
                    new AppError("organization_members.not_found", "Organization member not found."),
                    (int)HttpStatusCode.NotFound);
            }

            var accessDenied = await _organizationAccess.ValidateResourceAccessAsync<OrganizationMemberProfileDto>(
                member.OrganizationId,
                request.ScopedOrganizationId,
                cancellationToken);
            if (accessDenied is not null)
            {
                return accessDenied;
            }

            var profile = await _profileRepository.GetByOrganizationMemberIdAsync(
                request.OrganizationMemberId,
                cancellationToken);

            if (profile is null)
            {
                return Result<OrganizationMemberProfileDto>.Failure(
                    new AppError("organization_member_profiles.not_found", "Organization member profile not found."),
                    (int)HttpStatusCode.NotFound);
            }

            return Result<OrganizationMemberProfileDto>.Success(profile.ToDto(), (int)HttpStatusCode.OK);
        }
    }
}
