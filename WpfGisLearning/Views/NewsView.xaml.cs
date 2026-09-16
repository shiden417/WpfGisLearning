using System.Diagnostics;
using System.Windows;
using WpfGisLearning.Models;
using WpfGisLearning.ViewModels;

namespace WpfGisLearning.Views;

/// <summary>
/// ニュース画面のWPFイベントとNewsViewModelを接続するコードビハインドです。
/// 画面表示時の自動更新や、記事を外部ブラウザーで開く処理を担当します。
/// </summary>
public partial class NewsView : System.Windows.Controls.UserControl
{
    /// <summary>ニュース画面から店舗地図画面へ戻ることを通知するイベントです。</summary>
    public event EventHandler? RequestBack;

    /// <summary>ViewModelをDataContextへ設定してニュース画面を初期化します。</summary>
    public NewsView(NewsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        IsVisibleChanged += NewsView_IsVisibleChanged;
    }

    /// <summary>
    /// 画面が表示状態になったタイミングでニュースを再取得します。
    /// WPFのIsVisibleChangedイベントを使って画面ライフサイクルに処理を接続しています。
    /// </summary>
    private async void NewsView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is true && DataContext is NewsViewModel viewModel)
        {
            await viewModel.RefreshCommand.ExecuteAsync(null);
        }
    }

    /// <summary>戻るボタンのクリックを親画面へ通知します。</summary>
    private void BackButton_Click(object sender, RoutedEventArgs e) => RequestBack?.Invoke(this, EventArgs.Empty);

    /// <summary>ニュースカードのダブルクリックで元記事を既定ブラウザーへ開きます。</summary>
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
