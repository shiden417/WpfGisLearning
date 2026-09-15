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
using WpfGisLearning.ViewModels;
using WpfGisLearning.Views;

namespace WpfGisLearning;

public partial class MainWindow : Window
{
    private Mapsui.Map? _map;
    private MemoryLayer? _shopLayer;
    private bool _isSelectingLocation;

    private double _minLon = double.MaxValue;
    private double _maxLon = double.MinValue;
    private double _minLat = double.MaxValue;
    private double _maxLat = double.MinValue;

    private bool _hasValidCoords;
    private bool _initialMapPositionSet;

    private readonly ShopListViewModel _shopListViewModel;

    public MainWindow(
        MainViewModel viewModel,
        Views.ShopListView shopListView,
        MessageView messageView,
        Services.IShopService shopService)
    {
        InitializeComponent();

        DataContext = viewModel;
        Icon = CreateRameniaIcon();

        MainContent.Content = shopListView;

        _shopListViewModel =
            (ShopListViewModel)shopListView.DataContext;

        _shopListViewModel.SelectedShopChanged +=
            ShopListViewModel_SelectedShopChanged;
        _shopListViewModel.LocationSelectionRequested +=
            ShopListViewModel_LocationSelectionRequested;
        _shopListViewModel.ShopAdded +=
            ShopListViewModel_ShopAdded;

        CommandBindings.Add(
            new CommandBinding(
                AppCommands.SaveCommand,
                OnSaveExecuted,
                OnSaveCanExecute));

        try
        {
            var map = new Mapsui.Map();
            _map = map;

            map.Layers.Add(OpenStreetMap.CreateTileLayer());

            var features = new List<IFeature>();

            foreach (var shop in shopService.GetShops())
            {
                try
                {
                    var lat = shop.Latitude;
                    var lon = shop.Longitude;

                    if (!IsValidCoordinate(lat, lon))
                    {
                        continue;
                    }

                    var mapPoint = SphericalMercator
                        .FromLonLat(lon, lat)
                        .ToMPoint();

                    _hasValidCoords = true;

                    if (lon < _minLon) _minLon = lon;
                    if (lon > _maxLon) _maxLon = lon;
                    if (lat < _minLat) _minLat = lat;
                    if (lat > _maxLat) _maxLat = lat;

                    features.Add(CreateShopFeature(shop));
                }
                catch
                {
                    // 個々の店舗で問題が発生しても処理を継続
                }
            }

            _shopLayer = new MemoryLayer
            {
                Name = "Shops",
                Features = features
            };

            map.Layers.Add(_shopLayer);
            MapControl.Map = map;

            MapControl.MouseLeftButtonUp +=
                MapControl_MouseLeftButtonUp;

            MapControl.Loaded +=
                MapControl_Loaded;
        }
        catch
        {
            // 地図初期化失敗でもアプリの起動を妨げない
        }
    }

    private ImageSource CreateRameniaIcon()
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

            var x = (size - formattedText.Width) / 2;
            var y = (size - formattedText.Height) / 2;

