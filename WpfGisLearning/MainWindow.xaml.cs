using Microsoft.Win32;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using WpfGisLearning.Models;
using WpfGisLearning.Services;
using WpfGisLearning.Services.Interfaces;
using WpfGisLearning.Utilities;
using WpfGisLearning.ViewModels;
using WpfGisLearning.Views;

namespace WpfGisLearning;

public partial class MainWindow : Window
{
    private const int InfoCardInitialOffset = 18;
    private const int InfoCardFadeDurationMilliseconds = 180;
    private const int InfoCardSlideDurationMilliseconds = 220;

    private readonly MapController _mapController;
    private readonly ShopListViewModel _shopListViewModel;
    private readonly IShopService _shopService;
    private readonly IExcelShopDataService _excelShopDataService;
    private readonly ICurrentLocationService _currentLocationService;
    private readonly NewsView _newsView;
    private int? _selectedShopId;

    public MainWindow(
        ShopListView shopListView,
        IShopService shopService,
        IExcelShopDataService excelShopDataService,
        ICurrentLocationService currentLocationService,
        NewsView newsView)
    {
        InitializeComponent();
        Icon = RameniaIconFactory.Create();
        MainContent.Content = shopListView;
        _shopListViewModel = (ShopListViewModel)shopListView.DataContext;
        _shopService = shopService;
        _excelShopDataService = excelShopDataService;
        _currentLocationService = currentLocationService;
        _newsView = newsView;
        _mapController = new MapController(MapControl);

        _shopListViewModel.SelectedShopChanged += ShopListViewModel_SelectedShopChanged;
        _shopListViewModel.ShopsChanged += ShopListViewModel_ShopsChanged;
        shopListView.DownloadExcelTemplateRequested += DownloadExcelTemplateButton_Click;
        shopListView.ExportExcelRequested += ExportExcelButton_Click;
        shopListView.ImportExcelRequested += ImportExcelButton_Click;
        _newsView.RequestBack += NewsView_RequestBack;

        InitializeMap();
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

    private void RebuildShopLayer()
    {
        var filteredShops = _shopListViewModel.ShopsView.Cast<Shop>().ToList();
        _mapController.RebuildShopLayer(filteredShops, _selectedShopId);

        if (_selectedShopId.HasValue && !filteredShops.Any(shop => shop.Id == _selectedShopId.Value))
        {
            _selectedShopId = null;
            _shopListViewModel.SelectedShop = null;
            InfoCardBorder.Visibility = Visibility.Collapsed;
        }
    }

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

        if (shop is null)
        {
            InfoCardBorder.Visibility = Visibility.Collapsed;
            RebuildShopLayer();
            return;
        }

        RebuildShopLayer();

        if (!MapCoordinateValidator.IsValid(shop.Latitude, shop.Longitude))
        {
            InfoCardBorder.Visibility = Visibility.Collapsed;
            return;
        }

        if (!_shopListViewModel.ShopsView.Cast<Shop>().Any(filteredShop => filteredShop.Id == shop.Id))
        {
            _selectedShopId = null;
            InfoCardBorder.Visibility = Visibility.Collapsed;
            return;
        }

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
        InfoCardRating.Text = $"★ {shop.Rating:F1}";
        InfoCardFavorite.Text = shop.IsFavorite ? "♥ お気に入り" : string.Empty;

        if (!BusinessHoursStatusCalculator.HasOpeningHours(shop.OpeningHours))
        {
            InfoCardBusinessHoursStatus.Text = "営業時間未登録";
            InfoCardBusinessHoursStatus.Foreground = (Brush)FindResource("MutedBrush");
            InfoCardBusinessHoursBadge.Background = (Brush)FindResource("PanelBrush");
            InfoCardBusinessHoursBadge.BorderBrush = (Brush)FindResource("BorderBrush");
        }
        else if (BusinessHoursStatusCalculator.IsOpen(shop.OpeningHours, shop.ClosedDay, DateTime.Now))
        {
            InfoCardBusinessHoursStatus.Text = "● 営業中";
            InfoCardBusinessHoursStatus.Foreground = (Brush)FindResource("AccentDarkBrush");
            InfoCardBusinessHoursBadge.Background = (Brush)FindResource("AccentSoftBrush");
            InfoCardBusinessHoursBadge.BorderBrush = (Brush)FindResource("AccentBrush");
        }
        else
        {
            InfoCardBusinessHoursStatus.Text = "● 営業時間外";
            InfoCardBusinessHoursStatus.Foreground = (Brush)FindResource("MutedBrush");
            InfoCardBusinessHoursBadge.Background = (Brush)FindResource("PanelBrush");
            InfoCardBusinessHoursBadge.BorderBrush = (Brush)FindResource("BorderBrush");
        }

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

            _shopListViewModel.SetNearbyLocation(location.Latitude, location.Longitude);
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
        _shopListViewModel.ClearSearchCommand.Execute(null);
        InfoCardBorder.Visibility = Visibility.Collapsed;
        RebuildShopLayer();
        _mapController.InitialMapPositionSet = false;
        SetInitialMapPosition();
    }

