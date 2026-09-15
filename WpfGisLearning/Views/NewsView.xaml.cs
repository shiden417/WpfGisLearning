using System.Diagnostics;
using System.Windows;
using WpfGisLearning.Models;
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

    private void NewsCard_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (e.ClickCount != 2) return;
        if ((sender as FrameworkElement)?.DataContext is not NewsItem news || string.IsNullOrWhiteSpace(news.SourceUrl)) return;

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = news.SourceUrl,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"記事を開けませんでした。\n{ex.Message}", "ニュース", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
