using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using WpfGisLearning.Services;

namespace WpfGisLearning.Views;

public partial class ShopPage : Page
{
    private readonly IServiceProvider _provider;

    public ShopPage(IServiceProvider provider)
    {
        InitializeComponent();

        _provider = provider;
    }

    private void OpenDetail1_Click(object sender, RoutedEventArgs e)
    {
        OpenDetail(1);
    }

    private void OpenDetail2_Click(object sender, RoutedEventArgs e)
    {
        OpenDetail(2);
    }

    private void OpenDetail(int id)
    {
        // Create DetailPage via DI so it can receive constructor parameters
        var detailPage = ActivatorUtilities.CreateInstance<DetailPage>(_provider, id);

        // Use Page.NavigationService to navigate within the Frame
        this.NavigationService?.Navigate(detailPage);
    }
}
