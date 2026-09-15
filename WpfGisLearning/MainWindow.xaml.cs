using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.Tiling;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using WpfGisLearning.Models;
using WpfGisLearning.Services;
using WpfGisLearning.ViewModels;
using WpfGisLearning.Views;

namespace WpfGisLearning;

public partial class MainWindow : Window
{
    private Mapsui.Map? _map;
    private MemoryLayer? _shopLayer;
    private MemoryLayer? _currentLocationLayer;
    private double _minLon = double.MaxValue;
    private double _maxLon = double.MinValue;
    private double _minLat = double.MaxValue;
    private double _maxLat = double.MinValue;
    private bool _hasValidCoords;
    private bool _initialMapPositionSet;
    private int? _selectedShopId;

    private readonly ShopListViewModel _shopListViewModel;
    private readonly IShopService _shopService;
    private readonly INavigationService _navigationService;

    public MainWindow(MainViewModel viewModel, ShopListView shopListView, MessageView messageView, IShopService shopService, INavigationService navigationService)
    {
        InitializeComponent();
        DataContext = viewModel;
        Icon = CreateRameniaIcon();
        MainContent.Content = shopListView;
        _shopListViewModel = (ShopListViewModel)shopListView.DataContext;
        _shopService = shopService;
        _navigationService = navigationService;
        _shopListViewModel.SelectedShopChanged += ShopListViewModel_SelectedShopChanged;
        _shopListViewModel.ShopsChanged += ShopListViewModel_ShopsChanged;
        InitializeMap();
    }

    private void InitializeMap()
    {
        try
        {
            _map = new Mapsui.Map();
            _map.Layers.Add(OpenStreetMap.CreateTileLayer());
            MapControl.Map = _map;
            RebuildShopLayer();
            MapControl.MouseLeftButtonUp += MapControl_MouseLeftButtonUp;
            MapControl.Loaded += MapControl_Loaded;
        }
        catch { }
    }

    private void RebuildShopLayer()
    {
        if (_map is null)
            return;

        var features = new List<IFeature>();
        _minLon = double.MaxValue;
        _maxLon = double.MinValue;
        _minLat = double.MaxValue;
        _maxLat = double.MinValue;
        _hasValidCoords = false;

        foreach (var shop in _shopService.GetShops())
        {
            if (!IsValidCoordinate(shop.Latitude, shop.Longitude))
                continue;
            _hasValidCoords = true;
            _minLon = Math.Min(_minLon, shop.Longitude);
            _maxLon = Math.Max(_maxLon, shop.Longitude);
            _minLat = Math.Min(_minLat, shop.Latitude);
            _maxLat = Math.Max(_maxLat, shop.Latitude);
            features.Add(CreateShopFeature(shop, shop.Id == _selectedShopId));
        }

        _shopLayer = new MemoryLayer { Name = "Shops", Features = features };
        var oldLayer = _map.Layers.FirstOrDefault(layer => layer.Name == "Shops");
        if (oldLayer is not null)
            _map.Layers.Remove(oldLayer);
        _map.Layers.Add(_shopLayer);
        MapControl.Refresh();
    }

    private void MapControl_Loaded(object sender, RoutedEventArgs e)
    {
        if (_initialMapPositionSet || _map is null || !_hasValidCoords || MapControl.ActualWidth <= 0 || MapControl.ActualHeight <= 0)
            return;

        try
        {
            var centerLon = (_minLon + _maxLon) / 2.0;
            var centerLat = (_minLat + _maxLat) / 2.0;
            var center = SphericalMercator.FromLonLat(centerLon, centerLat).ToMPoint();
            double resolution;
            if (_minLon == _maxLon && _minLat == _maxLat)
                resolution = _map.Navigator.Resolutions.Count > 12 ? _map.Navigator.Resolutions[12] : _map.Navigator.Resolutions[^1];
            else
            {
                var minMap = SphericalMercator.FromLonLat(_minLon, _minLat).ToMPoint();
                var maxMap = SphericalMercator.FromLonLat(_maxLon, _maxLat).ToMPoint();
                var width = Math.Abs(maxMap.X - minMap.X);
                var height = Math.Abs(maxMap.Y - minMap.Y);
                resolution = Math.Max(width / MapControl.ActualWidth, height / MapControl.ActualHeight) * 1.2;
            }
            _map.Navigator.CenterOnAndZoomTo(center, resolution);
            _initialMapPositionSet = true;
        }
        catch { }
    }

    private void ShopListViewModel_ShopsChanged(object? sender, EventArgs e) => RebuildShopLayer();

    private void ShopListViewModel_SelectedShopChanged(object? sender, Shop? shop)
    {
        _selectedShopId = shop?.Id;
        RebuildShopLayer();
        if (shop is null || _map is null || !IsValidCoordinate(shop.Latitude, shop.Longitude))
            return;

        // 店舗選択ではズームを変更しない。現在の地図の表示範囲をそのまま維持する。
        ShowInfoCard(shop);
    }

    private void MapControl_MouseLeftButtonUp(object? sender, MouseButtonEventArgs e)
    {
        try
        {
            var pos = e.GetPosition(MapControl);
            var screenPos = new Mapsui.Manipulations.ScreenPosition((int)pos.X, (int)pos.Y);
            var mapInfo = MapControl.GetMapInfo(screenPos, MapControl.Map?.Layers ?? Enumerable.Empty<ILayer>());
            if (mapInfo?.Layer?.Name != "Shops" || mapInfo.Feature is null)
                return;
            if (mapInfo.Feature["Id"] is not null && int.TryParse(mapInfo.Feature["Id"]?.ToString(), out var shopId))
                _shopListViewModel.SelectShopById(shopId);
        }
        catch { }
    }

