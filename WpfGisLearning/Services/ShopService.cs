namespace WpfGisLearning.Services;

public class ShopService : IShopService
{
    // 簡易なサンプル：ショップ名やメッセージを返す
    public string GetWelcomeMessage()
    {
        Console.WriteLine("test");
        return "Welcome to Sample Shop! 今週のセール: 全品10%オフ";
    }
    public System.Collections.Generic.IEnumerable<WpfGisLearning.Models.Shop> GetShops()
    {
        // サンプルデータ
        return new[] {
            new WpfGisLearning.Models.Shop { Id = 1, Name = "Tokyo Station Store", Price = 1200m, Address = "東京都千代田区", Latitude = 35.681236, Longitude = 139.767125 },
            new WpfGisLearning.Models.Shop { Id = 2, Name = "Osaka Umeda Store", Price = 980m, Address = "大阪市北区", Latitude = 34.702485, Longitude = 135.495951 },
            new WpfGisLearning.Models.Shop { Id = 3, Name = "Sapporo Store", Price = 1500m, Address = "札幌市中央区", Latitude = 43.062096, Longitude = 141.354376 }
        };
    }
}
