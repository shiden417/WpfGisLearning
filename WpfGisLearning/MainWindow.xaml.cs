using System.Windows;
using System.Windows.Input;
using WpfGisLearning.Models;
using WpfGisLearning.Services;
using WpfGisLearning.Services.Interfaces;
using WpfGisLearning.Utilities;
using WpfGisLearning.ViewModels;
using WpfGisLearning.Views;

namespace WpfGisLearning;

public partial class MainWindow : Window
{
    private readonly MapController _mapController;
    private readonly ShopListViewModel _shopListViewModel;
    private readonly ICurrentLocationService _currentLocationService;
    private readonly NewsView _newsView;
    private readonly ShopInfoCardPresenter _infoCardPresenter;
    private readonly ExcelShopDataController _excelShopDataController;
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
        _currentLocationService = currentLocationService;
        _newsView = newsView;
        _mapController = new MapController(MapControl);
        _infoCardPresenter = new ShopInfoCardPresenter(
            this,
            InfoCardBorder,
            InfoCardName,
            InfoCardType,
            InfoCardAddress,
            InfoCardPrice,
            InfoCardRating,
            InfoCardFavorite,
            InfoCardBusinessHoursBadge,
            InfoCardBusinessHoursStatus);
        _excelShopDataController = new ExcelShopDataController(
            excelShopDataService,
            shopService,
            ResetAfterExcelImport);

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
            _infoCardPresenter.Hide();
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
            _infoCardPresenter.Hide();
            RebuildShopLayer();
            return;
        }

        RebuildShopLayer();

        if (!MapCoordinateValidator.IsValid(shop.Latitude, shop.Longitude))
        {
            _infoCardPresenter.Hide();
            return;
        }

        if (!_shopListViewModel.ShopsView.Cast<Shop>().Any(filteredShop => filteredShop.Id == shop.Id))
        {
            _selectedShopId = null;
            _infoCardPresenter.Hide();
            return;
        }

        _infoCardPresenter.Show(shop);
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

    private void InfoCardClose_Click(object sender, RoutedEventArgs e) => _infoCardPresenter.Hide();

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
        _infoCardPresenter.Hide();
        RebuildShopLayer();
        _mapController.InitialMapPositionSet = false;
        SetInitialMapPosition();
    }

    private void DownloadExcelTemplateButton_Click(object sender, RoutedEventArgs e) =>
        _excelShopDataController.DownloadTemplate();

    private void ExportExcelButton_Click(object sender, RoutedEventArgs e) =>
        _excelShopDataController.Export();

    private void ImportExcelButton_Click(object sender, RoutedEventArgs e) =>
        _excelShopDataController.Import();

    private void ResetAfterExcelImport()
    {
        _selectedShopId = null;
        _shopListViewModel.ClearSearchCommand.Execute(null);
        _shopListViewModel.RefreshFromService();
        _infoCardPresenter.Hide();
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
