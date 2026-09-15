using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using WpfGisLearning.Services;
using WpfGisLearning.ViewModels;
using WpfGisLearning.Views;

namespace WpfGisLearning;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        var services = new ServiceCollection();
        services.AddSingleton<MainViewModel>();
        services.AddTransient<LifetimeTestService>();
        services.AddSingleton<MainWindow>();
        services.AddSingleton<MessageViewModel>();
        services.AddSingleton<MessageView>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddTransient<DetailView>();
        services.AddTransient<ShopListView>();
        services.AddTransient<ShopListViewModel>();
        services.AddTransient<ShopEditView>();
        services.AddTransient<ShopEditViewModel>();
        services.AddTransient<Views.ShopPage>();
        services.AddTransient<Views.DetailPage>();
        services.AddTransient<NewsView>();
        services.AddTransient<NewsViewModel>();
        services.AddSingleton<NewsService>();
        services.AddSingleton<IShopService, ShopService>();
        services.AddSingleton<IPhotoService, PhotoService>();
        _serviceProvider = services.BuildServiceProvider();
        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }
}
