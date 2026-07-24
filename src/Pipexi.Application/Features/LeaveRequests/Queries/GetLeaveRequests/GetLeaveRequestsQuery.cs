using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.LeaveRequests.Dtos;
using Pipexi.Application.Features.OrganizationMembers;
using Pipexi.Application.Features.Organizations;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.LeaveRequests.Queries.GetLeaveRequests;

public sealed record GetLeaveRequestsQuery(
    Guid? OrganizationId,
    Guid? OrganizationMemberId = null) : IQuery<Result<IReadOnlyCollection<LeaveRequestDto>>>
{
    public sealed class Handler : IRequestHandler<GetLeaveRequestsQuery, Result<IReadOnlyCollection<LeaveRequestDto>>>
    {
        private readonly ILeaveRequestRepository _leaveRequestRepository;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IOrganizationMemberRepository _organizationMemberRepository;
        private readonly IUserRepository _userRepository;

        public Handler(
            ILeaveRequestRepository leaveRequestRepository,
            IOrganizationRepository organizationRepository,
            IOrganizationMemberRepository organizationMemberRepository,
            IUserRepository userRepository)
        {
            _leaveRequestRepository = leaveRequestRepository;
            _organizationRepository = organizationRepository;
            _organizationMemberRepository = organizationMemberRepository;
            _userRepository = userRepository;
        }

        public async Task<Result<IReadOnlyCollection<LeaveRequestDto>>> Handle(GetLeaveRequestsQuery request, CancellationToken cancellationToken)
        {
            IReadOnlyCollection<Domain.Entities.LeaveRequest> leaveRequests;
            if (request.OrganizationMemberId.HasValue)
            {
                leaveRequests = await _leaveRequestRepository.ListByOrganizationMemberIdAsync(request.OrganizationMemberId.Value, cancellationToken);
                if (request.OrganizationId.HasValue)
                {
                    leaveRequests = leaveRequests.Where(x => x.OrganizationId == request.OrganizationId.Value).ToList();
                }
            }
            else if (request.OrganizationId.HasValue)
            {
                leaveRequests = await _leaveRequestRepository.ListByOrganizationIdAsync(request.OrganizationId.Value, cancellationToken);
            }
            else
            {
                leaveRequests = await _leaveRequestRepository.GetAllAsync(cancellationToken);
            }

            var organizationIds = leaveRequests
                .Select(x => x.OrganizationId)
                .Distinct()
                .ToList();

            var organizationMemberIds = leaveRequests
                .Select(x => x.OrganizationMemberId)
                .Distinct()
                .ToList();

            var organizations = organizationIds.Count == 0
                ? []
                : await _organizationRepository.GetByIdsAsync(organizationIds, cancellationToken);
            var organizationMembers = organizationMemberIds.Count == 0
                ? []
                : await _organizationMemberRepository.GetByIdsAsync(organizationMemberIds, cancellationToken);

            var userIds = organizationMembers
                .Select(x => x.UserId)
                .Distinct()
                .ToList();
            var users = userIds.Count == 0
                ? []
                : await _userRepository.ListByIdsAsync(userIds, cancellationToken);

            var organizationMap = organizations.ToDictionary(x => x.Id, x => x.ToDto());
            var userMap = users.ToDictionary(x => x.Id, x => x);
            var organizationMemberMap = organizationMembers.ToDictionary(
                x => x.Id,
                x => x.ToDto(userMap.GetValueOrDefault(x.UserId)?.ToDto()));

            var dtos = leaveRequests
                .Select(x => x.ToDto(
                    organizationMap.GetValueOrDefault(x.OrganizationId),
                    organizationMemberMap.GetValueOrDefault(x.OrganizationMemberId)))
                .ToList();

            return Result<IReadOnlyCollection<LeaveRequestDto>>.Success(dtos);
        }
    }
}
