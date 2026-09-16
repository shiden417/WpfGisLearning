namespace WpfGisLearning.Services.Interfaces;

public interface INavigationService
{
    void NavigateToDetail(int id);
    void NavigateToShopEdit(int? id = null);
    void NavigateToShopList();
    void NavigateToShopPageFrame();
}
