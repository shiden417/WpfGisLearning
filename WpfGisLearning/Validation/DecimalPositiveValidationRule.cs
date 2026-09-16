using System.Globalization;
using System.Windows.Controls;

namespace WpfGisLearning.Validation;

public sealed class DecimalPositiveValidationRule : ValidationRule
{
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
