using FluentValidation;

namespace Pipexi.Application.Features.UserDevices.Commands.AddUserDevice;

public sealed class AddUserDeviceCommandValidator : AbstractValidator<AddUserDeviceCommand>
{
    public AddUserDeviceCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.FcmToken)
            .NotEmpty()
            .MaximumLength(1000);

        RuleFor(x => x.DeviceType)
            .MaximumLength(100);
    }
}
