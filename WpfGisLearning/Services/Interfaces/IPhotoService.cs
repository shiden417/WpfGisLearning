using WpfGisLearning.Models;

namespace WpfGisLearning.Services.Interfaces;

/// <summary>
/// 店舗写真ファイルの保存・削除・パス生成を担当するサービスの契約です。
/// ファイルシステム操作をShopServiceやViewModelから分離します。
/// </summary>
public interface IPhotoService
{
    /// <summary>店舗と写真メタデータから実際の保存先パスを生成します。</summary>
    string GetPhotoPath(Shop shop, ShopPhoto photo);

    /// <summary>選択された写真を店舗の写真フォルダーへコピーします。</summary>
    void SavePhotos(Shop shop, IEnumerable<(string SourcePath, ShopPhoto Photo)> photos);

    /// <summary>指定した店舗写真ファイルを削除します。</summary>
    void DeletePhoto(Shop shop, ShopPhoto photo);

    /// <summary>指定した店舗に属する写真フォルダーをまとめて削除します。</summary>
    void DeleteShopPhotos(Shop shop);
}
