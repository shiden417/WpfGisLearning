using WpfGisLearning.Models;
using WpfGisLearning.Services;

namespace WpfGisLearning.Tests;

[TestClass]
public class ShopMapLayerBuilderTests
{
    [TestMethod]
    public void Build_ExcludesInvalidCoordinatesAndCalculatesBounds()
    {
        var shops = new[]
        {
            new Shop { Id = 1, Name = "Valid 1", Latitude = 35.0, Longitude = 139.0 },
            new Shop { Id = 2, Name = "Invalid", Latitude = 91.0, Longitude = 139.0 },
            new Shop { Id = 3, Name = "Valid 2", Latitude = 36.0, Longitude = 140.0 }
        };

        var result = ShopMapLayerBuilder.Build(shops, selectedShopId: null);

        Assert.AreEqual(2, result.Layer.Features.Count());
        Assert.IsTrue(result.Bounds.HasValidCoordinates);
        Assert.AreEqual(139.0, result.Bounds.MinLongitude);
        Assert.AreEqual(140.0, result.Bounds.MaxLongitude);
        Assert.AreEqual(35.0, result.Bounds.MinLatitude);
        Assert.AreEqual(36.0, result.Bounds.MaxLatitude);
    }

    [TestMethod]
    public void Build_WithNoValidCoordinates_ReturnsEmptyLayerAndInvalidBounds()
    {
        var shops = new[]
        {
            new Shop { Id = 1, Name = "Invalid", Latitude = 0, Longitude = 0 },
            new Shop { Id = 2, Name = "Invalid", Latitude = 91, Longitude = 181 }
        };

        var result = ShopMapLayerBuilder.Build(shops, selectedShopId: null);

        Assert.AreEqual(0, result.Layer.Features.Count());
        Assert.IsFalse(result.Bounds.HasValidCoordinates);
    }

    [TestMethod]
    public void Build_SelectedShopCreatesSelectedMarker()
    {
        var shops = new[]
        {
            new Shop { Id = 1, Name = "Shop 1", Latitude = 35.0, Longitude = 139.0 },
            new Shop { Id = 2, Name = "Shop 2", Latitude = 36.0, Longitude = 140.0 }
        };

        var result = ShopMapLayerBuilder.Build(shops, selectedShopId: 2);
        var selectedFeature = result.Layer.Features.Single(feature => feature["Id"]?.ToString() == "2");
        var normalFeature = result.Layer.Features.Single(feature => feature["Id"]?.ToString() == "1");

        Assert.AreEqual(1, selectedFeature.Styles.Count);
        Assert.AreEqual(1, normalFeature.Styles.Count);
        Assert.AreNotEqual(
            selectedFeature.Styles.Single().ToString(),
            normalFeature.Styles.Single().ToString());
    }
}
