namespace WpfGisLearning.Services.Interfaces;

/// <summary>
/// ナビゲーション先のViewやFrameを生成するためのファクトリ契約です。
/// 生成方法をNavigationServiceから分離し、DIやテストで差し替えやすくします。
/// </summary>
public interface INavigationViewFactory
{
    /// <summary>店舗詳細Viewを生成します。</summary>
    /// <param name="id">表示対象の店舗IDです。</param>
    object CreateDetailView(int id);

    /// <summary>店舗登録・編集Viewを生成します。</summary>
    /// <param name="id">編集対象の店舗IDです。nullなら新規登録です。</param>
    object CreateShopEditView(int? id);

    /// <summary>Frameと店舗Pageを生成します。</summary>
    object CreateShopPageFrame();

    /// <summary>店舗一覧Viewを生成します。</summary>
    object CreateShopListView();
}
