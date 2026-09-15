using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Styles;
using WpfGisLearning.Models;

namespace WpfGisLearning.Services;

public sealed record ShopMapBounds(
    double MinLongitude,
    double MaxLongitude,
    double MinLatitude,
    double MaxLatitude)
{
    public bool HasValidCoordinates =>
        MinLongitude != double.MaxValue &&
        MaxLongitude != double.MinValue &&
        MinLatitude != double.MaxValue &&
        MaxLatitude != double.MinValue;
}

public sealed record ShopMapLayerResult(MemoryLayer Layer, ShopMapBounds Bounds);

public static class ShopMapLayerBuilder
{
    public const string LayerName = "Shops";

    private const double OverlapOffsetPixels = 12;
    private const double NormalMarkerScale = 1.15;
    private const double SelectedMarkerScale = 1.4;

    public static ShopMapLayerResult Build(IEnumerable<Shop> shops, int? selectedShopId)
    {
        var validShops = shops
            .Where(shop => MapCoordinateValidator.IsValid(shop.Latitude, shop.Longitude))
            .ToList();

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

    private static IEnumerable<IFeature> CreateFeatures(IReadOnlyList<Shop> shops, int? selectedShopId)
    {
        if (shops.Count == 1)
        {
            yield return CreateFeature(shops[0], selectedShopId == shops[0].Id, 0, 1);
            yield break;
        }

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

        var feature = new PointFeature(point)
        {
            ["Name"] = shop.Name,
            ["Address"] = shop.Address,
            ["Id"] = shop.Id
        };

        var markerStyle = ImageStyles.CreatePinStyle(
            Color.FromString(selected ? "#C56B4D" : "#4A90E2"),
            Color.White,
            selected ? SelectedMarkerScale : NormalMarkerScale);
        markerStyle.Offset = new Offset(offsetX, offsetY);
        feature.Styles.Add(markerStyle);

        if (overlapCount > 1)
        {
            feature.Styles.Add(new LabelStyle
            {
                Text = $"{overlapIndex + 1}/{overlapCount}",
                Font = new Font { Size = 10, Bold = true },
                ForeColor = Color.White,
                BackColor = new Brush(Color.FromString("#CC343A40")),
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
