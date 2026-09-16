using System.Globalization;
using System.Text.RegularExpressions;

namespace WpfGisLearning.Services;

/// <summary>
/// 営業時間と定休日から、指定日時に営業中かどうかを判定するヘルパーです。
/// 画面表示だけでなく、複数の場所から同じ判定ロジックを利用できます。
/// </summary>
public static class BusinessHoursStatusCalculator
{
    /// <summary>曜日名を日曜始まりの配列で保持します。DayOfWeekの数値と対応します。</summary>
    private static readonly string[] DayNames = ["日", "月", "火", "水", "木", "金", "土"];

    /// <summary>24時間営業を表す保存・表示用の文字列です。</summary>
    public const string Open24Hours = "24時間営業";

    /// <summary>営業時間が設定されているかを判定します。</summary>
    public static bool HasOpeningHours(string? openingHours) =>
        !string.IsNullOrWhiteSpace(openingHours);

    /// <summary>
    /// 指定日時が営業時間内で、かつ定休日ではないかを判定します。
    /// 日をまたぐ営業時間も扱えるよう、終了時刻が開始時刻以下なら翌日として計算します。
    /// </summary>
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

    /// <summary>指定曜日が定休日として登録されているかを判定します。</summary>
    private static bool IsClosedDay(string? closedDay, DayOfWeek dayOfWeek)
    {
        if (string.IsNullOrWhiteSpace(closedDay)) return false;

        // 「火曜日」「定休火」「火休み」のような表現でも曜日部分を判定できるように不要語を除去する。
        var normalized = closedDay.Replace("曜日", string.Empty, StringComparison.Ordinal)
            .Replace("定休", string.Empty, StringComparison.Ordinal)
            .Replace("休み", string.Empty, StringComparison.Ordinal);
        return normalized.Contains(DayNames[(int)dayOfWeek], StringComparison.Ordinal);
    }

    /// <summary>
    /// 「11:00-14:00、17:00-21:00」のような営業時間文字列から時刻範囲を抽出します。
    /// </summary>
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
