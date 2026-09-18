using System.Net.Http;
using System.Xml.Linq;
using WpfGisLearning.Models;
using WpfGisLearning.Services.Interfaces;

namespace WpfGisLearning.Services;

public class NewsService : INewsService
{
    private const string FeedBaseUrl = "https://www.bing.com/news/search";
    private const int MaxNewsItems = 20;
    private const int FeedPageSize = 10;
    private const int MaxFeedPages = 20;
    private const string FeedUserAgent = "Ramenia/1.0";
    private const string ArticleUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 Chrome/140 Safari/537.36";

    private static HttpClient CreateHttpClient() => new() { Timeout = TimeSpan.FromSeconds(10) };

    private readonly HttpClient _httpClient;

    public NewsService() : this(CreateHttpClient()) { }

    public NewsService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<NewsItem>> GetNewsAsync(
        DateTime date,
        CancellationToken cancellationToken = default)
    {
        var allItems = new List<NewsItem>();
        var seenUrls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Bing News RSSはページ間で結果が重複することがあるため、
        // 重複ページだけでは探索を終了せず、指定日まで次ページを確認する。
        for (var page = 0; page < MaxFeedPages; page++)
        {
            var offset = page * FeedPageSize + 1;
            var pageItems = await GetFeedPageAsync(offset, cancellationToken);

            if (pageItems.Count == 0)
                break;

            var newItems = pageItems
                .Where(item => seenUrls.Add(item.SourceUrl))
                .ToList();

            if (newItems.Count == 0)
                continue;

            allItems.AddRange(newItems);

            var oldestDate = newItems.Min(item => item.PublishedAt);
            if (oldestDate.Date <= date.Date)
                break;
        }

        var items = allItems
            .Where(item => item.PublishedAt.Date <= date.Date)
            .OrderByDescending(item => item.PublishedAt)
            .Take(MaxNewsItems)
            .ToList();

        foreach (var item in items.Where(item => !item.HasImage))
            item.ImageUrl = await TryGetArticleImageUrlAsync(item.SourceUrl, cancellationToken);

        return items;
    }

    private async Task<IReadOnlyList<NewsItem>> GetFeedPageAsync(
        int offset,
        CancellationToken cancellationToken)
    {
        var query = "ラーメン";
        var feedUrl =
            $"{FeedBaseUrl}?q={Uri.EscapeDataString(query)}" +
            $"&first={offset}" +
            "&qft=sortbydate%3d%221%22" +
            "&format=RSS&mkt=ja-JP";

        using var request = new HttpRequestMessage(HttpMethod.Get, feedUrl);
        request.Headers.UserAgent.ParseAdd(FeedUserAgent);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var document = await XDocument.LoadAsync(stream, LoadOptions.None, cancellationToken);

        return document.Descendants("item")
            .Select(NewsItemParser.Parse)
            .Where(item => item is not null)
            .Select(item => item!)
            .ToList();
    }

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
            throw;
        }
        catch
        {
            return string.Empty;
        }
    }
}
