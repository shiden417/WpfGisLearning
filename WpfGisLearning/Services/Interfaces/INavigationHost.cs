namespace WpfGisLearning.Services.Interfaces;

public interface INavigationHost
{
    void NavigateMainContent(object content);

    void ShowDialog(
        object content,
        string title,
        double width,
        double height,
        double minWidth,
        double minHeight,
        Action? closed = null);

    void Show(object content, string title);

    void RefreshShopData();
}
