using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using WpfGisLearning.Services;

namespace WpfGisLearning.Views;

/// <summary>
/// WPFのFrame + Pageによるページ遷移を学習するための店舗ページです。
/// </summary>
public partial class ShopPage : Page
{
    // DIコンテナからDetailPageを生成するためのサービスプロバイダーです。
    private readonly IServiceProvider _provider;

    /// <summary>DIコンテナを受け取り、ページを初期化します。</summary>
    public ShopPage(IServiceProvider provider)
    {
        InitializeComponent();
        _provider = provider;
    }

    /// <summary>固定ID 1の詳細ページを開きます。</summary>
    private void OpenDetail1_Click(object sender, RoutedEventArgs e) => OpenDetail(1);

    /// <summary>固定ID 2の詳細ページを開きます。</summary>
    private void OpenDetail2_Click(object sender, RoutedEventArgs e) => OpenDetail(2);

    /// <summary>
    /// 指定IDをコンストラクター引数としてDetailPageへ渡し、Frame内を遷移します。
    /// ActivatorUtilitiesを使うことでDI管理された依存関係と実行時引数を両立できます。
    /// </summary>
    private void OpenDetail(int id)
    {
        var detailPage = ActivatorUtilities.CreateInstance<DetailPage>(_provider, id);
        NavigationService?.Navigate(detailPage);
    }
}
