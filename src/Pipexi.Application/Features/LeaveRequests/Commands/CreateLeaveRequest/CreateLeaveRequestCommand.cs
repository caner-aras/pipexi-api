using System.Net;
using MediatR;
using Workforce.Application.Abstractions.Persistence;
using Workforce.Application.Common.Models;
using Workforce.Application.Features.LeaveRequests.Dtos;
using Workforce.Domain.Entities;
using Workforce.Shared.Errors;
using Workforce.Shared.Results;

namespace Workforce.Application.Features.LeaveRequests.Commands.CreateLeaveRequest;

public sealed record CreateLeaveRequestCommand(
    Guid OrganizationId,
    Guid OrganizationMemberId,
    string LeaveType,
    DateOnly StartDate,
    DateOnly EndDate,
    string Reason) : ICommand<Result<LeaveRequestDto>>
{
    public sealed class Handler : IRequestHandler<CreateLeaveRequestCommand, Result<LeaveRequestDto>>
    {
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IOrganizationMemberRepository _organizationMemberRepository;
        private readonly ILeaveRequestRepository _leaveRequestRepository;

        public Handler(
            IOrganizationRepository organizationRepository,
            IOrganizationMemberRepository organizationMemberRepository,
            ILeaveRequestRepository leaveRequestRepository)
        {
            _organizationRepository = organizationRepository;
            _organizationMemberRepository = organizationMemberRepository;
            _leaveRequestRepository = leaveRequestRepository;
        }

        public async Task<Result<LeaveRequestDto>> Handle(CreateLeaveRequestCommand request, CancellationToken cancellationToken)
        {
            var organization = await _organizationRepository.GetByIdAsync(request.OrganizationId, cancellationToken);
            if (organization is null)
            {
                return Result<LeaveRequestDto>.Failure(
                    new AppError("leave_requests.invalid_organization", "Organization not found."),
                    (int)HttpStatusCode.BadRequest);
            }

            var member = await _organizationMemberRepository.GetByIdAsync(request.OrganizationMemberId, cancellationToken);
            if (member is null || member.OrganizationId != request.OrganizationId)
            {
                return Result<LeaveRequestDto>.Failure(
                    new AppError("leave_requests.invalid_member", "Organization member not found for organization."),
                    (int)HttpStatusCode.BadRequest);
            }

            var leaveRequest = LeaveRequest.Create(
                request.OrganizationId,
                request.OrganizationMemberId,
                request.LeaveType,
                request.StartDate,
                request.EndDate,
                request.Reason);

            await _leaveRequestRepository.AddAsync(leaveRequest, cancellationToken);
            return Result<LeaveRequestDto>.Success(leaveRequest.ToDto(), (int)HttpStatusCode.Created);
        }
    }
}
