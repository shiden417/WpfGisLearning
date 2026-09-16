namespace WpfGisLearning.Services.Interfaces;

public interface INavigationViewFactory
{
    object CreateDetailView(int id);
    object CreateShopEditView(int? id);
    object CreateShopPageFrame();
    object CreateShopListView();
}
