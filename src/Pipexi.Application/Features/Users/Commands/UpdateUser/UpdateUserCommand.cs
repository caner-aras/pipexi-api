using System.Net;
using MediatR;
using Workforce.Application.Abstractions.Persistence;
using Workforce.Application.Common.Models;
using Workforce.Application.Features.Users.Dtos;
using Workforce.Shared.Errors;
using Workforce.Shared.Results;

namespace Workforce.Application.Features.Users.Commands.UpdateUser;

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
