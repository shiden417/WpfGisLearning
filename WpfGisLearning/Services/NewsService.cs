using System.Net.Http;
using System.Xml.Linq;
using WpfGisLearning.Models;
using WpfGisLearning.Services.Interfaces;

namespace WpfGisLearning.Services;

public class NewsService : INewsService
{
    private const string FeedBaseUrl = "https://news.google.com/rss/search";
    private const int MaxNewsItems = 20;
    private const string FeedUserAgent = "Ramenia/1.0";
    private const string ArticleUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 Chrome/140 Safari/537.36";

    private static HttpClient CreateHttpClient() => new() { Timeout = TimeSpan.FromSeconds(10) };

    private readonly HttpClient _httpClient;

    public NewsService()
        : this(CreateHttpClient())
    {
    }

    public NewsService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<NewsItem>> GetNewsAsync(
        DateTime date,
        CancellationToken cancellationToken = default)
    {
        var end = date.Date.AddDays(1);
        var query = $"ラーメン before:{end:yyyy-MM-dd}";
        var feedUrl = $"{FeedBaseUrl}?q={Uri.EscapeDataString(query)}&hl=ja&gl=JP&ceid=JP:ja";

        using var request = new HttpRequestMessage(HttpMethod.Get, feedUrl);
        request.Headers.UserAgent.ParseAdd(FeedUserAgent);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var document = await XDocument.LoadAsync(stream, LoadOptions.None, cancellationToken);

        var items = document.Descendants("item")
            .Select(NewsItemParser.Parse)
            .Where(item => item is not null)
            .Select(item => item!)
            .Where(item => item.PublishedAt.Date <= date.Date)
            .OrderByDescending(item => item.PublishedAt)
            .Take(MaxNewsItems)
            .ToList();

        foreach (var item in items)
            item.ImageUrl = await TryGetArticleImageUrlAsync(item.SourceUrl, cancellationToken);

        return items;
    }

    private async Task<string> TryGetArticleImageUrlAsync(string url, CancellationToken cancellationToken)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.UserAgent.ParseAdd(ArticleUserAgent);
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return string.Empty;

            var html = await response.Content.ReadAsStringAsync(cancellationToken);
            return NewsItemParser.FindArticleImageUrl(html) ?? string.Empty;
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
