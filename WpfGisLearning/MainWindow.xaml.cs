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
    private readonly ShopListViewModel _shopListViewModel;
    private readonly NewsView _newsView;
    private readonly ExcelShopDataController _excelShopDataController;

    /// <summary>メイン画面に必要なView、サービス、ViewModelを受け取って初期化します。</summary>
    public MainWindow(
        ShopListView shopListView,
        MapView mapView,
        IShopService shopService,
        IExcelShopDataService excelShopDataService,
        NewsView newsView)
    {
        InitializeComponent();
        Icon = RameniaIconFactory.Create();
        MainContent.Content = shopListView;
        MapContent.Content = mapView;
        _shopListViewModel = (ShopListViewModel)shopListView.DataContext;
        _newsView = newsView;
        _excelShopDataController = new ExcelShopDataController(
            excelShopDataService, shopService, ResetAfterExcelImport);

        shopListView.DownloadExcelTemplateRequested += DownloadExcelTemplateButton_Click;
        shopListView.ExportExcelRequested += ExportExcelButton_Click;
        shopListView.ImportExcelRequested += ImportExcelButton_Click;
        _newsView.RequestBack += NewsView_RequestBack;
    }

    /// <summary>ニュース画面を表示し、店舗・地図エリアを一時的に非表示にします。</summary>
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

    /// <summary>Excelインポート完了後に検索状態を初期化し、店舗データを再読み込みします。</summary>
    private void ResetAfterExcelImport()
    {
        _shopListViewModel.ClearSearchCommand.Execute(null);
        _shopListViewModel.RefreshFromService();
    }
}
