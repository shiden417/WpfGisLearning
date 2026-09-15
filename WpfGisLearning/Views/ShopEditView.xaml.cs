using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.Tiling;
using System.Windows;
using System.Windows.Input;
using WpfGisLearning.ViewModels;

namespace WpfGisLearning.Views;

public partial class ShopEditView : System.Windows.Controls.UserControl
{
    private readonly ShopEditViewModel _viewModel;
    private Mapsui.Map? _map;
    private MemoryLayer? _locationLayer;
    private MemoryLayer? _currentLocationLayer;
    private bool _isDraggingLocation;

    public ShopEditView(ShopEditViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        viewModel.RequestClose += ViewModel_RequestClose;
        Loaded += ShopEditView_Loaded;
        EditMapControl.PreviewMouseLeftButtonDown += EditMapControl_PreviewMouseLeftButtonDown;
        EditMapControl.PreviewMouseMove += EditMapControl_PreviewMouseMove;
        EditMapControl.PreviewMouseLeftButtonUp += EditMapControl_PreviewMouseLeftButtonUp;
    }

    private void ShopEditView_Loaded(object sender, RoutedEventArgs e)
    {
        if (_map is not null) return;
        _map = new Mapsui.Map();
        _map.Layers.Add(OpenStreetMap.CreateTileLayer());
        EditMapControl.Map = _map;

        if (_viewModel.ShopId.HasValue && _viewModel.ShopLatitude.HasValue && _viewModel.ShopLongitude.HasValue)
        {
            ShowLocation(_viewModel.ShopLatitude.Value, _viewModel.ShopLongitude.Value);
            var point = SphericalMercator.FromLonLat(_viewModel.ShopLongitude.Value, _viewModel.ShopLatitude.Value).ToMPoint();
            _map.Navigator.CenterOnAndZoomTo(point, 500);
        }
        else
        {
            SetJapanOverview();
        }
    }

    private void SetJapanOverview()
    {
        if (_map is null) return;
        _map.Navigator.CenterOnAndZoomTo(SphericalMercator.FromLonLat(138.0, 36.0).ToMPoint(), 6000);
    }

    private void EditMapControl_PreviewMouseLeftButtonDown(object? sender, MouseButtonEventArgs e)
    {
        if (_map is null) return;
        var pos = e.GetPosition(EditMapControl);

        if (e.ClickCount == 2)
        {
            SetLocationFromScreen(pos.X, pos.Y, false);
            e.Handled = true;
            return;
        }

        if (HasLocationAt(pos.X, pos.Y))
        {
            _isDraggingLocation = true;
            EditMapControl.CaptureMouse();
            e.Handled = true;
        }
    }

    private void EditMapControl_PreviewMouseMove(object? sender, MouseEventArgs e)
    {
        if (!_isDraggingLocation || _map is null) return;
        var pos = e.GetPosition(EditMapControl);
        SetLocationFromScreen(pos.X, pos.Y, false);
        e.Handled = true;
    }

    private void EditMapControl_PreviewMouseLeftButtonUp(object? sender, MouseButtonEventArgs e)
    {
        if (!_isDraggingLocation) return;
        _isDraggingLocation = false;
        EditMapControl.ReleaseMouseCapture();
        var pos = e.GetPosition(EditMapControl);
        SetLocationFromScreen(pos.X, pos.Y, false);
        e.Handled = true;
    }

    private bool HasLocationAt(double x, double y)
    {
        if (_locationLayer is null) return false;
        var screenPosition = new Mapsui.Manipulations.ScreenPosition((int)x, (int)y);
        var mapInfo = EditMapControl.GetMapInfo(screenPosition, new[] { _locationLayer });
        return mapInfo?.Feature is not null;
    }

    private void SetLocationFromScreen(double x, double y, bool recenter)
    {
        if (_map is null) return;
        var screenPosition = new Mapsui.Manipulations.ScreenPosition((int)x, (int)y);
        var worldPosition = _map.Navigator.Viewport.ScreenToWorld(screenPosition);
        var lonLat = SphericalMercator.ToLonLat(worldPosition);
        if (_viewModel.TrySetLocation(lonLat.Y, lonLat.X))
            ShowLocation(lonLat.Y, lonLat.X, recenter);
    }

    private void ShowLocation(double latitude, double longitude, bool recenter = false)
    {
        if (_map is null) return;
        var point = SphericalMercator.FromLonLat(longitude, latitude).ToMPoint();
        var feature = new PointFeature(point);
        feature.Styles.Add(ImageStyles.CreatePinStyle(Mapsui.Styles.Color.FromString("#C56B4D"), Mapsui.Styles.Color.FromString("#343A40"), 1.15));
        if (_locationLayer is not null) _map.Layers.Remove(_locationLayer);
        _locationLayer = new MemoryLayer { Name = "SelectedLocation", Features = new[] { feature } };
        _map.Layers.Add(_locationLayer);
        if (recenter) _map.Navigator.CenterOn(point);
        EditMapControl.Refresh();
    }

    private async void CurrentLocationButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var access = await Windows.Devices.Geolocation.Geolocator.RequestAccessAsync();
            if (access != Windows.Devices.Geolocation.GeolocationAccessStatus.Allowed)
            {
                _viewModel.ErrorMessage = "現在地へのアクセスが許可されていません。Windowsの位置情報設定を確認してください。";
                return;
            }
            var locator = new Windows.Devices.Geolocation.Geolocator { DesiredAccuracyInMeters = 50 };
            var position = await locator.GetGeopositionAsync();
            var latitude = position.Coordinate.Point.Position.Latitude;
            var longitude = position.Coordinate.Point.Position.Longitude;
            if (_viewModel.TrySetLocation(latitude, longitude))
                ShowLocation(latitude, longitude, false);
        }
        catch (Exception ex)
        {
            _viewModel.ErrorMessage = $"現在地を取得できませんでした。\n{ex.Message}";
        }
    }

    private void ZoomInButton_Click(object sender, RoutedEventArgs e) => _map?.Navigator.ZoomIn(500);
    private void ZoomOutButton_Click(object sender, RoutedEventArgs e) => _map?.Navigator.ZoomOut(500);

    private void ViewModel_RequestClose(object? sender, EventArgs e) => Window.GetWindow(this)?.Close();
}
