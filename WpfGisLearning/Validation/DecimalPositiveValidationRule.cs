using System.Globalization;
using System.Windows.Controls;

namespace WpfGisLearning.Validation;

/// <summary>
/// WPFのBindingで価格入力を検証するValidationRuleです。
/// TextBoxに入力された値が正の10進数として解釈できるか確認します。
/// </summary>
public sealed class DecimalPositiveValidationRule : ValidationRule
{
    /// <summary>
    /// Binding対象の値を検証し、1円以上の有効な価格かを返します。
    /// </summary>
    /// <param name="value">TextBoxから渡される入力値です。</param>
    /// <param name="cultureInfo">数値解析に使用するカルチャです。</param>
    /// <returns>有効ならValidResult、不正なら画面表示用のエラーを返します。</returns>
    public override ValidationResult Validate(object? value, CultureInfo cultureInfo)
    {
        if (value is null || string.IsNullOrWhiteSpace(value.ToString()))
            return new ValidationResult(false, "価格を入力してください。");

        if (!decimal.TryParse(value.ToString(), NumberStyles.Number, cultureInfo, out var number))
            return new ValidationResult(false, "価格は数値で入力してください。");

        return number > 0
            ? ValidationResult.ValidResult
            : new ValidationResult(false, "価格は1円以上で入力してください。");
    }
}
