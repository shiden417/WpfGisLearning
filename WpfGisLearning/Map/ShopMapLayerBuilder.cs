using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Styles;
using WpfGisLearning.Models;

namespace WpfGisLearning.Map;

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
    private const double ClusterCellSizePixels = 56;
    private const int MinimumClusterSize = 2;

    public static ShopMapLayerResult Build(
        IEnumerable<Shop> shops,
        int? selectedShopId,
        double resolution)
    {
        var validShops = shops
            .Where(shop => MapCoordinateValidator.IsValid(shop.Latitude, shop.Longitude))
            .ToList();

        var features = CreateFeatures(validShops, selectedShopId, resolution).ToList<IFeature>();
        var bounds = CalculateBounds(validShops);
        var layer = new MemoryLayer
        {
            Name = LayerName,
            Style = null,
            Features = features
        };

        return new ShopMapLayerResult(layer, bounds);
    }

    private static IEnumerable<IFeature> CreateFeatures(
        IReadOnlyList<Shop> shops,
        int? selectedShopId,
        double resolution)
    {
        if (shops.Count == 0)
            yield break;

        var safeResolution = resolution > 0 ? resolution : 1;
        var cellSize = safeResolution * ClusterCellSizePixels;
        var groups = shops
            .GroupBy(shop => GetClusterCell(shop, cellSize))
            .ToList();

        foreach (var group in groups)
        {
            var groupShops = group.ToList();

            if (groupShops.Count < MinimumClusterSize || groupShops.Any(shop => shop.Id == selectedShopId))
            {
                foreach (var shop in groupShops)
                {
                    yield return CreateShopFeature(shop, selectedShopId == shop.Id, 0, 1);
                }

                continue;
            }

            yield return CreateClusterFeature(groupShops);
        }
    }

    private static (long X, long Y) GetClusterCell(Shop shop, double cellSize)
    {
        var point = SphericalMercator.FromLonLat(shop.Longitude, shop.Latitude).ToMPoint();
        return (
            (long)Math.Floor(point.X / cellSize),
            (long)Math.Floor(point.Y / cellSize));
    }

    private static IFeature CreateShopFeature(
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
            ["Id"] = shop.Id,
            ["IsCluster"] = false
        };

        var markerStyle = MapMarkerStyleFactory.CreateShopMarker(selected);
        markerStyle.Offset = new Offset(offsetX, offsetY);
        feature.Styles.Add(markerStyle);

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

    private static IFeature CreateClusterFeature(IReadOnlyList<Shop> shops)
    {
        var points = shops
            .Select(shop => SphericalMercator.FromLonLat(shop.Longitude, shop.Latitude).ToMPoint())
            .ToList();
        var center = new MPoint(points.Average(point => point.X), points.Average(point => point.Y));

        var feature = new PointFeature(center)
        {
            ["IsCluster"] = true,
            ["ClusterIds"] = string.Join(",", shops.Select(shop => shop.Id)),
            ["ClusterCount"] = shops.Count
        };

        feature.Styles.Add(MapMarkerStyleFactory.CreateClusterMarker());
        feature.Styles.Add(new LabelStyle
        {
            Text = shops.Count.ToString(),
            Font = new Font { Size = 12, Bold = true },
            ForeColor = Color.White,
            HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center,
            VerticalAlignment = LabelStyle.VerticalAlignmentEnum.Center,
            CollisionDetection = false
        });

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
