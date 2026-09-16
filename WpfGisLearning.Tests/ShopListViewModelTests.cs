using WpfGisLearning.Models;
using WpfGisLearning.Services.Interfaces;
using WpfGisLearning.ViewModels;

namespace WpfGisLearning.Tests;

[TestClass]
public class ShopListViewModelTests
{
    [TestMethod]
    public void Filter_SearchKeywordMatchesNameAddressOrRamenType()
    {
        var viewModel = CreateViewModel(
            new Shop { Id = 1, Name = "東京ラーメン", Address = "千代田区", RamenType = "醤油" },
            new Shop { Id = 2, Name = "大阪店", Address = "新宿区", RamenType = "味噌" },
            new Shop { Id = 3, Name = "福岡店", Address = "博多区", RamenType = "豚骨" });

        viewModel.SearchKeyword = "東京";
        Assert.HasCount(1, GetVisibleShops(viewModel));
        Assert.AreEqual(1, GetVisibleShops(viewModel).Single().Id);
        viewModel.SearchKeyword = "新宿";
        Assert.AreEqual(2, GetVisibleShops(viewModel).Single().Id);
        viewModel.SearchKeyword = "豚骨";
        Assert.AreEqual(3, GetVisibleShops(viewModel).Single().Id);
    }

    [TestMethod]
    public void Filter_PriceAndFavoriteConditionsAreApplied()
    {
        var viewModel = CreateViewModel(
            new Shop { Id = 1, Name = "A", Price = 900, IsFavorite = true },
            new Shop { Id = 2, Name = "B", Price = 1200, IsFavorite = true },
            new Shop { Id = 3, Name = "C", Price = 900, IsFavorite = false });

        viewModel.SelectedPriceFilter = "1000円以下";
        Assert.IsTrue(GetVisibleShops(viewModel).Select(shop => shop.Id).SequenceEqual([1, 3]));
        viewModel.FavoriteOnly = true;
        Assert.AreEqual(1, GetVisibleShops(viewModel).Single().Id);
    }

    [TestMethod]
    public void Filter_RamenTypeConditionIsApplied()
    {
        var viewModel = CreateViewModel(
            new Shop { Id = 1, Name = "A", RamenType = "醤油" },
            new Shop { Id = 2, Name = "B", RamenType = "味噌" });
        viewModel.SelectedRamenType = "味噌";
        Assert.AreEqual(2, GetVisibleShops(viewModel).Single().Id);
    }

    [TestMethod]
    public void Filter_NearbyConditionUsesSelectedRadius()
    {
        var viewModel = CreateViewModel(
            new Shop { Id = 1, Name = "近い店舗", Latitude = 35.0000, Longitude = 139.0000 },
            new Shop { Id = 2, Name = "遠い店舗", Latitude = 35.0200, Longitude = 139.0000 });
        viewModel.SetNearbyLocation(35.0000, 139.0000);
        viewModel.SelectedNearbyRadius = "1km";
        Assert.AreEqual(1, GetVisibleShops(viewModel).Single().Id);
    }

    [TestMethod]
    public void Sort_ByRatingAndPriceAndNameUsesSelectedOrder()
    {
        var viewModel = CreateViewModel(
            new Shop { Id = 1, Name = "B", Price = 1000, Rating = 4.0 },
            new Shop { Id = 2, Name = "A", Price = 1200, Rating = 4.5 },
            new Shop { Id = 3, Name = "C", Price = 900, Rating = 4.5 });

        viewModel.SortByRating = true;
        Assert.IsTrue(GetVisibleShops(viewModel).Select(shop => shop.Id).SequenceEqual([2, 3, 1]));

        viewModel.SortByPrice = true;
        Assert.IsTrue(GetVisibleShops(viewModel).Select(shop => shop.Id).SequenceEqual([3, 1, 2]));

        viewModel.SortByName = true;
        Assert.IsTrue(GetVisibleShops(viewModel).Select(shop => shop.Id).SequenceEqual([2, 1, 3]));
    }

