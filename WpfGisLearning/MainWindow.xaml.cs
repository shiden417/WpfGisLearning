using System.Windows;
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

    public void RefreshShopData() => _shopListViewModel.RefreshFromService();

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
}
