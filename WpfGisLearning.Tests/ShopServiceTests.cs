using WpfGisLearning.Models;
using WpfGisLearning.Services;
using WpfGisLearning.Services.Interfaces;

namespace WpfGisLearning.Tests;

[TestClass]
public class ShopServiceTests
{
    [TestMethod]
    public void Constructor_WhenStoreIsEmptySeedsInitialShops()
    {
        var store = new FakeShopDataStore();
        var service = CreateService(store);

        Assert.HasCount(3, service.GetShops());
        Assert.AreEqual("Tokyo Station Store", service.GetShops().First().Name);
        Assert.IsTrue(store.SaveCalled);
    }

    [TestMethod]
    public void AddShop_AddsShopAndSaves()
    {
        var store = new FakeShopDataStore(new Shop { Id = 1, Name = "Existing" });
        var service = CreateService(store);
        store.SaveCalled = false;

        service.AddShop(new Shop { Id = 2, Name = "Added" });

        Assert.HasCount(2, service.GetShops());
        Assert.AreEqual("Added", service.GetShops().Single(shop => shop.Id == 2).Name);
        Assert.IsTrue(store.SaveCalled);
    }

    [TestMethod]
    public void UpdateShop_UpdatesExistingShopAndSaves()
    {
        var store = new FakeShopDataStore(new Shop { Id = 1, Name = "Before" });
        var service = CreateService(store);
        store.SaveCalled = false;

        service.UpdateShop(new Shop { Id = 1, Name = "After" });

        Assert.AreEqual("After", service.GetShops().Single().Name);
        Assert.IsTrue(store.SaveCalled);
    }

    [TestMethod]
    public void UpdateShop_WhenIdDoesNotExistDoesNotSave()
    {
        var store = new FakeShopDataStore(new Shop { Id = 1, Name = "Existing" });
        var service = CreateService(store);
        store.SaveCalled = false;

        service.UpdateShop(new Shop { Id = 999, Name = "Missing" });

        Assert.HasCount(1, service.GetShops());
        Assert.AreEqual("Existing", service.GetShops().Single().Name);
        Assert.IsFalse(store.SaveCalled);
    }

    [TestMethod]
    public void DeleteShop_RemovesShopDeletesPhotosAndSaves()
    {
        var shop = new Shop { Id = 1, Name = "Delete Me" };
        var store = new FakeShopDataStore(shop);
        var photoService = new FakePhotoService();
        var service = new ShopService(photoService, store);
        store.SaveCalled = false;

        service.DeleteShop(1);

        Assert.HasCount(0, service.GetShops());
        Assert.AreSame(shop, photoService.DeletedShop);
        Assert.IsTrue(store.SaveCalled);
    }

    [TestMethod]
    public void DeleteShop_WhenIdDoesNotExistStillSavesCurrentData()
    {
        var store = new FakeShopDataStore(new Shop { Id = 1, Name = "Existing" });
        var photoService = new FakePhotoService();
        var service = new ShopService(photoService, store);
        store.SaveCalled = false;

        service.DeleteShop(999);

        Assert.HasCount(1, service.GetShops());
        Assert.IsNull(photoService.DeletedShop);
        Assert.IsTrue(store.SaveCalled);
    }

    [TestMethod]
    public void ToggleFavorite_TogglesExistingShopAndSaves()
    {
        var shop = new Shop { Id = 1, Name = "Favorite", IsFavorite = false };
        var store = new FakeShopDataStore(shop);
        var service = CreateService(store);
        store.SaveCalled = false;

        service.ToggleFavorite(1);

        Assert.IsTrue(shop.IsFavorite);
        Assert.IsTrue(store.SaveCalled);
    }

    [TestMethod]
    public void ToggleFavorite_WhenIdDoesNotExistDoesNothing()
    {
        var store = new FakeShopDataStore(new Shop { Id = 1, Name = "Existing" });
        var service = CreateService(store);
        store.SaveCalled = false;

        service.ToggleFavorite(999);

        Assert.IsFalse(store.SaveCalled);
        Assert.HasCount(1, service.GetShops());
    }

    [TestMethod]
    public void MergeImported_UpdatesMatchingIdAndKeepsExistingShops()
    {
        var existing = new Shop { Id = 1, Name = "Existing", IsFavorite = true };
        var untouched = new Shop { Id = 2, Name = "Untouched" };
        var store = new FakeShopDataStore(existing, untouched);
        var service = CreateService(store);
        store.SaveCalled = false;

        service.MergeImported([
            new Shop { Id = 1, Name = "Updated" },
            new Shop { Id = 0, Name = "Imported" }
        ]);

        var shops = service.GetShops().ToList();
        Assert.HasCount(3, shops);
        Assert.AreEqual("Updated", shops.Single(x => x.Id == 1).Name);
        Assert.IsTrue(shops.Single(x => x.Id == 1).IsFavorite);
        Assert.AreEqual("Untouched", shops.Single(x => x.Id == 2).Name);
        Assert.AreEqual("Imported", shops.Single(x => x.Id == 3).Name);
        Assert.IsTrue(store.SaveCalled);
    }

    [TestMethod]
    public void MergeImported_PreservesExistingPhotosWhenUpdating()
    {
        var photo = new ShopPhoto { Id = "photo", IsMain = true, FileName = "main.jpg" };
        var existing = new Shop { Id = 1, Name = "Existing", Photos = [photo] };
        var store = new FakeShopDataStore(existing);
        var service = CreateService(store);

        service.MergeImported([new Shop { Id = 1, Name = "Updated" }]);

        var updated = service.GetShops().Single();
        Assert.HasCount(1, updated.Photos);
        Assert.AreEqual("main.jpg", updated.Photos.Single().FileName);
    }

    [TestMethod]
    public void Constructor_RefreshesMainPhotoPath()
    {
        var photo = new ShopPhoto { Id = "main", IsMain = true, FileName = "main.jpg" };
        var shop = new Shop { Id = 1, Name = "Photo Shop", Photos = [photo] };
        var photoService = new FakePhotoService { PhotoPath = "photo/path/main.jpg" };

        var service = new ShopService(photoService, new FakeShopDataStore(shop));

        Assert.AreEqual("photo/path/main.jpg", service.GetShops().Single().MainPhotoPath);
    }

    private static ShopService CreateService(FakeShopDataStore store) =>
        new(new FakePhotoService(), store);

    private sealed class FakeShopDataStore : IShopDataStore
    {
        private readonly List<Shop> _initialShops;
        public bool SaveCalled { get; set; }

        public FakeShopDataStore(params Shop[] shops) => _initialShops = shops.ToList();

        public List<Shop> Load() => _initialShops.ToList();

        public void Save(IEnumerable<Shop> shops) => SaveCalled = true;
    }

    private sealed class FakePhotoService : IPhotoService
    {
        public string PhotoPath { get; set; } = string.Empty;
        public Shop? DeletedShop { get; private set; }

        public string GetPhotoPath(Shop shop, ShopPhoto photo) => PhotoPath;
        public void SavePhotos(Shop shop, IEnumerable<(string SourcePath, ShopPhoto Photo)> photos) { }
        public void DeletePhoto(Shop shop, ShopPhoto photo) { }
        public void DeleteShopPhotos(Shop shop) => DeletedShop = shop;
    }
}
