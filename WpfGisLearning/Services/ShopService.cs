using WpfGisLearning.Models;
using WpfGisLearning.Services.Interfaces;

namespace WpfGisLearning.Services;

/// <summary>
/// 店舗データの業務ロジックをまとめるサービスです。
/// 永続化はIShopDataStore、写真ファイル管理はIPhotoServiceへ委譲します。
/// </summary>
public sealed class ShopService : IShopService
{
    /// <summary>店舗写真のパス更新や削除を担当するサービスです。</summary>
    private readonly IPhotoService _photoService;

    /// <summary>店舗一覧の永続化を担当するデータストアです。</summary>
    private readonly IShopDataStore _dataStore;

    /// <summary>アプリ実行中にメモリ上で保持する店舗一覧です。</summary>
    private readonly List<Shop> _shops;

    /// <summary>
    /// 店舗サービスを生成し、永続化された店舗一覧を読み込みます。
    /// データが空なら学習用の初期店舗を生成します。
    /// </summary>
    public ShopService(IPhotoService photoService, IShopDataStore dataStore)
    {
        _photoService = photoService;
        _dataStore = dataStore;
        _shops = _dataStore.Load();

        if (_shops.Count == 0)
        {
            _shops.AddRange(CreateInitialShops());
            Save();
        }

        RefreshPhotoPaths();
    }

    /// <summary>アプリの歓迎メッセージを返す学習用メソッドです。</summary>
    public string GetWelcomeMessage() => "Welcome to Ramenia!";

    /// <summary>現在メモリ上で管理している店舗一覧を返します。</summary>
    public IEnumerable<Shop> GetShops() => _shops;

    /// <summary>店舗を追加し、写真表示用パスを更新した後に永続化します。</summary>
    public void AddShop(Shop shop)
    {
        RefreshPhotoPath(shop);
        _shops.Add(shop);
        Save();
    }

    /// <summary>同じIDの既存店舗を置き換え、永続化します。</summary>
    public void UpdateShop(Shop shop)
    {
        RefreshPhotoPath(shop);

        var index = _shops.FindIndex(x => x.Id == shop.Id);
        if (index < 0)
            return;

        _shops[index] = shop;
        Save();
    }

    /// <summary>
    /// 指定IDの店舗を削除します。
    /// 店舗に関連する写真フォルダーも同時に削除します。
    /// </summary>
    public void DeleteShop(int id)
    {
        var shop = _shops.FirstOrDefault(x => x.Id == id);
        if (shop is not null)
            _photoService.DeleteShopPhotos(shop);

        _shops.RemoveAll(x => x.Id == id);
        Save();
    }

    /// <summary>指定IDの店舗のお気に入り状態を反転し、永続化します。</summary>
    public void ToggleFavorite(int id)
    {
        var shop = _shops.FirstOrDefault(x => x.Id == id);
        if (shop is null)
            return;

        shop.IsFavorite = !shop.IsFavorite;
        Save();
    }

    /// <summary>
    /// 店舗一覧を丸ごと置き換えて永続化します。
    /// Excelインポートなど、外部から一覧全体を置き換える処理で使用します。
    /// </summary>
    public void ReplaceAll(IEnumerable<Shop> shops)
    {
        var importedShops = shops.ToList();
        _shops.Clear();
        _shops.AddRange(importedShops);
        RefreshPhotoPaths();
        Save();
    }

    /// <summary>メモリ上の全店舗についてメイン写真の表示パスを更新します。</summary>
    private void RefreshPhotoPaths()
    {
        foreach (var shop in _shops)
            RefreshPhotoPath(shop);
    }

    /// <summary>1店舗のメイン写真を実ファイルパスへ解決してモデルへ設定します。</summary>
    private void RefreshPhotoPath(Shop shop)
    {
        var mainPhoto = shop.MainPhoto;
        shop.MainPhotoPath = mainPhoto is null
            ? string.Empty
            : _photoService.GetPhotoPath(shop, mainPhoto);
    }

    /// <summary>現在の店舗一覧をデータストアへ保存します。</summary>
    private void Save() => _dataStore.Save(_shops);

    /// <summary>
    /// 永続化データが初回起動時に空だった場合に使用するサンプル店舗を生成します。
    /// </summary>
    private static IEnumerable<Shop> CreateInitialShops() =>
    [
        new Shop
        {
            Id = 1,
            Name = "Tokyo Station Store",
            Price = 1200m,
            Address = "東京都千代田区",
            Latitude = 35.681236,
            Longitude = 139.767125,
            RamenType = "醤油",
            OpeningHours = "11:00〜21:00",
            ClosedDay = "なし",
            Rating = 4.2
        },
        new Shop
        {
            Id = 2,
            Name = "Osaka Umeda Store",
            Price = 980m,
            Address = "大阪市北区",
            Latitude = 34.702485,
            Longitude = 135.495951,
            RamenType = "豚骨",
            OpeningHours = "11:30〜22:00",
            ClosedDay = "火曜日",
            Rating = 4.5
        },
        new Shop
        {
            Id = 3,
            Name = "Sapporo Store",
            Price = 1500m,
            Address = "札幌市中央区",
            Latitude = 43.062096,
            Longitude = 141.354376,
            RamenType = "味噌",
            OpeningHours = "10:30〜20:30",
            ClosedDay = "水曜日",
            Rating = 4.0
        }
    ];
}
