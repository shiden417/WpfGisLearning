using WpfGisLearning.Models;

namespace WpfGisLearning.Map;

/// <summary>
/// 店舗情報から外部地図サービスのURLを組み立てるヘルパーです。
/// UIからURLの文字列生成処理を切り離しています。
/// </summary>
public static class ExternalMapLinkBuilder
{
    /// <summary>
    /// 店舗の緯度・経度を使ったGoogle Maps検索URLを生成します。
    /// </summary>
    /// <param name="shop">URLに使用する店舗情報です。</param>
    /// <returns>Google Mapsで店舗位置を開くためのURLです。</returns>
    public static string CreateGoogleMapsUrl(Shop shop)
    {
        ArgumentNullException.ThrowIfNull(shop);

        return $"https://www.google.com/maps/search/?api=1&query={shop.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)},{shop.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
    }
}
