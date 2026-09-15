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

        // ViewModels / Views / Services
        services.AddSingleton<MainViewModel>();
        services.AddTransient<LifetimeTestService>();
        services.AddSingleton<MainWindow>();
        services.AddSingleton<MessageViewModel>();
        services.AddSingleton<MessageView>();

        // Navigation
        services.AddSingleton<Services.INavigationService, Services.NavigationService>();

        // Detail view and viewmodel are created at navigation time. Register the view so
        // ActivatorUtilities can resolve any DI dependencies it needs.
        services.AddTransient<Views.DetailView>();
        services.AddTransient<Views.ShopListView>();
        services.AddTransient<ViewModels.ShopListViewModel>();
        // Frame+Page sample
        services.AddTransient<Views.ShopPage>();
        services.AddTransient<Views.DetailPage>();

        // Shop service
        services.AddSingleton<Services.IShopService, Services.ShopService>();

        _serviceProvider = services.BuildServiceProvider();

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();

        mainWindow.Show();
    }
}