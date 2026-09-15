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

                    if (double.IsNaN(lat) ||
                        double.IsNaN(lon) ||
                        double.IsInfinity(lat) ||
                        double.IsInfinity(lon))
                    {
                        continue;
                    }

                    if (lat < -90 || lat > 90 ||
                        lon < -180 || lon > 180)
                    {
                        continue;
                    }

                    if (lat == 0 && lon == 0)
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

                    features.Add(feature);
                }
                catch
                {
                    // 個々の店舗で問題が発生しても処理を継続
                }
            }

            var memoryLayer = new MemoryLayer
            {
                Name = "Shops",
                Features = features
            };

            map.Layers.Add(memoryLayer);
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

            ShowInfoCard(shop.Name, shop.Address);
        }
        catch
        {
            // 店舗選択時の地図移動に失敗しても動作を妨げない
        }
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

            var mapInfo = MapControl.GetMapInfo(
                screenPos,
                MapControl.Map?.Layers);

            if (mapInfo?.Layer?.Name != "Shops" ||
                mapInfo.Feature is null)
            {
                return;
            }

            var feature = mapInfo.Feature;
            var name = feature["Name"]?.ToString() ?? string.Empty;
            var address = feature["Address"]?.ToString() ?? string.Empty;

            ShowInfoCard(name, address);

            // 地図で選択した店舗を一覧側のSelectedShopにも反映する。
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

    private void ShowInfoCard(string name, string address)
    {
        InfoCardName.Text = name;
        InfoCardAddress.Text = address;
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