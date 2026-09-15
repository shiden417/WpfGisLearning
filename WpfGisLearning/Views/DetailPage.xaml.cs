using System.Windows;
using System.Windows.Controls;

namespace WpfGisLearning.Views;

public partial class DetailPage : Page
{
    public int Id { get; }

    public DetailPage(int id)
    {
        InitializeComponent();

        Id = id;

        IdText.Text = Id.ToString();
    }

    private void GoBack_Click(object sender, RoutedEventArgs e)
    {
        if (this.NavigationService?.CanGoBack == true)
        {
            this.NavigationService.GoBack();
        }
    }
}
