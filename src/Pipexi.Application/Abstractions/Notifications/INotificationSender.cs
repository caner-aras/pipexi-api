namespace Pipexi.Application.Abstractions.Notifications;

public interface INotificationSender
{
    Task SendAsync(string destination, string message, CancellationToken cancellationToken = default);
}
