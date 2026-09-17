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
        if (index < 0) return;
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
        if (shop is not null) _photoService.DeleteShopPhotos(shop);
        _shops.RemoveAll(x => x.Id == id);
        Save();
    }

    /// <summary>指定IDの店舗のお気に入り状態を反転し、永続化します。</summary>
    public void ToggleFavorite(int id)
    {
        var shop = _shops.FirstOrDefault(x => x.Id == id);
        if (shop is null) return;
        shop.IsFavorite = !shop.IsFavorite;
        Save();
    }

    /// <summary>明示的に店舗一覧を全置換します。</summary>
    public void ReplaceAll(IEnumerable<Shop> shops)
    {
        var importedShops = shops.ToList();
        _shops.Clear();
        _shops.AddRange(importedShops);
        RefreshPhotoPaths();
        Save();
    }

    /// <summary>
    /// Excelなどから取り込んだ店舗を既存データへ統合します。
    /// ID一致は更新、IDが0以下は新規追加します。
    /// Excelにない既存店舗は保持します。
    /// 既存店舗を更新する場合、Excelに存在しない写真とお気に入り状態は維持します。
    /// </summary>
    public void MergeImported(IEnumerable<Shop> shops)
    {
        var importedShops = shops.ToList();
        var reservedIds = importedShops
            .Where(shop => shop.Id > 0)
            .Select(shop => shop.Id)
            .ToHashSet();
        var nextId = _shops.Count == 0 ? 1 : _shops.Max(x => x.Id) + 1;

        foreach (var importedShop in importedShops)
        {
            if (importedShop.Id <= 0)
            {
                while (_shops.Any(x => x.Id == nextId) || reservedIds.Contains(nextId))
                    nextId++;
                importedShop.Id = nextId++;
                RefreshPhotoPath(importedShop);
                _shops.Add(importedShop);
                continue;
            }

            var existingIndex = _shops.FindIndex(x => x.Id == importedShop.Id);
            if (existingIndex < 0)
            {
                RefreshPhotoPath(importedShop);
                _shops.Add(importedShop);
                continue;
            }

            var existingShop = _shops[existingIndex];
            importedShop.IsFavorite = existingShop.IsFavorite;
            importedShop.Photos = existingShop.Photos;
            RefreshPhotoPath(importedShop);
            _shops[existingIndex] = importedShop;
        }

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

    /// <summary>初回起動時に使用する学習用のサンプル店舗を生成します。</summary>
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