    private void DownloadExcelTemplateButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog
        {
            Title = "Excel入力テンプレートを保存",
            Filter = "Excelファイル|*.xlsx",
            FileName = "ramenia-shop-import-template.xlsx",
            AddExtension = true,
            OverwritePrompt = true
        };

        if (dialog.ShowDialog() != true) return;

        try
        {
            _excelShopDataService.CreateImportTemplate(dialog.FileName);
            MessageBox.Show(
                $"Excel入力テンプレートを保存しました。\n\n保存先：\n{dialog.FileName}",
                "Excelテンプレート",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Excelテンプレートの作成に失敗しました。\n\n{ex.Message}",
                "Excelテンプレート",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void ExportExcelButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog
        {
            Title = "店舗データをExcelで保存",
            Filter = "Excelファイル|*.xlsx",
            FileName = $"ramenia-shops-{DateTime.Now:yyyyMMdd-HHmmss}.xlsx",
            AddExtension = true,
            OverwritePrompt = true
        };

        if (dialog.ShowDialog() != true) return;

        try
        {
            _excelShopDataService.Export(dialog.FileName, _shopService.GetShops());
            MessageBox.Show(
                $"店舗データをExcelで保存しました。\n\n保存先：\n{dialog.FileName}",
                "Excelエクスポート",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Excelの書き出しに失敗しました。\n\n{ex.Message}",
                "Excelエクスポート",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void ImportExcelButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "店舗データExcelを選択",
            Filter = "Excelファイル|*.xlsx|すべてのファイル|*.*",
            Multiselect = false
        };

        if (dialog.ShowDialog() != true) return;

        try
        {
            var shops = _excelShopDataService.Import(dialog.FileName);

            var result = MessageBox.Show(
                $"Excelから{shops.Count}件の店舗データを読み込みます。\nIDが一致する店舗は更新し、IDが空欄だった店舗は新規登録します。\n\n実行しますか？",
                "Excelインポートの確認",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            _shopService.ReplaceAll(shops);
            _selectedShopId = null;
            _shopListViewModel.ClearSearchCommand.Execute(null);
            _shopListViewModel.RefreshFromService();
            InfoCardBorder.Visibility = Visibility.Collapsed;

            MessageBox.Show(
                $"Excelから{shops.Count}件の店舗データを読み込みました。",
                "Excelインポート",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Excelの読み込みに失敗しました。\n\n{ex.Message}",
                "Excelインポート",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void ZoomInButton_Click(object sender, RoutedEventArgs e) => _mapController.ZoomIn();
    private void ZoomOutButton_Click(object sender, RoutedEventArgs e) => _mapController.ZoomOut();

    private void MapControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount != 2) return;
        e.Handled = true;
    }

    private void ShowMapError(string message, Exception exception)
    {
        MapStatusText.Text = $"{message}\n{exception.Message}";
        MapStatusText.Visibility = Visibility.Visible;
    }
}
