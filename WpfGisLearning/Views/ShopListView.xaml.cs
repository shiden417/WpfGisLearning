using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfGisLearning.Models;
using WpfGisLearning.ViewModels;

namespace WpfGisLearning.Views;

public partial class ShopListView : UserControl
{
    private readonly ShopListViewModel _viewModel;

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

    private void ShopContextMenu_Opened(object sender, RoutedEventArgs e)
    {
        if (sender is not ContextMenu contextMenu || contextMenu.PlacementTarget is not FrameworkElement target)
        {
            return;
        }

        if (target.DataContext is Shop shop)
        {
            _viewModel.SelectedShop = shop;
            contextMenu.DataContext = _viewModel;
        }
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
