using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Announcements.Dtos;
using Pipexi.Domain.Entities;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Announcements.Commands.CreateAnnouncement;

public sealed record CreateAnnouncementCommand(
    Guid OrganizationId,
    string Title,
    string Body,
    string AudienceType,
    Guid? AudienceId,
    DateTimeOffset? PublishedAt) : ICommand<Result<AnnouncementDto>>
{
    public sealed class Handler : IRequestHandler<CreateAnnouncementCommand, Result<AnnouncementDto>>
    {
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IAnnouncementRepository _announcementRepository;
        private readonly ILocationRepository _locationRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IOrganizationMemberRepository _organizationMemberRepository;
        private readonly ITeamRepository _teamRepository;

        public Handler(
            IOrganizationRepository organizationRepository,
            IAnnouncementRepository announcementRepository,
            ILocationRepository locationRepository,
            IRoleRepository roleRepository,
            IOrganizationMemberRepository organizationMemberRepository,
            ITeamRepository teamRepository)
        {
            _organizationRepository = organizationRepository;
            _announcementRepository = announcementRepository;
            _locationRepository = locationRepository;
            _roleRepository = roleRepository;
            _organizationMemberRepository = organizationMemberRepository;
            _teamRepository = teamRepository;
        }

        public async Task<Result<AnnouncementDto>> Handle(CreateAnnouncementCommand request, CancellationToken cancellationToken)
        {
            var organization = await _organizationRepository.GetByIdAsync(request.OrganizationId, cancellationToken);
            if (organization is null)
            {
                return Result<AnnouncementDto>.Failure(
                    new AppError("announcements.invalid_organization", "Organization not found."),
                    (int)HttpStatusCode.BadRequest);
            }

            var audienceType = AnnouncementAudience.Normalize(request.AudienceType);
            var audienceValidation = await ValidateAudienceTargetAsync(
                request.OrganizationId,
                audienceType,
                request.AudienceId,
                cancellationToken);
            if (audienceValidation is not null)
            {
                return audienceValidation;
            }

            var announcement = Announcement.Create(
                request.OrganizationId,
                request.Title,
                request.Body,
                audienceType,
                AnnouncementAudience.IsAll(audienceType) ? null : request.AudienceId,
                request.PublishedAt);

            await _announcementRepository.AddAsync(announcement, cancellationToken);
            return Result<AnnouncementDto>.Success(announcement.ToDto(), (int)HttpStatusCode.Created);
        }

        private async Task<Result<AnnouncementDto>?> ValidateAudienceTargetAsync(
            Guid organizationId,
            string audienceType,
            Guid? audienceId,
            CancellationToken cancellationToken)
        {
            if (AnnouncementAudience.IsAll(audienceType))
            {
                return null;
            }

            if (!audienceId.HasValue || audienceId.Value == Guid.Empty)
            {
                return Result<AnnouncementDto>.Failure(
                    new AppError("announcements.invalid_audience", "AudienceId is required."),
                    (int)HttpStatusCode.BadRequest);
            }

            switch (audienceType)
            {
                case AnnouncementAudience.Location:
                {
                    var location = await _locationRepository.GetByIdAsync(audienceId.Value, cancellationToken);
                    if (location is null || location.OrganizationId != organizationId)
                    {
                        return Result<AnnouncementDto>.Failure(
                            new AppError("announcements.invalid_audience", "Location not found for organization."),
                            (int)HttpStatusCode.BadRequest);
                    }

                    break;
                }
                case AnnouncementAudience.Role:
                {
                    var role = await _roleRepository.GetByIdAsync(audienceId.Value, cancellationToken);
                    if (role is null || role.OrganizationId != organizationId)
                    {
                        return Result<AnnouncementDto>.Failure(
                            new AppError("announcements.invalid_audience", "Role not found for organization."),
                            (int)HttpStatusCode.BadRequest);
                    }

                    break;
                }
                case AnnouncementAudience.Member:
                {
                    var member = await _organizationMemberRepository.GetByIdAsync(audienceId.Value, cancellationToken);
                    if (member is null || member.OrganizationId != organizationId)
                    {
                        return Result<AnnouncementDto>.Failure(
                            new AppError("announcements.invalid_audience", "Member not found for organization."),
                            (int)HttpStatusCode.BadRequest);
                    }

                    break;
                }
                case AnnouncementAudience.Team:
                {
                    var team = await _teamRepository.GetByIdAsync(audienceId.Value, cancellationToken);
                    if (team is null || team.OrganizationId != organizationId)
                    {
                        return Result<AnnouncementDto>.Failure(
                            new AppError("announcements.invalid_audience", "Team not found for organization."),
                            (int)HttpStatusCode.BadRequest);
                    }

                    break;
                }
            }

            return null;
        }
    }
}
