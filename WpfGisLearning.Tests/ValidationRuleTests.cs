using System.Globalization;
using WpfGisLearning.Models;
using WpfGisLearning.Services.Interfaces;
using WpfGisLearning.Validation;
using WpfGisLearning.ViewModels;

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
    public void AddPhoto_RejectsPhoto_WhenMaximumCountReached()
    {
        var viewModel = new TestableShopEditViewModel();
        viewModel.Photos.Add(new ShopPhoto());
        while (viewModel.Photos.Count < ShopEditViewModel.MaxPhotoCount)
            viewModel.Photos.Add(new ShopPhoto());

        viewModel.AddPhoto("missing.jpg");

        Assert.AreEqual($"写真は最大{ShopEditViewModel.MaxPhotoCount}枚まで登録できます。", viewModel.PhotoCountError);
        Assert.HasCount(ShopEditViewModel.MaxPhotoCount, viewModel.Photos);
    }

    private sealed class TestableShopEditViewModel : ShopEditViewModel
    {
        public TestableShopEditViewModel() : base(new TestShopService(), new TestPhotoService()) { }
    }

    private sealed class TestShopService : IShopService
    {
        public string GetWelcomeMessage() => string.Empty;
        public IEnumerable<Shop> GetShops() => [];
        public void AddShop(Shop shop) { }
        public void UpdateShop(Shop shop) { }
        public void DeleteShop(int id) { }
        public void ToggleFavorite(int id) { }
    }

    private sealed class TestPhotoService : IPhotoService
    {
        public string GetPhotoPath(Shop shop, ShopPhoto photo) => photo.FileName;
        public void SavePhotos(Shop shop, IEnumerable<(string SourcePath, ShopPhoto Photo)> photos) { }
        public void DeletePhoto(Shop shop, ShopPhoto photo) { }
        public void DeleteShopPhotos(Shop shop) { }
    }
}
