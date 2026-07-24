using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Users.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Users.Commands.UpdateUser;

public sealed record UpdateUserCommand(
    Guid Id,
    string? FirstName,
    string? LastName,
    string? Phone,
    string? AvatarUrl) : ICommand<Result<UserDto>>
{
    public sealed class Handler : IRequestHandler<UpdateUserCommand, Result<UserDto>>
    {
        private readonly IUserRepository _userRepository;

        public Handler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Result<UserDto>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
            if (user is null)
            {
                return Result<UserDto>.Failure(
                    new AppError("users.not_found", "User not found."),
                    (int)HttpStatusCode.NotFound);
            }

            user.UpdateProfile(request.FirstName, request.LastName, request.Phone, request.AvatarUrl);
            await _userRepository.UpdateAsync(user, cancellationToken);

            return Result<UserDto>.Success(user.ToDto());
        }
    }
}
