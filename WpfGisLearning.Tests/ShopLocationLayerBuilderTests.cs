using Mapsui.Styles;
using WpfGisLearning.Map;

namespace WpfGisLearning.Tests;

[TestClass]
public class ShopLocationLayerBuilderTests
{
    [TestMethod]
    public void Build_CreatesLayerWithExpectedNameAndMarker()
    {
        var result = ShopLocationLayerBuilder.Build(35.6812, 139.7671);

        Assert.AreEqual(ShopLocationLayerBuilder.DefaultLayerName, result.Layer.Name);
        Assert.HasCount(1, result.Layer.Features);
        Assert.HasCount(1, result.Layer.Features.Single().Styles.OfType<ImageStyle>());
    }

    [TestMethod]
    public void Build_UsesCustomLayerName()
    {
        var result = ShopLocationLayerBuilder.Build(35.6812, 139.7671, "SelectedLocation");

        Assert.AreEqual("SelectedLocation", result.Layer.Name);
    }

    [TestMethod]
    public void Build_SelectedMarkerUsesSelectedStyle()
    {
        var normal = ShopLocationLayerBuilder.Build(35.6812, 139.7671, selected: false);
        var selected = ShopLocationLayerBuilder.Build(35.6812, 139.7671, selected: true);

        var normalStyle = normal.Layer.Features.Single().Styles.OfType<ImageStyle>().Single();
        var selectedStyle = selected.Layer.Features.Single().Styles.OfType<ImageStyle>().Single();

        Assert.AreEqual(1.15, normalStyle.SymbolScale);
        Assert.AreEqual(1.4, selectedStyle.SymbolScale);
    }
}
