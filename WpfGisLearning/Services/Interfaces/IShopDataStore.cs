using WpfGisLearning.Models;

namespace WpfGisLearning.Services.Interfaces;

public interface IShopDataStore
{
    List<Shop> Load();
    void Save(IEnumerable<Shop> shops);
}
