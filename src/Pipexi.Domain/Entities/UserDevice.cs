using Pipexi.Domain.Entities;

namespace Pipexi.Domain.Entities;

public sealed class UserDevice : BaseEntity
{
    private UserDevice(
        Guid id,
        Guid userId,
        string fcmToken,
        string? deviceType,
        string status,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt = null)
        : base(id, status, createdAt)
    {
        UserId = userId;
        FcmToken = fcmToken;
        DeviceType = deviceType;
        UpdatedAt = updatedAt;
    }

    public Guid UserId { get; private set; }
    public string FcmToken { get; private set; }
    public string? DeviceType { get; private set; }

    public static UserDevice Create(Guid userId, string fcmToken, string? deviceType)
    {
        return new UserDevice(
            Guid.NewGuid(),
            userId,
            fcmToken.Trim(),
            string.IsNullOrWhiteSpace(deviceType) ? null : deviceType.Trim().ToLowerInvariant(),
            "active",
            DateTimeOffset.UtcNow);
    }

    public void UpdateToken(string fcmToken)
    {
        FcmToken = fcmToken.Trim();
        Touch();
    }
}
