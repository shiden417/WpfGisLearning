using WpfGisLearning.Models;

namespace WpfGisLearning.Services;

public static class ShopFilter
{
    public const string AllFilter = "すべて";
    public const string PriceFilter1000 = "1000円以下";
    public const string PriceFilter1500 = "1500円以下";
    public const string PriceFilter2000 = "2000円以下";

    private const double EarthRadiusKm = 6371.0;

    public static bool Matches(
        Shop shop,
        string searchKeyword,
        string selectedRamenType,
        string selectedPriceFilter,
        bool favoriteOnly,
        bool nearbyOnly,
        double? nearbyLatitude,
        double? nearbyLongitude,
        string selectedNearbyRadius)
    {
        return MatchesKeyword(shop, searchKeyword)
            && MatchesRamenType(shop, selectedRamenType)
            && MatchesFavorite(shop, favoriteOnly)
            && MatchesPrice(shop, selectedPriceFilter)
            && MatchesNearby(shop, nearbyOnly, nearbyLatitude, nearbyLongitude, selectedNearbyRadius);
    }

    private static bool MatchesKeyword(Shop shop, string searchKeyword)
    {
        var keyword = searchKeyword.Trim();
        if (string.IsNullOrWhiteSpace(keyword))
            return true;

        return shop.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase)
            || shop.Address.Contains(keyword, StringComparison.OrdinalIgnoreCase)
            || shop.RamenType.Contains(keyword, StringComparison.OrdinalIgnoreCase);
    }

    private static bool MatchesRamenType(Shop shop, string selectedRamenType) =>
        selectedRamenType == AllFilter || shop.RamenType == selectedRamenType;

    private static bool MatchesFavorite(Shop shop, bool favoriteOnly) =>
        !favoriteOnly || shop.IsFavorite;

    private static bool MatchesPrice(Shop shop, string selectedPriceFilter)
    {
        var maxPrice = selectedPriceFilter switch
        {
            AllFilter => decimal.MaxValue,
            PriceFilter1000 => 1000m,
            PriceFilter1500 => 1500m,
            PriceFilter2000 => 2000m,
            _ => decimal.MaxValue
        };

        return shop.Price <= maxPrice;
    }

    private static bool MatchesNearby(
        Shop shop,
        bool nearbyOnly,
        double? nearbyLatitude,
        double? nearbyLongitude,
        string selectedNearbyRadius)
    {
        if (!nearbyOnly)
            return true;

        if (!nearbyLatitude.HasValue || !nearbyLongitude.HasValue)
            return false;

        var radiusKm = ParseRadiusKm(selectedNearbyRadius);
        return DistanceKm(
            nearbyLatitude.Value,
            nearbyLongitude.Value,
            shop.Latitude,
            shop.Longitude) <= radiusKm;
    }

    private static double ParseRadiusKm(string radius) =>
        double.TryParse(radius.Replace("km", ""), out var value) ? value : 0;

    public static double DistanceKm(double latitude1, double longitude1, double latitude2, double longitude2)
    {
        var dLatitude = DegreesToRadians(latitude2 - latitude1);
        var dLongitude = DegreesToRadians(longitude2 - longitude1);
        var a = Math.Sin(dLatitude / 2) * Math.Sin(dLatitude / 2)
            + Math.Cos(DegreesToRadians(latitude1))
            * Math.Cos(DegreesToRadians(latitude2))
            * Math.Sin(dLongitude / 2)
            * Math.Sin(dLongitude / 2);

        return EarthRadiusKm * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180.0;
}
