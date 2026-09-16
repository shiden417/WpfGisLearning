using WpfGisLearning.Models;

namespace WpfGisLearning.Services.Interfaces;

public interface IExcelShopDataService
{
    void CreateImportTemplate(string filePath);
    void Export(string filePath, IEnumerable<Shop> shops);
    List<Shop> Import(string filePath);
}
