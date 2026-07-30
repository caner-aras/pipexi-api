using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.OrganizationMemberProfiles.Dtos;
using Pipexi.Domain.Entities;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.OrganizationMemberProfiles.Commands.UpsertOrganizationMemberProfile;

public sealed record UpsertOrganizationMemberProfileCommand(
    Guid OrganizationMemberId,
    DateOnly? DateOfBirth,
    string? Gender,
    string? AddressLine1,
    string? AddressLine2,
    string? City,
    string? State,
    string? PostalCode,
    string? Country,
    string? EmergencyContactName,
    string? EmergencyContactPhone,
    string? NationalId,
    Guid? ScopedOrganizationId = null) : ICommand<Result<OrganizationMemberProfileDto>>
{
    public sealed class Handler : IRequestHandler<UpsertOrganizationMemberProfileCommand, Result<OrganizationMemberProfileDto>>
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
            UpsertOrganizationMemberProfileCommand request,
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

            var existing = await _profileRepository.GetByOrganizationMemberIdAsync(
                request.OrganizationMemberId,
                cancellationToken);

            if (existing is null)
            {
                var created = OrganizationMemberProfile.Create(
                    request.OrganizationMemberId,
                    request.DateOfBirth,
                    request.Gender,
                    request.AddressLine1,
                    request.AddressLine2,
                    request.City,
                    request.State,
                    request.PostalCode,
                    request.Country,
                    request.EmergencyContactName,
                    request.EmergencyContactPhone,
                    request.NationalId);

                await _profileRepository.AddAsync(created, cancellationToken);
                return Result<OrganizationMemberProfileDto>.Success(created.ToDto(), (int)HttpStatusCode.Created);
            }

            existing.UpdateDetails(
                request.DateOfBirth,
                request.Gender,
                request.AddressLine1,
                request.AddressLine2,
                request.City,
                request.State,
                request.PostalCode,
                request.Country,
                request.EmergencyContactName,
                request.EmergencyContactPhone,
                request.NationalId);

            await _profileRepository.UpdateAsync(existing, cancellationToken);
            return Result<OrganizationMemberProfileDto>.Success(existing.ToDto(), (int)HttpStatusCode.OK);
        }
    }
}
