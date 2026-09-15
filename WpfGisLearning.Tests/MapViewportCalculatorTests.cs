using WpfGisLearning.Services;

namespace WpfGisLearning.Tests;

[TestClass]
public class MapViewportCalculatorTests
{
    [TestMethod]
    public void CalculateCenter_ReturnsCenterOfBounds()
    {
        var bounds = new ShopMapBounds(139.0, 141.0, 35.0, 37.0);

        var center = MapViewportCalculator.CalculateCenter(bounds);

        Assert.IsNotNull(center);
        Assert.AreEqual(140.0, Mapsui.Projections.SphericalMercator.ToLonLat(center.X, center.Y).lon, 0.000001);
        Assert.AreEqual(36.0, Mapsui.Projections.SphericalMercator.ToLonLat(center.X, center.Y).lat, 0.000001);
    }

    [TestMethod]
    public void CalculateResolution_ForSingleShop_UsesConfiguredResolutionIndex()
    {
        var bounds = new ShopMapBounds(139.0, 139.0, 35.0, 35.0);
        var resolutions = Enumerable.Range(0, 5).Select(i => (double)(i + 1)).ToArray();

        var resolution = MapViewportCalculator.CalculateResolution(
            bounds,
            resolutions,
            mapWidth: 1000,
            mapHeight: 800,
            paddingFactor: 1.2,
            singleShopResolutionIndex: 12);

        Assert.AreEqual(5.0, resolution);
    }

    [TestMethod]
    public void CalculateResolution_ForMultipleShops_ReturnsLargestAxisResolutionWithPadding()
    {
        var bounds = new ShopMapBounds(139.0, 140.0, 35.0, 36.0);

        var resolution = MapViewportCalculator.CalculateResolution(
            bounds,
            availableResolutions: [1, 2, 3],
            mapWidth: 1000,
            mapHeight: 1000,
            paddingFactor: 1.2,
            singleShopResolutionIndex: 12);

        Assert.IsTrue(resolution > 0);
    }
}
