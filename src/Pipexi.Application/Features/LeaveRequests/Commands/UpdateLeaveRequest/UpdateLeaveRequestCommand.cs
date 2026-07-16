using System.Net;
using MediatR;
using Workforce.Application.Abstractions.Persistence;
using Workforce.Application.Common.Models;
using Workforce.Application.Features.LeaveRequests.Dtos;
using Workforce.Shared.Errors;
using Workforce.Shared.Results;

namespace Workforce.Application.Features.LeaveRequests.Commands.UpdateLeaveRequest;

public sealed record UpdateLeaveRequestCommand(
    Guid Id,
    string? LeaveType,
    DateOnly? StartDate,
    DateOnly? EndDate,
    string? Reason,
    string? Status) : ICommand<Result<LeaveRequestDto>>
{
    public sealed class Handler : IRequestHandler<UpdateLeaveRequestCommand, Result<LeaveRequestDto>>
    {
        private readonly ILeaveRequestRepository _leaveRequestRepository;

        public Handler(ILeaveRequestRepository leaveRequestRepository)
        {
            _leaveRequestRepository = leaveRequestRepository;
        }

        public async Task<Result<LeaveRequestDto>> Handle(UpdateLeaveRequestCommand request, CancellationToken cancellationToken)
        {
            var leaveRequest = await _leaveRequestRepository.GetByIdAsync(request.Id, cancellationToken);
            if (leaveRequest is null)
            {
                return Result<LeaveRequestDto>.Failure(
                    new AppError("leave_requests.not_found", "Leave request not found."),
                    (int)HttpStatusCode.NotFound);
            }

            leaveRequest.UpdateDetails(
                request.LeaveType,
                request.StartDate,
                request.EndDate,
                request.Reason,
                request.Status);

            await _leaveRequestRepository.UpdateAsync(leaveRequest, cancellationToken);
            return Result<LeaveRequestDto>.Success(leaveRequest.ToDto());
        }
    }
}
