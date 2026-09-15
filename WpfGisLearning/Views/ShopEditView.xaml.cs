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
        EditMapControl.MouseLeftButtonDown += EditMapControl_MouseLeftButtonDown;
        EditMapControl.MouseMove += EditMapControl_MouseMove;
        EditMapControl.MouseLeftButtonUp += EditMapControl_MouseLeftButtonUp;
    }

    private void ShopEditView_Loaded(object sender, RoutedEventArgs e)
    {
        if (_map is not null)
            return;

        _map = new Mapsui.Map();
        _map.Layers.Add(OpenStreetMap.CreateTileLayer());
        EditMapControl.Map = _map;

        if (_viewModel.ShopLatitude.HasValue && _viewModel.ShopLongitude.HasValue)
        {
            ShowLocation(_viewModel.ShopLatitude.Value, _viewModel.ShopLongitude.Value);
            if (_viewModel.ShopId.HasValue)
            {
                var point = SphericalMercator.FromLonLat(_viewModel.ShopLongitude.Value, _viewModel.ShopLatitude.Value).ToMPoint();
                _map.Navigator.CenterOnAndZoomTo(point, 500);
            }
            else
            {
                SetJapanOverview();
            }
        }
        else
        {
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

    private void EditMapControl_MouseLeftButtonDown(object? sender, MouseButtonEventArgs e)
    {
        if (_map is null)
            return;

        var pos = e.GetPosition(EditMapControl);
        var screenPosition = new Mapsui.Manipulations.ScreenPosition((int)pos.X, (int)pos.Y);
        var mapInfo = EditMapControl.GetMapInfo(screenPosition, _map.Layers);

        if (mapInfo?.Layer?.Name != "SelectedLocation")
            return;

        _isDraggingLocation = true;
        EditMapControl.CaptureMouse();
        e.Handled = true;
    }

    private void EditMapControl_MouseMove(object? sender, MouseEventArgs e)
    {
        if (!_isDraggingLocation || _map is null)
            return;

        var pos = e.GetPosition(EditMapControl);
        UpdateLocationFromScreen(pos.X, pos.Y);
        e.Handled = true;
    }

    private void EditMapControl_MouseLeftButtonUp(object? sender, MouseButtonEventArgs e)
    {
        if (!_isDraggingLocation)
            return;

        _isDraggingLocation = false;
        EditMapControl.ReleaseMouseCapture();
        var pos = e.GetPosition(EditMapControl);
        UpdateLocationFromScreen(pos.X, pos.Y);
        e.Handled = true;
    }

    private void UpdateLocationFromScreen(double x, double y)
    {
        if (_map is null)
            return;

        var screenPosition = new Mapsui.Manipulations.ScreenPosition((int)x, (int)y);
        var worldPosition = _map.Navigator.Viewport.ScreenToWorld(screenPosition);
        var lonLat = SphericalMercator.ToLonLat(worldPosition);

        if (_viewModel.TrySetLocation(lonLat.Y, lonLat.X))
            ShowLocation(lonLat.Y, lonLat.X, recenter: false);
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
