using WpfGisLearning.Models;
using WpfGisLearning.Services;

namespace WpfGisLearning.Tests;

[TestClass]
public class JsonShopDataStoreTests
{
    [TestMethod]
    public void Load_WhenFileDoesNotExistReturnsEmptyList()
    {
        using var tempDirectory = new TemporaryDirectory();
        var path = Path.Combine(tempDirectory.Path, "shops.json");
        var store = new JsonShopDataStore(path);

        var shops = store.Load();

        Assert.HasCount(0, shops);
    }

    [TestMethod]
    public void SaveAndLoad_RoundTripsShopData()
    {
        using var tempDirectory = new TemporaryDirectory();
        var path = Path.Combine(tempDirectory.Path, "shops.json");
        var store = new JsonShopDataStore(path);
        var shops = new List<Shop>
        {
            new()
            {
                Id = 7,
                Name = "Test Shop",
                Price = 1200,
                Address = "Tokyo",
                Latitude = 35.68,
                Longitude = 139.76,
                RamenType = "醤油",
                Rating = 4.5,
                IsFavorite = true
            }
        };

        store.Save(shops);
        var loaded = store.Load();

        Assert.HasCount(1, loaded);
        var shop = loaded[0];
        Assert.AreEqual(7, shop.Id);
        Assert.AreEqual("Test Shop", shop.Name);
        Assert.AreEqual(1200m, shop.Price);
        Assert.AreEqual("Tokyo", shop.Address);
        Assert.AreEqual(35.68, shop.Latitude);
        Assert.AreEqual(139.76, shop.Longitude);
        Assert.AreEqual("醤油", shop.RamenType);
        Assert.AreEqual(4.5, shop.Rating);
        Assert.IsTrue(shop.IsFavorite);
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        public string Path { get; } = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            "WpfGisLearningTests",
            Guid.NewGuid().ToString("N"));

        public TemporaryDirectory() => Directory.CreateDirectory(Path);

        public void Dispose()
        {
            if (Directory.Exists(Path))
                Directory.Delete(Path, recursive: true);
        }
    }
}
