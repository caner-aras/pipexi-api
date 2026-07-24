using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Users.Dtos;
using Pipexi.Domain.Entities;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Users.Commands.CreateUser;

public sealed record CreateUserCommand(
    string? AuthProviderId,
    string Email,
    string FirstName,
    string LastName,
    string? Phone,
    string? AvatarUrl) : ICommand<Result<UserDto>>
{
    public sealed class Handler : IRequestHandler<CreateUserCommand, Result<UserDto>>
    {
        private readonly IUserRepository _userRepository;

        public Handler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Result<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var authProviderId = string.IsNullOrWhiteSpace(request.AuthProviderId)
                ? $"local:{Guid.NewGuid():N}"
                : request.AuthProviderId;

            var userId = Guid.TryParse(authProviderId, out var parsedUserId)
                ? parsedUserId
                : Guid.NewGuid();

            var user = User.Create(
                userId,
                authProviderId,
                request.Email,
                request.FirstName,
                request.LastName,
                request.Phone,
                request.AvatarUrl);

            await _userRepository.AddAsync(user, cancellationToken);
            return Result<UserDto>.Success(user.ToDto(), (int)HttpStatusCode.Created);
        }
    }
}
