using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Styles;
using WpfGisLearning.Models;

namespace WpfGisLearning.Map;

/// <summary>
/// 店舗群の表示範囲を表す緯度・経度の境界値です。
/// 初期表示の地図中心・解像度を計算するために使用します。
/// </summary>
/// <param name="MinLongitude">店舗群の最小経度です。</param>
/// <param name="MaxLongitude">店舗群の最大経度です。</param>
/// <param name="MinLatitude">店舗群の最小緯度です。</param>
/// <param name="MaxLatitude">店舗群の最大緯度です。</param>
public sealed record ShopMapBounds(
    double MinLongitude,
    double MaxLongitude,
    double MinLatitude,
    double MaxLatitude)
{
    /// <summary>有効な店舗座標が1件以上存在する境界かを判定します。</summary>
    public bool HasValidCoordinates =>
        MinLongitude != double.MaxValue &&
        MaxLongitude != double.MinValue &&
        MinLatitude != double.MaxValue &&
        MaxLatitude != double.MinValue;
}

/// <summary>
/// 店舗マーカー用のMemoryLayerと、その店舗群の境界情報をまとめた結果です。
/// </summary>
public sealed record ShopMapLayerResult(MemoryLayer Layer, ShopMapBounds Bounds);

/// <summary>
/// 複数店舗をMapsuiのFeatureへ変換し、店舗レイヤーを生成するビルダーです。
/// 同一座標に店舗が重なる場合は、視認できるようマーカーを円形にずらします。
/// </summary>
public static class ShopMapLayerBuilder
{
    /// <summary>店舗表示用レイヤーの固定名です。</summary>
    public const string LayerName = "Shops";

    /// <summary>同一座標の店舗をずらして表示するピクセル量です。</summary>
    private const double OverlapOffsetPixels = 12;

    /// <summary>
    /// 店舗一覧から、店舗マーカーのレイヤーと表示範囲を生成します。
    /// 座標が不正な店舗は地図表示対象から除外します。
    /// </summary>
    /// <param name="shops">表示対象となる店舗一覧です。</param>
    /// <param name="selectedShopId">選択中店舗のIDです。</param>
    /// <returns>生成した店舗レイヤーと座標境界です。</returns>
    public static ShopMapLayerResult Build(IEnumerable<Shop> shops, int? selectedShopId)
    {
        // 地図に描画できない座標を先に除外する。
        var validShops = shops
            .Where(shop => MapCoordinateValidator.IsValid(shop.Latitude, shop.Longitude))
            .ToList();

        // 同一座標の店舗をグループ化し、重なりを避けたFeatureを生成する。
        var features = validShops
            .GroupBy(shop => (shop.Latitude, shop.Longitude))
            .SelectMany(group => CreateFeatures(group.ToList(), selectedShopId))
            .ToList<IFeature>();

        var bounds = CalculateBounds(validShops);
        var layer = new MemoryLayer
        {
            Name = LayerName,
            Style = null,
            Features = features
        };

        return new ShopMapLayerResult(layer, bounds);
    }

    /// <summary>
    /// 同じ座標にある店舗群から、必要な数のマーカーFeatureを生成します。
    /// </summary>
    private static IEnumerable<IFeature> CreateFeatures(IReadOnlyList<Shop> shops, int? selectedShopId)
    {
        if (shops.Count == 1)
        {
            yield return CreateFeature(shops[0], selectedShopId == shops[0].Id, 0, 1);
            yield break;
        }

        // 同一座標の店舗を円形に配置し、クリック可能な状態を保つ。
        for (var index = 0; index < shops.Count; index++)
        {
            var shop = shops[index];
            var angle = 2 * Math.PI * index / shops.Count;
            var offsetX = Math.Cos(angle) * OverlapOffsetPixels;
            var offsetY = Math.Sin(angle) * OverlapOffsetPixels;
            yield return CreateFeature(
                shop,
                selectedShopId == shop.Id,
                index,
                shops.Count,
                offsetX,
                offsetY);
        }
    }

    /// <summary>
    /// 1店舗分のPointFeatureを生成し、必要なら重複番号ラベルも追加します。
    /// </summary>
    private static IFeature CreateFeature(
        Shop shop,
        bool selected,
        int overlapIndex,
        int overlapCount,
        double offsetX = 0,
        double offsetY = 0)
    {
        var point = SphericalMercator
            .FromLonLat(shop.Longitude, shop.Latitude)
            .ToMPoint();

        // Featureへ画面側で利用する店舗情報を属性として保持する。
        var feature = new PointFeature(point)
        {
            ["Name"] = shop.Name,
            ["Address"] = shop.Address,
            ["Id"] = shop.Id
        };

        var markerStyle = MapMarkerStyleFactory.CreateShopMarker(selected);
        markerStyle.Offset = new Offset(offsetX, offsetY);
        feature.Styles.Add(markerStyle);

        // 同じ位置に複数店舗ある場合、何番目の店舗かをラベルで示す。
        if (overlapCount > 1)
        {
            feature.Styles.Add(new LabelStyle
            {
                Text = $"{overlapIndex + 1}/{overlapCount}",
                Font = new Font { Size = 10, Bold = true },
                ForeColor = Color.White,
                BackColor = new Brush(Color.FromString("#343A40")),
                BorderColor = Color.White,
                BorderThickness = 1,
                CornerRounding = 6,
                HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center,
                VerticalAlignment = LabelStyle.VerticalAlignmentEnum.Bottom,
                Offset = new Offset(offsetX, offsetY - 18),
                CollisionDetection = false
            });
        }

        return feature;
    }

    /// <summary>
    /// 有効な店舗一覧から緯度・経度の最小値と最大値を計算します。
    /// </summary>
    private static ShopMapBounds CalculateBounds(IReadOnlyList<Shop> shops)
    {
        if (shops.Count == 0)
        {
            return new ShopMapBounds(
                double.MaxValue,
                double.MinValue,
                double.MaxValue,
                double.MinValue);
        }

        return new ShopMapBounds(
            shops.Min(shop => shop.Longitude),
            shops.Max(shop => shop.Longitude),
            shops.Min(shop => shop.Latitude),
            shops.Max(shop => shop.Latitude));
    }
}
