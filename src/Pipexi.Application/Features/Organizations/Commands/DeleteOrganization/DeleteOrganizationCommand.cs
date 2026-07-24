using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Organizations.Commands.DeleteOrganization;

public sealed record DeleteOrganizationCommand(Guid Id) : ICommand<Result<object?>>
{
    public sealed class Handler : IRequestHandler<DeleteOrganizationCommand, Result<object?>>
    {
        private readonly IOrganizationRepository _organizationRepository;

        public Handler(IOrganizationRepository organizationRepository)
        {
            _organizationRepository = organizationRepository;
        }

        public async Task<Result<object?>> Handle(
            DeleteOrganizationCommand request,
            CancellationToken cancellationToken)
        {
            var organization = await _organizationRepository.GetByIdAsync(request.Id, cancellationToken);
            if (organization is null)
            {
                return Result<object?>.Failure(
                    new AppError("organizations.not_found", "Organization not found."),
                    (int)HttpStatusCode.NotFound);
            }

            await _organizationRepository.DeleteAsync(organization, cancellationToken);
            return Result<object?>.Success(null, (int)HttpStatusCode.OK);
        }
    }
}
