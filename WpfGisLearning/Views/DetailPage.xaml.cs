using System.Windows;
using System.Windows.Controls;

namespace WpfGisLearning.Views;

/// <summary>
/// Frame内のページ遷移を学習するための店舗詳細Pageです。
/// </summary>
public partial class DetailPage : Page
{
    /// <summary>表示対象店舗のIDです。</summary>
    public int Id { get; }

    /// <summary>店舗IDを受け取り、画面へ表示します。</summary>
    public DetailPage(int id)
    {
        InitializeComponent();
        Id = id;
        IdText.Text = Id.ToString();
    }

    /// <summary>戻る履歴が存在する場合、WPF FrameのNavigationServiceで前のPageへ戻ります。</summary>
    private void GoBack_Click(object sender, RoutedEventArgs e)
    {
        if (NavigationService?.CanGoBack == true)
        {
            NavigationService.GoBack();
        }
    }
}
