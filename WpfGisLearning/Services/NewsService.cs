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
        // Google News の after/before は境界が厳密なため、前後1日を含めて取得し、
        // 最後に公開日時を日本時間へ変換して指定日だけに絞り込む。
        var start = date.Date.AddDays(-1);
        var end = date.Date.AddDays(2);
        var query = $"ラーメン after:{start:yyyy-MM-dd} before:{end:yyyy-MM-dd}";
        var feedUrl = $"{FeedBaseUrl}?q={Uri.EscapeDataString(query)}&hl=ja&gl=JP&ceid=JP:ja";

        using var request = new HttpRequestMessage(HttpMethod.Get, feedUrl);
        request.Headers.UserAgent.ParseAdd("Ramenia/1.0");
        using var response = await HttpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var document = await XDocument.LoadAsync(stream, LoadOptions.None, cancellationToken);

        return document.Descendants("item")
            .Select(ParseItem)
            .Where(x => x is not null)
            .Select(x => x!)
            .Where(x => x.PublishedAt.Date == date.Date)
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
        if (!DateTimeOffset.TryParse(publishedText, out var publishedAt)) return null;

        var localPublishedAt = publishedAt.ToLocalTime();
        var cleanTitle = title;
        if (!string.IsNullOrWhiteSpace(source) && cleanTitle.EndsWith($" - {source}", StringComparison.OrdinalIgnoreCase))
            cleanTitle = cleanTitle[..^(source.Length + 3)].Trim();

        return new NewsItem
        {
            Id = link, Title = cleanTitle, Summary = CleanDescription(description),
            Category = DetectCategory(cleanTitle), Region = DetectRegion(cleanTitle),
            PublishedAt = localPublishedAt.DateTime,
            SourceName = string.IsNullOrWhiteSpace(source) ? "Google ニュース" : source,
            SourceUrl = link
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
