using System.Windows;
using WpfGisLearning.Utilities;

namespace WpfGisLearning.Services;

/// <summary>
/// NavigationServiceからWPF固有のWindow操作を引き受けるナビゲーションホストです。
/// WPFの表示処理をインターフェースの実装へ閉じ込めています。
/// </summary>
public sealed class WpfNavigationHost : Interfaces.INavigationHost
{
    /// <summary>
    /// メイン画面が存在する場合はそのContentControlへ表示し、存在しなければ新しいWindowを表示します。
    /// </summary>
    /// <param name="content">表示するViewまたはFrameです。</param>
    public void NavigateMainContent(object content)
    {
        if (Application.Current?.MainWindow is MainWindow main)
        {
            main.MainContent.Content = content;
            return;
        }

        // メインWindowが取得できない場合のフォールバックとして独立Windowを作る。
        var window = new Window
        {
            Title = "WpfGisLearning",
            Content = content,
            SizeToContent = SizeToContent.WidthAndHeight,
            Owner = Application.Current?.MainWindow
        };
        window.Show();
    }

    /// <summary>指定サイズのモーダルWindowを開き、閉じた後にコールバックを実行します。</summary>
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

    /// <summary>指定したコンテンツを独立Windowとして表示します。</summary>
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

    /// <summary>現在のMainWindowへ店舗データの再読み込みを依頼します。</summary>
    public void RefreshShopData()
    {
        if (Application.Current?.MainWindow is MainWindow main)
            main.RefreshShopData();
    }

    /// <summary>
    /// ダイアログ用Windowを生成し、共通サイズ・所有者・アイコンなどを設定します。
    /// </summary>
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
