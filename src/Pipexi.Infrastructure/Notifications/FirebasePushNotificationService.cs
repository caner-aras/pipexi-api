using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Pipexi.Application.Abstractions.Notifications;
using Polly;
using Polly.Retry;

namespace Pipexi.Infrastructure.Notifications;

public sealed class FirebasePushNotificationService : IPushNotificationService
{
    private readonly ILogger<FirebasePushNotificationService> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;

    public FirebasePushNotificationService(
        IConfiguration configuration,
        ILogger<FirebasePushNotificationService> logger)
    {
        _logger = logger;

        InitializeFirebase(configuration);

        _retryPolicy = Policy
            .Handle<FirebaseMessagingException>(ex => 
                ex.MessagingErrorCode == MessagingErrorCode.Internal || 
                ex.MessagingErrorCode == MessagingErrorCode.Unavailable ||
                ex.MessagingErrorCode == MessagingErrorCode.QuotaExceeded)
            .Or<HttpRequestException>()
            .WaitAndRetryAsync(
                3,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning(
                        exception,
                        "Failed to send push notification. Retrying {RetryCount} in {TimeSpan}...",
                        retryCount,
                        timeSpan);
                });
    }

    private void InitializeFirebase(IConfiguration configuration)
    {
        if (FirebaseApp.DefaultInstance != null)
        {
            return;
        }

        var credentialsPath = configuration["Firebase:CredentialsPath"];
        
        try
        {
            if (!string.IsNullOrWhiteSpace(credentialsPath))
            {
                if (!Path.IsPathRooted(credentialsPath))
                {
                    credentialsPath = Path.Combine(AppContext.BaseDirectory, credentialsPath);
                }

                if (File.Exists(credentialsPath))
                {
                    FirebaseApp.Create(new AppOptions
                    {
                        Credential = GoogleCredential.FromFile(credentialsPath)
                    });
                    _logger.LogInformation("FirebaseApp initialized using credentials file at {Path}", credentialsPath);
                }
                else
                {
                    _logger.LogWarning("Firebase credentials file not found at {Path}. Falling back to Default Credentials.", credentialsPath);
                    FirebaseApp.Create(new AppOptions { Credential = GoogleCredential.GetApplicationDefault(), ProjectId = "pipexi-5feca" });
                }
            }
            else
            {
                // Fallback to Application Default Credentials (e.g. environment variable GOOGLE_APPLICATION_CREDENTIALS)
                FirebaseApp.Create(new AppOptions
                {
                    Credential = GoogleCredential.GetApplicationDefault()
                });
                _logger.LogInformation("FirebaseApp initialized using Application Default Credentials.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize FirebaseApp. Push notifications may not work.");
        }
    }

    public async Task SendPushNotificationAsync(
        IReadOnlyCollection<string> deviceTokens,
        string title,
        string body,
        IReadOnlyDictionary<string, string>? data = null,
        CancellationToken cancellationToken = default)
    {
        if (deviceTokens.Count == 0)
        {
            return;
        }

        if (FirebaseApp.DefaultInstance == null)
        {
            _logger.LogWarning("Cannot send push notification because FirebaseApp is not initialized.");
            return;
        }

        var message = new MulticastMessage
        {
            Tokens = deviceTokens.ToList(),
            Notification = new Notification
            {
                Title = title,
                Body = body
            },
            Data = data?.ToDictionary(x => x.Key, x => x.Value) ?? new Dictionary<string, string>()
        };

        try
        {
            await _retryPolicy.ExecuteAsync(async () =>
            {
                var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message, cancellationToken);
                
                if (response.FailureCount > 0)
                {
                    for (int i = 0; i < response.Responses.Count; i++)
                    {
                        if (!response.Responses[i].IsSuccess)
                        {
                            var error = response.Responses[i].Exception;
                            var failedToken = deviceTokens.ElementAtOrDefault(i);
                            _logger.LogWarning(
                                error,
                                "Failed to send notification to token {Token}. Reason: {Reason}",
                                failedToken,
                                error.Message);
                        }
                    }
                }
                
                _logger.LogInformation(
                    "Successfully sent push notification to {SuccessCount} out of {TotalCount} devices.",
                    response.SuccessCount,
                    deviceTokens.Count);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send push notification after retries. Title: {Title}", title);
        }
    }
}
