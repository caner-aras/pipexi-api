namespace Pipexi.Application.Abstractions.Notifications;

public interface IPushNotificationService
{
    Task SendPushNotificationAsync(
        IReadOnlyCollection<string> deviceTokens,
        string title,
        string body,
        IReadOnlyDictionary<string, string>? data = null,
        CancellationToken cancellationToken = default);
}
