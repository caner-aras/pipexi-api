using TimeZoneConverter;

namespace Pipexi.Domain.Time;

public static class IanaTimeZone
{
    public static bool IsValid(string timezone)
    {
        if (string.IsNullOrWhiteSpace(timezone))
        {
            return false;
        }

        var normalizedTimezone = timezone.Trim();

        return TZConvert.KnownIanaTimeZoneNames
            .Any(x => x.Equals(normalizedTimezone, StringComparison.OrdinalIgnoreCase));
    }
}