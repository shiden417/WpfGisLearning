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

        // Ramenia アイコンを設定
        Icon = CreateRameniaIcon();

        // メインコンテンツ
        MainContent.Content = shopListView;

        // 店舗一覧の選択変更を監視
        _shopListViewModel =
            (ShopListViewModel)shopListView.DataContext;

        _shopListViewModel.SelectedShopChanged +=
            ShopListViewModel_SelectedShopChanged;

        // CommandBinding
        CommandBindings.Add(
            new CommandBinding(
                AppCommands.SaveCommand,
                OnSaveExecuted,
                OnSaveCanExecute));

        // Mapsui 初期化
        try
        {
            var map = new Mapsui.Map();
            _map = map;

            // OpenStreetMap
            map.Layers.Add(OpenStreetMap.CreateTileLayer());

            // 店舗を地図上に表示
            var features = new List<IFeature>();

            foreach (var shop in shopService.GetShops())
            {
                try
                {
                    var lat = shop.Latitude;
                    var lon = shop.Longitude;

                    // 無効な座標はスキップ
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

                    // 0/0 は未設定として扱う
                    if (lat == 0 && lon == 0)
                    {
                        continue;
                    }

                    var mapPoint = SphericalMercator
                        .FromLonLat(lon, lat)
                        .ToMPoint();

                    _hasValidCoords = true;

                    if (lon < _minLon)
                        _minLon = lon;

                    if (lon > _maxLon)
                        _maxLon = lon;

                    if (lat < _minLat)
                        _minLat = lat;

                    if (lat > _maxLat)
                        _maxLat = lat;

                    var feature = new PointFeature(mapPoint);

                    feature["Name"] = shop.Name;
                    feature["Address"] = shop.Address;
                    feature["Id"] = shop.Id;

                    // Ramenia のアクセントカラー
                    feature.Styles.Add(
                        new Mapsui.Styles.SymbolStyle
                        {
                            SymbolType = SymbolType.Ellipse,

                            Fill = new Mapsui.Styles.Brush(
                                Mapsui.Styles.Color.FromString(
                                    "#B4552B")),

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

            // 店舗レイヤー
            var memoryLayer = new MemoryLayer
            {
                Name = "Shops",
                Features = features
            };

            map.Layers.Add(memoryLayer);

            // MapControlへ設定
            MapControl.Map = map;

            // マーカークリック検出
            MapControl.MouseLeftButtonUp +=
                MapControl_MouseLeftButtonUp;

            // レイアウト確定後に初期表示を設定
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

        var visual =
            new System.Windows.Media.DrawingVisual();

        using (var context = visual.RenderOpen())
        {
            var formattedText =
                new System.Windows.Media.FormattedText(
                    "🍜",
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new System.Windows.Media.Typeface(
                        new System.Windows.Media.FontFamily(
                            "Segoe UI Emoji"),
                        FontStyles.Normal,
                        FontWeights.Normal,
                        FontStretches.Normal),
                    48,
                    System.Windows.Media.Brushes.Black,
                    1.0);

            // 絵文字を中央に配置
            var x =
                (size - formattedText.Width) / 2;

            var y =
                (size - formattedText.Height) / 2;

            context.DrawText(
                formattedText,
                new Point(x, y));
        }

        var bitmap =
            new System.Windows.Media.Imaging.RenderTargetBitmap(
                size,
                size,
                96,
                96,
                System.Windows.Media.PixelFormats.Pbgra32);

        bitmap.Render(visual);

        bitmap.Freeze();

        return bitmap;
    }

    private void MapControl_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        if (_initialMapPositionSet)
            return;

        if (_map is null || !_hasValidCoords)
            return;

        if (MapControl.ActualWidth <= 0 ||
            MapControl.ActualHeight <= 0)
        {
            return;
        }

        try
        {
            var centerLon =
                (_minLon + _maxLon) / 2.0;

            var centerLat =
                (_minLat + _maxLat) / 2.0;

            var center =
                SphericalMercator
                    .FromLonLat(centerLon, centerLat)
                    .ToMPoint();

            double resolution;

            // 店舗が1店舗だけの場合
            if (_minLon == _maxLon &&
                _minLat == _maxLat)
            {
                resolution =
                    _map.Navigator.Resolutions.Count > 12
                        ? _map.Navigator.Resolutions[12]
                        : _map.Navigator.Resolutions[^1];
            }
            else
            {
                var minMap =
                    SphericalMercator
                        .FromLonLat(_minLon, _minLat)
                        .ToMPoint();

                var maxMap =
                    SphericalMercator
                        .FromLonLat(_maxLon, _maxLat)
                        .ToMPoint();

                var width =
                    Math.Abs(
                        maxMap.X - minMap.X);

                var height =
                    Math.Abs(
                        maxMap.Y - minMap.Y);

                var resX =
                    width / MapControl.ActualWidth;

                var resY =
                    height / MapControl.ActualHeight;

                // 少し余白を確保
                resolution =
                    Math.Max(resX, resY) * 1.2;

                if (resolution <= 0 ||
                    double.IsNaN(resolution) ||
                    double.IsInfinity(resolution))
                {
                    resolution =
                        _map.Navigator.Resolutions.Count > 12
                            ? _map.Navigator.Resolutions[12]
                            : _map.Navigator.Resolutions[^1];
                }
            }

            _map.Navigator.CenterOnAndZoomTo(
                center,
                resolution);

            _initialMapPositionSet = true;
        }
        catch
        {
            // 初期表示調整に失敗しても起動を妨げない
        }
    }

    // 店舗一覧で店舗が選択されたとき
    private void ShopListViewModel_SelectedShopChanged(
        object? sender,
        Shop? shop)
    {
        if (shop is null)
            return;

        if (_map is null)
            return;

        try
        {
            var lat = shop.Latitude;
            var lon = shop.Longitude;

            // 無効な座標の場合は何もしない
            if (double.IsNaN(lat) ||
                double.IsNaN(lon) ||
                double.IsInfinity(lat) ||
                double.IsInfinity(lon))
            {
                return;
            }

            if (lat < -90 || lat > 90 ||
                lon < -180 || lon > 180)
            {
                return;
            }

            if (lat == 0 && lon == 0)
            {
                return;
            }

            var mapPoint =
                SphericalMercator
                    .FromLonLat(lon, lat)
                    .ToMPoint();

            // 選択した店舗へ地図を移動
            var resolution =
                _map.Navigator.Resolutions.Count > 12
                    ? _map.Navigator.Resolutions[12]
                    : _map.Navigator.Resolutions[^1];

            _map.Navigator.CenterOnAndZoomTo(
                mapPoint,
                resolution);

            // InfoCardを表示
            InfoCardName.Text =
                shop.Name;

            InfoCardAddress.Text =
                shop.Address;

            InfoCardBorder.Visibility =
                Visibility.Visible;
        }
        catch
        {
            // 店舗選択時の地図移動に失敗しても
            // アプリの動作を妨げない
        }
    }

    private void MapControl_MouseLeftButtonUp(
        object? sender,
        MouseButtonEventArgs e)
    {
        try
        {
            var pos =
                e.GetPosition(MapControl);

            var screenPos =
                new Mapsui.Manipulations.ScreenPosition(
                    (int)pos.X,
                    (int)pos.Y);

            var mapInfo =
                MapControl.GetMapInfo(
                    screenPos,
                    MapControl.Map?.Layers);

            var layerName =
                mapInfo?.Layer?.Name;

            // 店舗レイヤーだけを対象にする
            if (layerName == "Shops" &&
                mapInfo?.Feature != null)
            {
                var feature =
                    mapInfo.Feature;

                var name =
                    feature["Name"]?.ToString()
                    ?? string.Empty;

                var address =
                    feature["Address"]?.ToString()
                    ?? string.Empty;

                InfoCardName.Text =
                    name;

                InfoCardAddress.Text =
                    address;

                InfoCardBorder.Visibility =
                    Visibility.Visible;
            }
        }
        catch
        {
            // クリック処理で例外が発生しても無視
        }
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
        InfoCardBorder.Visibility =
            Visibility.Collapsed;
    }
}
