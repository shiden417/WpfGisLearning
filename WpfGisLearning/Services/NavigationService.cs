using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WpfGisLearning.Services.Interfaces;
using WpfGisLearning.Utilities;
using WpfGisLearning.ViewModels;
using WpfGisLearning.Views;

namespace WpfGisLearning.Services;

public class NavigationService : INavigationService
{
    private readonly IServiceProvider _provider;

    public NavigationService(IServiceProvider provider) => _provider = provider;

    public void NavigateToDetail(int id)
    {
        var viewModel = ActivatorUtilities.CreateInstance<DetailViewModel>(_provider, id);
        var view = ActivatorUtilities.CreateInstance<DetailView>(_provider, viewModel);

        var content = new Grid
        {
            Background = (Brush)Application.Current.FindResource("WindowBackgroundBrush")
        };
        content.RowDefinitions.Add(new RowDefinition { Height = new GridLength(72) });
        content.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        var header = new Border
        {
            Background = (Brush)Application.Current.FindResource("HeaderBrush")
        };
        var headerGrid = new Grid { Margin = new Thickness(24, 0, 24, 0) };
        var headerStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
        headerStack.Children.Add(new TextBlock
        {
            Text = "🍜 Ramenia",
            Foreground = (Brush)Application.Current.FindResource("SurfaceBrush"),
            FontSize = 15,
            FontWeight = FontWeights.SemiBold
        });
        headerStack.Children.Add(new TextBlock
        {
            Text = "店舗詳細",
            Foreground = (Brush)Application.Current.FindResource("SurfaceBrush"),
            FontSize = 26,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 4, 0, 0)
        });
        headerGrid.Children.Add(headerStack);
        header.Child = headerGrid;
        Grid.SetRow(header, 0);
        content.Children.Add(header);

        Grid.SetRow(view, 1);
        content.Children.Add(view);

        var window = new Window
        {
            Title = "店舗詳細 - Ramenia",
            Content = content,
            Width = 1000,
            Height = 900,
            MinWidth = 820,
            MinHeight = 700,
            Owner = Application.Current?.MainWindow,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Icon = RameniaIconFactory.Create()
        };

        window.ShowDialog();
        if (Application.Current?.MainWindow is MainWindow main)
            main.RefreshShopData();
    }

    public void NavigateToShopEdit(int? id = null)
    {
        var viewModel = _provider.GetRequiredService<ShopEditViewModel>();
        viewModel.Load(id);
        var view = ActivatorUtilities.CreateInstance<ShopEditView>(_provider, viewModel);

        var window = new Window
        {
            Title = id.HasValue ? "店舗を編集 - Ramenia" : "店舗を登録 - Ramenia",
            Content = view,
            Width = 1250,
            Height = 850,
            MinWidth = 1000,
            MinHeight = 700,
            Owner = Application.Current?.MainWindow,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Icon = RameniaIconFactory.Create()
        };

        window.ShowDialog();
    }

    public void NavigateToShopPageFrame()
    {
        var frame = new Frame();
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
}
