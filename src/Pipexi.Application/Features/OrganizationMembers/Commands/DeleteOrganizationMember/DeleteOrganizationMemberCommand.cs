using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.OrganizationMembers.Commands.DeleteOrganizationMember;

public sealed record DeleteOrganizationMemberCommand(Guid Id) : ICommand<Result<object?>>
{
    public sealed class Handler : IRequestHandler<DeleteOrganizationMemberCommand, Result<object?>>
    {
        private readonly IOrganizationMemberRepository _organizationMemberRepository;

        public Handler(IOrganizationMemberRepository organizationMemberRepository)
        {
            _organizationMemberRepository = organizationMemberRepository;
        }

        public async Task<Result<object?>> Handle(
            DeleteOrganizationMemberCommand request,
            CancellationToken cancellationToken)
        {
            var organizationMember = await _organizationMemberRepository.GetByIdAsync(request.Id, cancellationToken);
            if (organizationMember is null)
            {
                return Result<object?>.Failure(
                    new AppError("organization_members.not_found", "Organization member not found."),
                    (int)HttpStatusCode.NotFound);
            }

            await _organizationMemberRepository.DeleteAsync(organizationMember, cancellationToken);
            return Result<object?>.Success(null, (int)HttpStatusCode.OK);
        }
    }
}
