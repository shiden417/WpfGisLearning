using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using WpfGisLearning.Services;
using WpfGisLearning.Services.Interfaces;
using WpfGisLearning.ViewModels;
using WpfGisLearning.Views;

namespace WpfGisLearning;

/// <summary>
/// WPFアプリケーションの起動処理とDIコンテナの構築を担当します。
/// アプリ全体で共有するサービスと、画面生成時に新しく作るView/ViewModelをここで登録します。
/// </summary>
public partial class App : Application
{
    /// <summary>アプリケーション全体で利用するDIコンテナです。</summary>
    private ServiceProvider? _serviceProvider;

    /// <summary>
    /// WPF起動時に呼ばれ、依存関係を登録してMainWindowを表示します。
    /// </summary>
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // ServiceCollectionへアプリケーションの依存関係を登録する。
        var services = new ServiceCollection();

        // データやサービスはアプリ全体で共有するSingletonとして登録する。
        services.AddSingleton<MainWindow>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IShopService, ShopService>();
        services.AddSingleton<IShopDataStore, JsonShopDataStore>();
        services.AddSingleton<IPhotoService, PhotoService>();
        services.AddSingleton<IExcelShopDataService, ExcelShopDataService>();
        services.AddSingleton<IGeolocationProvider, WindowsGeolocationProvider>();
        services.AddSingleton<ICurrentLocationService, CurrentLocationService>();
        services.AddSingleton<IReverseGeocodingService, NominatimReverseGeocodingService>();
        services.AddSingleton<INewsService, NewsService>();

        // ViewやViewModelは画面生成ごとに独立したインスタンスを作るTransientとして登録する。
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

    /// <summary>アプリ終了時にDIコンテナを破棄し、所有するリソースを解放します。</summary>
    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        _serviceProvider = null;
        base.OnExit(e);
    }
}
