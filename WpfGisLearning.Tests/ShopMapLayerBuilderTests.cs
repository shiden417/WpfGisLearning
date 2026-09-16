using Mapsui.Styles;
using WpfGisLearning.Map;
using WpfGisLearning.Models;

namespace WpfGisLearning.Tests;

[TestClass]
public class ShopMapLayerBuilderTests
{
    private const double HighResolution = 1000;
    private const double LowResolution = 1;

    [TestMethod]
    public void Build_ExcludesInvalidCoordinatesAndCalculatesBounds()
    {
        var shops = new[]
        {
            new Shop { Id = 1, Name = "Valid 1", Latitude = 35.0, Longitude = 139.0 },
            new Shop { Id = 2, Name = "Invalid", Latitude = 91.0, Longitude = 139.0 },
            new Shop { Id = 3, Name = "Valid 2", Latitude = 36.0, Longitude = 140.0 }
        };

        var result = ShopMapLayerBuilder.Build(shops, selectedShopId: null, resolution: LowResolution);

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

        var result = ShopMapLayerBuilder.Build(shops, selectedShopId: null, resolution: LowResolution);

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

        var result = ShopMapLayerBuilder.Build(shops, selectedShopId: 2, resolution: LowResolution);
        var selectedFeature = result.Layer.Features.Single(feature => feature["Id"]?.ToString() == "2");
        var normalFeature = result.Layer.Features.Single(feature => feature["Id"]?.ToString() == "1");
        var selectedStyle = selectedFeature.Styles.OfType<ImageStyle>().Single();
        var normalStyle = normalFeature.Styles.OfType<ImageStyle>().Single();

        Assert.AreEqual(1.4, selectedStyle.SymbolScale);
        Assert.AreEqual(1.15, normalStyle.SymbolScale);
    }

    [TestMethod]
    public void Build_OverlappingShopsAreSeparatedAndNumbered()
    {
        var shops = new[]
        {
            new Shop { Id = 1, Name = "Shop 1", Latitude = 35.0, Longitude = 139.0 },
            new Shop { Id = 2, Name = "Shop 2", Latitude = 35.0, Longitude = 139.0 },
            new Shop { Id = 3, Name = "Shop 3", Latitude = 35.0, Longitude = 139.0 }
        };

        var result = ShopMapLayerBuilder.Build(shops, selectedShopId: null, resolution: LowResolution);
        var markerOffsets = result.Layer.Features
            .SelectMany(feature => feature.Styles.OfType<ImageStyle>())
            .Select(style => (style.Offset.X, style.Offset.Y))
            .ToList();
        var labels = result.Layer.Features
            .Select(feature => feature.Styles.OfType<LabelStyle>().Single().GetLabelText(feature))
            .OrderBy(text => text)
            .ToList();

        Assert.HasCount(3, markerOffsets);
        Assert.HasCount(3, markerOffsets.Distinct().ToList());
        CollectionAssert.AreEqual(new[] { "1/3", "2/3", "3/3" }, labels);
    }

    [TestMethod]
    public void Build_DenseShopsAreClusteredAtLowZoom()
    {
        var shops = new[]
        {
            new Shop { Id = 1, Name = "Shop 1", Latitude = 35.0000, Longitude = 139.0000 },
            new Shop { Id = 2, Name = "Shop 2", Latitude = 35.0001, Longitude = 139.0001 },
            new Shop { Id = 3, Name = "Shop 3", Latitude = 35.0002, Longitude = 139.0002 }
        };

        var result = ShopMapLayerBuilder.Build(shops, selectedShopId: null, resolution: HighResolution);

        Assert.HasCount(1, result.Layer.Features);
        var cluster = result.Layer.Features.Single();
        Assert.AreEqual("True", cluster["IsCluster"]?.ToString());
        Assert.AreEqual("3", cluster["ClusterCount"]?.ToString());
        Assert.AreEqual("1,2,3", cluster["ClusterIds"]?.ToString());
        Assert.IsNotNull(cluster.Styles.OfType<LabelStyle>().Single());
    }

    [TestMethod]
    public void Build_SelectedShopPreventsItsClusterAndKeepsMarkerVisible()
    {
        var shops = new[]
        {
            new Shop { Id = 1, Name = "Shop 1", Latitude = 35.0000, Longitude = 139.0000 },
            new Shop { Id = 2, Name = "Shop 2", Latitude = 35.0001, Longitude = 139.0001 },
            new Shop { Id = 3, Name = "Shop 3", Latitude = 35.0002, Longitude = 139.0002 }
        };

        var result = ShopMapLayerBuilder.Build(shops, selectedShopId: 2, resolution: HighResolution);

        Assert.HasCount(3, result.Layer.Features);
        var selected = result.Layer.Features.Single(feature => feature["Id"]?.ToString() == "2");
        Assert.AreEqual("False", selected["IsCluster"]?.ToString());
        Assert.AreEqual(1.4, selected.Styles.OfType<ImageStyle>().Single().SymbolScale);
    }
}