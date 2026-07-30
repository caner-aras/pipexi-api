using System.Net;
using MediatR;
using Microsoft.Extensions.Logging;
using Pipexi.Application.Abstractions.Auth;
using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.OrganizationMembers.Commands.ResetOrganizationMemberPassword;

public sealed record ResetOrganizationMemberPasswordCommand(
    Guid OrganizationMemberId,
    Guid? ScopedOrganizationId = null) : ICommand<Result<object?>>
{
    public sealed class Handler : IRequestHandler<ResetOrganizationMemberPasswordCommand, Result<object?>>
    {
        private readonly IOrganizationMemberRepository _organizationMemberRepository;
        private readonly IUserRepository _userRepository;
        private readonly IOrganizationAccessService _organizationAccess;
        private readonly ITokenService _tokenService;
        private readonly ILogger<Handler> _logger;

        public Handler(
            IOrganizationMemberRepository organizationMemberRepository,
            IUserRepository userRepository,
            IOrganizationAccessService organizationAccess,
            ITokenService tokenService,
            ILogger<Handler> logger)
        {
            _organizationMemberRepository = organizationMemberRepository;
            _userRepository = userRepository;
            _organizationAccess = organizationAccess;
            _tokenService = tokenService;
            _logger = logger;
        }

        public async Task<Result<object?>> Handle(
            ResetOrganizationMemberPasswordCommand request,
            CancellationToken cancellationToken)
        {
            var member = await _organizationMemberRepository.GetByIdAsync(
                request.OrganizationMemberId,
                cancellationToken);

            if (member is null)
            {
                return Result<object?>.Failure(
                    new AppError("organization_members.not_found", "Organization member not found."),
                    (int)HttpStatusCode.NotFound);
            }

            var accessDenied = await _organizationAccess.ValidateResourceAccessAsync<object?>(
                member.OrganizationId,
                request.ScopedOrganizationId,
                cancellationToken);
            if (accessDenied is not null)
            {
                return accessDenied;
            }

            var user = await _userRepository.GetByIdAsync(member.UserId, cancellationToken);
            if (user is null || string.IsNullOrWhiteSpace(user.Email))
            {
                return Result<object?>.Failure(
                    new AppError("organization_members.user_not_found", "Organization member user not found."),
                    (int)HttpStatusCode.NotFound);
            }

            var recoverResult = await _tokenService.SendPasswordRecoveryEmailAsync(
                user.Email,
                cancellationToken);

            if (!recoverResult.IsSuccess)
            {
                _logger.LogWarning(
                    "Password recovery email failed for organization member {OrganizationMemberId}: {ErrorCode} {ErrorMessage}",
                    request.OrganizationMemberId,
                    recoverResult.Error?.Code,
                    recoverResult.Error?.Message);
            }

            // Always return a generic success once the member is authorized, to avoid leaking auth-provider state.
            return Result<object?>.Success(null, (int)HttpStatusCode.OK);
        }
    }
}
