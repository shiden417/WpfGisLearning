using WpfGisLearning.Models;

namespace WpfGisLearning.Services.Interfaces;

/// <summary>
/// ニュースデータを取得するサービスの契約です。
/// ViewModelがRSSやHTTP通信の実装詳細を意識しないようにします。
/// </summary>
public interface INewsService
{
    /// <summary>指定日以前のニュースを非同期で取得します。</summary>
    /// <param name="date">取得対象とする基準日です。</param>
    /// <param name="cancellationToken">通信キャンセル用のトークンです。</param>
    Task<IReadOnlyList<NewsItem>> GetNewsAsync(DateTime date, CancellationToken cancellationToken = default);
}
