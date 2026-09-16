using Mapsui.Styles;
using WpfGisLearning.Map;

namespace WpfGisLearning.Tests;

[TestClass]
public class MapMarkerStyleFactoryTests
{
    [TestMethod]
    public void CreateShopMarker_UsesNormalScale_WhenNotSelected()
    {
        var style = MapMarkerStyleFactory.CreateShopMarker(selected: false);

        Assert.IsNotNull(style);
        Assert.AreEqual(1.15, style.SymbolScale);
    }

    [TestMethod]
    public void CreateShopMarker_UsesLargerScale_WhenSelected()
    {
        var style = MapMarkerStyleFactory.CreateShopMarker(selected: true);

        Assert.IsNotNull(style);
        Assert.AreEqual(1.4, style.SymbolScale);
    }

    [TestMethod]
    public void CreateCurrentLocationMarker_UsesEllipseAndExpectedScale()
    {
        var style = MapMarkerStyleFactory.CreateCurrentLocationMarker();

        Assert.AreEqual(SymbolType.Ellipse, style.SymbolType);
        Assert.AreEqual(0.65, style.SymbolScale);
        Assert.IsNotNull(style.Fill);
        Assert.IsNotNull(style.Outline);
        Assert.AreEqual(3, style.Outline!.Width);
    }
}
