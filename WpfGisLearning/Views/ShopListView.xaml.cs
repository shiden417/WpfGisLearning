using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfGisLearning.Models;
using WpfGisLearning.ViewModels;

namespace WpfGisLearning.Views;

public partial class ShopListView : UserControl
{
    public ShopListView(ShopListViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void ShopListItem_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is not ShopListViewModel viewModel)
            return;

        var item = ItemsControl.ContainerFromElement(
            (ItemsControl)sender,
            e.OriginalSource as DependencyObject) as ListBoxItem;

        if (item?.DataContext is not Shop shop)
            return;

        viewModel.SelectedShop = shop;
        viewModel.OpenSelectedShopCommand.Execute(null);
        e.Handled = true;
    }
}