    private static IFeature CreateShopFeature(Shop shop, bool selected)
    {
        var mapPoint = SphericalMercator.FromLonLat(shop.Longitude, shop.Latitude).ToMPoint();
        var feature = new PointFeature(mapPoint);
        feature["Name"] = shop.Name;
        feature["Address"] = shop.Address;
        feature["Id"] = shop.Id;
        feature.Styles.Add(ImageStyles.CreatePinStyle(
            Mapsui.Styles.Color.FromString(selected ? "#C56B4D" : "#343A40"),
            Mapsui.Styles.Color.FromString("#343A40"),
            selected ? 1.35 : 1.15));
        return feature;
    }

    private static bool IsValidCoordinate(double lat, double lon) =>
        !double.IsNaN(lat) && !double.IsNaN(lon) && !double.IsInfinity(lat) && !double.IsInfinity(lon) &&
        lat >= -90 && lat <= 90 && lon >= -180 && lon <= 180 && !(lat == 0 && lon == 0);

    private void ShowInfoCard(Shop shop)
    {
        InfoCardName.Text = shop.Name;
        InfoCardType.Text = $"{shop.RamenType}  ·  {shop.RecommendedMenu}";
        InfoCardAddress.Text = string.IsNullOrWhiteSpace(shop.Address) ? "住所未登録" : shop.Address;
        InfoCardPrice.Text = $"¥{shop.Price:N0}";
        InfoCardRating.Text = $"★ {shop.Rating:F1}  {(shop.IsFavorite ? "★ お気に入り" : string.Empty)}";
        InfoCardBorder.Visibility = Visibility.Visible;
    }

    private async void CurrentLocationButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var access = await Windows.Devices.Geolocation.Geolocator.RequestAccessAsync();
            if (access != Windows.Devices.Geolocation.GeolocationAccessStatus.Allowed)
            {
                MessageBox.Show("現在地へのアクセスが許可されていません。Windowsの位置情報設定を確認してください。", "現在地", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var locator = new Windows.Devices.Geolocation.Geolocator { DesiredAccuracyInMeters = 50 };
            var position = await locator.GetGeopositionAsync();
            var latitude = position.Coordinate.Point.Position.Latitude;
            var longitude = position.Coordinate.Point.Position.Longitude;
            if (_map is null || !IsValidCoordinate(latitude, longitude))
                return;

            _shopListViewModel.SetNearbyLocation(latitude, longitude);
            ShowCurrentLocation(latitude, longitude);
            var point = SphericalMercator.FromLonLat(longitude, latitude).ToMPoint();
            _map.Navigator.CenterOn(point);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"現在地を取得できませんでした。\n{ex.Message}", "現在地", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void ShowCurrentLocation(double latitude, double longitude)
    {
        if (_map is null)
            return;

        var point = SphericalMercator.FromLonLat(longitude, latitude).ToMPoint();
        var feature = new PointFeature(point);
        feature.Styles.Add(ImageStyles.CreatePinStyle(
            Mapsui.Styles.Color.FromString("#3E7CB1"),
            Mapsui.Styles.Color.FromString("#3E7CB1"),
            1.0));

        _currentLocationLayer ??= new MemoryLayer { Name = "CurrentLocation" };
        _currentLocationLayer.Features = new[] { feature };
        if (!_map.Layers.Contains(_currentLocationLayer))
            _map.Layers.Add(_currentLocationLayer);
        MapControl.Refresh();
    }

    private void ShowAllShopsButton_Click(object sender, RoutedEventArgs e)
    {
        _selectedShopId = null;
        _shopListViewModel.NearbyOnly = false;
        InfoCardBorder.Visibility = Visibility.Collapsed;
        if (_map is not null && _currentLocationLayer is not null)
            _map.Layers.Remove(_currentLocationLayer);
        RebuildShopLayer();
        _initialMapPositionSet = false;
        MapControl_Loaded(sender, e);
    }

    private void InfoCardDetail_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedShopId.HasValue)
            _navigationService.NavigateToDetail(_selectedShopId.Value);
    }

    private void InfoCardEdit_Click(object sender, RoutedEventArgs e)
    {
        if (!_selectedShopId.HasValue)
            return;
        _navigationService.NavigateToShopEdit(_selectedShopId.Value);
        RefreshShopData();
    }

    private void InfoCardClose_Click(object sender, RoutedEventArgs e) => InfoCardBorder.Visibility = Visibility.Collapsed;

    public void RefreshShopData()
    {
        _shopListViewModel.RefreshFromService();
        RebuildShopLayer();
    }

    private static ImageSource CreateRameniaIcon()
    {
        const int size = 64;
        var visual = new System.Windows.Media.DrawingVisual();
        using (var context = visual.RenderOpen())
        {
            var formattedText = new System.Windows.Media.FormattedText(
                "🍜", System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                new System.Windows.Media.Typeface(new System.Windows.Media.FontFamily("Segoe UI Emoji"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
                48, System.Windows.Media.Brushes.Black, 1.0);
            context.DrawText(formattedText, new Point((size - formattedText.Width) / 2, (size - formattedText.Height) / 2));
        }
        var bitmap = new System.Windows.Media.Imaging.RenderTargetBitmap(size, size, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
        bitmap.Render(visual);
        bitmap.Freeze();
        return bitmap;
    }
}
