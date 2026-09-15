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
        if (_map is not null)
            return;

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
            // 新規登録ではピンを表示せず、日本全体を見渡せる状態から開始する。
            SetJapanOverview();
        }
    }

    private void SetJapanOverview()
    {
        if (_map is null)
            return;

        var japanCenter = SphericalMercator.FromLonLat(138.0, 36.0).ToMPoint();
        _map.Navigator.CenterOnAndZoomTo(japanCenter, 6000);
    }

    private void EditMapControl_PreviewMouseLeftButtonDown(object? sender, MouseButtonEventArgs e)
    {
        if (_map is null)
            return;

        var pos = e.GetPosition(EditMapControl);

        // 新規登録では最初のダブルクリック地点を店舗位置として設定する。
        if (!_viewModel.ShopId.HasValue && e.ClickCount == 2)
        {
            SetLocationFromScreen(pos.X, pos.Y, true);
            e.Handled = true;
            return;
        }

        // 既存のマーカー上で押した場合はドラッグを開始する。
        if (HasLocationAt(pos.X, pos.Y))
        {
            _isDraggingLocation = true;
            EditMapControl.CaptureMouse();
            e.Handled = true;
        }
    }

    private void EditMapControl_PreviewMouseMove(object? sender, MouseEventArgs e)
    {
        if (!_isDraggingLocation || _map is null)
            return;

        var pos = e.GetPosition(EditMapControl);
        SetLocationFromScreen(pos.X, pos.Y, false);
        e.Handled = true;
    }

    private void EditMapControl_PreviewMouseLeftButtonUp(object? sender, MouseButtonEventArgs e)
    {
        if (!_isDraggingLocation)
            return;

        _isDraggingLocation = false;
        EditMapControl.ReleaseMouseCapture();
        var pos = e.GetPosition(EditMapControl);
        SetLocationFromScreen(pos.X, pos.Y, false);
        e.Handled = true;
    }

    private bool HasLocationAt(double x, double y)
    {
        if (_map is null || _locationLayer is null)
            return false;

        var screenPosition = new Mapsui.Manipulations.ScreenPosition((int)x, (int)y);
        var mapInfo = EditMapControl.GetMapInfo(screenPosition, new[] { _locationLayer });
        return mapInfo?.Feature is not null;
    }

    private void SetLocationFromScreen(double x, double y, bool recenter)
    {
        if (_map is null)
            return;

        var screenPosition = new Mapsui.Manipulations.ScreenPosition((int)x, (int)y);
        var worldPosition = _map.Navigator.Viewport.ScreenToWorld(screenPosition);
        var lonLat = SphericalMercator.ToLonLat(worldPosition);

        if (_viewModel.TrySetLocation(lonLat.Y, lonLat.X))
            ShowLocation(lonLat.Y, lonLat.X, recenter);
    }

    private void ShowLocation(double latitude, double longitude, bool recenter = false)
    {
        if (_map is null)
            return;

        var point = SphericalMercator.FromLonLat(longitude, latitude).ToMPoint();
        var feature = new PointFeature(point);
        feature.Styles.Add(
            ImageStyles.CreatePinStyle(
                Mapsui.Styles.Color.FromString("#C56B4D"),
                Mapsui.Styles.Color.FromString("#343A40"),
                1.15));

        if (_locationLayer is not null)
            _map.Layers.Remove(_locationLayer);

        _locationLayer = new MemoryLayer
        {
            Name = "SelectedLocation",
            Features = new[] { feature }
        };

        _map.Layers.Add(_locationLayer);
        if (recenter)
            _map.Navigator.CenterOn(point);
        EditMapControl.Refresh();
    }

    private void ViewModel_RequestClose(object? sender, EventArgs e)
    {
        Window.GetWindow(this)?.Close();
    }
}
