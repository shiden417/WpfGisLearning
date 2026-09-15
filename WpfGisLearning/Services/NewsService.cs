using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using WpfGisLearning.Models;

namespace WpfGisLearning.Services;

public class NewsService
{
    private const string FeedBaseUrl = "https://news.google.com/rss/search";
    private static readonly HttpClient HttpClient = new() { Timeout = TimeSpan.FromSeconds(10) };

    public async Task<IReadOnlyList<NewsItem>> GetNewsAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        var end = date.Date.AddDays(1);
        var query = $"ラーメン before:{end:yyyy-MM-dd}";
        var feedUrl = $"{FeedBaseUrl}?q={Uri.EscapeDataString(query)}&hl=ja&gl=JP&ceid=JP:ja";

        using var request = new HttpRequestMessage(HttpMethod.Get, feedUrl);
        request.Headers.UserAgent.ParseAdd("Ramenia/1.0");
        using var response = await HttpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var document = await XDocument.LoadAsync(stream, LoadOptions.None, cancellationToken);

        var items = document.Descendants("item")
            .Select(ParseItem)
            .Where(x => x is not null)
            .Select(x => x!)
            .Where(x => x.PublishedAt.Date <= date.Date)
            .OrderByDescending(x => x.PublishedAt)
            .Take(20)
            .ToList();

        foreach (var item in items)
        {
            item.ImageUrl = await TryGetArticleImageUrlAsync(item.SourceUrl, cancellationToken);
        }

        return items;
    }

    private static NewsItem? ParseItem(XElement item)
    {
        var title = item.Element("title")?.Value?.Trim();
        var link = item.Element("link")?.Value?.Trim();
        var publishedText = item.Element("pubDate")?.Value?.Trim();
        var description = item.Element("description")?.Value ?? string.Empty;
        var source = item.Element("source")?.Value?.Trim();
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(link)) return null;
        if (!DateTimeOffset.TryParse(publishedText, out var publishedAt)) return null;

        var localPublishedAt = publishedAt.ToLocalTime();
        var cleanTitle = title;
        if (!string.IsNullOrWhiteSpace(source) && cleanTitle.EndsWith($" - {source}", StringComparison.OrdinalIgnoreCase))
            cleanTitle = cleanTitle[..^(source.Length + 3)].Trim();

        return new NewsItem
        {
            Id = link,
            Title = cleanTitle,
            Summary = CleanDescription(description),
            Category = DetectCategory(cleanTitle),
            Region = DetectRegion(cleanTitle),
            PublishedAt = localPublishedAt.DateTime,
            SourceName = string.IsNullOrWhiteSpace(source) ? "Google ニュース" : source,
            SourceUrl = link
        };
    }

    private static async Task<string> TryGetArticleImageUrlAsync(string url, CancellationToken cancellationToken)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 Chrome/140 Safari/537.36");
            using var response = await HttpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode) return string.Empty;

            var html = await response.Content.ReadAsStringAsync(cancellationToken);
            var imageUrl = FindMetaContent(html, "property", "og:image")
                ?? FindMetaContent(html, "name", "twitter:image")
                ?? FindMetaContent(html, "property", "og:image:url");

            return IsArticleImageUrl(imageUrl) ? imageUrl! : string.Empty;
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

    private static string? FindMetaContent(string html, string attributeName, string attributeValue)
    {
        var pattern = $"<meta[^>]+{Regex.Escape(attributeName)}=[\\\"']{Regex.Escape(attributeValue)}[\\\"'][^>]+content=[\\\"'](?<content>[^\\\"']+)[\\\"']";
        var match = Regex.Match(html, pattern, RegexOptions.IgnoreCase);
        if (match.Success) return WebUtility.HtmlDecode(match.Groups["content"].Value.Trim());

        pattern = $"<meta[^>]+content=[\\\"'](?<content>[^\\\"']+)[\\\"'][^>]+{Regex.Escape(attributeName)}=[\\\"']{Regex.Escape(attributeValue)}[\\\"']";
        match = Regex.Match(html, pattern, RegexOptions.IgnoreCase);
        return match.Success ? WebUtility.HtmlDecode(match.Groups["content"].Value.Trim()) : null;
    }

    private static bool IsArticleImageUrl(string? url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) return false;
        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) return false;

        var host = uri.Host.ToLowerInvariant();
        var path = uri.AbsolutePath.ToLowerInvariant();
        var value = uri.AbsoluteUri.ToLowerInvariant();

        if (host == "news.google.com" || host.EndsWith(".google.com", StringComparison.OrdinalIgnoreCase)) return false;
        if (host.Contains("googleusercontent") || host.Contains("gstatic")) return false;
        if (path.Contains("favicon") || path.Contains("logo") || path.Contains("icon") || value.Contains("favicon") || value.Contains("logo")) return false;

        return true;
    }

    private static string CleanDescription(string html)
    {
        var text = Regex.Replace(html, "<[^>]+>", " ");
        text = WebUtility.HtmlDecode(text);
        text = Regex.Replace(text, @"\s+", " ").Trim();
        return text.Length > 180 ? text[..180] + "…" : text;
    }

    private static string DetectCategory(string title)
    {
        if (title.Contains("イベント") || title.Contains("フェス") || title.Contains("開催")) return "イベント";
        if (title.Contains("限定") || title.Contains("期間") || title.Contains("発売")) return "限定";
        if (title.Contains("新店") || title.Contains("オープン") || title.Contains("開店")) return "新店";
        return "特集";
    }

    private static string DetectRegion(string title)
    {
        var regions = new[] { "北海道", "東京", "大阪", "京都", "神奈川", "千葉", "埼玉", "愛知", "福岡", "兵庫", "宮城", "広島", "静岡", "茨城", "栃木", "群馬", "長野", "新潟" };
        return regions.FirstOrDefault(title.Contains) ?? "全国";
    }
}