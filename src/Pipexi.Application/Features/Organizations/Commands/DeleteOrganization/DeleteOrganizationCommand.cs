using System.Net;
using MediatR;
using Workforce.Application.Abstractions.Persistence;
using Workforce.Application.Common.Models;
using Workforce.Shared.Errors;
using Workforce.Shared.Results;

namespace Workforce.Application.Features.Organizations.Commands.DeleteOrganization;

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
