using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;
using WpfGisLearning.Services.Interfaces;
using WpfGisLearning.ViewModels;
using WpfGisLearning.Views;

namespace WpfGisLearning.Services;

/// <summary>
/// DIコンテナを利用して、WPFのViewとそれに必要なViewModelを生成するファクトリです。
/// NavigationServiceから具体的な生成処理を分離します。
/// </summary>
/// <param name="provider">ViewやViewModelを解決するDIコンテナです。</param>
public sealed class WpfNavigationViewFactory(IServiceProvider provider) : INavigationViewFactory
{
    /// <summary>店舗詳細Viewと、指定IDを初期化したViewModelを生成します。</summary>
    public object CreateDetailView(int id)
    {
        // ViewModelだけは店舗IDという実行時引数が必要なため、ActivatorUtilitiesで生成する。
        var viewModel = ActivatorUtilities.CreateInstance<DetailViewModel>(provider, id);
        return ActivatorUtilities.CreateInstance<DetailView>(provider, viewModel);
    }

    /// <summary>店舗登録・編集Viewを生成し、ViewModelへ対象IDを読み込ませます。</summary>
    public object CreateShopEditView(int? id)
    {
        var viewModel = provider.GetRequiredService<ShopEditViewModel>();
        viewModel.Load(id);
        return ActivatorUtilities.CreateInstance<ShopEditView>(provider, viewModel);
    }

    /// <summary>WPF Frameと、その中に表示する店舗Pageを生成します。</summary>
    public object CreateShopPageFrame()
    {
        var frame = new Frame
        {
            // Page間の戻る・進むUIをFrame自身に表示させる学習用設定。
            NavigationUIVisibility = System.Windows.Navigation.NavigationUIVisibility.Visible
        };
        var shopPage = ActivatorUtilities.CreateInstance<Views.ShopPage>(provider);
        frame.Navigate(shopPage);
        return frame;
    }

    /// <summary>DIコンテナから店舗一覧Viewを生成します。</summary>
    public object CreateShopListView() =>
        ActivatorUtilities.CreateInstance<Views.ShopListView>(provider);
}
