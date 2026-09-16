using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Tiling;
using System.Windows;
using WpfGisLearning.Models;

namespace WpfGisLearning.Map;

public sealed class MapController
{
    private const string CurrentLocationLayerName = "CurrentLocation";
    private const double InitialMapPaddingFactor = 1.2;
    private const int SingleShopResolutionIndex = 12;
    private const long ZoomAmount = 500;

    private readonly IMapControlAdapter _mapControl;
    private Mapsui.Map? _map;
    private MemoryLayer? _currentLocationLayer;
    private ShopMapLayerResult? _shopMapLayerResult;

    public MapController(Mapsui.UI.Wpf.MapControl mapControl)
        : this(new MapControlAdapter(mapControl))
    {
    }

    public MapController(IMapControlAdapter mapControl)
    {
        _mapControl = mapControl;
    }

    public Mapsui.Map? Map => _map;
    public bool InitialMapPositionSet { get; set; }

    public void Initialize()
    {
        _map = new Mapsui.Map();
        _map.Layers.Add(OpenStreetMap.CreateTileLayer());
        _mapControl.Map = _map;
    }

    public void RebuildShopLayer(IEnumerable<Shop> shops, int? selectedShopId)
    {
        if (_map is null) return;

        _shopMapLayerResult = ShopMapLayerBuilder.Build(shops, selectedShopId);
        var oldLayer = _map.Layers.FirstOrDefault(layer => layer.Name == ShopMapLayerBuilder.LayerName);
        if (oldLayer is not null)
            _map.Layers.Remove(oldLayer);

        _map.Layers.Add(_shopMapLayerResult.Layer);
        _mapControl.Refresh();
    }

    public void SetInitialMapPosition()
    {
        var bounds = _shopMapLayerResult?.Bounds;
        if (_map is null || bounds is null || !bounds.HasValidCoordinates) return;
        if (_mapControl.ActualWidth <= 0 || _mapControl.ActualHeight <= 0) return;

        var center = MapViewportCalculator.CalculateCenter(bounds);
        var resolution = MapViewportCalculator.CalculateResolution(
            bounds,
            _map.Navigator.Resolutions,
            _mapControl.ActualWidth,
            _mapControl.ActualHeight,
            InitialMapPaddingFactor,
            SingleShopResolutionIndex);

        _map.Navigator.CenterOnAndZoomTo(center, resolution);
        InitialMapPositionSet = true;
    }

    public void CenterOnShop(Shop shop)
    {
        if (_map is null) return;

        var point = SphericalMercator.FromLonLat(shop.Longitude, shop.Latitude).ToMPoint();
        _map.Navigator.CenterOn(point);
    }

    public int? GetShopIdAt(Point position)
    {
        var mapInfo = _mapControl.GetMapInfo(
            new Mapsui.Manipulations.ScreenPosition((int)position.X, (int)position.Y),
            _mapControl.Map?.Layers ?? Enumerable.Empty<ILayer>());

        if (mapInfo?.Layer?.Name != ShopMapLayerBuilder.LayerName || mapInfo.Feature is null)
            return null;

        return int.TryParse(mapInfo.Feature["Id"]?.ToString(), out var shopId) ? shopId : null;
    }

    public void ShowCurrentLocation(double latitude, double longitude)
    {
        if (_map is null) return;

        var point = SphericalMercator.FromLonLat(longitude, latitude).ToMPoint();
        var feature = new PointFeature(point);
        feature.Styles.Add(MapMarkerStyleFactory.CreateCurrentLocationMarker());

        _currentLocationLayer ??= new MemoryLayer
        {
            Name = CurrentLocationLayerName,
            Style = null
        };

        _currentLocationLayer.Features = new[] { feature };
        if (!_map.Layers.Contains(_currentLocationLayer))
            _map.Layers.Add(_currentLocationLayer);

        _mapControl.Refresh();
    }

    public void CenterOn(double latitude, double longitude)
    {
        if (_map is null) return;

        var point = SphericalMercator.FromLonLat(longitude, latitude).ToMPoint();
        _map.Navigator.CenterOn(point);
    }

    public void ZoomIn() => _map?.Navigator.ZoomIn(ZoomAmount);
    public void ZoomOut() => _map?.Navigator.ZoomOut(ZoomAmount);
}
