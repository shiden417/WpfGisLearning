using System.IO;
using System.Text.Json;
using WpfGisLearning.Models;
using WpfGisLearning.Services.Interfaces;

namespace WpfGisLearning.Services;

/// <summary>
/// 店舗データをJSONファイルへ保存・読み込みするデータストアです。
/// ファイルパスをコンストラクターで差し替えられるため、テストでも一時ファイルを利用できます。
/// </summary>
public sealed class JsonShopDataStore : IShopDataStore
{
    /// <summary>店舗データを保存するJSONファイルのパスです。</summary>
    private readonly string _dataPath;

    /// <summary>アプリケーションのローカルデータフォルダー配下を既定の保存先にします。</summary>
    public JsonShopDataStore()
        : this(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Ramenia",
            "shops.json"))
    {
    }

    /// <summary>任意の保存先を指定してデータストアを生成します。</summary>
    /// <param name="dataPath">店舗データを保存するJSONファイルのパスです。</param>
    public JsonShopDataStore(string dataPath)
    {
        _dataPath = dataPath;
    }

    /// <summary>
    /// JSONファイルを読み込み、店舗一覧へデシリアライズします。
    /// ファイルが存在しない、または読み込みに失敗した場合は空一覧を返します。
    /// </summary>
    public List<Shop> Load()
    {
        try
        {
            if (!File.Exists(_dataPath))
                return [];

            return JsonSerializer.Deserialize<List<Shop>>(File.ReadAllText(_dataPath)) ?? [];
        }
        catch
        {
            // 学習用アプリでは破損・不正なファイルがあっても起動できるよう空一覧へフォールバックする。
            return [];
        }
    }

    /// <summary>
    /// 店舗一覧を読みやすいインデント付きJSONとして保存します。
    /// 保存先ディレクトリが存在しない場合は先に作成します。
    /// </summary>
    /// <param name="shops">保存対象の店舗一覧です。</param>
    public void Save(IEnumerable<Shop> shops)
    {
        try
        {
            var directory = Path.GetDirectoryName(_dataPath);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            var json = JsonSerializer.Serialize(
                shops,
                new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_dataPath, json);
        }
        catch
        {
            // データ保存に失敗してもUI操作自体は継続できるようにする。
        }
    }
}
