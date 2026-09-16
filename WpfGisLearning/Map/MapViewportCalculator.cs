using Mapsui;
using Mapsui.Extensions;
using Mapsui.Projections;

namespace WpfGisLearning.Map;

/// <summary>
/// 店舗群に合わせた地図の中心位置と解像度を計算するヘルパーです。
/// 画面サイズに依存する計算だけを担当し、MapControlそのものは操作しません。
/// </summary>
public static class MapViewportCalculator
{
    /// <summary>
    /// 店舗群の境界の中心点をWeb Mercator座標で計算します。
    /// </summary>
    /// <param name="bounds">店舗群の緯度・経度の最小・最大値です。</param>
    /// <returns>Mapsuiで使用する中心座標です。</returns>
    public static MPoint CalculateCenter(ShopMapBounds bounds)
    {
        return SphericalMercator
            .FromLonLat(
                (bounds.MinLongitude + bounds.MaxLongitude) / 2,
                (bounds.MinLatitude + bounds.MaxLatitude) / 2)
            .ToMPoint();
    }

    /// <summary>
    /// 店舗群全体が画面内に収まるように地図の解像度を計算します。
    /// </summary>
    /// <param name="bounds">表示対象店舗群の境界です。</param>
    /// <param name="availableResolutions">Mapsuiが用意している解像度一覧です。</param>
    /// <param name="mapWidth">地図表示領域の幅です。</param>
    /// <param name="mapHeight">地図表示領域の高さです。</param>
    /// <param name="paddingFactor">周囲に余白を作るための倍率です。</param>
    /// <param name="singleShopResolutionIndex">店舗が1件だけの場合に使う解像度のインデックスです。</param>
    /// <returns>Navigatorへ渡す解像度です。</returns>
    public static double CalculateResolution(
        ShopMapBounds bounds,
        IReadOnlyList<double> availableResolutions,
        double mapWidth,
        double mapHeight,
        double paddingFactor,
        int singleShopResolutionIndex)
    {
        // 1店舗だけの場合は、店舗間の距離から解像度を計算できないため既定値を使う。
        if (bounds.MinLongitude == bounds.MaxLongitude
            && bounds.MinLatitude == bounds.MaxLatitude)
        {
            var index = Math.Min(singleShopResolutionIndex, availableResolutions.Count - 1);
            return availableResolutions[index];
        }

        // 緯度・経度を画面上の距離に相当するWeb Mercator座標へ変換する。
        var minMap = SphericalMercator
            .FromLonLat(bounds.MinLongitude, bounds.MinLatitude)
            .ToMPoint();
        var maxMap = SphericalMercator
            .FromLonLat(bounds.MaxLongitude, bounds.MaxLatitude)
            .ToMPoint();

        // 横幅と縦幅それぞれで、1画面ピクセルあたりの必要解像度を求める。
        var widthResolution = Math.Abs(maxMap.X - minMap.X) / mapWidth;
        var heightResolution = Math.Abs(maxMap.Y - minMap.Y) / mapHeight;

        // より大きい方を採用し、どちらの方向でも店舗が切れないようにする。
        return Math.Max(widthResolution, heightResolution) * paddingFactor;
    }
}
