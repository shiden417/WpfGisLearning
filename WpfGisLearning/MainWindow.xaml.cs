using System.Windows;
using WpfGisLearning.Services;
using WpfGisLearning.Services.Interfaces;
using WpfGisLearning.Utilities;
using WpfGisLearning.ViewModels;
using WpfGisLearning.Views;

namespace WpfGisLearning;

/// <summary>
/// アプリケーションのメインWindowです。
/// 店舗一覧・地図・ニュースなど主要画面を配置し、Viewやサービス同士の接続を管理します。
/// </summary>
public partial class MainWindow : Window
{
    // メイン地図の生成・店舗レイヤー更新・現在地表示を担当します。
    private readonly MapController _mapController;
    // 店舗一覧の検索、選択、フィルターを管理します。
    private readonly ShopListViewModel _shopListViewModel;
    // Windowsの現在地取得を担当します。
    private readonly ICurrentLocationService _currentLocationService;
    // ニュース画面のViewです。
    private readonly NewsView _newsView;
    // 地図上に表示する店舗情報カードを担当します。
    private readonly ShopInfoCardPresenter _infoCardPresenter;
    // Excel入出力に関するWPF操作を担当します。
    private readonly ExcelShopDataController _excelShopDataController;
    // 現在地図上で選択されている店舗IDです。
    private int? _selectedShopId;

    /// <summary>メイン画面に必要なView、サービス、ViewModelを受け取って初期化します。</summary>
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
            this, InfoCardBorder, InfoCardName, InfoCardType, InfoCardAddress,
            InfoCardPrice, InfoCardRating, InfoCardFavorite,
            InfoCardBusinessHoursBadge, InfoCardBusinessHoursStatus);
        _excelShopDataController = new ExcelShopDataController(
            excelShopDataService, shopService, ResetAfterExcelImport);

        _shopListViewModel.SelectedShopChanged += ShopListViewModel_SelectedShopChanged;
        _shopListViewModel.ShopsChanged += ShopListViewModel_ShopsChanged;
        shopListView.DownloadExcelTemplateRequested += DownloadExcelTemplateButton_Click;
        shopListView.ExportExcelRequested += ExportExcelButton_Click;
        shopListView.ImportExcelRequested += ImportExcelButton_Click;
        _newsView.RequestBack += NewsView_RequestBack;

        InitializeMap();
    }

    /// <summary>ニュース画面を表示し、店舗エリアを一時的に非表示にします。</summary>
    private void NewsButton_Click(object sender, RoutedEventArgs e)
    {
        MainArea.Visibility = Visibility.Collapsed;
        NewsContent.Content = _newsView;
        NewsContent.Visibility = Visibility.Visible;
    }

    /// <summary>店舗・地図画面へ戻します。</summary>
    private void ShopMapButton_Click(object sender, RoutedEventArgs e) => ShowShopMap();

    /// <summary>ニュース画面から戻る要求を受け取り、店舗・地図画面へ戻します。</summary>
    private void NewsView_RequestBack(object? sender, EventArgs e) => ShowShopMap();

    /// <summary>ニュース表示を解除してメインの店舗・地図エリアを表示します。</summary>
    private void ShowShopMap()
    {
        NewsContent.Visibility = Visibility.Collapsed;
        NewsContent.Content = null;
        MainArea.Visibility = Visibility.Visible;
    }

    /// <summary>店舗データをサービスから再読み込みします。</summary>
    public void RefreshShopData() => _shopListViewModel.RefreshFromService();

    /// <summary>Excelテンプレート作成をコントローラーへ委譲します。</summary>
    private void DownloadExcelTemplateButton_Click(object sender, RoutedEventArgs e) =>
        _excelShopDataController.DownloadTemplate();

    /// <summary>店舗データのExcel出力をコントローラーへ委譲します。</summary>
    private void ExportExcelButton_Click(object sender, RoutedEventArgs e) =>
        _excelShopDataController.Export();

    /// <summary>店舗データのExcel一括登録をコントローラーへ委譲します。</summary>
    private void ImportExcelButton_Click(object sender, RoutedEventArgs e) =>
        _excelShopDataController.Import();

    /// <summary>Excelインポート完了後に選択状態・検索状態・情報カードを初期化します。</summary>
    private void ResetAfterExcelImport()
    {
        _selectedShopId = null;
        _shopListViewModel.ClearSearchCommand.Execute(null);
        _shopListViewModel.RefreshFromService();
        _infoCardPresenter.Hide();
    }
}
