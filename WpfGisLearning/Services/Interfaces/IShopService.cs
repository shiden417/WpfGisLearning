using WpfGisLearning.Models;

namespace WpfGisLearning.Services.Interfaces;

/// <summary>
/// 店舗データの取得・登録・更新・削除・お気に入り操作を提供するサービスの契約です。
/// </summary>
public interface IShopService
{
    /// <summary>学習用の歓迎メッセージを取得します。</summary>
    string GetWelcomeMessage();

    /// <summary>現在保持している店舗一覧を取得します。</summary>
    IEnumerable<Shop> GetShops();

    /// <summary>新しい店舗を追加して永続化します。</summary>
    void AddShop(Shop shop);

    /// <summary>既存店舗をIDで更新して永続化します。</summary>
    void UpdateShop(Shop shop);

    /// <summary>指定IDの店舗を削除し、関連写真も削除します。</summary>
    void DeleteShop(int id);

    /// <summary>指定IDの店舗のお気に入り状態を反転します。</summary>
    void ToggleFavorite(int id);

    /// <summary>店舗一覧全体を置き換えて永続化します。Excelインポートなどで使用します。</summary>
    void ReplaceAll(IEnumerable<Shop> shops);
}
