using System.Net;
using MediatR;
using Workforce.Application.Abstractions.Persistence;
using Workforce.Application.Common.Models;
using Workforce.Shared.Errors;
using Workforce.Shared.Results;

namespace Workforce.Application.Features.LeaveRequests.Commands.DeleteLeaveRequest;

public sealed record DeleteLeaveRequestCommand(Guid Id) : ICommand<Result<object?>>
{
    public sealed class Handler : IRequestHandler<DeleteLeaveRequestCommand, Result<object?>>
    {
        private readonly ILeaveRequestRepository _leaveRequestRepository;

        public Handler(ILeaveRequestRepository leaveRequestRepository)
        {
            _leaveRequestRepository = leaveRequestRepository;
        }

        public async Task<Result<object?>> Handle(DeleteLeaveRequestCommand request, CancellationToken cancellationToken)
        {
            var leaveRequest = await _leaveRequestRepository.GetByIdAsync(request.Id, cancellationToken);
            if (leaveRequest is null)
            {
                return Result<object?>.Failure(
                    new AppError("leave_requests.not_found", "Leave request not found."),
                    (int)HttpStatusCode.NotFound);
            }

            await _leaveRequestRepository.DeleteAsync(leaveRequest, cancellationToken);
            return Result<object?>.Success(null, (int)HttpStatusCode.OK);
        }
    }
}
