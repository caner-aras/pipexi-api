using System.Net;
using MediatR;
using Workforce.Application.Abstractions.Persistence;
using Workforce.Application.Common.Models;
using Workforce.Application.Features.OrganizationMembers.Dtos;
using Workforce.Domain.Entities;
using Workforce.Shared.Errors;
using Workforce.Shared.Results;

namespace Workforce.Application.Features.OrganizationMembers.Commands.CreateOrganizationMember;

public sealed record CreateOrganizationMemberCommand(
    Guid OrganizationId,
    Guid UserId,
    Guid RoleId,
    string? JobTitle) : ICommand<Result<OrganizationMemberDto>>
{
    public sealed class Handler : IRequestHandler<CreateOrganizationMemberCommand, Result<OrganizationMemberDto>>
    {
        private readonly IOrganizationMemberRepository _organizationMemberRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IUserRepository _userRepository;

        public Handler(
            IOrganizationMemberRepository organizationMemberRepository,
            IRoleRepository roleRepository,
            IUserRepository userRepository)
        {
            _organizationMemberRepository = organizationMemberRepository;
            _roleRepository = roleRepository;
            _userRepository = userRepository;
        }

        public async Task<Result<OrganizationMemberDto>> Handle(
            CreateOrganizationMemberCommand request,
            CancellationToken cancellationToken)
        {
            var exists = await _organizationMemberRepository.ExistsAsync(
                request.OrganizationId,
                request.UserId,
                cancellationToken: cancellationToken);

            if (exists)
            {
                return Result<OrganizationMemberDto>.Failure(
                    new AppError("organization_members.conflict", "Organization member already exists."),
                    (int)HttpStatusCode.Conflict);
            }

            var role = await _roleRepository.GetByIdAsync(request.RoleId, cancellationToken);
            if (role is null || role.OrganizationId != request.OrganizationId)
            {
                return Result<OrganizationMemberDto>.Failure(
                    new AppError("organization_members.invalid_role", "Role not found for organization."),
                    (int)HttpStatusCode.BadRequest);
            }

            var organizationMember = OrganizationMember.Create(
                request.OrganizationId,
                request.UserId,
                request.RoleId,
                request.JobTitle);

            await _organizationMemberRepository.AddAsync(organizationMember, cancellationToken);

            var user = await _userRepository.GetByIdAsync(organizationMember.UserId, cancellationToken);

            return Result<OrganizationMemberDto>.Success(
                organizationMember.ToDto(user?.ToDto()),
                (int)HttpStatusCode.Created);
        }
    }
}
