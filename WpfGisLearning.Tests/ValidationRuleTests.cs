using System.Globalization;
using WpfGisLearning.Validation;

namespace WpfGisLearning.Tests;

[TestClass]
public class ValidationRuleTests
{
    private static readonly CultureInfo Culture = CultureInfo.CurrentCulture;

    [TestMethod]
    public void DecimalPositiveValidationRule_RejectsZero()
    {
        var rule = new DecimalPositiveValidationRule();

        var result = rule.Validate("0", Culture);

        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public void DecimalPositiveValidationRule_AcceptsPositiveValue()
    {
        var rule = new DecimalPositiveValidationRule();

        var result = rule.Validate("1000", Culture);

        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void DoubleRangeValidationRule_RejectsValueOutsideRange()
    {
        var rule = new DoubleRangeValidationRule { Minimum = 0, Maximum = 5 };

        var result = rule.Validate("5.1", Culture);

        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public void DoubleRangeValidationRule_AcceptsValueInsideRange()
    {
        var rule = new DoubleRangeValidationRule { Minimum = 0, Maximum = 5 };

        var result = rule.Validate("4.5", Culture);

        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void AddPhoto_StopsAtMaximumPhotoCount()
    {
        var viewModel = new ShopEditViewModelTests.TestableShopEditViewModel();

        for (var i = 0; i < ShopEditViewModel.MaxPhotoCount; i++)
            viewModel.AddPhoto($"missing-{i}.jpg");

        Assert.HasCount(0, viewModel.Photos);
    }
}
