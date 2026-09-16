using WpfGisLearning.Models;
using WpfGisLearning.Services;

namespace WpfGisLearning.Tests;

[TestClass]
public class PhotoServiceTests
{
    [TestMethod]
    public void SavePhotos_CopiesFilesAndAssignsExtension()
    {
        var root = CreateTempDirectory();
        var source = Path.Combine(root, "source.PNG");
        File.WriteAllText(source, "test-image");
        var service = new PhotoService(root);
        var shop = new Shop { Id = 42 };
        var photo = new ShopPhoto { Id = "photo1" };

        service.SavePhotos(shop, [(source, photo)]);

        Assert.AreEqual("photo1.png", photo.FileName);
        Assert.IsTrue(File.Exists(service.GetPhotoPath(shop, photo)));
        Assert.AreEqual("test-image", File.ReadAllText(service.GetPhotoPath(shop, photo)));

        Directory.Delete(root, true);
    }

    [TestMethod]
    public void SavePhotos_UsesJpgWhenSourceHasNoExtension()
    {
        var root = CreateTempDirectory();
        var source = Path.Combine(root, "source");
        File.WriteAllText(source, "test-image");
        var service = new PhotoService(root);
        var shop = new Shop { Id = 7 };
        var photo = new ShopPhoto { Id = "photo2" };

        service.SavePhotos(shop, [(source, photo)]);

        Assert.AreEqual("photo2.jpg", photo.FileName);
        Assert.IsTrue(File.Exists(service.GetPhotoPath(shop, photo)));

        Directory.Delete(root, true);
    }

    [TestMethod]
    public void DeletePhoto_RemovesExistingFile()
    {
        var root = CreateTempDirectory();
        var service = new PhotoService(root);
        var shop = new Shop { Id = 5 };
        var photo = new ShopPhoto { Id = "photo3", FileName = "photo3.jpg" };
        Directory.CreateDirectory(Path.Combine(root, "5"));
        File.WriteAllText(service.GetPhotoPath(shop, photo), "test-image");

        service.DeletePhoto(shop, photo);

        Assert.IsFalse(File.Exists(service.GetPhotoPath(shop, photo)));
        Directory.Delete(root, true);
    }

    [TestMethod]
    public void DeleteShopPhotos_RemovesShopDirectory()
    {
        var root = CreateTempDirectory();
        var service = new PhotoService(root);
        var shop = new Shop { Id = 9 };
        Directory.CreateDirectory(Path.Combine(root, "9"));
        File.WriteAllText(Path.Combine(root, "9", "photo.jpg"), "test-image");

        service.DeleteShopPhotos(shop);

        Assert.IsFalse(Directory.Exists(Path.Combine(root, "9")));
        Directory.Delete(root, true);
    }

    private static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "WpfGisLearningTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }
}
