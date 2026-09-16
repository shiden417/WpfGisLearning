using Mapsui;
using Mapsui.Extensions;
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
        var point = SphericalMercator.FromLonLat(longitude, latitude).ToMPoint();
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
