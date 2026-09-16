namespace WpfGisLearning.Services.Interfaces;

/// <summary>
/// 画面遷移を依頼するためのアプリケーション側の契約です。
/// ViewModelからWPFのWindowやFrameを直接操作しないために使用します。
/// </summary>
public interface INavigationService
{
    /// <summary>店舗詳細画面へ遷移します。</summary>
    /// <param name="id">表示する店舗IDです。</param>
    void NavigateToDetail(int id);

    /// <summary>店舗登録・編集画面へ遷移します。</summary>
    /// <param name="id">編集対象の店舗IDです。nullなら新規登録です。</param>
    void NavigateToShopEdit(int? id = null);

    /// <summary>店舗一覧画面へ遷移します。</summary>
    void NavigateToShopList();

    /// <summary>Frameを利用した店舗ページの学習用画面へ遷移します。</summary>
    void NavigateToShopPageFrame();
}
