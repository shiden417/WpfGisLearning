using System.Globalization;
using System.Windows.Controls;

namespace WpfGisLearning.Validation;

public sealed class DoubleRangeValidationRule : ValidationRule
{
    public double Minimum { get; set; }
    public double Maximum { get; set; }
    public int? DecimalPlaces { get; set; }

    public override ValidationResult Validate(object? value, CultureInfo cultureInfo)
    {
        var text = value?.ToString() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(text))
            return new ValidationResult(false, "数値を入力してください。");

        var separator = cultureInfo.NumberFormat.NumberDecimalSeparator;
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
