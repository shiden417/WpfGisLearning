using Mapsui;
using Mapsui.Layers;
using WpfGisLearning.Map;

namespace WpfGisLearning.Tests;

[TestClass]
public class MapControllerTests
{
    [TestMethod]
    public void Initialize_CreatesMapAndAssignsItToAdapter()
    {
        var adapter = new FakeMapControlAdapter();
        var controller = new MapController(adapter);

        controller.Initialize();

        Assert.IsNotNull(controller.Map);
        Assert.AreSame(controller.Map, adapter.Map);
        Assert.IsTrue(controller.Map!.Layers.Any());
    }

    [TestMethod]
    public void RebuildShopLayer_ReplacesShopLayerAndRefreshesMap()
    {
        var adapter = new FakeMapControlAdapter { Map = new Mapsui.Map() };
        var controller = new MapController(adapter);
        var shops =
        [
            new Models.Shop
            {
                Id = 1,
                Name = "Test Ramen",
                Price = 1000m,
                Latitude = 35.681236,
                Longitude = 139.767125,
                RamenType = "醤油"
            }
        ];

        controller.RebuildShopLayer(shops, selectedShopId: null);
        controller.RebuildShopLayer(shops, selectedShopId: 1);

        var shopLayers = controller.Map!.Layers
            .Where(layer => layer.Name == ShopMapLayerBuilder.LayerName)
            .ToList();

        Assert.AreEqual(1, shopLayers.Count);
        Assert.AreEqual(2, adapter.RefreshCount);
    }

    [TestMethod]
    public void ShowCurrentLocation_AddsAndReusesCurrentLocationLayer()
    {
        var adapter = new FakeMapControlAdapter { Map = new Mapsui.Map() };
        var controller = new MapController(adapter);

        controller.ShowCurrentLocation(35.681236, 139.767125);
        controller.ShowCurrentLocation(35.682000, 139.768000);

        var currentLocationLayers = controller.Map!.Layers
            .Where(layer => layer.Name == "CurrentLocation")
            .ToList();

        Assert.AreEqual(1, currentLocationLayers.Count);
        Assert.AreEqual(2, adapter.RefreshCount);
    }

    private sealed class FakeMapControlAdapter : IMapControlAdapter
    {
        public Mapsui.Map? Map { get; set; }
        public double ActualWidth { get; set; } = 1000;
        public double ActualHeight { get; set; } = 800;
        public int RefreshCount { get; private set; }

        public void Refresh() => RefreshCount++;

        public MapInfo? GetMapInfo(Mapsui.Manipulations.ScreenPosition position, IEnumerable<ILayer> layers) => null;
    }
}
