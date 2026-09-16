using WpfGisLearning.Services;

namespace WpfGisLearning.Validation;

public static class ShopValidation
{
    public const string UnsetOpeningHoursMode = "未設定";
    public const string SpecifiedOpeningHoursMode = "時間指定";

    public static IReadOnlyList<string> RamenTypes { get; } =
    ["醤油", "塩", "味噌", "豚骨", "家系", "二郎系", "つけ麺", "その他"];

    public static IReadOnlyList<string> OpeningHoursModes { get; } =
    [UnsetOpeningHoursMode, SpecifiedOpeningHoursMode, BusinessHoursStatusCalculator.Open24Hours];

    public static IReadOnlyList<string> TimeOptions { get; } = CreateTimeOptions();

    public static string? ValidateName(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "店舗名を入力してください。" : null;

    public static string? ValidatePrice(decimal value) =>
        value <= 0 ? "価格は1円以上で入力してください。" : null;

    public static string? ValidateRating(double value)
    {
        if (value < 0 || value > 5)
            return "評価は0～5の範囲で入力してください。";

        return Math.Abs(value * 10 - Math.Round(value * 10)) > 1e-9
            ? "評価は小数第1位までで入力してください。"
            : null;
    }

    public static bool IsValidRamenType(string? value) =>
        RamenTypes.Contains(value ?? string.Empty, StringComparer.Ordinal);

    public static string? ValidateRamenType(string? value) =>
        string.IsNullOrWhiteSpace(value) || IsValidRamenType(value)
            ? null
            : "ラーメンの種類が不正です。";

    public static string? ValidateLocation(double latitude, double longitude) =>
        MapCoordinateValidator.IsValid(latitude, longitude)
            ? null
            : "有効な緯度・経度を地図上で指定してください。";

    public static string? ValidateOpeningHours(string? mode, string? openingTime, string? closingTime)
    {
        var normalizedMode = mode?.Trim() ?? string.Empty;
        if (!OpeningHoursModes.Contains(normalizedMode, StringComparer.Ordinal))
            return "営業時間モードが不正です。";

        if (!string.Equals(normalizedMode, SpecifiedOpeningHoursMode, StringComparison.Ordinal))
            return null;

        if (!IsValidTime(openingTime) || !IsValidTime(closingTime))
            return "開始時刻・終了時刻は30分単位で入力してください。";

        if (string.Equals(openingTime?.Trim(), closingTime?.Trim(), StringComparison.Ordinal))
            return "開始時刻と終了時刻は異なる時刻を選択してください。24時間営業の場合は「24時間営業」を選択してください。";

        return null;
    }

    public static bool TryBuildOpeningHours(
        string? mode,
        string? openingTime,
        string? closingTime,
        out string openingHours,
        out string? errorMessage)
    {
        openingHours = string.Empty;
        errorMessage = ValidateOpeningHours(mode, openingTime, closingTime);
        if (errorMessage is not null)
            return false;

        var normalizedMode = mode?.Trim() ?? string.Empty;
        openingHours = normalizedMode switch
        {
            UnsetOpeningHoursMode or "" => string.Empty,
            BusinessHoursStatusCalculator.Open24Hours => BusinessHoursStatusCalculator.Open24Hours,
            SpecifiedOpeningHoursMode => $"{openingTime!.Trim()}-{closingTime!.Trim()}",
            _ => string.Empty
        };
        return true;
    }

    public static bool IsValidTime(string? value) =>
        !string.IsNullOrWhiteSpace(value)
        && TimeSpan.TryParseExact(value.Trim(), @"hh\:mm", null, out var time)
        && time.Minutes is 0 or 30
        && time.Hours < 24;

    private static IReadOnlyList<string> CreateTimeOptions()
    {
        var values = new List<string>(48);
        for (var hour = 0; hour < 24; hour++)
        {
            values.Add($"{hour:00}:00");
            values.Add($"{hour:00}:30");
        }
        return values;
    }
}
