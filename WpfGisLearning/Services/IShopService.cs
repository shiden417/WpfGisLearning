namespace WpfGisLearning.Services;

public interface IShopService
{
    string GetWelcomeMessage();
    System.Collections.Generic.IEnumerable<WpfGisLearning.Models.Shop> GetShops();
}
