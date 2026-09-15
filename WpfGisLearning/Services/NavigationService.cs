using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfGisLearning.ViewModels;
using WpfGisLearning.Views;

namespace WpfGisLearning.Services;

public class NavigationService : INavigationService
{
    private readonly IServiceProvider _provider;

    public NavigationService(IServiceProvider provider)
    {
        _provider = provider;
    }

    public void NavigateToDetail(int id)
    {
        // DetailViewModel はナビゲーション時に渡される値 (id) を受け取る。
        var viewModel =
            ActivatorUtilities.CreateInstance<DetailViewModel>(
                _provider,
                id);

        // Create view and inject the viewModel
        var view =
            ActivatorUtilities.CreateInstance<DetailView>(
                _provider,
                viewModel);

        // Host the view in a Window
        var window = new Window()
        {
            Title = "Detail",
            Content = view,
            SizeToContent = SizeToContent.WidthAndHeight,
            Owner = Application.Current?.MainWindow,
            Icon = CreateRameniaIcon()
        };

        window.Show();
    }

    public void NavigateToShopPageFrame()
    {
        var frame = new System.Windows.Controls.Frame();

        var shopPage =
            ActivatorUtilities.CreateInstance<Views.ShopPage>(
                _provider);

        frame.NavigationUIVisibility =
            System.Windows.Navigation.NavigationUIVisibility.Visible;

        frame.Navigate(shopPage);

        if (Application.Current?.MainWindow is MainWindow main)
        {
            main.MainContent.Content = frame;
            return;
        }

        var window = new Window()
        {
            Title = "Shop Page (Frame)",
            Content = frame,
            SizeToContent = SizeToContent.WidthAndHeight,
            Owner = Application.Current?.MainWindow
        };

        window.Show();
    }

    public void NavigateToShopList()
    {
        var view =
            ActivatorUtilities.CreateInstance<Views.ShopListView>(
                _provider);

        if (Application.Current?.MainWindow is MainWindow main)
        {
            main.MainContent.Content = view;
            return;
        }

        var window = new Window()
        {
            Title = "Shop List",
            Content = view,
            SizeToContent = SizeToContent.WidthAndHeight,
            Owner = Application.Current?.MainWindow
        };

        window.Show();
    }

    private ImageSource CreateRameniaIcon()
    {
        const int size = 64;

        var visual =
            new DrawingVisual();

        using (var context = visual.RenderOpen())
        {
            var formattedText =
                new FormattedText(
                    "🍜",
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface(
                        new FontFamily("Segoe UI Emoji"),
                        FontStyles.Normal,
                        FontWeights.Normal,
                        FontStretches.Normal),
                    48,
                    Brushes.Black,
                    1.0);

            // 絵文字を中央に配置
            var x =
                (size - formattedText.Width) / 2;

            var y =
                (size - formattedText.Height) / 2;

            context.DrawText(
                formattedText,
                new Point(x, y));
        }

        var bitmap =
            new RenderTargetBitmap(
                size,
                size,
                96,
                96,
                PixelFormats.Pbgra32);

        bitmap.Render(visual);

        bitmap.Freeze();

        return bitmap;
    }
}