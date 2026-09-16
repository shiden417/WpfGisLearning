using WpfGisLearning.Models;

namespace WpfGisLearning.Filters;

/// <summary>
/// 店舗一覧に適用する検索・フィルター条件を判定するヘルパーです。
/// UIやViewModelから条件判定の詳細を切り離しています。
/// </summary>
public static class ShopFilter
{
    /// <summary>「すべて」を意味する共通フィルター値です。</summary>
    public const string AllFilter = "すべて";

    /// <summary>1000円以下の価格フィルター値です。</summary>
    public const string PriceFilter1000 = "1000円以下";

    /// <summary>1500円以下の価格フィルター値です。</summary>
    public const string PriceFilter1500 = "1500円以下";

    /// <summary>2000円以下の価格フィルター値です。</summary>
    public const string PriceFilter2000 = "2000円以下";

    /// <summary>距離計算で使用する地球半径の近似値（km）です。</summary>
    private const double EarthRadiusKm = 6371.0;

    /// <summary>
    /// 指定した全条件を店舗が満たすか判定します。
    /// 各条件をANDで結合するため、1つでも不一致ならfalseになります。
    /// </summary>
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

    /// <summary>店舗名・住所・ラーメン種別のいずれかにキーワードを含むか判定します。</summary>
    private static bool MatchesKeyword(Shop shop, string searchKeyword)
    {
        var keyword = searchKeyword.Trim();
        if (string.IsNullOrWhiteSpace(keyword))
            return true;

        return shop.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase)
            || shop.Address.Contains(keyword, StringComparison.OrdinalIgnoreCase)
            || shop.RamenType.Contains(keyword, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>ラーメン種別が選択条件と一致するか判定します。</summary>
    private static bool MatchesRamenType(Shop shop, string selectedRamenType) =>
        selectedRamenType == AllFilter || shop.RamenType == selectedRamenType;

    /// <summary>お気に入り限定条件を満たすか判定します。</summary>
    private static bool MatchesFavorite(Shop shop, bool favoriteOnly) =>
        !favoriteOnly || shop.IsFavorite;

    /// <summary>選択された上限価格を店舗価格が満たすか判定します。</summary>
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

    /// <summary>現在地から指定半径以内に店舗があるか判定します。</summary>
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

    /// <summary>「5km」のような表示値から数値の距離（km）を取り出します。</summary>
    private static double ParseRadiusKm(string radius) =>
        double.TryParse(radius.Replace("km", ""), out var value) ? value : 0;

    /// <summary>
    /// 2地点間の大円距離をkmで計算します。
    /// Haversine公式を使った近似計算です。
    /// </summary>
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

    /// <summary>度単位の角度をラジアンへ変換します。</summary>
    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180.0;
}
