using System.IO;
using System.Text.Json;
using WpfGisLearning.Models;
using WpfGisLearning.Services.Interfaces;

namespace WpfGisLearning.Services;

public sealed class JsonShopDataStore : IShopDataStore
{
    private readonly string _dataPath;

    public JsonShopDataStore()
        : this(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Ramenia",
            "shops.json"))
    {
    }

    public JsonShopDataStore(string dataPath)
    {
        _dataPath = dataPath;
    }

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
            return [];
        }
    }

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
