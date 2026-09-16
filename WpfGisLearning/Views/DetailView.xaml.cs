using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Tiling;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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
        _map.Layers.Add(new MemoryLayer { Name = "Shop", Style = null, Features = new[] { feature } });
        _map.Navigator.CenterOnAndZoomTo(point, 500);
    }

    private void ZoomInButton_Click(object sender, RoutedEventArgs e) => _map?.Navigator.ZoomIn(ZoomAmount);
    private void ZoomOutButton_Click(object sender, RoutedEventArgs e) => _map?.Navigator.ZoomOut(ZoomAmount);

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

        void Previous() { currentIndex = currentIndex == 0 ? _viewModel.PhotoPaths.Count - 1 : currentIndex - 1; UpdatePhoto(); }
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

    private static BitmapImage LoadBitmap(string path)
    {
        if (!File.Exists(path)) throw new FileNotFoundException("写真ファイルが見つかりません。", path);
        var bitmap = new BitmapImage();
        using var stream = File.OpenRead(path);
        bitmap.BeginInit(); bitmap.CacheOption = BitmapCacheOption.OnLoad; bitmap.StreamSource = stream; bitmap.EndInit(); bitmap.Freeze();
        return bitmap;
    }

    private void EditButton_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.Shop is null) return;

        var shopId = _viewModel.Shop.Id;
        Window.GetWindow(this)?.Close();
        _navigationService.NavigateToShopEdit(shopId);
        _navigationService.NavigateToDetail(shopId);
    }

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

    private static void ShowErrorDialog(string message, Exception ex) =>
        MessageBox.Show($"{message}\n{ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Window.GetWindow(this)?.Close();
}
