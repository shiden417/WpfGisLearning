using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Tiling;
using System.Windows;
using WpfGisLearning.Models;

namespace WpfGisLearning.Map;

/// <summary>
/// MapControl上の地図初期化、店舗レイヤー、現在地、移動、ズームなどをまとめて制御するクラスです。
/// WPF固有の処理はIMapControlAdapterに任せ、地図そのものの操作を担当します。
/// </summary>
public sealed class MapController
{
    /// <summary>現在地を表示するレイヤー名です。</summary>
    private const string CurrentLocationLayerName = "CurrentLocation";

    /// <summary>初期表示で店舗周辺に確保する余白倍率です。</summary>
    private const double InitialMapPaddingFactor = 1.2;

    /// <summary>店舗が1件だけの場合に選択するMapsui解像度一覧のインデックスです。</summary>
    private const int SingleShopResolutionIndex = 12;

    /// <summary>ズーム操作で使用する倍率値です。</summary>
    private const long ZoomAmount = 500;

    /// <summary>WPF MapControlへアクセスする抽象化です。</summary>
    private readonly IMapControlAdapter _mapControl;

    /// <summary>現在表示しているMapsuiのMapです。</summary>
    private Mapsui.Map? _map;

    /// <summary>現在地マーカーを保持するレイヤーです。</summary>
    private MemoryLayer? _currentLocationLayer;

    /// <summary>現在表示している店舗レイヤーと境界情報です。</summary>
    private ShopMapLayerResult? _shopMapLayerResult;

    /// <summary>WPF MapControlから利用する通常のコンストラクターです。</summary>
    public MapController(Mapsui.UI.Wpf.MapControl mapControl)
        : this(new MapControlAdapter(mapControl))
    {
    }

    /// <summary>テスト可能なように抽象化されたMapControlアダプターを受け取ります。</summary>
    /// <param name="mapControl">MapControlへのアクセスを提供するアダプターです。</param>
    public MapController(IMapControlAdapter mapControl)
    {
        _mapControl = mapControl;
    }

    /// <summary>現在のMapsui Mapを取得します。</summary>
    public Mapsui.Map? Map => _map;

    /// <summary>初期表示位置をすでに設定したかを示します。</summary>
    public bool InitialMapPositionSet { get; set; }

    /// <summary>MapsuiのMapを作成し、OpenStreetMapのタイルレイヤーを追加します。</summary>
    public void Initialize()
    {
        _map = new Mapsui.Map();
        _map.Layers.Add(OpenStreetMap.CreateTileLayer());
        _mapControl.Map = _map;
    }

    /// <summary>
    /// 店舗一覧から店舗レイヤーを再構築し、選択中の店舗を強調表示します。
    /// </summary>
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

    /// <summary>店舗群全体が見えるように、中心と初期解像度を計算して地図を移動します。</summary>
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

    /// <summary>指定した店舗の位置へ地図の中心を移動します。</summary>
    /// <param name="shop">中心に表示する店舗です。</param>
    public void CenterOnShop(Shop shop)
    {
        if (_map is null) return;

        var point = SphericalMercator.FromLonLat(shop.Longitude, shop.Latitude).ToMPoint();
        _map.Navigator.CenterOn(point);
    }

    /// <summary>画面座標上でクリックされた店舗のIDを取得します。</summary>
    public int? GetShopIdAt(Point position)
    {
        var mapInfo = _mapControl.GetMapInfo(
            new Mapsui.Manipulations.ScreenPosition((int)position.X, (int)position.Y),
            _mapControl.Map?.Layers ?? Enumerable.Empty<ILayer>());

        if (mapInfo?.Layer?.Name != ShopMapLayerBuilder.LayerName || mapInfo.Feature is null)
            return null;

        return int.TryParse(mapInfo.Feature["Id"]?.ToString(), out var shopId) ? shopId : null;
    }

    /// <summary>現在地マーカーを作成または更新します。</summary>
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

    /// <summary>指定した緯度・経度へ地図の中心を移動します。</summary>
    public void CenterOn(double latitude, double longitude)
    {
        if (_map is null) return;

        var point = SphericalMercator.FromLonLat(longitude, latitude).ToMPoint();
        _map.Navigator.CenterOn(point);
    }

    /// <summary>地図を拡大します。</summary>
    public void ZoomIn() => _map?.Navigator.ZoomIn(ZoomAmount);

    /// <summary>地図を縮小します。</summary>
    public void ZoomOut() => _map?.Navigator.ZoomOut(ZoomAmount);
}
