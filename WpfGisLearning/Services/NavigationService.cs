using WpfGisLearning.Services.Interfaces;

namespace WpfGisLearning.Services;

public class NavigationService : INavigationService
{
    private readonly INavigationHost _host;
    private readonly INavigationViewFactory _viewFactory;

    public NavigationService(IServiceProvider provider)
        : this(provider, new WpfNavigationHost(), new WpfNavigationViewFactory(provider))
    {
    }

    public NavigationService(
        IServiceProvider provider,
        INavigationHost host,
        INavigationViewFactory viewFactory)
    {
        _host = host;
        _viewFactory = viewFactory;
    }

    public void NavigateToDetail(int id)
    {
        var view = _viewFactory.CreateDetailView(id);

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
        var view = _viewFactory.CreateShopEditView(id);

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
        var content = _viewFactory.CreateShopPageFrame();
        _host.NavigateMainContent(content);
    }

    public void NavigateToShopList()
    {
        var content = _viewFactory.CreateShopListView();
        _host.NavigateMainContent(content);
    }
}
