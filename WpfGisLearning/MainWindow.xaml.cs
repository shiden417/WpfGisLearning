using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using WpfGisLearning.Models;
using WpfGisLearning.Services;
using WpfGisLearning.Services.Interfaces;
using WpfGisLearning.ViewModels;
using WpfGisLearning.Views;

namespace WpfGisLearning;

public partial class MainWindow : Window
{
    private const int InfoCardInitialOffset = 18;
    private const int InfoCardFadeDurationMilliseconds = 180;
    private const int InfoCardSlideDurationMilliseconds = 220;
    private const int RameniaIconSize = 64;
    private const int RameniaIconFontSize = 48;

    private readonly MapController _mapController;
    private readonly ShopListViewModel _shopListViewModel;
    private readonly ICurrentLocationService _currentLocationService;
    private readonly NewsView _newsView;
    private bool _initialLocationRequested;
    private int? _selectedShopId;

    public MainWindow(
        ShopListView shopListView,
        ICurrentLocationService currentLocationService,
        NewsView newsView)
    {
        InitializeComponent();
        Icon = CreateRameniaIcon();
        MainContent.Content = shopListView;
        _shopListViewModel = (ShopListViewModel)shopListView.DataContext;
        _currentLocationService = currentLocationService;
        _newsView = newsView;
        _mapController = new MapController(MapControl);

        _shopListViewModel.SelectedShopChanged += ShopListViewModel_SelectedShopChanged;
        _shopListViewModel.ShopsChanged += ShopListViewModel_ShopsChanged;
        _newsView.RequestBack += NewsView_RequestBack;
        Loaded += MainWindow_Loaded;

        InitializeMap();
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        if (_initialLocationRequested) return;
        _initialLocationRequested = true;
        await TryShowCurrentLocationAsync(showMessageOnFailure: false);
    }

    private void NewsButton_Click(object sender, RoutedEventArgs e)
    {
        MainArea.Visibility = Visibility.Collapsed;
        NewsContent.Content = _newsView;
        NewsContent.Visibility = Visibility.Visible;
    }

    private void ShopMapButton_Click(object sender, RoutedEventArgs e) => ShowShopMap();
    private void NewsView_RequestBack(object? sender, EventArgs e) => ShowShopMap();

    private void ShowShopMap()
    {
        NewsContent.Visibility = Visibility.Collapsed;
        NewsContent.Content = null;
        MainArea.Visibility = Visibility.Visible;
    }

