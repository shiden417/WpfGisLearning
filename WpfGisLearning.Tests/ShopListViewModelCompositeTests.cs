using WpfGisLearning.Models;
using WpfGisLearning.Services.Interfaces;
using WpfGisLearning.ViewModels;

namespace WpfGisLearning.Tests;

[TestClass]
public class ShopListViewModelCompositeTests
{
    [TestMethod]
    public void Filter_AppliesRamenTypePriceAndFavoriteTogether()
    {
        var viewModel = CreateViewModel(
            new Shop { Id = 1, Name = "A", RamenType = "醤油", Price = 900, IsFavorite = true },
            new Shop { Id = 2, Name = "B", RamenType = "醤油", Price = 1400, IsFavorite = true },
            new Shop { Id = 3, Name = "C", RamenType = "味噌", Price = 900, IsFavorite = true },
            new Shop { Id = 4, Name = "D", RamenType = "醤油", Price = 900, IsFavorite = false });

        viewModel.SelectedRamenType = "醤油";
        viewModel.SelectedPriceFilter = "1000円以下";
        viewModel.FavoriteOnly = true;

        Assert.HasCount(1, GetVisibleShops(viewModel));
        Assert.AreEqual(1, GetVisibleShops(viewModel).Single().Id);
    }

    [TestMethod]
    public void Filter_AppliesKeywordAndNearbyConditionsTogether()
    {
        var viewModel = CreateViewModel(
            new Shop { Id = 1, Name = "東京駅店", Address = "東京駅", Latitude = 35.6812, Longitude = 139.7671 },
            new Shop { Id = 2, Name = "東京郊外店", Address = "東京", Latitude = 35.7200, Longitude = 139.7671 },
            new Shop { Id = 3, Name = "新宿店", Address = "新宿", Latitude = 35.6896, Longitude = 139.7006 });

        viewModel.SetNearbyLocation(35.6812, 139.7671);
        viewModel.SelectedNearbyRadius = "3km";
        viewModel.SearchKeyword = "東京";

        Assert.HasCount(1, GetVisibleShops(viewModel));
        Assert.AreEqual(1, GetVisibleShops(viewModel).Single().Id);
    }

    [TestMethod]
    public void Filter_AppliesNearbyRadiusAndFavoriteTogether()
    {
        var viewModel = CreateViewModel(
            new Shop { Id = 1, Name = "近いお気に入り", Latitude = 35.0000, Longitude = 139.0000, IsFavorite = true },
            new Shop { Id = 2, Name = "近い非お気に入り", Latitude = 35.0010, Longitude = 139.0000, IsFavorite = false },
            new Shop { Id = 3, Name = "遠いお気に入り", Latitude = 35.0300, Longitude = 139.0000, IsFavorite = true });

        viewModel.SetNearbyLocation(35.0000, 139.0000);
        viewModel.SelectedNearbyRadius = "3km";
        viewModel.FavoriteOnly = true;

        Assert.HasCount(1, GetVisibleShops(viewModel));
        Assert.AreEqual(1, GetVisibleShops(viewModel).Single().Id);
    }

    [TestMethod]
    public void Sort_ByRatingAndPriceUsesRatingThenPrice()
    {
        var viewModel = CreateViewModel(
            new Shop { Id = 1, Name = "A", Rating = 4.5, Price = 1200 },
            new Shop { Id = 2, Name = "B", Rating = 4.5, Price = 900 },
            new Shop { Id = 3, Name = "C", Rating = 4.0, Price = 800 });

        viewModel.SortByRating = true;
        viewModel.SortByPrice = true;

        Assert.IsTrue(GetVisibleShops(viewModel).Select(shop => shop.Id).SequenceEqual([2, 1, 3]));
    }

    private static ShopListViewModel CreateViewModel(params Shop[] shops) =>
        new(new FakeShopService(shops), new FakeNavigationService());

    private static List<Shop> GetVisibleShops(ShopListViewModel viewModel) =>
        viewModel.ShopsView.Cast<Shop>().ToList();

    private sealed class FakeShopService(params Shop[] shops) : IShopService
    {
        private readonly List<Shop> _shops = shops.ToList();
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
        public void ReplaceAll(IEnumerable<Shop> shops)
        {
            _shops.Clear();
            _shops.AddRange(shops);
        }
    }

    private sealed class FakeNavigationService : INavigationService
    {
        public void NavigateToDetail(int id) { }
        public void NavigateToShopEdit(int? id = null) { }
        public void NavigateToShopList() { }
        public void NavigateToShopPageFrame() { }
    }
}
