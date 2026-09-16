namespace WpfGisLearning.Models;

/// <summary>
/// ニュース画面で表示する1件の記事情報を表します。
/// RSSから取得した記事を画面表示用のデータに変換した結果として使用します。
/// </summary>
public class NewsItem
{
    /// <summary>記事を一意に識別するIDです。RSSでは記事URLを使用します。</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>記事タイトルです。</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>画面に表示する記事概要です。</summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>記事を分類するカテゴリです。</summary>
    public string Category { get; set; } = "特集";

    /// <summary>記事から判定した地域です。</summary>
    public string Region { get; set; } = "全国";

    /// <summary>記事が公開された日時です。</summary>
    public DateTime PublishedAt { get; set; }

    /// <summary>ローカルに保存して使用する画像パスです。</summary>
    public string ImagePath { get; set; } = string.Empty;

    /// <summary>記事ページなどから取得した画像URLです。</summary>
    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>記事の提供元名称です。</summary>
    public string SourceName { get; set; } = "Ramenia編集部";

    /// <summary>元記事のURLです。</summary>
    public string SourceUrl { get; set; } = string.Empty;

    /// <summary>ニュース一覧の注目記事として扱うかを示します。</summary>
    public bool IsFeatured { get; set; }

    /// <summary>画像URLが設定されているかを画面側から判定しやすくしたプロパティです。</summary>
    public bool HasImage => !string.IsNullOrWhiteSpace(ImageUrl);
}
