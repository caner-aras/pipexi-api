using Pipexi.Application.Features.Users.Dtos;
using Pipexi.Domain.Entities;

namespace Pipexi.Application.Features.Users;

internal static class UserMappings
{
    public static UserDto ToDto(this User user)
    {
        return new UserDto(
            user.Id,
            user.AuthProviderId,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Phone,
            user.AvatarUrl);
    }
}
