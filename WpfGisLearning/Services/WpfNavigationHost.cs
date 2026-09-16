using System.Windows;
using WpfGisLearning.Utilities;

namespace WpfGisLearning.Services;

public sealed class WpfNavigationHost : Interfaces.INavigationHost
{
    public void NavigateMainContent(object content)
    {
        if (Application.Current?.MainWindow is MainWindow main)
        {
            main.MainContent.Content = content;
            return;
        }

        var window = new Window
        {
            Title = "WpfGisLearning",
            Content = content,
            SizeToContent = SizeToContent.WidthAndHeight,
            Owner = Application.Current?.MainWindow
        };
        window.Show();
    }

    public void ShowDialog(
        object content,
        string title,
        double width,
        double height,
        double minWidth,
        double minHeight,
        Action? closed = null)
    {
        var window = CreateWindow(content, title, width, height, minWidth, minHeight);
        window.ShowDialog();
        closed?.Invoke();
    }

    public void Show(object content, string title)
    {
        var window = new Window
        {
            Title = title,
            Content = content,
            SizeToContent = SizeToContent.WidthAndHeight,
            Owner = Application.Current?.MainWindow
        };
        window.Show();
    }

    public void RefreshShopData()
    {
        if (Application.Current?.MainWindow is MainWindow main)
            main.RefreshShopData();
    }

    private static Window CreateWindow(
        object content,
        string title,
        double width,
        double height,
        double minWidth,
        double minHeight) =>
        new()
        {
            Title = title,
            Content = content,
            Width = width,
            Height = height,
            MinWidth = minWidth,
            MinHeight = minHeight,
            Owner = Application.Current?.MainWindow,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Icon = RameniaIconFactory.Create()
        };
}
