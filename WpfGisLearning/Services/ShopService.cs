using WpfGisLearning.Models;

namespace WpfGisLearning.Services;

public class ShopService : IShopService
{
    private readonly List<Shop> _shops =
    [
        new Shop { Id = 1, Name = "Tokyo Station Store", Price = 1200m, Address = "東京都千代田区", Latitude = 35.681236, Longitude = 139.767125 },
        new Shop { Id = 2, Name = "Osaka Umeda Store", Price = 980m, Address = "大阪市北区", Latitude = 34.702485, Longitude = 135.495951 },
        new Shop { Id = 3, Name = "Sapporo Store", Price = 1500m, Address = "札幌市中央区", Latitude = 43.062096, Longitude = 141.354376 }
    ];

    public string GetWelcomeMessage() => "Welcome to Ramenia!";

    public IEnumerable<Shop> GetShops() => _shops;

    public void AddShop(Shop shop)
    {
        _shops.Add(shop);
    }

    public void UpdateShop(Shop shop)
    {
        var index = _shops.FindIndex(x => x.Id == shop.Id);

        if (index >= 0)
        {
            _shops[index] = shop;
        }
    }

    public void DeleteShop(int id)
    {
        _shops.RemoveAll(x => x.Id == id);
    }
}
