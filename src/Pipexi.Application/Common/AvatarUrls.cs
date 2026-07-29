namespace Pipexi.Application.Common;

public static class AvatarUrls
{
    public static string Generate(Guid userId)
    {
        var seed = userId.ToString("N");
        var backgrounds = new[] { "ffedd5", "d1fae5", "fef3c7", "ccfbf1", "dcfce7" };
        var background = backgrounds[Math.Abs(seed.GetHashCode()) % backgrounds.Length];
        return $"https://api.dicebear.com/9.x/notionists/png?seed={seed}&size=128&backgroundColor={background}";
    }

    public static string Resolve(Guid userId, string? avatarUrl)
    {
        return string.IsNullOrWhiteSpace(avatarUrl)
            ? Generate(userId)
            : avatarUrl.Trim();
    }
}
