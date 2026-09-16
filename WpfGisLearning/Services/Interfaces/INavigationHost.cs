namespace WpfGisLearning.Services.Interfaces;

/// <summary>
/// ナビゲーション先を実際のWPFウィンドウや画面領域へ表示するホストの契約です。
/// NavigationServiceからWPFの表示処理を分離するために使用します。
/// </summary>
public interface INavigationHost
{
    /// <summary>メインコンテンツ領域へ画面を表示します。</summary>
    void NavigateMainContent(object content);

    /// <summary>指定したコンテンツをダイアログとして表示します。</summary>
    void ShowDialog(
        object content,
        string title,
        double width,
        double height,
        double minWidth,
        double minHeight,
        Action? closed = null);

    /// <summary>指定したコンテンツを独立したウィンドウとして表示します。</summary>
    void Show(object content, string title);

    /// <summary>店舗データを表示側へ再読み込みさせます。</summary>
    void RefreshShopData();
}
