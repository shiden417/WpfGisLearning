using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Tiling;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using WpfImage = System.Windows.Controls.Image;
using WpfGisLearning.Services;
using WpfGisLearning.Services.Interfaces;
using WpfGisLearning.ViewModels;

namespace WpfGisLearning.Views;

public partial class DetailView : System.Windows.Controls.UserControl
{
    private const long ZoomAmount = 500;

    private readonly DetailViewModel _viewModel;
    private readonly INavigationService _navigationService;
    private readonly IShopService _shopService;
    private Mapsui.Map? _map;

    public DetailView(DetailViewModel viewModel, INavigationService navigationService, IShopService shopService)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _navigationService = navigationService;
        _shopService = shopService;
        DataContext = viewModel;
        InitializeMap(viewModel);
    }

    private void InitializeMap(DetailViewModel viewModel)
    {
        _map = new Mapsui.Map();
        _map.Layers.Add(OpenStreetMap.CreateTileLayer());
        DetailMap.Map = _map;

        if (viewModel.Shop is null || !MapCoordinateValidator.IsValid(viewModel.Shop.Latitude, viewModel.Shop.Longitude)) return;

        var point = SphericalMercator.FromLonLat(viewModel.Shop.Longitude, viewModel.Shop.Latitude).ToMPoint();
        var feature = new PointFeature(point);
        feature.Styles.Add(MapMarkerStyleFactory.CreateShopMarker(selected: false));

        _map.Layers.Add(new MemoryLayer
        {
            Name = "Shop",
            Style = null,
            Features = new[] { feature }
        });
        _map.Navigator.CenterOnAndZoomTo(point, 500);
    }

    private void ZoomInButton_Click(object sender, RoutedEventArgs e) => _map?.Navigator.ZoomIn(ZoomAmount);
    private void ZoomOutButton_Click(object sender, RoutedEventArgs e) => _map?.Navigator.ZoomOut(ZoomAmount);

    private void Photo_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        var path = (sender as FrameworkElement)?.Tag as string;
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return;

        var bitmap = new BitmapImage();
        using (var stream = File.OpenRead(path))
        {
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = stream;
            bitmap.EndInit();
        }
        bitmap.Freeze();

        var image = new WpfImage { Source = bitmap, Stretch = System.Windows.Media.Stretch.Uniform, Margin = new Thickness(20) };
        var window = new Window { Title = "写真 - Ramenia", Content = image, Width = 1000, Height = 750, Owner = Window.GetWindow(this), Background = System.Windows.Media.Brushes.Black, WindowStartupLocation = WindowStartupLocation.CenterOwner };
        window.ShowDialog();
    }

    private void EditButton_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.Shop is null) return;
        _navigationService.NavigateToShopEdit(_viewModel.Shop.Id);
        Window.GetWindow(this)?.Close();
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.Shop is null) return;
        var result = MessageBox.Show($"「{_viewModel.Shop.Name}」を削除しますか？", "店舗削除の確認", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (result != MessageBoxResult.Yes) return;
        _shopService.DeleteShop(_viewModel.Shop.Id);
        Window.GetWindow(this)?.Close();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Window.GetWindow(this)?.Close();
}
