using System.Globalization;
using System.Windows.Controls;

namespace WpfGisLearning.Validation;

public sealed class DoubleRangeValidationRule : ValidationRule
{
    public double Minimum { get; set; }
    public double Maximum { get; set; }

    public override ValidationResult Validate(object? value, CultureInfo cultureInfo)
    {
        if (value is null || string.IsNullOrWhiteSpace(value.ToString()))
            return new ValidationResult(false, "数値を入力してください。");

        if (!double.TryParse(value.ToString(), NumberStyles.Float, cultureInfo, out var number))
            return new ValidationResult(false, "数値を入力してください。");

        return number >= Minimum && number <= Maximum
            ? ValidationResult.ValidResult
            : new ValidationResult(false, $"{Minimum:0}～{Maximum:0}の範囲で入力してください。");
    }
}
