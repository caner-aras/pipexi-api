namespace Pipexi.Application.Features.Announcements;

internal static class AnnouncementAudience
{
    public const string All = "all";
    public const string Location = "location";
    public const string Role = "role";
    public const string Member = "member";
    public const string Team = "team";

    public static readonly HashSet<string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        All,
        Location,
        Role,
        Member,
        Team
    };

    public static string Normalize(string audienceType) =>
        audienceType.Trim().ToLowerInvariant();

    public static bool IsAll(string? audienceType) =>
        string.Equals(Normalize(audienceType ?? string.Empty), All, StringComparison.OrdinalIgnoreCase);

    public static bool RequiresAudienceId(string audienceType) =>
        !IsAll(audienceType);
}