    private void InitializeMap()
    {
        try
        {
            _mapController.Initialize();
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

    private void RebuildShopLayer() =>
        _mapController.RebuildShopLayer(_shopListViewModel.Shops, _selectedShopId);

    private void MapControl_Loaded(object sender, RoutedEventArgs e)
    {
        if (_mapController.InitialMapPositionSet) return;
        SetInitialMapPosition();
    }

    private void SetInitialMapPosition()
    {
        try
        {
            _mapController.SetInitialMapPosition();
        }
        catch (Exception ex)
        {
            ShowMapError("地図の表示位置を設定できませんでした。", ex);
        }
    }

    private void ShopListViewModel_ShopsChanged(object? sender, EventArgs e) => RebuildShopLayer();

    private void ShopListViewModel_SelectedShopChanged(object? sender, Shop? shop)
    {
        _selectedShopId = shop?.Id;
        RebuildShopLayer();

        if (shop is null || !MapCoordinateValidator.IsValid(shop.Latitude, shop.Longitude))
            return;

        ShowInfoCard(shop);
        _mapController.CenterOnShop(shop);
    }

    private void MapControl_MouseLeftButtonUp(object? sender, MouseButtonEventArgs e)
    {
        try
        {
            var position = e.GetPosition(MapControl);
            var shopId = _mapController.GetShopIdAt(position);
            if (shopId is not null)
                _shopListViewModel.SelectShopById(shopId.Value);
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
        InfoCardAddress.Text = string.IsNullOrWhiteSpace(shop.Address) ? "住所未登録" : shop.Address;
        InfoCardPrice.Text = $"¥{shop.Price:N0}";
        InfoCardRating.Text = $"★ {shop.Rating:F1}  {(shop.IsFavorite ? "★ お気に入り" : string.Empty)}";
        InfoCardBorder.Visibility = Visibility.Visible;
        AnimateInfoCard();
    }

    private void InfoCardClose_Click(object sender, RoutedEventArgs e) => InfoCardBorder.Visibility = Visibility.Collapsed;

    private void AnimateInfoCard()
    {
        var transform = (TranslateTransform)InfoCardBorder.RenderTransform;
        transform.X = InfoCardInitialOffset;
        transform.Y = InfoCardInitialOffset;
        InfoCardBorder.Opacity = 0;
        var storyboard = new Storyboard();
        AddInfoCardAnimation(storyboard, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(InfoCardFadeDurationMilliseconds)), InfoCardBorder, UIElement.OpacityProperty);
        AddInfoCardAnimation(storyboard, new DoubleAnimation(InfoCardInitialOffset, 0, TimeSpan.FromMilliseconds(InfoCardSlideDurationMilliseconds)), transform, TranslateTransform.XProperty);
        AddInfoCardAnimation(storyboard, new DoubleAnimation(InfoCardInitialOffset, 0, TimeSpan.FromMilliseconds(InfoCardSlideDurationMilliseconds)), transform, TranslateTransform.YProperty);
        storyboard.Begin();
    }

    private static void AddInfoCardAnimation(Storyboard storyboard, AnimationTimeline animation, DependencyObject target, DependencyProperty property)
    {
        Storyboard.SetTarget(animation, target);
        Storyboard.SetTargetProperty(animation, new PropertyPath(property));
        storyboard.Children.Add(animation);
    }

    private async void CurrentLocationButton_Click(object sender, RoutedEventArgs e) =>
        await TryShowCurrentLocationAsync(showMessageOnFailure: true);

    private async Task TryShowCurrentLocationAsync(bool showMessageOnFailure)
    {
        try
        {
            var location = await _currentLocationService.GetCurrentLocationAsync();
            if (location is null)
            {
                if (showMessageOnFailure)
                {
                    MessageBox.Show(
                        "現在地を取得できませんでした。位置情報の利用を許可しているか確認してください。",
                        "現在地",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                return;
            }

            _mapController.ShowCurrentLocation(location.Latitude, location.Longitude);
            _mapController.CenterOn(location.Latitude, location.Longitude);
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

    public void RefreshShopData() => _shopListViewModel.RefreshFromService();

    private void ShowAllShopsButton_Click(object sender, RoutedEventArgs e)
    {
        _selectedShopId = null;
        _shopListViewModel.NearbyOnly = false;
        InfoCardBorder.Visibility = Visibility.Collapsed;
        RebuildShopLayer();
        _mapController.InitialMapPositionSet = false;
        SetInitialMapPosition();
    }

    private void ZoomInButton_Click(object sender, RoutedEventArgs e) => _mapController.ZoomIn();
    private void ZoomOutButton_Click(object sender, RoutedEventArgs e) => _mapController.ZoomOut();

    private void MapControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2) e.Handled = true;
    }

    private void ShowMapError(string message, Exception ex)
    {
        MapStatusText.Text = $"{message}\n{ex.Message}";
        MapStatusText.Visibility = Visibility.Visible;
    }

    private static ImageSource CreateRameniaIcon()
    {
        const int size = RameniaIconSize;
        var visual = new DrawingVisual();
        using (var context = visual.RenderOpen())
        {
            var formattedText = new FormattedText(
                "🍜",
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(new FontFamily("Segoe UI Emoji"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
                RameniaIconFontSize,
                Brushes.Black,
                1.0);
            context.DrawText(formattedText, new Point((size - formattedText.Width) / 2, (size - formattedText.Height) / 2));
        }

        var bitmap = new RenderTargetBitmap(size, size, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(visual);
        bitmap.Freeze();
        return bitmap;
    }
}
