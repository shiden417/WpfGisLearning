using WpfGisLearning.Models;
using WpfGisLearning.Services.Interfaces;

namespace WpfGisLearning.Services;

public sealed class ShopService : IShopService
{
    private readonly IPhotoService _photoService;
    private readonly IShopDataStore _dataStore;
    private readonly List<Shop> _shops;

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

    public string GetWelcomeMessage() => "Welcome to Ramenia!";

    public IEnumerable<Shop> GetShops() => _shops;

    public void AddShop(Shop shop)
    {
        RefreshPhotoPath(shop);
        _shops.Add(shop);
        Save();
    }

    public void UpdateShop(Shop shop)
    {
        RefreshPhotoPath(shop);

        var index = _shops.FindIndex(x => x.Id == shop.Id);
        if (index < 0)
            return;

        _shops[index] = shop;
        Save();
    }

    public void DeleteShop(int id)
    {
        var shop = _shops.FirstOrDefault(x => x.Id == id);
        if (shop is not null)
            _photoService.DeleteShopPhotos(shop);

        _shops.RemoveAll(x => x.Id == id);
        Save();
    }

    public void ToggleFavorite(int id)
    {
        var shop = _shops.FirstOrDefault(x => x.Id == id);
        if (shop is null)
            return;

        shop.IsFavorite = !shop.IsFavorite;
        Save();
    }

    public void ReplaceAll(IEnumerable<Shop> shops)
    {
        var importedShops = shops.ToList();
        _shops.Clear();
        _shops.AddRange(importedShops);
        RefreshPhotoPaths();
        Save();
    }

    private void RefreshPhotoPaths()
    {
        foreach (var shop in _shops)
            RefreshPhotoPath(shop);
    }

    private void RefreshPhotoPath(Shop shop)
    {
        var mainPhoto = shop.MainPhoto;
        shop.MainPhotoPath = mainPhoto is null
            ? string.Empty
            : _photoService.GetPhotoPath(shop, mainPhoto);
    }

    private void Save() => _dataStore.Save(_shops);

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
