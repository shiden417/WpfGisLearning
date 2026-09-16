using WpfGisLearning.Services.Interfaces;

namespace WpfGisLearning.Services;

/// <summary>
/// アプリケーションの画面遷移を仲介するサービスです。
/// どのViewを生成するかはファクトリ、実際にどう表示するかはホストへ委譲します。
/// </summary>
public class NavigationService : INavigationService
{
    /// <summary>生成したViewを実際のWPF画面へ表示するホストです。</summary>
    private readonly INavigationHost _host;

    /// <summary>遷移先のViewを生成するファクトリです。</summary>
    private readonly INavigationViewFactory _viewFactory;

    /// <summary>
    /// 実アプリ用のWPFナビゲーションホストとViewファクトリを生成します。
    /// </summary>
    /// <param name="provider">ViewやViewModelの生成に使用するDIコンテナです。</param>
    public NavigationService(IServiceProvider provider)
        : this(provider, new WpfNavigationHost(), new WpfNavigationViewFactory(provider))
    {
    }

    /// <summary>
    /// ホストとファクトリを外部から受け取るコンストラクターです。
    /// テストではWPFを起動せず、Fake実装へ差し替えます。
    /// </summary>
    public NavigationService(
        IServiceProvider provider,
        INavigationHost host,
        INavigationViewFactory viewFactory)
    {
        _host = host;
        _viewFactory = viewFactory;
    }

    /// <summary>店舗詳細Viewをモーダルダイアログとして表示します。</summary>
    /// <param name="id">表示対象の店舗IDです。</param>
    public void NavigateToDetail(int id)
    {
        var view = _viewFactory.CreateDetailView(id);

        // 詳細画面を閉じた後、店舗一覧側のデータを再読み込みする。
        _host.ShowDialog(
            view,
            "店舗詳細 - Ramenia",
            1000,
            900,
            820,
            700,
            _host.RefreshShopData);
    }

    /// <summary>店舗登録または編集Viewをモーダルダイアログとして表示します。</summary>
    /// <param name="id">編集対象ID。nullなら新規登録です。</param>
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

    /// <summary>FrameとPageを使った店舗画面へ遷移します。WPF Page学習用です。</summary>
    public void NavigateToShopPageFrame()
    {
        var content = _viewFactory.CreateShopPageFrame();
        _host.NavigateMainContent(content);
    }

    /// <summary>店舗一覧Viewをメインコンテンツ領域へ表示します。</summary>
    public void NavigateToShopList()
    {
        var content = _viewFactory.CreateShopListView();
        _host.NavigateMainContent(content);
    }
}
