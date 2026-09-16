using WpfGisLearning.Models;

namespace WpfGisLearning.Services.Interfaces;

/// <summary>
/// 店舗データの永続化方式を抽象化する契約です。
/// 現在はJSONを使用していますが、別の保存方式へ差し替えられます。
/// </summary>
public interface IShopDataStore
{
    /// <summary>永続化された店舗一覧を読み込みます。</summary>
    List<Shop> Load();

    /// <summary>指定した店舗一覧を永続化します。</summary>
    void Save(IEnumerable<Shop> shops);
}
