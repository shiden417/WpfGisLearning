using System.Windows;
using WpfGisLearning.ViewModels;

namespace WpfGisLearning.Views;

public partial class NewsView : System.Windows.Controls.UserControl
{
    public event EventHandler? RequestBack;

    public NewsView(NewsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void BackButton_Click(object sender, RoutedEventArgs e) => RequestBack?.Invoke(this, EventArgs.Empty);
}
