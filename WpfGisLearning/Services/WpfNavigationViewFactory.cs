using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;
using WpfGisLearning.Services.Interfaces;
using WpfGisLearning.ViewModels;
using WpfGisLearning.Views;

namespace WpfGisLearning.Services;

public sealed class WpfNavigationViewFactory(IServiceProvider provider) : INavigationViewFactory
{
    public object CreateDetailView(int id)
    {
        var viewModel = ActivatorUtilities.CreateInstance<DetailViewModel>(provider, id);
        return ActivatorUtilities.CreateInstance<DetailView>(provider, viewModel);
    }

    public object CreateShopEditView(int? id)
    {
        var viewModel = provider.GetRequiredService<ShopEditViewModel>();
        viewModel.Load(id);
        return ActivatorUtilities.CreateInstance<ShopEditView>(provider, viewModel);
    }

    public object CreateShopPageFrame()
    {
        var frame = new Frame
        {
            NavigationUIVisibility = System.Windows.Navigation.NavigationUIVisibility.Visible
        };
        var shopPage = ActivatorUtilities.CreateInstance<Views.ShopPage>(provider);
        frame.Navigate(shopPage);
        return frame;
    }

    public object CreateShopListView() =>
        ActivatorUtilities.CreateInstance<Views.ShopListView>(provider);
}
