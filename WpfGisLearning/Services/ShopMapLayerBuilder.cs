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

    public static ShopMapLayerResult Build(IEnumerable<Shop> shops, int? selectedShopId)
    {
        var features = new List<IFeature>();
        var minLongitude = double.MaxValue;
        var maxLongitude = double.MinValue;
        var minLatitude = double.MaxValue;
        var maxLatitude = double.MinValue;

        foreach (var shop in shops)
        {
            if (!MapCoordinateValidator.IsValid(shop.Latitude, shop.Longitude))
            {
                continue;
            }

            minLongitude = Math.Min(minLongitude, shop.Longitude);
            maxLongitude = Math.Max(maxLongitude, shop.Longitude);
            minLatitude = Math.Min(minLatitude, shop.Latitude);
            maxLatitude = Math.Max(maxLatitude, shop.Latitude);
            features.Add(CreateFeature(shop, shop.Id == selectedShopId));
        }

        var layer = new MemoryLayer
        {
            Name = LayerName,
            Features = features
        };

        var bounds = new ShopMapBounds(
            minLongitude,
            maxLongitude,
            minLatitude,
            maxLatitude);

        return new ShopMapLayerResult(layer, bounds);
    }

    private static IFeature CreateFeature(Shop shop, bool selected)
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

        feature.Styles.Add(ImageStyles.CreatePinStyle(
            Color.FromString(selected ? "#C56B4D" : "#343A40"),
            Color.FromString("#343A40"),
            selected ? 1.4 : 1.15));

        return feature;
    }
}
