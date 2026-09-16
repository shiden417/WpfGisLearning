using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfGisLearning.Models;
using WpfGisLearning.ViewModels;

namespace WpfGisLearning.Views;

public partial class ShopListView : UserControl
{
    private readonly ShopListViewModel _viewModel;

    public event EventHandler? DownloadExcelTemplateRequested;
    public event EventHandler? ExportExcelRequested;
    public event EventHandler? ImportExcelRequested;

    public ShopListView(ShopListViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        _viewModel.SelectedShopChanged += ViewModel_SelectedShopChanged;
    }

    private void ViewModel_SelectedShopChanged(object? sender, Shop? shop)
    {
        if (shop is null)
        {
            return;
        }

        Dispatcher.BeginInvoke(() => ShopListBox.ScrollIntoView(shop));
    }

    private void DownloadExcelTemplateButton_Click(object sender, RoutedEventArgs e) =>
        DownloadExcelTemplateRequested?.Invoke(this, EventArgs.Empty);

    private void ExportExcelButton_Click(object sender, RoutedEventArgs e) =>
        ExportExcelRequested?.Invoke(this, EventArgs.Empty);

    private void ImportExcelButton_Click(object sender, RoutedEventArgs e) =>
        ImportExcelRequested?.Invoke(this, EventArgs.Empty);

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

    private void ShopContextMenu_Detail_Click(object sender, RoutedEventArgs e)
    {
        var shop = GetShopFromContextMenu(sender);
        if (shop is null) return;

        _viewModel.SelectedShop = shop;
        _viewModel.OpenSelectedShopCommand.Execute(null);
    }

    private void ShopContextMenu_Edit_Click(object sender, RoutedEventArgs e)
    {
        var shop = GetShopFromContextMenu(sender);
        if (shop is null) return;

        _viewModel.SelectedShop = shop;
        _viewModel.EditSelectedShopCommand.Execute(null);
    }

    private void ShopContextMenu_Favorite_Click(object sender, RoutedEventArgs e)
    {
        var shop = GetShopFromContextMenu(sender);
        if (shop is null) return;

        _viewModel.SelectedShop = shop;
        _viewModel.ToggleFavoriteCommand.Execute(shop);
    }

    private void ShopContextMenu_Delete_Click(object sender, RoutedEventArgs e)
    {
        var shop = GetShopFromContextMenu(sender);
        if (shop is null) return;

        _viewModel.SelectedShop = shop;
        _viewModel.DeleteSelectedShopCommand.Execute(null);
    }

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
