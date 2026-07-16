using System.Net;
using MediatR;
using Workforce.Application.Abstractions.Persistence;
using Workforce.Application.Common.Models;
using Workforce.Application.Features.OrganizationMembers.Dtos;
using Workforce.Shared.Errors;
using Workforce.Shared.Results;

namespace Workforce.Application.Features.OrganizationMembers.Commands.UpdateOrganizationMember;

public sealed record UpdateOrganizationMemberCommand(
    Guid Id,
    Guid? RoleId,
    string? JobTitle,
    string? Status) : ICommand<Result<OrganizationMemberDto>>
{
    public sealed class Handler : IRequestHandler<UpdateOrganizationMemberCommand, Result<OrganizationMemberDto>>
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
            UpdateOrganizationMemberCommand request,
            CancellationToken cancellationToken)
        {
            var organizationMember = await _organizationMemberRepository.GetByIdAsync(request.Id, cancellationToken);
            if (organizationMember is null)
            {
                return Result<OrganizationMemberDto>.Failure(
                    new AppError("organization_members.not_found", "Organization member not found."),
                    (int)HttpStatusCode.NotFound);
            }

            if (request.RoleId.HasValue)
            {
                var role = await _roleRepository.GetByIdAsync(request.RoleId.Value, cancellationToken);
                if (role is null || role.OrganizationId != organizationMember.OrganizationId)
                {
                    return Result<OrganizationMemberDto>.Failure(
                        new AppError("organization_members.invalid_role", "Role not found for organization."),
                        (int)HttpStatusCode.BadRequest);
                }
            }

            organizationMember.UpdateDetails(request.RoleId, request.JobTitle, request.Status);
            await _organizationMemberRepository.UpdateAsync(organizationMember, cancellationToken);

            var user = await _userRepository.GetByIdAsync(organizationMember.UserId, cancellationToken);

            return Result<OrganizationMemberDto>.Success(
                organizationMember.ToDto(user?.ToDto()),
                (int)HttpStatusCode.OK);
        }
    }
}
