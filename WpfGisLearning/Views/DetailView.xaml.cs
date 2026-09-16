using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfGisLearning.Map;
using WpfGisLearning.Services.Interfaces;
using WpfGisLearning.ViewModels;

namespace WpfGisLearning.Views;

/// <summary>
/// 店舗詳細画面のViewです。
/// 店舗情報表示に加えて、WPF側で必要な地図操作・写真ビューア・外部地図起動を担当します。
/// </summary>
public partial class DetailView : System.Windows.Controls.UserControl
{
    // 詳細画面の地図で使用するズーム量です。
    private const long ZoomAmount = 500;

    // 画面に表示している店舗のViewModelです。
    private readonly DetailViewModel _viewModel;

    // 編集画面への遷移を担当するサービスです。
    private readonly INavigationService _navigationService;

    // 店舗削除など、店舗データ操作を担当するサービスです。
    private readonly IShopService _shopService;

    // 詳細画面専用のMapsui地図です。
    private Mapsui.Map? _map;

    /// <summary>依存するViewModelとサービスを受け取り、画面を初期化します。</summary>
    public DetailView(DetailViewModel viewModel, INavigationService navigationService, IShopService shopService)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _navigationService = navigationService;
        _shopService = shopService;
        DataContext = viewModel;
        InitializeMap(viewModel);
    }

    /// <summary>店舗位置を中心にした詳細画面用の地図を生成します。</summary>
    private void InitializeMap(DetailViewModel viewModel)
    {
        _map = new Mapsui.Map();
        _map.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());
        DetailMap.Map = _map;
        if (viewModel.Shop is null || !MapCoordinateValidator.IsValid(viewModel.Shop.Latitude, viewModel.Shop.Longitude)) return;

        var result = ShopLocationLayerBuilder.Build(viewModel.Shop.Latitude, viewModel.Shop.Longitude);
        _map.Layers.Add(result.Layer);
        _map.Navigator.CenterOnAndZoomTo(result.Point, ZoomAmount);
    }

    /// <summary>編集後などにViewModelと地図を再読み込みします。</summary>
    private void RefreshDetailView()
    {
        _viewModel.Reload();
        InitializeMap(_viewModel);
    }

    /// <summary>詳細画面の地図を拡大します。</summary>
    private void ZoomInButton_Click(object sender, RoutedEventArgs e) => _map?.Navigator.ZoomIn(ZoomAmount);

    /// <summary>詳細画面の地図を縮小します。</summary>
    private void ZoomOutButton_Click(object sender, RoutedEventArgs e) => _map?.Navigator.ZoomOut(ZoomAmount);

    /// <summary>店舗位置をGoogle Mapsで開きます。</summary>
    private void OpenExternalMapButton_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.Shop is null) return;
        try
        {
            var url = ExternalMapLinkBuilder.CreateGoogleMapsUrl(_viewModel.Shop);
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            ShowErrorDialog("外部地図を開けませんでした。", ex);
        }
    }

    /// <summary>店舗写真をクリックしたとき、写真ビューアを開きます。</summary>
    private void Photo_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if ((sender as FrameworkElement)?.Tag is not string path) return;
        var startIndex = _viewModel.PhotoPaths.IndexOf(path);
        if (startIndex < 0) return;
        try
        {
            CreatePhotoViewerWindow(startIndex).ShowDialog();
        }
        catch (Exception ex)
        {
            ShowErrorDialog("写真を表示できませんでした。", ex);
        }
    }

    /// <summary>写真を前後に切り替えられるモーダルビューアWindowを生成します。</summary>
    private Window CreatePhotoViewerWindow(int startIndex)
    {
        var window = new Window
        {
            Title = $"写真 - {_viewModel.Shop?.Name ?? "店舗"}",
            Width = 1000,
            Height = 750,
            Owner = Window.GetWindow(this),
            Background = Brushes.Black,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        var image = new System.Windows.Controls.Image
        {
            Stretch = Stretch.Uniform,
            Margin = new Thickness(48, 48, 48, 72),
            Focusable = true
        };
        var counter = new TextBlock
        {
            Foreground = Brushes.White,
            FontSize = 14,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(0, 0, 0, 22)
        };
        var previousButton = CreatePhotoNavigationButton(isPrevious: true);
        var nextButton = CreatePhotoNavigationButton(isPrevious: false);
        var closeButton = new Button
        {
            Content = CreateCloseIcon(),
            Width = 44,
            Height = 44,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 12, 12, 0),
            Padding = new Thickness(0),
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center
        };

        var grid = new Grid();
        grid.Children.Add(image);
        grid.Children.Add(previousButton);
        grid.Children.Add(nextButton);
        grid.Children.Add(counter);
        grid.Children.Add(closeButton);
        window.Content = grid;

        var currentIndex = startIndex;

        // 現在位置の写真、件数表示、前後ボタンの表示状態を更新します。
        void UpdatePhoto()
        {
            if (_viewModel.PhotoPaths.Count == 0) { window.Close(); return; }
            currentIndex = Math.Clamp(currentIndex, 0, _viewModel.PhotoPaths.Count - 1);
            image.Source = LoadBitmap(_viewModel.PhotoPaths[currentIndex]);
            counter.Text = $"{currentIndex + 1} / {_viewModel.PhotoPaths.Count}";
            var showNavigation = _viewModel.PhotoPaths.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
            previousButton.Visibility = showNavigation;
            nextButton.Visibility = showNavigation;
        }

        // 前の写真へ移動し、先頭からは末尾へ循環します。
        void Previous() { currentIndex = currentIndex == 0 ? _viewModel.PhotoPaths.Count - 1 : currentIndex - 1; UpdatePhoto(); }

        // 次の写真へ移動し、末尾からは先頭へ循環します。
        void Next() { currentIndex = currentIndex == _viewModel.PhotoPaths.Count - 1 ? 0 : currentIndex + 1; UpdatePhoto(); }

        previousButton.Click += (_, _) => Previous();
        nextButton.Click += (_, _) => Next();
        closeButton.Click += (_, _) => window.Close();
        window.KeyDown += (_, args) =>
        {
            if (args.Key == System.Windows.Input.Key.Left) { Previous(); args.Handled = true; }
            else if (args.Key == System.Windows.Input.Key.Right) { Next(); args.Handled = true; }
            else if (args.Key == System.Windows.Input.Key.Escape) { window.Close(); args.Handled = true; }
        };
        UpdatePhoto();
        window.Loaded += (_, _) => image.Focus();
        return window;
    }

    /// <summary>写真ビューアの前へ・次へボタンを生成します。</summary>
    private static Button CreatePhotoNavigationButton(bool isPrevious)
    {
        var path = new System.Windows.Shapes.Path
        {
            Data = Geometry.Parse(isPrevious ? "M 9,2 L 2,9 L 9,16" : "M 2,2 L 9,9 L 2,16"),
            Stroke = Brushes.DimGray,
            StrokeThickness = 2.5,
            StrokeStartLineCap = PenLineCap.Round,
            StrokeEndLineCap = PenLineCap.Round,
            StrokeLineJoin = PenLineJoin.Round,
            Width = 14,
            Height = 18,
            Stretch = Stretch.Fill,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0),
            RenderTransformOrigin = new Point(0.5, 0.5)
        };

        return new Button
        {
            Content = path,
            Width = 56,
            Height = 72,
            HorizontalAlignment = isPrevious ? HorizontalAlignment.Left : HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = isPrevious ? new Thickness(12, 0, 0, 0) : new Thickness(0, 0, 12, 0),
            Padding = new Thickness(0),
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center
        };
    }

    /// <summary>写真ビューア右上の閉じるアイコンを生成します。</summary>
    private static System.Windows.Shapes.Path CreateCloseIcon() => new()
    {
        Data = Geometry.Parse("M 4,4 L 16,16 M 16,4 L 4,16"),
        Stroke = Brushes.DimGray,
        StrokeThickness = 2.25,
        StrokeStartLineCap = PenLineCap.Round,
        StrokeEndLineCap = PenLineCap.Round,
        Width = 20,
        Height = 20,
        Stretch = Stretch.Fill,
        HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment = VerticalAlignment.Center,
        Margin = new Thickness(0)
    };

    /// <summary>指定された画像ファイルをWPFのBitmapImageへ読み込みます。</summary>
    private static BitmapImage LoadBitmap(string path)
    {
        if (!File.Exists(path)) throw new FileNotFoundException("写真ファイルが見つかりません。", path);
        var bitmap = new BitmapImage();
        using var stream = File.OpenRead(path);
        bitmap.BeginInit(); bitmap.CacheOption = BitmapCacheOption.OnLoad; bitmap.StreamSource = stream; bitmap.EndInit(); bitmap.Freeze();
        return bitmap;
    }

    /// <summary>現在の店舗を編集画面へ渡します。</summary>
    private void EditButton_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.Shop is null) return;
        var shopId = _viewModel.Shop.Id;
        _navigationService.NavigateToShopEdit(shopId);
        RefreshDetailView();
    }

    /// <summary>確認後に現在の店舗を削除し、詳細Windowを閉じます。</summary>
    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.Shop is null) return;
        var result = MessageBox.Show($"「{_viewModel.Shop.Name}」を削除しますか？", "店舗削除の確認", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (result != MessageBoxResult.Yes) return;
        try
        {
            _shopService.DeleteShop(_viewModel.Shop.Id);
            Window.GetWindow(this)?.Close();
        }
        catch (Exception ex)
        {
            ShowErrorDialog("店舗を削除できませんでした。", ex);
        }
    }

    /// <summary>WPFのMessageBoxで統一したエラー表示を行います。</summary>
    private static void ShowErrorDialog(string message, Exception ex) =>
        MessageBox.Show($"{message}\n{ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);

    /// <summary>詳細画面をホストしているWindowを閉じます。</summary>
    private void CloseButton_Click(object sender, RoutedEventArgs e) => Window.GetWindow(this)?.Close();
}
