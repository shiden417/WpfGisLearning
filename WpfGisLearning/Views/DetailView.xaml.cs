using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.Tiling;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using WpfGisLearning.Services;
using WpfGisLearning.ViewModels;

namespace WpfGisLearning.Views;

public partial class DetailView : System.Windows.Controls.UserControl
{
    private readonly DetailViewModel _viewModel;
    private readonly INavigationService _navigationService;
    private readonly IShopService _shopService;

    public DetailView(DetailViewModel viewModel, INavigationService navigationService, IShopService shopService)
    {
        InitializeComponent(); _viewModel = viewModel; _navigationService = navigationService; _shopService = shopService; DataContext = viewModel; InitializeMap(viewModel);
    }

    private void InitializeMap(DetailViewModel viewModel)
    {
        var map = new Mapsui.Map(); map.Layers.Add(OpenStreetMap.CreateTileLayer()); DetailMap.Map = map;
        if (viewModel.Shop is null || !IsValidCoordinate(viewModel.Shop.Latitude, viewModel.Shop.Longitude)) return;
        var point = SphericalMercator.FromLonLat(viewModel.Shop.Longitude, viewModel.Shop.Latitude).ToMPoint();
        var feature = new PointFeature(point); feature.Styles.Add(ImageStyles.CreatePinStyle(Mapsui.Styles.Color.FromString("#343A40"), Mapsui.Styles.Color.FromString("#343A40"), 1.15));
        map.Layers.Add(new MemoryLayer { Name = "Shop", Features = new[] { feature } }); map.Navigator.CenterOnAndZoomTo(point, 500);
    }

    private void Photo_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        var path = (sender as FrameworkElement)?.Tag as string;
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return;
        var image = new Image { Source = new BitmapImage(new Uri(path)), Stretch = System.Windows.Media.Stretch.Uniform, Margin = new Thickness(20) };
        var window = new Window { Title = "写真 - Ramenia", Content = image, Width = 1000, Height = 750, Owner = Window.GetWindow(this), Background = System.Windows.Media.Brushes.Black, WindowStartupLocation = WindowStartupLocation.CenterOwner };
        window.ShowDialog();
    }

    private void EditButton_Click(object sender, RoutedEventArgs e) { if (_viewModel.Shop is null) return; _navigationService.NavigateToShopEdit(_viewModel.Shop.Id); Window.GetWindow(this)?.Close(); }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.Shop is null) return;
        var result = MessageBox.Show($"「{_viewModel.Shop.Name}」を削除しますか？", "店舗削除の確認", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (result != MessageBoxResult.Yes) return;
        _shopService.DeleteShop(_viewModel.Shop.Id); Window.GetWindow(this)?.Close();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Window.GetWindow(this)?.Close();

    private static bool IsValidCoordinate(double lat, double lon) => !double.IsNaN(lat) && !double.IsNaN(lon) && !double.IsInfinity(lat) && !double.IsInfinity(lon) && lat >= -90 && lat <= 90 && lon >= -180 && lon <= 180 && !(lat == 0 && lon == 0);
}
