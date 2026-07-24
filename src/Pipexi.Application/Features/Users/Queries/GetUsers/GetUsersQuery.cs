using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Users.Dtos;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Users.Queries.GetUsers;

public sealed record GetUsersQuery() : IQuery<Result<IReadOnlyCollection<UserDto>>>
{
    public sealed class Handler : IRequestHandler<GetUsersQuery, Result<IReadOnlyCollection<UserDto>>>
    {
        private readonly IUserRepository _userRepository;

        public Handler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Result<IReadOnlyCollection<UserDto>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
        {
            _ = request;

            var users = await _userRepository.GetAllAsync(cancellationToken);
            var dtos = users.Select(x => x.ToDto()).ToList();
            return Result<IReadOnlyCollection<UserDto>>.Success(dtos);
        }
    }
}
