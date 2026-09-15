using System.IO;
using WpfGisLearning.Models;

namespace WpfGisLearning.Services;

public class PhotoService : IPhotoService
{
    private readonly string _rootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Ramenia", "Images");

    public string GetPhotoPath(Shop shop, ShopPhoto photo) => Path.Combine(_rootPath, shop.Id.ToString(), photo.FileName);

    public void SavePhotos(Shop shop, IEnumerable<(string SourcePath, ShopPhoto Photo)> photos)
    {
        var shopDirectory = Path.Combine(_rootPath, shop.Id.ToString());
        Directory.CreateDirectory(shopDirectory);

        foreach (var (sourcePath, photo) in photos)
        {
            var extension = Path.GetExtension(sourcePath);
            if (string.IsNullOrWhiteSpace(extension)) extension = ".jpg";
            photo.FileName = $"{photo.Id}{extension.ToLowerInvariant()}";
            File.Copy(sourcePath, GetPhotoPath(shop, photo), true);
        }
    }

    public void DeletePhoto(Shop shop, ShopPhoto photo)
    {
        var path = GetPhotoPath(shop, photo);
        if (File.Exists(path)) File.Delete(path);
    }

    public void DeleteShopPhotos(Shop shop)
    {
        var directory = Path.Combine(_rootPath, shop.Id.ToString());
        if (Directory.Exists(directory)) Directory.Delete(directory, true);
    }
}
