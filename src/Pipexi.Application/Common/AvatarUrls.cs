namespace Pipexi.Application.Common;

public static class AvatarUrls
{
    public static string Generate(Guid userId)
    {
        return $"https://api.dicebear.com/9.x/notionists/png?seed={userId:N}&size=128";
    }

    public static string Resolve(Guid userId, string? avatarUrl)
    {
        return string.IsNullOrWhiteSpace(avatarUrl)
            ? Generate(userId)
            : avatarUrl.Trim();
    }
}
