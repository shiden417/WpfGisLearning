using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.Tiling;
using System.Windows;
using WpfGisLearning.Services;
using WpfGisLearning.ViewModels;

namespace WpfGisLearning.Views;

public partial class DetailView : System.Windows.Controls.UserControl
{
    private readonly DetailViewModel _viewModel;
    private readonly INavigationService _navigationService;

    public DetailView(DetailViewModel viewModel, INavigationService navigationService)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _navigationService = navigationService;
        DataContext = viewModel;
        InitializeMap(viewModel);
    }

    private void InitializeMap(DetailViewModel viewModel)
    {
        var map = new Mapsui.Map();
        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        DetailMap.Map = map;

        if (viewModel.Shop is null || !IsValidCoordinate(viewModel.Shop.Latitude, viewModel.Shop.Longitude))
            return;

        var point = SphericalMercator
            .FromLonLat(viewModel.Shop.Longitude, viewModel.Shop.Latitude)
            .ToMPoint();

        var feature = new PointFeature(point);
        feature.Styles.Add(
            ImageStyles.CreatePinStyle(
                Mapsui.Styles.Color.FromString("#B83D2E"),
                Mapsui.Styles.Color.White,
                1.15));

        map.Layers.Add(new MemoryLayer
        {
            Name = "Shop",
            Features = new[] { feature }
        });

        map.Navigator.CenterOnAndZoomTo(point, 500);
    }

    private void EditButton_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.Shop is null)
            return;

        _navigationService.NavigateToShopEdit(_viewModel.Shop.Id);
        Window.GetWindow(this)?.Close();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Window.GetWindow(this)?.Close();
    }

    private static bool IsValidCoordinate(double lat, double lon)
    {
        return !double.IsNaN(lat) && !double.IsNaN(lon) &&
               !double.IsInfinity(lat) && !double.IsInfinity(lon) &&
               lat >= -90 && lat <= 90 && lon >= -180 && lon <= 180 &&
               !(lat == 0 && lon == 0);
    }
}
