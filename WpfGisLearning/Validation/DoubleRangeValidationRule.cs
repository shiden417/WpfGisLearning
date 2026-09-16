using System.Globalization;
using System.Windows.Controls;

namespace WpfGisLearning.Validation;

/// <summary>
/// WPFのBindingでdouble値の範囲と小数桁数を検証するValidationRuleです。
/// TextBoxの途中入力も扱えるよう、末尾の小数点などを考慮しています。
/// </summary>
public sealed class DoubleRangeValidationRule : ValidationRule
{
    /// <summary>許可する最小値です。</summary>
    public double Minimum { get; set; }

    /// <summary>許可する最大値です。</summary>
    public double Maximum { get; set; }

    /// <summary>許可する小数桁数です。未指定なら桁数制限を行いません。</summary>
    public int? DecimalPlaces { get; set; }

    /// <summary>
    /// 入力文字列を解析し、指定範囲と小数桁数を満たすか判定します。
    /// </summary>
    public override ValidationResult Validate(object? value, CultureInfo cultureInfo)
    {
        var text = value?.ToString() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(text))
            return new ValidationResult(false, "数値を入力してください。");

        var separator = cultureInfo.NumberFormat.NumberDecimalSeparator;

        // 「12.」のような入力途中の状態をエラーにしないため、整数部分だけを先に検証する。
        if (DecimalPlaces.HasValue && text.EndsWith(separator, StringComparison.Ordinal))
        {
            var integerPart = text[..^separator.Length];
            if (integerPart.Length == 0)
                return ValidationResult.ValidResult;

            if (double.TryParse(integerPart, NumberStyles.Integer, cultureInfo, out var partialNumber))
                return partialNumber >= Minimum && partialNumber <= Maximum
                    ? ValidationResult.ValidResult
                    : new ValidationResult(false, $"{Minimum:0.0}～{Maximum:0.0}の範囲で入力してください。");
        }

        // 「.」だけの状態も、次の数字が入力される途中状態として許可する。
        if (text == separator)
            return DecimalPlaces.HasValue ? ValidationResult.ValidResult : new ValidationResult(false, "数値を入力してください。");

        if (!double.TryParse(text, NumberStyles.Float, cultureInfo, out var number))
            return new ValidationResult(false, "数値を入力してください。");

        if (number < Minimum || number > Maximum)
            return new ValidationResult(false, $"{Minimum:0.0}～{Maximum:0.0}の範囲で入力してください。");

        if (DecimalPlaces.HasValue)
        {
            var separatorIndex = text.IndexOf(separator, StringComparison.Ordinal);
            if (separatorIndex >= 0)
            {
                var decimalDigitCount = text[(separatorIndex + separator.Length)..].Length;
                if (decimalDigitCount > DecimalPlaces.Value)
                    return new ValidationResult(false, $"小数第{DecimalPlaces.Value}位までで入力してください。");
            }
        }

        return ValidationResult.ValidResult;
    }
}
