using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.Tiling;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using WpfGisLearning.Models;
using WpfGisLearning.Services;
using WpfGisLearning.ViewModels;
using WpfGisLearning.Views;

namespace WpfGisLearning;

public partial class MainWindow : Window
{
    private const string ShopLayerName = ShopMapLayerBuilder.LayerName;
    private const string CurrentLocationLayerName = "CurrentLocation";
    private const double InitialMapPaddingFactor = 1.2;
    private const int SingleShopResolutionIndex = 12;
    private const long ZoomAmount = 500;
    private const uint CurrentLocationAccuracyMeters = 50;
    private const double InfoCardInitialOffset = 18;
    private const int InfoCardFadeDurationMilliseconds = 180;
    private const int InfoCardSlideDurationMilliseconds = 220;
    private const int RameniaIconSize = 64;
    private const int RameniaIconFontSize = 48;
    private const int RameniaIconOffset = 8;

    private Mapsui.Map? _map;
    private MemoryLayer? _currentLocationLayer;
    private bool _initialMapPositionSet;
    private bool _initialLocationRequested;
    private int? _selectedShopId;
    private readonly ShopListViewModel _shopListViewModel;
    private readonly IShopService _shopService;
    private readonly INavigationService _navigationService;
    private readonly NewsView _newsView;

    public MainWindow(
        MainViewModel viewModel,
        ShopListView shopListView,
        MessageView messageView,
        IShopService shopService,
        INavigationService navigationService,
        NewsView newsView)
    {
        InitializeComponent();

        DataContext = viewModel;
        Icon = CreateRameniaIcon();
        MainContent.Content = shopListView;

        _shopListViewModel = (ShopListViewModel)shopListView.DataContext;
        _shopService = shopService;
        _navigationService = navigationService;
        _newsView = newsView;

        _shopListViewModel.SelectedShopChanged += ShopListViewModel_SelectedShopChanged;
        _shopListViewModel.ShopsChanged += ShopListViewModel_ShopsChanged;
        _newsView.RequestBack += NewsView_RequestBack;
        Loaded += MainWindow_Loaded;

        InitializeMap();
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        if (_initialLocationRequested)
        {
            return;
        }

        _initialLocationRequested = true;
        await TryShowCurrentLocationAsync(showMessageOnFailure: false, filterNearby: false);
    }

    private void NewsButton_Click(object sender, RoutedEventArgs e)
    {
        MainArea.Visibility = Visibility.Collapsed;
        NewsContent.Content = _newsView;
        NewsContent.Visibility = Visibility.Visible;
        HeaderSubtitle.Text = "ラーメンの最新情報";
    }

    private void ShopMapButton_Click(object sender, RoutedEventArgs e)
    {
        ShowShopMap();
    }

    private void NewsView_RequestBack(object? sender, EventArgs e)
    {
        ShowShopMap();
    }

