using WpfGisLearning.Filters;
using WpfGisLearning.Models;

namespace WpfGisLearning.Tests;

[TestClass]
public class ShopFilterTests
{
    [TestMethod]
    public void Matches_EmptyKeywordMatchesAllShops()
    {
        var shop = CreateShop();

        Assert.IsTrue(Matches(shop, searchKeyword: ""));
        Assert.IsTrue(Matches(shop, searchKeyword: "   "));
    }

    [TestMethod]
    public void Matches_KeywordIsCaseInsensitiveAndSearchesMainFields()
    {
        var shop = CreateShop(Name: "Tokyo Ramen", Address: "東京都千代田区", RamenType: "醤油");

        Assert.IsTrue(Matches(shop, searchKeyword: "TOKYO"));
        Assert.IsTrue(Matches(shop, searchKeyword: "千代田"));
        Assert.IsTrue(Matches(shop, searchKeyword: "醤油"));
        Assert.IsFalse(Matches(shop, searchKeyword: "大阪"));
    }

    [TestMethod]
    public void Matches_RamenTypeUsesAllAsDefault()
    {
        var shop = CreateShop(RamenType: "味噌");

        Assert.IsTrue(Matches(shop, selectedRamenType: ShopFilter.AllFilter));
        Assert.IsTrue(Matches(shop, selectedRamenType: "味噌"));
        Assert.IsFalse(Matches(shop, selectedRamenType: "醤油"));
    }

    [TestMethod]
    [DataRow(1000, "1000円以下", true)]
    [DataRow(1001, "1000円以下", false)]
    [DataRow(1500, "1500円以下", true)]
    [DataRow(1501, "1500円以下", false)]
    [DataRow(2000, "2000円以下", true)]
    [DataRow(2001, "2000円以下", false)]
    public void Matches_PriceFilterUsesInclusiveUpperBoundary(decimal price, string filter, bool expected)
    {
        var shop = CreateShop(Price: price);

        Assert.AreEqual(expected, Matches(shop, selectedPriceFilter: filter));
    }

    [TestMethod]
    public void Matches_UnknownPriceFilterDoesNotRestrictResults()
    {
        var shop = CreateShop(Price: 5000);

        Assert.IsTrue(Matches(shop, selectedPriceFilter: "不明な条件"));
    }

    [TestMethod]
    public void Matches_FavoriteOnlyExcludesNonFavorites()
    {
        var favorite = CreateShop(IsFavorite: true);
        var notFavorite = CreateShop(IsFavorite: false);

        Assert.IsTrue(Matches(favorite, favoriteOnly: true));
        Assert.IsFalse(Matches(notFavorite, favoriteOnly: true));
        Assert.IsTrue(Matches(notFavorite, favoriteOnly: false));
    }

    [TestMethod]
    public void Matches_NearbyOnlyRequiresCurrentLocation()
    {
        var shop = CreateShop(Latitude: 35.6812, Longitude: 139.7671);

        Assert.IsFalse(Matches(shop, nearbyOnly: true, nearbyLatitude: null, nearbyLongitude: null));
    }

    [TestMethod]
    public void Matches_NearbyRadiusUsesSelectedDistance()
    {
        var shop = CreateShop(Latitude: 35.6812, Longitude: 139.7671);

        Assert.IsTrue(Matches(shop, nearbyOnly: true, nearbyLatitude: 35.6812, nearbyLongitude: 139.7671, selectedNearbyRadius: "1km"));
        Assert.IsFalse(Matches(shop, nearbyOnly: true, nearbyLatitude: 35.6812, nearbyLongitude: 139.7671, selectedNearbyRadius: "0km"));
    }

    [TestMethod]
    public void Matches_InvalidNearbyRadiusDoesNotAccidentallyMatchDistantShop()
    {
        var shop = CreateShop(Latitude: 35.7000, Longitude: 139.8000);

        Assert.IsFalse(Matches(shop, nearbyOnly: true, nearbyLatitude: 35.6812, nearbyLongitude: 139.7671, selectedNearbyRadius: "invalid"));
    }

    [TestMethod]
    public void DistanceKm_ReturnsExpectedApproximateDistance()
    {
        var distance = ShopFilter.DistanceKm(35.6812, 139.7671, 35.6900, 139.7671);

        Assert.IsTrue(distance > 0.9 && distance < 1.1, $"Actual distance: {distance} km");
    }

    private static bool Matches(
        Shop shop,
        string searchKeyword = "",
        string selectedRamenType = ShopFilter.AllFilter,
        string selectedPriceFilter = ShopFilter.AllFilter,
        bool favoriteOnly = false,
        bool nearbyOnly = false,
        double? nearbyLatitude = null,
        double? nearbyLongitude = null,
        string selectedNearbyRadius = "5km") =>
        ShopFilter.Matches(
            shop,
            searchKeyword,
            selectedRamenType,
            selectedPriceFilter,
            favoriteOnly,
            nearbyOnly,
            nearbyLatitude,
            nearbyLongitude,
            selectedNearbyRadius);

    private static Shop CreateShop(
        string Name = "Test Shop",
        string Address = "Tokyo",
        string RamenType = "醤油",
        decimal Price = 1000,
        bool IsFavorite = false,
        double Latitude = 35.6812,
        double Longitude = 139.7671) => new()
        {
            Name = Name,
            Address = Address,
            RamenType = RamenType,
            Price = Price,
            IsFavorite = IsFavorite,
            Latitude = Latitude,
            Longitude = Longitude
        };
}
