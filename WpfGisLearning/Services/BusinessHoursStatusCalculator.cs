using System.Globalization;
using System.Text.RegularExpressions;

namespace WpfGisLearning.Services;

public static class BusinessHoursStatusCalculator
{
    private static readonly string[] DayNames = ["日", "月", "火", "水", "木", "金", "土"];
    public const string Open24Hours = "24時間営業";

    public static bool HasOpeningHours(string? openingHours) =>
        !string.IsNullOrWhiteSpace(openingHours);

    public static bool IsOpen(string? openingHours, string? closedDay, DateTime dateTime)
    {
        if (!HasOpeningHours(openingHours)) return false;
        if (IsClosedDay(closedDay, dateTime.DayOfWeek)) return false;

        var normalizedOpeningHours = openingHours!.Trim();
        if (string.Equals(normalizedOpeningHours, Open24Hours, StringComparison.Ordinal)) return true;

        foreach (var (start, end) in ParseTimeRanges(normalizedOpeningHours))
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

    private static IEnumerable<(TimeSpan Start, TimeSpan End)> ParseTimeRanges(string openingHours)
    {
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
