using System.Net;
using MediatR;
using Workforce.Application.Abstractions.Persistence;
using Workforce.Application.Common.Models;
using Workforce.Application.Features.LeaveRequests.Dtos;
using Workforce.Shared.Errors;
using Workforce.Shared.Results;

namespace Workforce.Application.Features.LeaveRequests.Queries.GetLeaveRequestById;

public sealed record GetLeaveRequestByIdQuery(Guid Id) : IQuery<Result<LeaveRequestDto>>
{
    public sealed class Handler : IRequestHandler<GetLeaveRequestByIdQuery, Result<LeaveRequestDto>>
    {
        private readonly ILeaveRequestRepository _leaveRequestRepository;

        public Handler(ILeaveRequestRepository leaveRequestRepository)
        {
            _leaveRequestRepository = leaveRequestRepository;
        }

        public async Task<Result<LeaveRequestDto>> Handle(GetLeaveRequestByIdQuery request, CancellationToken cancellationToken)
        {
            var leaveRequest = await _leaveRequestRepository.GetByIdAsync(request.Id, cancellationToken);
            if (leaveRequest is null)
            {
                return Result<LeaveRequestDto>.Failure(
                    new AppError("leave_requests.not_found", "Leave request not found."),
                    (int)HttpStatusCode.NotFound);
            }

            return Result<LeaveRequestDto>.Success(leaveRequest.ToDto());
        }
    }
}
