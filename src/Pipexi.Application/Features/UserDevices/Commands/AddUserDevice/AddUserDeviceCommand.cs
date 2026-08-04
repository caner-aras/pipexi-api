using MediatR;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.UserDevices.Commands.AddUserDevice;

public sealed record AddUserDeviceCommand(
    Guid UserId,
    string FcmToken,
    string? DeviceType) : IRequest<Result<Guid>>;
