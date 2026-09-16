using Mapsui;
using Mapsui.Layers;
using Mapsui.Projections;

namespace WpfGisLearning.Map;

/// <summary>
/// 生成した店舗位置レイヤーと、同じ位置を表すMapsui座標をまとめた結果です。
/// </summary>
/// <param name="Layer">1つの店舗位置を表すMemoryLayerです。</param>
/// <param name="Point">Web Mercatorへ変換した地図座標です。</param>
public sealed record ShopLocationLayerResult(MemoryLayer Layer, MPoint Point);

/// <summary>
/// 1店舗分の位置マーカー用レイヤーを生成するビルダーです。
/// 詳細画面や編集画面で共通して利用できるよう、WPFコントロールには依存しません。
/// </summary>
public static class ShopLocationLayerBuilder
{
    /// <summary>レイヤー名を指定しなかった場合の既定値です。</summary>
    public const string DefaultLayerName = "Shop";

    /// <summary>
    /// 緯度・経度から店舗マーカーのMemoryLayerと地図座標を生成します。
    /// </summary>
    /// <param name="latitude">店舗位置の緯度です。</param>
    /// <param name="longitude">店舗位置の経度です。</param>
    /// <param name="layerName">生成するレイヤーの名前です。</param>
    /// <param name="selected">選択状態のマーカーとして生成するかを指定します。</param>
    /// <returns>生成したレイヤーと地図座標です。</returns>
    public static ShopLocationLayerResult Build(
        double latitude,
        double longitude,
        string layerName = DefaultLayerName,
        bool selected = false)
    {
        // Mapsuiは緯度・経度をそのまま描画するのではなく、Web Mercator座標へ変換して扱う。
        var (x, y) = SphericalMercator.FromLonLat(longitude, latitude);
        var point = new MPoint(x, y);

        // PointFeatureは地図上の1点を表すMapsuiのFeature。
        var feature = new PointFeature(point);
        feature.Styles.Add(MapMarkerStyleFactory.CreateShopMarker(selected));

        // MemoryLayerにFeatureをまとめてMapへ追加できるようにする。
        var layer = new MemoryLayer
        {
            Name = layerName,
            Style = null,
            Features = new[] { feature }
        };

        return new ShopLocationLayerResult(layer, point);
    }
}
