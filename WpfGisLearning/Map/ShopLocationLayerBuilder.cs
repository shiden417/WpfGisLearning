using Mapsui;
using Mapsui.Layers;
using Mapsui.Projections;

namespace WpfGisLearning.Map;

public sealed record ShopLocationLayerResult(MemoryLayer Layer, MPoint Point);

public static class ShopLocationLayerBuilder
{
    public const string DefaultLayerName = "Shop";

    public static ShopLocationLayerResult Build(
        double latitude,
        double longitude,
        string layerName = DefaultLayerName,
        bool selected = false)
    {
        var (x, y) = SphericalMercator.FromLonLat(longitude, latitude);
        var point = new MPoint(x, y);
        var feature = new PointFeature(point);
        feature.Styles.Add(MapMarkerStyleFactory.CreateShopMarker(selected));

        var layer = new MemoryLayer
        {
            Name = layerName,
            Style = null,
            Features = new[] { feature }
        };

        return new ShopLocationLayerResult(layer, point);
    }
}
