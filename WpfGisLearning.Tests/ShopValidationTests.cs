using WpfGisLearning.Services;
using WpfGisLearning.Validation;

namespace WpfGisLearning.Tests;

[TestClass]
public class ShopValidationTests
{
    [TestMethod]
    public void ValidateName_RejectsBlank()
    {
        Assert.AreEqual("店舗名を入力してください。", ShopValidation.ValidateName("  "));
    }

    [TestMethod]
    public void ValidatePrice_RejectsZero()
    {
        Assert.AreEqual("価格は1円以上で入力してください。", ShopValidation.ValidatePrice(0));
    }

    [TestMethod]
    public void ValidateRating_AcceptsOneDecimalPlace()
    {
        Assert.IsNull(ShopValidation.ValidateRating(4.5));
    }

    [TestMethod]
    public void ValidateRating_RejectsMoreThanOneDecimalPlace()
    {
        Assert.AreEqual("評価は小数第1位までで入力してください。", ShopValidation.ValidateRating(4.55));
    }

    [TestMethod]
    public void ValidateRamenType_RejectsUnknownType()
    {
        Assert.AreEqual("ラーメンの種類が不正です。", ShopValidation.ValidateRamenType("豚汁"));
    }

    [TestMethod]
    public void ValidateLocation_RejectsInvalidCoordinates()
    {
        Assert.AreEqual("有効な緯度・経度を地図上で指定してください。", ShopValidation.ValidateLocation(91, 139));
    }

    [TestMethod]
    public void ValidateOpeningHours_AcceptsUnsetMode()
    {
        Assert.IsNull(ShopValidation.ValidateOpeningHours(ShopValidation.UnsetOpeningHoursMode, "", ""));
    }

    [TestMethod]
    public void ValidateOpeningHours_RejectsEqualTimes()
    {
        var error = ShopValidation.ValidateOpeningHours(ShopValidation.SpecifiedOpeningHoursMode, "11:00", "11:00");

        Assert.AreEqual(
            "開始時刻と終了時刻は異なる時刻を選択してください。24時間営業の場合は「24時間営業」を選択してください。",
            error);
    }

    [TestMethod]
    public void ValidateOpeningHours_RejectsNonHalfHourTime()
    {
        Assert.AreEqual(
            "開始時刻・終了時刻は30分単位で入力してください。",
            ShopValidation.ValidateOpeningHours(ShopValidation.SpecifiedOpeningHoursMode, "11:10", "21:00"));
    }

    [TestMethod]
    public void TryBuildOpeningHours_BuildsSpecifiedHours()
    {
        var result = ShopValidation.TryBuildOpeningHours(
            ShopValidation.SpecifiedOpeningHoursMode,
            "11:00",
            "21:30",
            out var openingHours,
            out var errorMessage);

        Assert.IsTrue(result);
        Assert.AreEqual("11:00-21:30", openingHours);
        Assert.IsNull(errorMessage);
    }

    [TestMethod]
    public void TryBuildOpeningHours_BuildsOpen24Hours()
    {
        var result = ShopValidation.TryBuildOpeningHours(
            BusinessHoursStatusCalculator.Open24Hours,
            "",
            "",
            out var openingHours,
            out var errorMessage);

        Assert.IsTrue(result);
        Assert.AreEqual(BusinessHoursStatusCalculator.Open24Hours, openingHours);
        Assert.IsNull(errorMessage);
    }

    [TestMethod]
    public void TimeOptions_ContainsEveryHalfHour()
    {
        Assert.AreEqual(48, ShopValidation.TimeOptions.Count);
        CollectionAssert.Contains(ShopValidation.TimeOptions.ToArray(), "00:00");
        CollectionAssert.Contains(ShopValidation.TimeOptions.ToArray(), "23:30");
    }
}
