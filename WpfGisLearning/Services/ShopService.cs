using WpfGisLearning.Models;

namespace WpfGisLearning.Services;

public class ShopService : IShopService
{
    private readonly List<Shop> _shops =
    [
        new Shop { Id = 1, Name = "Tokyo Station Store", Price = 1200m, Address = "東京都千代田区", Latitude = 35.681236, Longitude = 139.767125, RamenType = "醤油", Tags = "駅近, 人気", RecommendedMenu = "特製醤油ラーメン", OpeningHours = "11:00〜21:00", ClosedDay = "なし", Rating = 4.2 },
        new Shop { Id = 2, Name = "Osaka Umeda Store", Price = 980m, Address = "大阪市北区", Latitude = 34.702485, Longitude = 135.495951, RamenType = "豚骨", Tags = "濃厚, 深夜", RecommendedMenu = "濃厚豚骨ラーメン", OpeningHours = "11:30〜22:00", ClosedDay = "火曜日", Rating = 4.5 },
        new Shop { Id = 3, Name = "Sapporo Store", Price = 1500m, Address = "札幌市中央区", Latitude = 43.062096, Longitude = 141.354376, RamenType = "味噌", Tags = "札幌, 老舗", RecommendedMenu = "札幌味噌ラーメン", OpeningHours = "10:30〜20:30", ClosedDay = "水曜日", Rating = 4.0 }
    ];

    public string GetWelcomeMessage() => "Welcome to Ramenia!";
    public IEnumerable<Shop> GetShops() => _shops;
    public void AddShop(Shop shop) => _shops.Add(shop);

    public void UpdateShop(Shop shop)
    {
        var index = _shops.FindIndex(x => x.Id == shop.Id);
        if (index >= 0)
            _shops[index] = shop;
    }

    public void DeleteShop(int id) => _shops.RemoveAll(x => x.Id == id);

    public void ToggleFavorite(int id)
    {
        var shop = _shops.FirstOrDefault(x => x.Id == id);
        if (shop is not null)
            shop.IsFavorite = !shop.IsFavorite;
    }
}
