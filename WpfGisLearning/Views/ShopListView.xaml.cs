using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfGisLearning.Models;
using WpfGisLearning.ViewModels;

namespace WpfGisLearning.Views
{
    public partial class ShopListView : UserControl
    {
        public ShopListView(ShopListViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void ShopListItem_PreviewMouseDoubleClick(
            object sender,
            MouseButtonEventArgs e)
        {
            if (sender is not ListBoxItem item)
            {
                return;
            }

            if (item.DataContext is not Shop shop)
            {
                return;
            }

            var viewModel = DataContext as ShopListViewModel;

            if (viewModel is null)
            {
                return;
            }

            viewModel.SelectedShop = shop;

            if (viewModel.OpenSelectedShopCommand.CanExecute(null))
            {
                viewModel.OpenSelectedShopCommand.Execute(null);
            }

            e.Handled = true;
        }
    }
}