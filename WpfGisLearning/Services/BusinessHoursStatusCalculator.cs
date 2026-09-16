using System.Globalization;
using System.Text.RegularExpressions;

namespace WpfGisLearning.Services;

public static class BusinessHoursStatusCalculator
{
    private static readonly string[] DayNames = ["日", "月", "火", "水", "木", "金", "土"];

    public static bool IsOpen(string? openingHours, string? closedDay, DateTime dateTime)
    {
        if (IsClosedDay(closedDay, dateTime.DayOfWeek)) return false;

        foreach (var (start, end) in ParseTimeRanges(openingHours))
        {
            var startTime = dateTime.Date.Add(start);
            var endTime = dateTime.Date.Add(end);
            if (end <= start) endTime = endTime.AddDays(1);
            if (dateTime >= startTime && dateTime < endTime) return true;
        }

        return false;
    }

    private static bool IsClosedDay(string? closedDay, DayOfWeek dayOfWeek)
    {
        if (string.IsNullOrWhiteSpace(closedDay)) return false;
        var normalized = closedDay.Replace("曜日", string.Empty, StringComparison.Ordinal)
            .Replace("定休", string.Empty, StringComparison.Ordinal)
            .Replace("休み", string.Empty, StringComparison.Ordinal);
        return normalized.Contains(DayNames[(int)dayOfWeek], StringComparison.Ordinal);
    }

    private static IEnumerable<(TimeSpan Start, TimeSpan End)> ParseTimeRanges(string? openingHours)
    {
        if (string.IsNullOrWhiteSpace(openingHours)) yield break;

        var matches = Regex.Matches(
            openingHours,
            @"(?<start>\d{1,2}:\d{2})\s*(?:-|ー|−|–|〜|~)\s*(?<end>\d{1,2}:\d{2})",
            RegexOptions.CultureInvariant);

        foreach (Match match in matches)
        {
            if (!TimeSpan.TryParseExact(match.Groups["start"].Value, @"h\:mm", CultureInfo.InvariantCulture, out var start)) continue;
            if (!TimeSpan.TryParseExact(match.Groups["end"].Value, @"h\:mm", CultureInfo.InvariantCulture, out var end)) continue;
            yield return (start, end);
        }
    }
}
