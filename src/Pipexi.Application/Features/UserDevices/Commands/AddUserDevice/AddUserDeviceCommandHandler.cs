using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Domain.Entities;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.UserDevices.Commands.AddUserDevice;

public sealed class AddUserDeviceCommandHandler : IRequestHandler<AddUserDeviceCommand, Result<Guid>>
{
    private readonly IUserDeviceRepository _userDeviceRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddUserDeviceCommandHandler(
        IUserDeviceRepository userDeviceRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _userDeviceRepository = userDeviceRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(AddUserDeviceCommand request, CancellationToken cancellationToken)
    {
        var userExists = await _userRepository.ExistsAsync(request.UserId, cancellationToken);
        if (!userExists)
        {
            return Result<Guid>.Failure(
                new AppError("user.not_found", "User not found."),
                404);
        }

        var existingDevice = await _userDeviceRepository.GetByTokenAsync(request.FcmToken, cancellationToken);
        if (existingDevice is not null)
        {
            if (existingDevice.UserId == request.UserId)
            {
                // Already associated with this user
                return Result<Guid>.Success(existingDevice.Id);
            }
            
            // The token exists but for a different user (e.g. user logged out and another logged in).
            // We should update the token to the new user or delete the old one.
            // But UserId in UserDevice is private set and we probably shouldn't change the owner like this without care, 
            // but it's simpler to just delete it and recreate.
            await _userDeviceRepository.DeleteAsync(existingDevice, cancellationToken);
        }

        var device = UserDevice.Create(request.UserId, request.FcmToken, request.DeviceType);

        await _userDeviceRepository.AddAsync(device, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(device.Id, 201);
    }
}
