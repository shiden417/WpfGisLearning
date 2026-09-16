using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Tiling;
using Mapsui.UI.Wpf;
using System.Windows;
using WpfGisLearning.Models;

namespace WpfGisLearning.Map;

public sealed record MapFeatureSelection(int? ShopId, MPoint? ClusterCenter, int ClusterCount);

public sealed class MapController
{
    private const string CurrentLocationLayerName = "CurrentLocation";
    private const double InitialMapPaddingFactor = 1.2;
    private const int SingleShopResolutionIndex = 12;
    private const long ZoomAmount = 500;
    private const int ClusterZoomInLevel = 1;

    private readonly MapControl _mapControl;
    private Mapsui.Map? _map;
    private MemoryLayer? _currentLocationLayer;
    private ShopMapLayerResult? _shopMapLayerResult;
    private IReadOnlyList<Shop> _shops = [];
    private int? _selectedShopId;

    public MapController(MapControl mapControl)
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
        _map.Navigator.ViewportChanged += MapNavigator_ViewportChanged;
    }

    public void RebuildShopLayer(IEnumerable<Shop> shops, int? selectedShopId)
    {
        if (_map is null) return;

        _shops = shops.ToList();
        _selectedShopId = selectedShopId;
        _shopMapLayerResult = ShopMapLayerBuilder.Build(
            _shops,
            _selectedShopId,
            _map.Navigator.Viewport.Resolution);

        ReplaceShopLayer();
    }

    public void RefreshShopLayer()
    {
        if (_map is null) return;

        _shopMapLayerResult = ShopMapLayerBuilder.Build(
            _shops,
            _selectedShopId,
            _map.Navigator.Viewport.Resolution);

        ReplaceShopLayer();
    }

    private void MapNavigator_ViewportChanged(object sender, ViewportChangedEventArgs e)
    {
        RefreshShopLayer();
    }

    private void ReplaceShopLayer()
    {
        if (_map is null || _shopMapLayerResult is null) return;

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

    public MapFeatureSelection? GetFeatureAt(Point position)
    {
        var mapInfo = _mapControl.GetMapInfo(
            new Mapsui.Manipulations.ScreenPosition((int)position.X, (int)position.Y),
            _mapControl.Map?.Layers ?? Enumerable.Empty<ILayer>());

        if (mapInfo?.Layer?.Name != ShopMapLayerBuilder.LayerName || mapInfo.Feature is not PointFeature pointFeature)
            return null;

        var isCluster = bool.TryParse(mapInfo.Feature["IsCluster"]?.ToString(), out var parsed) && parsed;
        if (isCluster)
        {
            var count = int.TryParse(mapInfo.Feature["ClusterCount"]?.ToString(), out var parsedCount)
                ? parsedCount
                : 0;
            return new MapFeatureSelection(null, pointFeature.Point, count);
        }

        return int.TryParse(mapInfo.Feature["Id"]?.ToString(), out var shopId)
            ? new MapFeatureSelection(shopId, null, 0)
            : null;
    }

    public void ZoomIntoCluster(MPoint center)
    {
        if (_map is null) return;

        _map.Navigator.CenterOn(center);
        _map.Navigator.ZoomToLevel(
            Math.Min(
                _map.Navigator.Resolutions.Count - 1,
                GetCurrentResolutionIndex() + ClusterZoomInLevel));
    }

    private int GetCurrentResolutionIndex()
    {
        if (_map is null || _map.Navigator.Resolutions.Count == 0)
            return 0;

        var current = _map.Navigator.Viewport.Resolution;
        var nearest = 0;
        var nearestDistance = double.MaxValue;

        for (var index = 0; index < _map.Navigator.Resolutions.Count; index++)
        {
            var distance = Math.Abs(_map.Navigator.Resolutions[index] - current);
            if (distance >= nearestDistance) continue;

            nearestDistance = distance;
            nearest = index;
        }

        return nearest;
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
