using WpfGisLearning.Services;

namespace WpfGisLearning.Validation;

/// <summary>
/// 店舗登録・編集・Excelインポートで共通利用する入力検証ロジックです。
/// UIに依存しないため、ViewModelやテストから直接呼び出せます。
/// </summary>
public static class ShopValidation
{
    /// <summary>営業時間を未設定にする場合の選択値です。</summary>
    public const string UnsetOpeningHoursMode = "未設定";

    /// <summary>営業時間を開始・終了時刻で指定する場合の選択値です。</summary>
    public const string SpecifiedOpeningHoursMode = "時間指定";

    /// <summary>画面やExcelで選択できるラーメン種別の一覧です。</summary>
    public static IReadOnlyList<string> RamenTypes { get; } =
    ["醤油", "塩", "味噌", "豚骨", "家系", "二郎系", "つけ麺", "その他"];

    /// <summary>営業時間モードの選択肢です。</summary>
    public static IReadOnlyList<string> OpeningHoursModes { get; } =
    [UnsetOpeningHoursMode, SpecifiedOpeningHoursMode, BusinessHoursStatusCalculator.Open24Hours];

    /// <summary>30分単位で生成した開始・終了時刻の選択肢です。</summary>
    public static IReadOnlyList<string> TimeOptions { get; } = CreateTimeOptions();

    /// <summary>店舗名が入力されているかを検証します。</summary>
    public static string? ValidateName(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "店舗名を入力してください。" : null;

    /// <summary>価格が1円以上かを検証します。</summary>
    public static string? ValidatePrice(decimal value) =>
        value <= 0 ? "価格は1円以上で入力してください。" : null;

    /// <summary>評価が0～5かつ小数第1位までかを検証します。</summary>
    public static string? ValidateRating(double value)
    {
        if (value < 0 || value > 5)
            return "評価は0～5の範囲で入力してください。";

        return Math.Abs(value * 10 - Math.Round(value * 10)) > 1e-9
            ? "評価は小数第1位までで入力してください。"
            : null;
    }

    /// <summary>指定されたラーメン種別が選択肢に存在するかを判定します。</summary>
    public static bool IsValidRamenType(string? value) =>
        RamenTypes.Contains(value ?? string.Empty, StringComparer.Ordinal);

    /// <summary>ラーメン種別の入力値を検証します。</summary>
    public static string? ValidateRamenType(string? value) =>
        string.IsNullOrWhiteSpace(value) || IsValidRamenType(value)
            ? null
            : "ラーメンの種類が不正です。";

    /// <summary>緯度・経度が地図で利用可能な値かを検証します。</summary>
    public static string? ValidateLocation(double latitude, double longitude) =>
        MapCoordinateValidator.IsValid(latitude, longitude)
            ? null
            : "有効な緯度・経度を地図上で指定してください。";

    /// <summary>営業時間モードと時刻の組み合わせが正しいかを検証します。</summary>
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

    /// <summary>
    /// 営業時間の入力値から保存用文字列を生成します。
    /// 検証にも同時に利用し、成功時のみtrueを返します。
    /// </summary>
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

    /// <summary>時刻が30分単位の有効な24時間表記かを判定します。</summary>
    public static bool IsValidTime(string? value) =>
        !string.IsNullOrWhiteSpace(value)
        && TimeSpan.TryParseExact(value.Trim(), @"hh\:mm", null, out var time)
        && time.Minutes is 0 or 30
        && time.Hours < 24;

    /// <summary>00:00～23:30を30分間隔で生成します。</summary>
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
