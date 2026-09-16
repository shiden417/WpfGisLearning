using WpfGisLearning.Models;

namespace WpfGisLearning.Services.Interfaces;

public interface IPhotoService
{
    string GetPhotoPath(Shop shop, ShopPhoto photo);
    void SavePhotos(Shop shop, IEnumerable<(string SourcePath, ShopPhoto Photo)> photos);
    void DeletePhoto(Shop shop, ShopPhoto photo);
    void DeleteShopPhotos(Shop shop);
}
