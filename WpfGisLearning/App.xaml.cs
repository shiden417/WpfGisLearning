using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using WpfGisLearning.Services;
using WpfGisLearning.Services.Interfaces;
using WpfGisLearning.Views;

namespace WpfGisLearning;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();

        services.AddSingleton<MainWindow>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IShopService, ShopService>();
        services.AddSingleton<IPhotoService, PhotoService>();
        services.AddSingleton<ICurrentLocationService, CurrentLocationService>();
        services.AddSingleton<IReverseGeocodingService, NominatimReverseGeocodingService>();
        services.AddSingleton<NewsService>();

        services.AddTransient<DetailView>();
        services.AddTransient<ShopListView>();
        services.AddTransient<ShopListViewModel>();
        services.AddTransient<ShopEditView>();
        services.AddTransient<ShopEditViewModel>();
        services.AddTransient<Views.ShopPage>();
        services.AddTransient<Views.DetailPage>();
        services.AddTransient<NewsView>();
        services.AddTransient<NewsViewModel>();

        _serviceProvider = services.BuildServiceProvider();
        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }
}
