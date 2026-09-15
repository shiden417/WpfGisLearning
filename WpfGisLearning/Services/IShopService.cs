using WpfGisLearning.Models;

namespace WpfGisLearning.Services;

public interface IShopService
{
    string GetWelcomeMessage();
    IEnumerable<Shop> GetShops();
    void AddShop(Shop shop);
    void UpdateShop(Shop shop);
    void DeleteShop(int id);
    void ToggleFavorite(int id);
}
