using System.Net;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using WpfGisLearning.Models;

namespace WpfGisLearning.Services;

public class NewsService
{
    private const string FeedUrl = "https://news.google.com/rss/search?q=%E3%83%A9%E3%83%BC%E3%83%A1%E3%83%B3+when%3A7d&hl=ja&gl=JP&ceid=JP%3Aja";
    private static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(10)
    };

    public async Task<IReadOnlyList<NewsItem>> GetNewsAsync(CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, FeedUrl);
        request.Headers.UserAgent.ParseAdd("Ramenia/1.0");

        using var response = await HttpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var document = await XDocument.LoadAsync(stream, LoadOptions.None, cancellationToken);

        return document.Descendants("item")
            .Select(ParseItem)
            .Where(x => x is not null)
            .Select(x => x!)
            .OrderByDescending(x => x.PublishedAt)
            .Take(20)
            .ToList();
    }

    private static NewsItem? ParseItem(XElement item)
    {
        var title = item.Element("title")?.Value?.Trim();
        var link = item.Element("link")?.Value?.Trim();
        var publishedText = item.Element("pubDate")?.Value?.Trim();
        var description = item.Element("description")?.Value ?? string.Empty;
        var source = item.Element("source")?.Value?.Trim();

        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(link)) return null;
        if (!DateTimeOffset.TryParse(publishedText, out var publishedAt)) publishedAt = DateTimeOffset.Now;

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
            PublishedAt = publishedAt.LocalDateTime,
            SourceName = string.IsNullOrWhiteSpace(source) ? "Google ニュース" : source,
            SourceUrl = link,
            IsFeatured = false
        };
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