    private void ShowShopMap()
    {
        NewsContent.Visibility = Visibility.Collapsed;
        NewsContent.Content = null;
        MainArea.Visibility = Visibility.Visible;
        HeaderSubtitle.Text = "ラーメン店を地図から探す";
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
            MapStatusText.Visibility = Visibility.Collapsed;
        }
        catch (Exception ex)
        {
            ShowMapError("地図を初期化できませんでした。", ex);
        }
    }

    private void RebuildShopLayer()
    {
        if (_map is null)
        {
            return;
        }

        var result = ShopMapLayerBuilder.Build(
            _shopService.GetShops(),
            _selectedShopId);

        ReplaceShopLayer(result.Layer);
    }

    private void ReplaceShopLayer(MemoryLayer newLayer)
    {
        var oldLayer = _map?.Layers.FirstOrDefault(layer => layer.Name == ShopLayerName);
        if (oldLayer is not null)
        {
            _map!.Layers.Remove(oldLayer);
        }

        _map?.Layers.Add(newLayer);
        MapControl.Refresh();
    }

    private void MapControl_Loaded(object sender, RoutedEventArgs e)
    {
        if (_initialMapPositionSet || _map is null)
        {
            return;
        }

        if (MapControl.ActualWidth <= 0 || MapControl.ActualHeight <= 0)
        {
            return;
        }

        var result = ShopMapLayerBuilder.Build(
            _shopService.GetShops(),
            _selectedShopId);

        if (!result.Bounds.HasValidCoordinates)
        {
            return;
        }

        try
        {
            var bounds = result.Bounds;
            var center = SphericalMercator
                .FromLonLat(
                    (bounds.MinLongitude + bounds.MaxLongitude) / 2,
                    (bounds.MinLatitude + bounds.MaxLatitude) / 2)
                .ToMPoint();

            var resolution = CalculateMapResolution(bounds);
            _map.Navigator.CenterOnAndZoomTo(center, resolution);
            _initialMapPositionSet = true;
        }
        catch (Exception ex)
        {
            ShowMapError("地図の表示位置を設定できませんでした。", ex);
        }
    }

    private double CalculateMapResolution(ShopMapBounds bounds)
    {
        if (bounds.MinLongitude == bounds.MaxLongitude
            && bounds.MinLatitude == bounds.MaxLatitude)
        {
            var resolutions = _map!.Navigator.Resolutions;
            var index = Math.Min(SingleShopResolutionIndex, resolutions.Count - 1);
            return resolutions[index];
        }

        var minMap = SphericalMercator
            .FromLonLat(bounds.MinLongitude, bounds.MinLatitude)
            .ToMPoint();
        var maxMap = SphericalMercator
            .FromLonLat(bounds.MaxLongitude, bounds.MaxLatitude)
            .ToMPoint();

        var widthResolution =
            Math.Abs(maxMap.X - minMap.X) / MapControl.ActualWidth;
        var heightResolution =
            Math.Abs(maxMap.Y - minMap.Y) / MapControl.ActualHeight;

        return Math.Max(widthResolution, heightResolution)
            * InitialMapPaddingFactor;
    }

    private void ShopListViewModel_ShopsChanged(object? sender, EventArgs e)
    {
        RebuildShopLayer();
    }

    private void ShopListViewModel_SelectedShopChanged(object? sender, Shop? shop)
    {
        _selectedShopId = shop?.Id;
        RebuildShopLayer();

        if (shop is null || !MapCoordinateValidator.IsValid(shop.Latitude, shop.Longitude))
        {
            return;
        }

        ShowInfoCard(shop);
    }

    private void MapControl_MouseLeftButtonUp(object? sender, MouseButtonEventArgs e)
    {
        try
        {
            var position = e.GetPosition(MapControl);
            var mapInfo = MapControl.GetMapInfo(
                new Mapsui.Manipulations.ScreenPosition(
                    (int)position.X,
                    (int)position.Y),
                MapControl.Map?.Layers ?? Enumerable.Empty<ILayer>());

            if (mapInfo?.Layer?.Name != ShopLayerName || mapInfo.Feature is null)
            {
                return;
            }

            if (mapInfo.Feature["Id"] is not null
                && int.TryParse(mapInfo.Feature["Id"]?.ToString(), out var shopId))
            {
                _shopListViewModel.SelectShopById(shopId);
            }
        }
        catch (Exception ex)
        {
            ShowMapError("地図上の店舗情報を取得できませんでした。", ex);
        }
    }

    private void ShowInfoCard(Shop shop)
    {
        InfoCardName.Text = shop.Name;
        InfoCardType.Text = shop.RamenType;
        InfoCardAddress.Text = string.IsNullOrWhiteSpace(shop.Address)
            ? "住所未登録"
            : shop.Address;
        InfoCardPrice.Text = $"¥{shop.Price:N0}";
        InfoCardRating.Text = $"★ {shop.Rating:F1}  {(shop.IsFavorite ? "★ お気に入り" : string.Empty)}";
        InfoCardBorder.Visibility = Visibility.Visible;

        AnimateInfoCard();
    }

    private void AnimateInfoCard()
    {
        var transform = (TranslateTransform)InfoCardBorder.RenderTransform;
        transform.X = InfoCardInitialOffset;
        transform.Y = InfoCardInitialOffset;
        InfoCardBorder.Opacity = 0;

        var storyboard = new Storyboard();

        AddInfoCardAnimation(
            storyboard,
            new DoubleAnimation(
                0,
                1,
                TimeSpan.FromMilliseconds(InfoCardFadeDurationMilliseconds)),
            InfoCardBorder,
            UIElement.OpacityProperty);

        AddInfoCardAnimation(
            storyboard,
            new DoubleAnimation(
                InfoCardInitialOffset,
                0,
                TimeSpan.FromMilliseconds(InfoCardSlideDurationMilliseconds)),
            transform,
            TranslateTransform.XProperty);

        AddInfoCardAnimation(
            storyboard,
            new DoubleAnimation(
                InfoCardInitialOffset,
                0,
                TimeSpan.FromMilliseconds(InfoCardSlideDurationMilliseconds)),
            transform,
            TranslateTransform.YProperty);

        storyboard.Begin();
    }

    private static void AddInfoCardAnimation(
        Storyboard storyboard,
        AnimationTimeline animation,
        DependencyObject target,
        DependencyProperty property)
    {
        Storyboard.SetTarget(animation, target);
        Storyboard.SetTargetProperty(animation, new PropertyPath(property));
        storyboard.Children.Add(animation);
    }

    private void InfoCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount != 2
            || e.OriginalSource is System.Windows.Controls.Button
            || !_selectedShopId.HasValue)
        {
            return;
        }

        _navigationService.NavigateToDetail(_selectedShopId.Value);
        e.Handled = true;
    }

    private async void CurrentLocationButton_Click(object sender, RoutedEventArgs e)
    {
        await TryShowCurrentLocationAsync(
            showMessageOnFailure: true,
            filterNearby: true);
    }

    private async Task TryShowCurrentLocationAsync(
        bool showMessageOnFailure,
        bool filterNearby)
    {
        try
        {
            var access = await Windows.Devices.Geolocation.Geolocator.RequestAccessAsync();
            if (access != Windows.Devices.Geolocation.GeolocationAccessStatus.Allowed)
            {
                if (showMessageOnFailure)
                {
                    MessageBox.Show(
                        "現在地へのアクセスが許可されていません。Windowsの位置情報設定を確認してください。",
                        "現在地",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }

                return;
            }

            var geolocator = new Windows.Devices.Geolocation.Geolocator
            {
                DesiredAccuracyInMeters = CurrentLocationAccuracyMeters
            };
            var position = await geolocator.GetGeopositionAsync();
            var latitude = position.Coordinate.Point.Position.Latitude;
            var longitude = position.Coordinate.Point.Position.Longitude;

            if (_map is null || !MapCoordinateValidator.IsValid(latitude, longitude))
            {
                return;
            }

            if (filterNearby)
            {
                _shopListViewModel.SetNearbyLocation(latitude, longitude);
            }

            ShowCurrentLocation(latitude, longitude);
            _map.Navigator.CenterOn(
                SphericalMercator
                    .FromLonLat(longitude, latitude)
                    .ToMPoint());
        }
        catch (Exception ex)
        {
            ShowMapError("現在地を取得できませんでした。", ex);

            if (showMessageOnFailure)
            {
                MessageBox.Show(
                    $"現在地を取得できませんでした。\n{ex.Message}",
                    "現在地",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }
    }

    private void ShowCurrentLocation(double latitude, double longitude)
    {
        if (_map is null)
        {
            return;
        }

        var point = SphericalMercator
            .FromLonLat(longitude, latitude)
            .ToMPoint();
        var feature = new PointFeature(point);
        feature.Styles.Add(new SymbolStyle
        {
            SymbolType = SymbolType.Ellipse,
            SymbolScale = 1.15,
            Fill = new Mapsui.Styles.Brush(
                Mapsui.Styles.Color.FromString("#4A90E2")),
            Outline = new Mapsui.Styles.Pen(
                Mapsui.Styles.Color.FromString("#FFFFFF"),
                3)
        });

        _currentLocationLayer ??= new MemoryLayer
        {
            Name = CurrentLocationLayerName
        };
        _currentLocationLayer.Features = new[] { feature };

        if (!_map.Layers.Contains(_currentLocationLayer))
        {
            _map.Layers.Add(_currentLocationLayer);
        }

        MapControl.Refresh();
    }

    private void ShowAllShopsButton_Click(object sender, RoutedEventArgs e)
    {
        _selectedShopId = null;
        _shopListViewModel.NearbyOnly = false;
        InfoCardBorder.Visibility = Visibility.Collapsed;
        RebuildShopLayer();

        _initialMapPositionSet = false;
        MapControl_Loaded(sender, e);
    }

    private void ZoomInButton_Click(object sender, RoutedEventArgs e)
    {
        _map?.Navigator.ZoomIn(ZoomAmount);
    }

    private void ZoomOutButton_Click(object sender, RoutedEventArgs e)
    {
        _map?.Navigator.ZoomOut(ZoomAmount);
    }

    private void InfoCardDetail_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedShopId.HasValue)
        {
            _navigationService.NavigateToDetail(_selectedShopId.Value);
        }
    }

    private void InfoCardEdit_Click(object sender, RoutedEventArgs e)
    {
        if (!_selectedShopId.HasValue)
        {
            return;
        }

        _navigationService.NavigateToShopEdit(_selectedShopId.Value);
        RefreshShopData();
    }

    private void InfoCardClose_Click(object sender, RoutedEventArgs e)
    {
        InfoCardBorder.Visibility = Visibility.Collapsed;
        InfoCardBorder.Opacity = 0;
    }

    public void RefreshShopData()
    {
        _shopListViewModel.RefreshFromService();
        RebuildShopLayer();
    }

    private void ShowMapError(string message, Exception exception)
    {
        MapStatusText.Text = $"{message}\n{exception.Message}";
        MapStatusText.Visibility = Visibility.Visible;
    }

    private static ImageSource CreateRameniaIcon()
    {
        var visual = new DrawingVisual();

        using (var context = visual.RenderOpen())
        {
            var formattedText = new FormattedText(
                "🍜",
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(
                    new FontFamily("Segoe UI Emoji"),
                    FontStyles.Normal,
                    FontWeights.Normal,
                    FontStretches.Normal),
                RameniaIconFontSize,
                Brushes.Black,
                1.0);

            context.DrawText(
                formattedText,
                new System.Windows.Point(
                    RameniaIconOffset,
                    RameniaIconOffset));
        }

        var bitmap = new RenderTargetBitmap(
            RameniaIconSize,
            RameniaIconSize,
            96,
            96,
            System.Windows.Media.PixelFormats.Pbgra32);
        bitmap.Render(visual);

        return bitmap;
    }
}
