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
        var viewModel = ActivatorUtilities.CreateInstance<DetailViewModel>(_provider, id);
        var view = ActivatorUtilities.CreateInstance<DetailView>(_provider, viewModel);

        var window = new Window
        {
            Title = "店舗詳細 - Ramenia",
            Content = view,
            Width = 720,
            Height = 780,
            Owner = Application.Current?.MainWindow,
            Icon = CreateRameniaIcon()
        };

        window.Show();
    }

    public void NavigateToShopEdit(int? id = null)
    {
        var viewModel = ActivatorUtilities.CreateInstance<ShopEditViewModel>(_provider, id);
        var view = ActivatorUtilities.CreateInstance<ShopEditView>(_provider, viewModel);

        var window = new Window
        {
            Title = id.HasValue ? "店舗を編集 - Ramenia" : "店舗を登録 - Ramenia",
            Content = view,
            Width = 1100,
            Height = 760,
            MinWidth = 900,
            MinHeight = 650,
            Owner = Application.Current?.MainWindow,
            Icon = CreateRameniaIcon()
        };

        window.ShowDialog();
    }

    public void NavigateToShopPageFrame()
    {
        var frame = new System.Windows.Controls.Frame();
        var shopPage = ActivatorUtilities.CreateInstance<Views.ShopPage>(_provider);
        frame.NavigationUIVisibility = System.Windows.Navigation.NavigationUIVisibility.Visible;
        frame.Navigate(shopPage);

        if (Application.Current?.MainWindow is MainWindow main)
        {
            main.MainContent.Content = frame;
            return;
        }

        var window = new Window
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
        var view = ActivatorUtilities.CreateInstance<Views.ShopListView>(_provider);

        if (Application.Current?.MainWindow is MainWindow main)
        {
            main.MainContent.Content = view;
            return;
        }

        var window = new Window
        {
            Title = "Shop List",
            Content = view,
            SizeToContent = SizeToContent.WidthAndHeight,
            Owner = Application.Current?.MainWindow
        };
        window.Show();
    }

    private static ImageSource CreateRameniaIcon()
    {
        const int size = 64;
        var visual = new DrawingVisual();

        using (var context = visual.RenderOpen())
        {
            var formattedText = new FormattedText(
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

            context.DrawText(
                formattedText,
                new Point((size - formattedText.Width) / 2, (size - formattedText.Height) / 2));
        }

        var bitmap = new RenderTargetBitmap(size, size, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(visual);
        bitmap.Freeze();
        return bitmap;
    }
}
