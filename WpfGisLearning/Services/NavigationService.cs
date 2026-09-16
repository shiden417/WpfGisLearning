using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;
using WpfGisLearning.Services.Interfaces;
using WpfGisLearning.ViewModels;
using WpfGisLearning.Views;

namespace WpfGisLearning.Services;

public class NavigationService : INavigationService
{
    private readonly IServiceProvider _provider;
    private readonly INavigationHost _host;

    public NavigationService(IServiceProvider provider)
        : this(provider, new WpfNavigationHost())
    {
    }

    public NavigationService(IServiceProvider provider, INavigationHost host)
    {
        _provider = provider;
        _host = host;
    }

    public void NavigateToDetail(int id)
    {
        var viewModel = ActivatorUtilities.CreateInstance<DetailViewModel>(_provider, id);
        var view = ActivatorUtilities.CreateInstance<DetailView>(_provider, viewModel);

        _host.ShowDialog(
            view,
            "店舗詳細 - Ramenia",
            1000,
            900,
            820,
            700,
            _host.RefreshShopData);
    }

    public void NavigateToShopEdit(int? id = null)
    {
        var viewModel = _provider.GetRequiredService<ShopEditViewModel>();
        viewModel.Load(id);
        var view = ActivatorUtilities.CreateInstance<ShopEditView>(_provider, viewModel);

        _host.ShowDialog(
            view,
            id.HasValue ? "店舗を編集 - Ramenia" : "店舗を登録 - Ramenia",
            1250,
            850,
            1000,
            700);
    }

    public void NavigateToShopPageFrame()
    {
        var frame = new Frame
        {
            NavigationUIVisibility = System.Windows.Navigation.NavigationUIVisibility.Visible
        };
        var shopPage = ActivatorUtilities.CreateInstance<Views.ShopPage>(_provider);
        frame.Navigate(shopPage);
        _host.NavigateMainContent(frame);
    }

    public void NavigateToShopList()
    {
        var view = ActivatorUtilities.CreateInstance<Views.ShopListView>(_provider);
        _host.NavigateMainContent(view);
    }
}
