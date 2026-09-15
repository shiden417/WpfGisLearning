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

    public ShopEditView(ShopEditViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        viewModel.RequestClose += ViewModel_RequestClose;
        Loaded += ShopEditView_Loaded;
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
            ShowLocation(_viewModel.ShopLatitude.Value, _viewModel.ShopLongitude.Value);
    }

    private void EditMapControl_MouseLeftButtonUp(object? sender, MouseButtonEventArgs e)
    {
        if (_map is null)
            return;

        var pos = e.GetPosition(EditMapControl);
        var screenPosition = new Mapsui.Manipulations.ScreenPosition((int)pos.X, (int)pos.Y);
        var worldPosition = _map.Navigator.Viewport.ScreenToWorld(screenPosition);
        var lonLat = SphericalMercator.ToLonLat(worldPosition);

        if (_viewModel.TrySetLocation(lonLat.Y, lonLat.X))
            ShowLocation(lonLat.Y, lonLat.X);
    }

    private void ShowLocation(double latitude, double longitude)
    {
        if (_map is null)
            return;

        var point = SphericalMercator.FromLonLat(longitude, latitude).ToMPoint();
        var feature = new PointFeature(point);
        feature.Styles.Add(
            ImageStyles.CreatePinStyle(
                Mapsui.Styles.Color.FromString("#F28C28"),
                Mapsui.Styles.Color.White,
                1.15));

        if (_locationLayer is not null)
            _map.Layers.Remove(_locationLayer);

        _locationLayer = new MemoryLayer
        {
            Name = "SelectedLocation",
            Features = new[] { feature }
        };

        _map.Layers.Add(_locationLayer);
        _map.Navigator.CenterOnAndZoomTo(point, 500);
        EditMapControl.Refresh();
    }

    private void ViewModel_RequestClose(object? sender, EventArgs e)
    {
        Window.GetWindow(this)?.Close();
    }
}