    [TestMethod]
    public void ToggleFavorite_UpdatesShopAndRaisesShopsChanged()
    {
        var viewModel = CreateViewModel(new Shop { Id = 1, Name = "A", IsFavorite = false });
        var eventRaised = false;
        viewModel.ShopsChanged += (_, _) => eventRaised = true;

        viewModel.ToggleFavoriteCommand.Execute(viewModel.Shops[0]);

        Assert.IsTrue(viewModel.Shops[0].IsFavorite);
        Assert.IsTrue(eventRaised);
    }

    [TestMethod]
    public void SelectShopById_SelectsExistingShopAndRaisesEvent()
    {
        var viewModel = CreateViewModel(
            new Shop { Id = 1, Name = "A" },
            new Shop { Id = 2, Name = "B" });
        Shop? selected = null;
        viewModel.SelectedShopChanged += (_, shop) => selected = shop;

        viewModel.SelectShopById(2);

        Assert.AreEqual(2, viewModel.SelectedShop?.Id);
        Assert.AreEqual(2, selected?.Id);
    }

    [TestMethod]
    public void OpenSelectedShop_NavigatesToSelectedShopDetail()
    {
        var navigation = new FakeNavigationService();
        var viewModel = CreateViewModel(navigation, new Shop { Id = 42, Name = "A" });
        viewModel.SelectedShop = viewModel.Shops[0];

        viewModel.OpenSelectedShopCommand.Execute(null);

        Assert.AreEqual(42, navigation.DetailShopId);
    }

    [TestMethod]
    public void ClearSearch_RestoresDefaultFilters()
    {
        var viewModel = CreateViewModel(
            new Shop { Id = 1, Name = "A", RamenType = "醤油", Price = 900, IsFavorite = true },
            new Shop { Id = 2, Name = "B", RamenType = "味噌", Price = 1500 });
        viewModel.SearchKeyword = "A";
        viewModel.SelectedRamenType = "醤油";
        viewModel.SelectedPriceFilter = "1000円以下";
        viewModel.FavoriteOnly = true;
        viewModel.NearbyOnly = true;
        viewModel.SortByName = true;
        viewModel.ClearSearchCommand.Execute(null);
        Assert.AreEqual(string.Empty, viewModel.SearchKeyword);
        Assert.AreEqual("すべて", viewModel.SelectedRamenType);
        Assert.AreEqual("すべて", viewModel.SelectedPriceFilter);
        Assert.IsFalse(viewModel.FavoriteOnly);
        Assert.IsFalse(viewModel.NearbyOnly);
        Assert.IsFalse(viewModel.SortByName);
        Assert.HasCount(2, GetVisibleShops(viewModel));
    }

    private static ShopListViewModel CreateViewModel(params Shop[] shops) =>
        CreateViewModel(new FakeNavigationService(), shops);

    private static ShopListViewModel CreateViewModel(FakeNavigationService navigation, params Shop[] shops) =>
        new(new FakeShopService(shops), navigation);

    private static List<Shop> GetVisibleShops(ShopListViewModel viewModel) =>
        viewModel.ShopsView.Cast<Shop>().ToList();

    private sealed class FakeShopService : IShopService
    {
        private readonly List<Shop> _shops;
        public FakeShopService(IEnumerable<Shop> shops) => _shops = shops.ToList();
        public string GetWelcomeMessage() => string.Empty;
        public IEnumerable<Shop> GetShops() => _shops;
        public void AddShop(Shop shop) => _shops.Add(shop);
        public void UpdateShop(Shop shop) { }
        public void DeleteShop(int id) => _shops.RemoveAll(shop => shop.Id == id);
        public void ToggleFavorite(int id)
        {
            var shop = _shops.FirstOrDefault(x => x.Id == id);
            if (shop is not null) shop.IsFavorite = !shop.IsFavorite;
        }
    }

    private sealed class FakeNavigationService : INavigationService
    {
        public int? DetailShopId { get; private set; }
        public void NavigateToDetail(int id) => DetailShopId = id;
        public void NavigateToShopEdit(int? id = null) { }
        public void NavigateToShopList() { }
        public void NavigateToShopPageFrame() { }
    }
}
