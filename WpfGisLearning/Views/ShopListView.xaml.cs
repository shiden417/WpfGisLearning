using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfGisLearning.Models;
using WpfGisLearning.ViewModels;

namespace WpfGisLearning.Views;

/// <summary>
/// 店舗一覧画面のViewです。
/// WPF固有のマウス操作、キーボード操作、ContextMenu、スクロールなどを担当します。
/// 店舗データそのものの操作はViewModelへ委譲します。
/// </summary>
public partial class ShopListView : UserControl
{
    // この画面が利用する店舗一覧ViewModelです。
    private readonly ShopListViewModel _viewModel;

    /// <summary>Excelテンプレートのダウンロードを親画面へ通知するイベントです。</summary>
    public event RoutedEventHandler? DownloadExcelTemplateRequested;

    /// <summary>Excel出力を親画面へ通知するイベントです。</summary>
    public event RoutedEventHandler? ExportExcelRequested;

    /// <summary>Excel一括登録を親画面へ通知するイベントです。</summary>
    public event RoutedEventHandler? ImportExcelRequested;

    /// <summary>画面を生成し、ViewModelをDataContextへ設定します。</summary>
    public ShopListView(ShopListViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        _viewModel.SelectedShopChanged += ViewModel_SelectedShopChanged;
    }

    /// <summary>
    /// ViewModelで店舗選択が変わったとき、選択された店舗が見える位置までListBoxをスクロールします。
    /// Dispatcherを使うのは、WPFのレイアウト更新後にスクロール処理を実行する必要があるためです。
    /// </summary>
    private void ViewModel_SelectedShopChanged(object? sender, Shop? shop)
    {
        if (shop is null)
        {
            return;
        }

        Dispatcher.BeginInvoke(() => ShopListBox.ScrollIntoView(shop));
    }

    /// <summary>Excel操作用ContextMenuをボタンの位置に表示します。</summary>
    private void ExcelMenuButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.ContextMenu is ContextMenu contextMenu)
        {
            contextMenu.PlacementTarget = button;
            contextMenu.IsOpen = true;
        }
    }

    /// <summary>Excelテンプレート作成要求を親画面へ転送します。</summary>
    private void DownloadExcelTemplateButton_Click(object sender, RoutedEventArgs e) =>
        DownloadExcelTemplateRequested?.Invoke(sender, e);

    /// <summary>Excel出力要求を親画面へ転送します。</summary>
    private void ExportExcelButton_Click(object sender, RoutedEventArgs e) =>
        ExportExcelRequested?.Invoke(sender, e);

    /// <summary>Excel一括登録要求を親画面へ転送します。</summary>
    private void ImportExcelButton_Click(object sender, RoutedEventArgs e) =>
        ImportExcelRequested?.Invoke(sender, e);

    /// <summary>店舗をダブルクリックしたとき、その店舗の詳細画面を開きます。</summary>
    private void ShopListItem_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is not ShopListViewModel viewModel)
        {
            return;
        }

        var item = ItemsControl.ContainerFromElement(
            (ItemsControl)sender,
            e.OriginalSource as DependencyObject) as ListBoxItem;

        if (item?.DataContext is not Shop shop)
        {
            return;
        }

        viewModel.SelectedShop = shop;
        viewModel.OpenSelectedShopCommand.Execute(null);
        e.Handled = true;
    }

    /// <summary>ContextMenuを開いた元の店舗データを取得します。</summary>
    private static Shop? GetShopFromContextMenu(object sender)
    {
        if (sender is not MenuItem menuItem ||
            menuItem.Parent is not ContextMenu contextMenu ||
            contextMenu.PlacementTarget is not FrameworkElement target)
        {
            return null;
        }

        return target.DataContext as Shop;
    }

    /// <summary>ContextMenuから店舗詳細表示を実行します。</summary>
    private void ShopContextMenu_Detail_Click(object sender, RoutedEventArgs e)
    {
        var shop = GetShopFromContextMenu(sender);
        if (shop is null) return;

        _viewModel.SelectedShop = shop;
        _viewModel.OpenSelectedShopCommand.Execute(null);
    }

    /// <summary>ContextMenuから店舗編集を実行します。</summary>
    private void ShopContextMenu_Edit_Click(object sender, RoutedEventArgs e)
    {
        var shop = GetShopFromContextMenu(sender);
        if (shop is null) return;

        _viewModel.SelectedShop = shop;
        _viewModel.EditSelectedShopCommand.Execute(null);
    }

    /// <summary>ContextMenuからお気に入り切り替えを実行します。</summary>
    private void ShopContextMenu_Favorite_Click(object sender, RoutedEventArgs e)
    {
        var shop = GetShopFromContextMenu(sender);
        if (shop is null) return;

        _viewModel.SelectedShop = shop;
        _viewModel.ToggleFavoriteCommand.Execute(shop);
    }

    /// <summary>ContextMenuから店舗削除を実行します。</summary>
    private void ShopContextMenu_Delete_Click(object sender, RoutedEventArgs e)
    {
        var shop = GetShopFromContextMenu(sender);
        if (shop is null) return;

        _viewModel.SelectedShop = shop;
        _viewModel.DeleteSelectedShopCommand.Execute(null);
    }

    /// <summary>店舗一覧でCtrl+F、Escape、EnterなどのWPFキーボード操作を処理します。</summary>
    private void ShopListView_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.F)
        {
            SearchTextBox.Focus();
            SearchTextBox.SelectAll();
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Escape)
        {
            _viewModel.ClearSearchCommand.Execute(null);
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Enter && SearchTextBox.IsKeyboardFocusWithin)
        {
            _viewModel.SearchCommand.Execute(null);
            e.Handled = true;
        }
    }
}
