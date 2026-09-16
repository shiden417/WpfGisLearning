using System.IO;
using WpfGisLearning.Models;
using WpfGisLearning.Services.Interfaces;

namespace WpfGisLearning.Services;

/// <summary>
/// 店舗写真ファイルをアプリのローカルフォルダーで管理するサービスです。
/// 写真メタデータとは別に、実際の画像ファイルのコピー・削除を担当します。
/// </summary>
public class PhotoService : IPhotoService
{
    /// <summary>店舗写真を保存するルートフォルダーです。</summary>
    private readonly string _rootPath;

    /// <summary>
    /// ユーザーのローカルアプリケーションデータ配下を既定の写真保存先にします。
    /// </summary>
    public PhotoService()
        : this(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Ramenia", "Images"))
    {
    }

    /// <summary>任意のルートフォルダーを指定して写真サービスを生成します。</summary>
    /// <param name="rootPath">写真を保存するルートフォルダーです。</param>
    public PhotoService(string rootPath)
    {
        _rootPath = rootPath;
    }

    /// <summary>
    /// 店舗IDと写真ファイル名から、画像の保存先パスを組み立てます。
    /// </summary>
    public string GetPhotoPath(Shop shop, ShopPhoto photo) => Path.Combine(_rootPath, shop.Id.ToString(), photo.FileName);

    /// <summary>
    /// 選択された元画像を店舗専用フォルダーへコピーし、写真ファイル名を設定します。
    /// </summary>
    public void SavePhotos(Shop shop, IEnumerable<(string SourcePath, ShopPhoto Photo)> photos)
    {
        var shopDirectory = Path.Combine(_rootPath, shop.Id.ToString());
        Directory.CreateDirectory(shopDirectory);

        foreach (var (sourcePath, photo) in photos)
        {
            // 元画像の拡張子を維持し、拡張子がない場合はjpgを既定値にする。
            var extension = Path.GetExtension(sourcePath);
            if (string.IsNullOrWhiteSpace(extension)) extension = ".jpg";
            photo.FileName = $"{photo.Id}{extension.ToLowerInvariant()}";
            File.Copy(sourcePath, GetPhotoPath(shop, photo), true);
        }
    }

    /// <summary>指定した店舗写真が存在する場合、そのファイルを削除します。</summary>
    public void DeletePhoto(Shop shop, ShopPhoto photo)
    {
        var path = GetPhotoPath(shop, photo);
        if (File.Exists(path)) File.Delete(path);
    }

    /// <summary>指定した店舗の写真フォルダーを存在する場合にまとめて削除します。</summary>
    public void DeleteShopPhotos(Shop shop)
    {
        var directory = Path.Combine(_rootPath, shop.Id.ToString());
        if (Directory.Exists(directory)) Directory.Delete(directory, true);
    }
}
