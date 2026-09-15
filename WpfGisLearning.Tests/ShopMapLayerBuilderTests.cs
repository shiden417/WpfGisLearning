using Mapsui.Styles;
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

        Assert.HasCount(2, result.Layer.Features);
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

        Assert.HasCount(0, result.Layer.Features);
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
        var selectedStyle = selectedFeature.Styles.Single() as ImageStyle;
        var normalStyle = normalFeature.Styles.Single() as ImageStyle;

        Assert.HasCount(1, selectedFeature.Styles);
        Assert.HasCount(1, normalFeature.Styles);
        Assert.IsNotNull(selectedStyle);
        Assert.IsNotNull(normalStyle);
        Assert.AreEqual(1.4, selectedStyle!.SymbolScale);
        Assert.AreEqual(1.15, normalStyle!.SymbolScale);
    }
}
