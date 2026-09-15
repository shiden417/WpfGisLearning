using Mapsui;
using Mapsui.Extensions;
using Mapsui.Projections;

namespace WpfGisLearning.Services;

public static class MapViewportCalculator
{
    public static MPoint CalculateCenter(ShopMapBounds bounds)
    {
        return SphericalMercator
            .FromLonLat(
                (bounds.MinLongitude + bounds.MaxLongitude) / 2,
                (bounds.MinLatitude + bounds.MaxLatitude) / 2)
            .ToMPoint();
    }

    public static double CalculateResolution(
        ShopMapBounds bounds,
        IReadOnlyList<double> availableResolutions,
        double mapWidth,
        double mapHeight,
        double paddingFactor,
        int singleShopResolutionIndex)
    {
        if (bounds.MinLongitude == bounds.MaxLongitude
            && bounds.MinLatitude == bounds.MaxLatitude)
        {
            var index = Math.Min(singleShopResolutionIndex, availableResolutions.Count - 1);
            return availableResolutions[index];
        }

        var minMap = SphericalMercator
            .FromLonLat(bounds.MinLongitude, bounds.MinLatitude)
            .ToMPoint();
        var maxMap = SphericalMercator
            .FromLonLat(bounds.MaxLongitude, bounds.MaxLatitude)
            .ToMPoint();

        var widthResolution = Math.Abs(maxMap.X - minMap.X) / mapWidth;
        var heightResolution = Math.Abs(maxMap.Y - minMap.Y) / mapHeight;

        return Math.Max(widthResolution, heightResolution) * paddingFactor;
    }
}