            context.DrawText(formattedText, new Point(x, y));
        }

        var bitmap = new System.Windows.Media.Imaging.RenderTargetBitmap(
            size,
            size,
            96,
            96,
            System.Windows.Media.PixelFormats.Pbgra32);

        bitmap.Render(visual);
        bitmap.Freeze();
        return bitmap;
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

            var center = SphericalMercator
                .FromLonLat(centerLon, centerLat)
                .ToMPoint();

            double resolution;

            if (_minLon == _maxLon && _minLat == _maxLat)
            {
                resolution = _map.Navigator.Resolutions.Count > 12
                    ? _map.Navigator.Resolutions[12]
                    : _map.Navigator.Resolutions[^1];
            }
            else
            {
                var minMap = SphericalMercator
                    .FromLonLat(_minLon, _minLat)
                    .ToMPoint();

                var maxMap = SphericalMercator
                    .FromLonLat(_maxLon, _maxLat)
                    .ToMPoint();

                var width = Math.Abs(maxMap.X - minMap.X);
                var height = Math.Abs(maxMap.Y - minMap.Y);
                var resX = width / MapControl.ActualWidth;
                var resY = height / MapControl.ActualHeight;

                resolution = Math.Max(resX, resY) * 1.2;

                if (resolution <= 0 ||
                    double.IsNaN(resolution) ||
                    double.IsInfinity(resolution))
                {
                    resolution = _map.Navigator.Resolutions.Count > 12
                        ? _map.Navigator.Resolutions[12]
                        : _map.Navigator.Resolutions[^1];
                }
            }

            _map.Navigator.CenterOnAndZoomTo(center, resolution);
            _initialMapPositionSet = true;
        }
        catch
        {
            // 初期表示調整に失敗しても起動を妨げない
        }
    }

    private void ShopListViewModel_SelectedShopChanged(
        object? sender,
        Shop? shop)
    {
        if (shop is null || _map is null)
            return;

        try
        {
            var lat = shop.Latitude;
            var lon = shop.Longitude;

            if (!IsValidCoordinate(lat, lon))
                return;

            var mapPoint = SphericalMercator
                .FromLonLat(lon, lat)
                .ToMPoint();

            var resolution = _map.Navigator.Resolutions.Count > 12
                ? _map.Navigator.Resolutions[12]
                : _map.Navigator.Resolutions[^1];

            _map.Navigator.CenterOnAndZoomTo(mapPoint, resolution);
            ShowInfoCard(shop);
        }
        catch
        {
            // 店舗選択時の地図移動に失敗しても動作を妨げない
        }
    }

    private void ShopListViewModel_LocationSelectionRequested(
        object? sender,
        EventArgs e)
    {
        _isSelectingLocation = true;
    }

    private void ShopListViewModel_ShopAdded(
        object? sender,
        Shop shop)
    {
        AddShopFeatureToMap(shop);
        _isSelectingLocation = false;
    }

    private void MapControl_MouseLeftButtonUp(
        object? sender,
        MouseButtonEventArgs e)
    {
        try
        {
            var pos = e.GetPosition(MapControl);
            var screenPos = new Mapsui.Manipulations.ScreenPosition(
                (int)pos.X,
                (int)pos.Y);

            if (_isSelectingLocation && _map is not null)
            {
                var worldPosition = _map.Navigator.Viewport
                    .ScreenToWorld(screenPos);

                var lonLat = SphericalMercator.ToLonLat(worldPosition);
                var lon = lonLat.X;
                var lat = lonLat.Y;

                if (IsValidCoordinate(lat, lon))
                {
                    _shopListViewModel.SetNewShopLocation(lat, lon);
                    _isSelectingLocation = false;
                }

                return;
            }

            var mapInfo = MapControl.GetMapInfo(
                screenPos,
                MapControl.Map?.Layers ?? Enumerable.Empty<ILayer>());

            if (mapInfo?.Layer?.Name != "Shops" ||
                mapInfo.Feature is null)
            {
                return;
            }

            var feature = mapInfo.Feature;

            if (feature["Id"] is not null &&
                int.TryParse(feature["Id"]?.ToString(), out var shopId))
            {
                _shopListViewModel.SelectShopById(shopId);
            }
        }
        catch
        {
            // クリック処理で例外が発生しても無視
        }
    }

    private void AddShopFeatureToMap(Shop shop)
    {
        if (_shopLayer is null || !IsValidCoordinate(shop.Latitude, shop.Longitude))
            return;

        try
        {
            _shopLayer.Features = _shopLayer.Features
                .Concat(new[] { CreateShopFeature(shop) })
                .ToList();

            MapControl.Refresh();
        }
        catch
        {
            // 店舗追加後の地図更新に失敗しても登録自体は維持
        }
    }

    private static IFeature CreateShopFeature(Shop shop)
    {
        var mapPoint = SphericalMercator
            .FromLonLat(shop.Longitude, shop.Latitude)
            .ToMPoint();

        var feature = new PointFeature(mapPoint);

        feature["Name"] = shop.Name;
        feature["Address"] = shop.Address;
        feature["Id"] = shop.Id;

        feature.Styles.Add(
            new Mapsui.Styles.SymbolStyle
            {
                SymbolType = SymbolType.Ellipse,
                Fill = new Mapsui.Styles.Brush(
                    Mapsui.Styles.Color.FromString("#B4552B")),
                Outline = new Mapsui.Styles.Pen(
                    Mapsui.Styles.Color.White,
                    2),
                SymbolScale = 1.5
            });

        return feature;
    }

    private static bool IsValidCoordinate(double lat, double lon)
    {
        return !double.IsNaN(lat) &&
               !double.IsNaN(lon) &&
               !double.IsInfinity(lat) &&
               !double.IsInfinity(lon) &&
               lat >= -90 && lat <= 90 &&
               lon >= -180 && lon <= 180 &&
               !(lat == 0 && lon == 0);
    }

    private void ShowInfoCard(Shop shop)
    {
        InfoCardName.Text = shop.Name;
        InfoCardAddress.Text = string.IsNullOrWhiteSpace(shop.Address)
            ? "住所未登録"
            : shop.Address;
        InfoCardPrice.Text = $"¥{shop.Price:N0}";
        InfoCardLatitude.Text = shop.Latitude.ToString("F6");
        InfoCardLongitude.Text = shop.Longitude.ToString("F6");
        InfoCardBorder.Visibility = Visibility.Visible;
    }

    private void OnSaveCanExecute(
        object sender,
        CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = true;
    }

    private void OnSaveExecuted(
        object sender,
        ExecutedRoutedEventArgs e)
    {
        MessageBox.Show(
            "保存しました",
            "Save",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private void InfoCardClose_Click(
        object sender,
        RoutedEventArgs e)
    {
        InfoCardBorder.Visibility = Visibility.Collapsed;
    }
}
