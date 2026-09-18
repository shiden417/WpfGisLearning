using System.Net.Http;
using System.Xml.Linq;
using WpfGisLearning.Models;
using WpfGisLearning.Services.Interfaces;

namespace WpfGisLearning.Services;

/// <summary>
/// GoogleニュースのRSSからラーメン関連ニュースを取得するサービスです。
/// RSS取得と元記事のOG画像取得をまとめて行い、画面側にはNewsItemとして返します。
/// </summary>
public class NewsService : INewsService
{
    /// <summary>GoogleニュースRSS検索APIのエンドポイントです。</summary>
    private const string FeedBaseUrl = "https://www.bing.com/news/search";

    /// <summary>1回の取得で画面に返す最大ニュース件数です。</summary>
    private const int MaxNewsItems = 20;

    /// <summary>RSS取得時に送るUser-Agentです。</summary>
    private const string FeedUserAgent = "Ramenia/1.0";

    /// <summary>元記事HTML取得時に送るブラウザー風User-Agentです。</summary>
    private const string ArticleUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 Chrome/140 Safari/537.36";

    /// <summary>ニュース取得用HTTPクライアントを生成します。</summary>
    private static HttpClient CreateHttpClient() => new() { Timeout = TimeSpan.FromSeconds(10) };

    /// <summary>RSSや記事HTMLの取得に使用するHTTPクライアントです。</summary>
    private readonly HttpClient _httpClient;

    /// <summary>既定のHTTPクライアントを使ってニュースサービスを生成します。</summary>
    public NewsService()
        : this(CreateHttpClient())
    {
    }

    /// <summary>テストやDIで差し替え可能なHTTPクライアントを受け取ります。</summary>
    public NewsService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// 指定日以前のニュースをRSSから取得し、記事画像も可能な範囲で補完します。
    /// </summary>
    /// <param name="date">取得対象とする基準日です。</param>
    /// <param name="cancellationToken">通信をキャンセルするためのトークンです。</param>
    /// <returns>公開日時の新しい順に並べたニュース一覧です。</returns>
    public async Task<IReadOnlyList<NewsItem>> GetNewsAsync(
        DateTime date,
        CancellationToken cancellationToken = default)
    {
        // before条件は基準日の翌日を指定し、基準日当日分まで含める。
        var end = date.Date.AddDays(1);
        var query = $"ラーメン before:{end:yyyy-MM-dd}";
        var feedUrl = $"{FeedBaseUrl}?q={Uri.EscapeDataString(query)}&format=RSS&mkt=ja-JP";

        using var request = new HttpRequestMessage(HttpMethod.Get, feedUrl);
        request.Headers.UserAgent.ParseAdd(FeedUserAgent);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var document = await XDocument.LoadAsync(stream, LoadOptions.None, cancellationToken);

        // XMLのitemをNewsItemへ変換し、公開日で絞り込み・並び替え・件数制限を行う。
        var items = document.Descendants("item")
            .Select(NewsItemParser.Parse)
            .Where(item => item is not null)
            .Select(item => item!)
            .Where(item => item.PublishedAt.Date <= date.Date)
            .OrderByDescending(item => item.PublishedAt)
            .Take(MaxNewsItems)
            .ToList();

        // RSSには元記事画像が含まれない場合があるため、各記事のHTMLからOG画像を探す。
        foreach (var item in items)
            item.ImageUrl = await TryGetArticleImageUrlAsync(item.SourceUrl, cancellationToken);

        return items;
    }

    /// <summary>
    /// 元記事のHTMLからOG画像などの画像URLを取得します。
    /// 画像取得失敗はニュース全体の取得失敗にせず、画像なしとして扱います。
    /// </summary>
    private async Task<string> TryGetArticleImageUrlAsync(string url, CancellationToken cancellationToken)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.UserAgent.ParseAdd(ArticleUserAgent);
            request.Headers.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8");
            request.Headers.AcceptLanguage.ParseAdd("ja-JP,ja;q=0.9,en-US;q=0.8,en;q=0.7");

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return string.Empty;

            var html = await response.Content.ReadAsStringAsync(cancellationToken);
            var finalUri = response.RequestMessage?.RequestUri;
            var imageUrl = NewsItemParser.FindArticleImageUrl(html, finalUri);
            if (!string.IsNullOrWhiteSpace(imageUrl))
                return imageUrl;

            // Googleニュースの中継ページが返った場合はcanonical URLを再取得する。
            var canonicalUrl = NewsItemParser.FindCanonicalUrl(html, finalUri);
            if (!string.IsNullOrWhiteSpace(canonicalUrl)
                && !string.Equals(canonicalUrl, finalUri?.AbsoluteUri, StringComparison.OrdinalIgnoreCase))
            {
                using var canonicalRequest = new HttpRequestMessage(HttpMethod.Get, canonicalUrl);
                canonicalRequest.Headers.UserAgent.ParseAdd(ArticleUserAgent);
                canonicalRequest.Headers.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8");
                canonicalRequest.Headers.AcceptLanguage.ParseAdd("ja-JP,ja;q=0.9,en-US;q=0.8,en;q=0.7");

                using var canonicalResponse = await _httpClient.SendAsync(canonicalRequest, cancellationToken);
                if (!canonicalResponse.IsSuccessStatusCode)
                    return string.Empty;

                var canonicalHtml = await canonicalResponse.Content.ReadAsStringAsync(cancellationToken);
                return NewsItemParser.FindArticleImageUrl(
                    canonicalHtml,
                    canonicalResponse.RequestMessage?.RequestUri) ?? string.Empty;
            }

            return string.Empty;
        }
        catch (OperationCanceledException)
        {
            // キャンセルだけは呼び出し元へ伝えて、UIから通信を停止できるようにする。
            throw;
        }
        catch
        {
            // 一部記事の画像取得失敗でニュース一覧全体が使えなくならないようにする。
            return string.Empty;
        }
    }
}
