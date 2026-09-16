using WpfGisLearning.Models;
using WpfGisLearning.Services.Interfaces;
using WpfGisLearning.ViewModels;

namespace WpfGisLearning.Tests;

[TestClass]
public class ShopEditViewModelTests
{
    [TestMethod]
    public void NewViewModel_IsNotDirtyInitially()
    {
        var viewModel = CreateViewModel(new TestShopService());

        Assert.IsFalse(viewModel.IsDirty);
    }

    [TestMethod]
    public void LoadNewShop_IsNotDirty()
    {
        var viewModel = CreateViewModel(new TestShopService());

        viewModel.Load(null);

        Assert.IsFalse(viewModel.IsDirty);
    }

    [TestMethod]
    public void ChangingField_MakesViewModelDirty()
    {
        var viewModel = CreateViewModel(new TestShopService());
        viewModel.Load(null);

        viewModel.ShopName = "テスト店舗";

        Assert.IsTrue(viewModel.IsDirty);
    }

    [TestMethod]
    public void RevertingField_ClearsDirtyState()
    {
        var viewModel = CreateViewModel(new TestShopService());
        viewModel.Load(null);

        viewModel.ShopName = "テスト店舗";
        viewModel.ShopName = string.Empty;

        Assert.IsFalse(viewModel.IsDirty);
    }

    [TestMethod]
    public void Save_WithInvalidRequiredValues_DoesNotSave()
    {
        var shopService = new TestShopService();
        var viewModel = CreateViewModel(shopService);

        viewModel.SaveCommand.Execute(null);

        Assert.AreEqual("店舗名を入力してください。", viewModel.ShopNameError);
        Assert.AreEqual("価格は1円以上で入力してください。", viewModel.ShopPriceError);
        Assert.AreEqual("有効な緯度・経度を地図上で指定してください。", viewModel.LocationError);
        Assert.HasCount(0, shopService.Shops);
    }

    [TestMethod]
    public void Save_WithValidValues_AddsShop()
    {
        var shopService = new TestShopService();
        var viewModel = CreateViewModel(shopService);
        viewModel.ShopName = "  テスト店舗  ";
        viewModel.ShopPrice = 1000;
        viewModel.ShopAddress = "東京都";
        viewModel.ShopLatitude = 35.6812;
        viewModel.ShopLongitude = 139.7671;
        viewModel.RamenType = "醤油";
        viewModel.Rating = 4.5;
        viewModel.OpeningHoursMode = "時間指定";
        viewModel.OpeningTime = "11:00";
        viewModel.ClosingTime = "21:30";
        viewModel.ClosedMonday = true;
        viewModel.ClosedTuesday = true;

        viewModel.SaveCommand.ExecuteAsync(null).GetAwaiter().GetResult();

        Assert.HasCount(1, shopService.Shops);
        var shop = shopService.Shops[0];
        Assert.AreEqual("テスト店舗", shop.Name);
        Assert.AreEqual(1000m, shop.Price);
        Assert.AreEqual(35.6812, shop.Latitude);
        Assert.AreEqual(139.7671, shop.Longitude);
        Assert.AreEqual(4.5, shop.Rating);
        Assert.AreEqual("11:00-21:30", shop.OpeningHours);
        Assert.AreEqual("月・火", shop.ClosedDay);
    }

    [TestMethod]
    public void Save_With24HourBusiness_Saves24HourValue()
    {
        var shopService = new TestShopService();
        var viewModel = CreateViewModel(shopService);
        viewModel.ShopName = "24時間店舗";
        viewModel.ShopPrice = 1000;
        viewModel.ShopLatitude = 35.6812;
        viewModel.ShopLongitude = 139.7671;
        viewModel.OpeningHoursMode = "24時間営業";

        viewModel.SaveCommand.ExecuteAsync(null).GetAwaiter().GetResult();

        Assert.HasCount(1, shopService.Shops);
        Assert.AreEqual("24時間営業", shopService.Shops[0].OpeningHours);
    }

    [TestMethod]
    public void Save_WithValidValues_WithoutBusinessHoursLeavesHoursEmpty()
    {
        var shopService = new TestShopService();
        var viewModel = CreateViewModel(shopService);
        viewModel.ShopName = "営業時間未設定店舗";
        viewModel.ShopPrice = 1000;
        viewModel.ShopLatitude = 35.6812;
        viewModel.ShopLongitude = 139.7671;
        viewModel.OpeningHoursMode = "未設定";

        viewModel.SaveCommand.ExecuteAsync(null).GetAwaiter().GetResult();

        Assert.HasCount(1, shopService.Shops);
        Assert.AreEqual(string.Empty, shopService.Shops[0].OpeningHours);
    }

    [TestMethod]
    public void Save_WhenEditingMissingShop_ShowsErrorAndDoesNotSave()
    {
        var shopService = new TestShopService();
        var viewModel = CreateViewModel(shopService);
        viewModel.Load(999);

        viewModel.ShopName = "テスト店舗";
        viewModel.ShopPrice = 1000;
        viewModel.ShopLatitude = 35.6812;
        viewModel.ShopLongitude = 139.7671;
        viewModel.SaveCommand.Execute(null);

        Assert.AreEqual("編集対象の店舗が見つかりません。画面を閉じてもう一度お試しください。", viewModel.ErrorMessage);
        Assert.HasCount(0, shopService.Shops);
    }

    [TestMethod]
    public void SetMainPhoto_ChangesMainPhotoAndMakesDirty()
    {
        var viewModel = CreateViewModel(new TestShopService());
        viewModel.Load(null);
        var firstPhoto = new ShopPhoto { Id = "first", IsMain = true };
        var secondPhoto = new ShopPhoto { Id = "second", IsMain = false };
        viewModel.Photos.Add(firstPhoto);
        viewModel.Photos.Add(secondPhoto);

        viewModel.SetMainPhoto(secondPhoto);

        Assert.IsFalse(firstPhoto.IsMain);
        Assert.IsTrue(secondPhoto.IsMain);
        Assert.IsTrue(viewModel.IsDirty);
        Assert.AreEqual("first", viewModel.Photos[0].Id);
        Assert.AreEqual("second", viewModel.Photos[1].Id);
    }

    private static ShopEditViewModel CreateViewModel(TestShopService shopService) => new(shopService, new TestPhotoService());

    private sealed class TestShopService : IShopService
    {
        public List<Shop> Shops { get; } = [];
        public string GetWelcomeMessage() => string.Empty;
        public IEnumerable<Shop> GetShops() => Shops;
        public void AddShop(Shop shop) => Shops.Add(shop);
        public void UpdateShop(Shop shop)
        {
            var index = Shops.FindIndex(x => x.Id == shop.Id);
            if (index >= 0) Shops[index] = shop;
        }
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
