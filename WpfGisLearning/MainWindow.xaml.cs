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

    public MainWindow(
        MainViewModel viewModel,
        ShopListView shopListView,
        MessageView messageView,
        IShopService shopService,
        INavigationService navigationService)
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
        catch
        {
            // 地図初期化に失敗しても一覧画面は利用できるようにする
        }
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
            features.Add(CreateShopFeature(shop));
        }

        _shopLayer = new MemoryLayer
        {
            Name = "Shops",
            Features = features
        };

        var oldLayer = _map.Layers.FirstOrDefault(layer => layer.Name == "Shops");
        if (oldLayer is not null)
            _map.Layers.Remove(oldLayer);

        _map.Layers.Add(_shopLayer);
        MapControl.Refresh();
    }

    private void MapControl_Loaded(object sender, RoutedEventArgs e)
    {
        if (_initialMapPositionSet || _map is null || !_hasValidCoords)
            return;

        if (MapControl.ActualWidth <= 0 || MapControl.ActualHeight <= 0)
            return;

        try
        {
            var centerLon = (_minLon + _maxLon) / 2.0;
            var centerLat = (_minLat + _maxLat) / 2.0;
            var center = SphericalMercator.FromLonLat(centerLon, centerLat).ToMPoint();

            double resolution;
            if (_minLon == _maxLon && _minLat == _maxLat)
            {
                resolution = _map.Navigator.Resolutions.Count > 12
                    ? _map.Navigator.Resolutions[12]
                    : _map.Navigator.Resolutions[^1];
            }
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
        catch
        {
            // 初期位置調整に失敗しても起動を妨げない
        }
    }

    private void ShopListViewModel_ShopsChanged(object? sender, EventArgs e)
    {
        RebuildShopLayer();
    }

    private void ShopListViewModel_SelectedShopChanged(object? sender, Shop? shop)
    {
        if (shop is null || _map is null || !IsValidCoordinate(shop.Latitude, shop.Longitude))
            return;

        _selectedShopId = shop.Id;
        var mapPoint = SphericalMercator.FromLonLat(shop.Longitude, shop.Latitude).ToMPoint();
        var resolution = _map.Navigator.Resolutions.Count > 12
            ? _map.Navigator.Resolutions[12]
            : _map.Navigator.Resolutions[^1];

        _map.Navigator.CenterOnAndZoomTo(mapPoint, resolution);
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

            if (mapInfo.Feature["Id"] is not null &&
                int.TryParse(mapInfo.Feature["Id"]?.ToString(), out var shopId))
            {
                _shopListViewModel.SelectShopById(shopId);
            }
        }
        catch
        {
            // マーカークリックで例外が発生してもアプリを停止しない
        }
    }

    private static IFeature CreateShopFeature(Shop shop)
    {
        var mapPoint = SphericalMercator.FromLonLat(shop.Longitude, shop.Latitude).ToMPoint();
        var feature = new PointFeature(mapPoint);

        feature["Name"] = shop.Name;
        feature["Address"] = shop.Address;
        feature["Id"] = shop.Id;

        feature.Styles.Add(
            ImageStyles.CreatePinStyle(
                Mapsui.Styles.Color.FromString("#B83D2E"),
                Mapsui.Styles.Color.White,
                1.15));

        return feature;
    }

    private static bool IsValidCoordinate(double lat, double lon)
    {
        return !double.IsNaN(lat) && !double.IsNaN(lon) &&
               !double.IsInfinity(lat) && !double.IsInfinity(lon) &&
               lat >= -90 && lat <= 90 && lon >= -180 && lon <= 180 &&
               !(lat == 0 && lon == 0);
    }

    private void ShowInfoCard(Shop shop)
    {
        InfoCardName.Text = shop.Name;
        InfoCardAddress.Text = string.IsNullOrWhiteSpace(shop.Address) ? "住所未登録" : shop.Address;
        InfoCardPrice.Text = $"¥{shop.Price:N0}";
        InfoCardBorder.Visibility = Visibility.Visible;
    }

    private void NewShopButton_Click(object sender, RoutedEventArgs e)
    {
        _navigationService.NavigateToShopEdit();
        _shopListViewModel.RefreshFromService();
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
        _shopListViewModel.RefreshFromService();
    }

    private void InfoCardClose_Click(object sender, RoutedEventArgs e)
    {
        InfoCardBorder.Visibility = Visibility.Collapsed;
    }

    private static ImageSource CreateRameniaIcon()
    {
        const int size = 64;
        var visual = new System.Windows.Media.DrawingVisual();

        using (var context = visual.RenderOpen())
        {
            var formattedText = new System.Windows.Media.FormattedText(
                "🍜",
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new System.Windows.Media.Typeface(
                    new System.Windows.Media.FontFamily("Segoe UI Emoji"),
                    FontStyles.Normal,
                    FontWeights.Normal,
                    FontStretches.Normal),
                48,
                System.Windows.Media.Brushes.Black,
                1.0);

            context.DrawText(formattedText, new Point((size - formattedText.Width) / 2, (size - formattedText.Height) / 2));
        }

        var bitmap = new System.Windows.Media.Imaging.RenderTargetBitmap(
            size, size, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
        bitmap.Render(visual);
        bitmap.Freeze();
        return bitmap;
    }
}
