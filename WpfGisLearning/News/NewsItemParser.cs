using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using WpfGisLearning.Models;

namespace WpfGisLearning.News;

/// <summary>
/// GoogleニュースRSSのXMLや記事HTMLから、画面表示用のNewsItem情報を抽出するパーサーです。
/// </summary>
public static class NewsItemParser
{
    /// <summary>タイトルから地域を判定するために確認する地域名一覧です。</summary>
    private static readonly string[] Regions =
    [
        "北海道", "東京", "大阪", "京都", "神奈川", "千葉", "埼玉", "愛知", "福岡",
        "兵庫", "宮城", "広島", "静岡", "茨城", "栃木", "群馬", "長野", "新潟"
    ];

    /// <summary>
    /// RSSのitem要素1件をNewsItemへ変換します。
    /// 必須情報や公開日時がない記事はnullを返して一覧から除外します。
    /// </summary>
    public static NewsItem? Parse(XElement item)
    {
        var title = item.Element("title")?.Value?.Trim();
        var link = item.Element("link")?.Value?.Trim();
        var publishedText = item.Element("pubDate")?.Value?.Trim();
        var description = item.Element("description")?.Value ?? string.Empty;
        var source = item.Element("source")?.Value?.Trim();
        var imageUrl = FindRssImageUrl(item);

        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(link))
            return null;

        if (!DateTimeOffset.TryParse(publishedText, out var publishedAt))
            return null;

        var localPublishedAt = publishedAt.ToLocalTime();
        var cleanTitle = RemoveSourceSuffix(title, source);

        return new NewsItem
        {
            Id = link,
            Title = cleanTitle,
            Summary = CleanDescription(description),
            Category = DetectCategory(cleanTitle),
            Region = DetectRegion(cleanTitle),
            PublishedAt = localPublishedAt.DateTime,
            SourceName = string.IsNullOrWhiteSpace(source) ? "Google ニュース" : source,
            SourceUrl = link,
            ImageUrl = imageUrl ?? string.Empty
        };
    }

    /// <summary>
    /// 記事HTMLからog:image、twitter:imageなどの代表画像URLを探します。
    /// 無効なURLやGoogle内部画像は除外します。
    /// </summary>
    public static string? FindArticleImageUrl(string html, Uri? baseUri = null)
    {
        var imageUrl = FindMetaContent(html, "property", "og:image")
            ?? FindMetaContent(html, "name", "twitter:image")
            ?? FindMetaContent(html, "name", "twitter:image:src")
            ?? FindMetaContent(html, "property", "og:image:url")
            ?? FindMetaContent(html, "itemprop", "image")
            ?? FindLinkHref(html, "image_src")
            ?? FindJsonLdImageUrl(html);

        return NormalizeImageUrl(imageUrl, baseUri);
    }

    /// <summary>
    /// HTMLのcanonical URLを取得します。
    /// Googleニュースのリダイレクト先が中継ページだった場合に元記事URLを再取得するために使用します。
    /// </summary>
    public static string? FindCanonicalUrl(string html, Uri? baseUri = null)
    {
        var url = FindMetaContent(html, "property", "og:url")
            ?? FindLinkHref(html, "canonical");

        if (!Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri))
            return null;

        if (!uri.IsAbsoluteUri && baseUri is not null)
            uri = new Uri(baseUri, uri);

        return uri.IsAbsoluteUri && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
            ? uri.AbsoluteUri
            : null;
    }

    /// <summary>RSSのmedia:contentやenclosureから記事画像URLを取得します。</summary>
    private static string? FindRssImageUrl(XElement item)
    {
        XNamespace media = "http://search.yahoo.com/mrss/";

        var url = item.Elements(media + "content")
            .Select(element => (string?)element.Attribute("url"))
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));

        url ??= item.Elements("enclosure")
            .Where(element => string.Equals((string?)element.Attribute("type"), "image/jpeg", StringComparison.OrdinalIgnoreCase)
                || string.Equals((string?)element.Attribute("type"), "image/png", StringComparison.OrdinalIgnoreCase)
                || string.Equals((string?)element.Attribute("type"), "image/webp", StringComparison.OrdinalIgnoreCase))
            .Select(element => (string?)element.Attribute("url"))
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));

        return NormalizeImageUrl(url, null);
    }

    /// <summary>HTMLのlink要素から指定relのhrefを取得します。</summary>
    private static string? FindLinkHref(string html, string rel)
    {
        var pattern = $@"<link[^>]+rel=[""']{Regex.Escape(rel)}[""'][^>]+href=[""'](?<href>[^""']+)[""']";
        var match = Regex.Match(html, pattern, RegexOptions.IgnoreCase);
        if (match.Success)
            return WebUtility.HtmlDecode(match.Groups["href"].Value.Trim());

        pattern = $@"<link[^>]+href=[""'](?<href>[^""']+)[""'][^>]+rel=[""']{Regex.Escape(rel)}[""']";
        match = Regex.Match(html, pattern, RegexOptions.IgnoreCase);
        return match.Success ? WebUtility.HtmlDecode(match.Groups["href"].Value.Trim()) : null;
    }

    /// <summary>JSON-LDから記事画像URLを取得します。</summary>
    private static string? FindJsonLdImageUrl(string html)
    {
        foreach (Match match in Regex.Matches(
                     html,
                     @"<script[^>]*>(?<json>\s*\{.*?\}\s*)</script>",
                     RegexOptions.IgnoreCase | RegexOptions.Singleline))
        {
            try
            {
                using var document = JsonDocument.Parse(WebUtility.HtmlDecode(match.Groups["json"].Value));
                var result = FindImageInJson(document.RootElement);
                if (!string.IsNullOrWhiteSpace(result))
                    return result;
            }
            catch (JsonException)
            {
                // 一部サイトの不正なJSON-LDは無視して次の候補を探します。
            }
        }

        return null;
    }

    /// <summary>JSON-LDのオブジェクトを再帰的に探索してimage値を取得します。</summary>
    private static string? FindImageInJson(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            if (element.TryGetProperty("image", out var image))
            {
                var value = image.ValueKind switch
                {
                    JsonValueKind.String => image.GetString(),
                    JsonValueKind.Array => image.EnumerateArray()
                        .Select(item => item.ValueKind == JsonValueKind.String ? item.GetString() : null)
                        .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)),
                    JsonValueKind.Object when image.TryGetProperty("url", out var url)
                        && url.ValueKind == JsonValueKind.String => url.GetString(),
                    _ => null
                };

                if (!string.IsNullOrWhiteSpace(value))
                    return value;
            }

            foreach (var property in element.EnumerateObject())
            {
                var result = FindImageInJson(property.Value);
                if (!string.IsNullOrWhiteSpace(result))
                    return result;
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                var result = FindImageInJson(item);
                if (!string.IsNullOrWhiteSpace(result))
                    return result;
            }
        }

        return null;
    }

    /// <summary>相対URLを絶対URLへ変換し、記事画像として利用可能か検証します。</summary>
    private static string? NormalizeImageUrl(string? url, Uri? baseUri)
    {
        if (!Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri))
            return null;

        if (!uri.IsAbsoluteUri && baseUri is not null)
            uri = new Uri(baseUri, uri);

        return uri.IsAbsoluteUri && IsArticleImageUrl(uri.AbsoluteUri)
            ? uri.AbsoluteUri
            : null;
    }

    /// <summary>Googleニュースがタイトル末尾へ付ける「 - 情報元」を取り除きます。</summary>
    private static string RemoveSourceSuffix(string title, string? source)
    {
        if (string.IsNullOrWhiteSpace(source)
            || !title.EndsWith($" - {source}", StringComparison.OrdinalIgnoreCase))
            return title;

        return title[..^(source.Length + 3)].Trim();
    }

    /// <summary>
    /// HTMLのmeta要素から指定属性とcontentの組み合わせを取得します。
    /// 属性順が異なる2パターンを試します。
    /// </summary>
    private static string? FindMetaContent(string html, string attributeName, string attributeValue)
    {
        var pattern = $"<meta[^>]+{Regex.Escape(attributeName)}=[\\\"']{Regex.Escape(attributeValue)}[\\\"'][^>]+content=[\\\"'](?<content>[^\\\"']+)[\\\"']";
        var match = Regex.Match(html, pattern, RegexOptions.IgnoreCase);
        if (match.Success)
            return WebUtility.HtmlDecode(match.Groups["content"].Value.Trim());

        pattern = $"<meta[^>]+content=[\\\"'](?<content>[^\\\"']+)[\\\"'][^>]+{Regex.Escape(attributeName)}=[\\\"']{Regex.Escape(attributeValue)}[\\\"']";
        match = Regex.Match(html, pattern, RegexOptions.IgnoreCase);
        return match.Success ? WebUtility.HtmlDecode(match.Groups["content"].Value.Trim()) : null;
    }

    /// <summary>取得した画像URLが実際に記事画像として利用可能そうかを判定します。</summary>
    private static bool IsArticleImageUrl(string? url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return false;

        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            return false;

        var host = uri.Host.ToLowerInvariant();
        var path = uri.AbsolutePath.ToLowerInvariant();
        var value = uri.AbsoluteUri.ToLowerInvariant();

        // Googleニュース自身やGoogle内部アセットは記事画像として扱わない。
        if (host == "news.google.com" || host.EndsWith(".google.com", StringComparison.OrdinalIgnoreCase))
            return false;

        if (host.Contains("googleusercontent") || host.Contains("gstatic"))
            return false;

        // faviconやロゴなど、記事本文以外の小さな画像を除外する。
        if (path.Contains("favicon") || path.Contains("logo") || path.Contains("icon")
            || value.Contains("favicon") || value.Contains("logo"))
            return false;

        return true;
    }

    /// <summary>HTMLタグを除去して概要文字列を整形し、最大180文字に切り詰めます。</summary>
    private static string CleanDescription(string html)
    {
        var text = Regex.Replace(html, "<[^>]+>", " ");
        text = WebUtility.HtmlDecode(text);
        text = Regex.Replace(text, @"\s+", " ").Trim();
        return text.Length > 180 ? text[..180] + "…" : text;
    }

    /// <summary>タイトル中のキーワードからニュースカテゴリを判定します。</summary>
    private static string DetectCategory(string title)
    {
        if (title.Contains("イベント") || title.Contains("フェス") || title.Contains("開催"))
            return "イベント";
        if (title.Contains("限定") || title.Contains("期間") || title.Contains("発売"))
            return "限定";
        if (title.Contains("新店") || title.Contains("オープン") || title.Contains("開店"))
            return "新店";
        return "特集";
    }

    /// <summary>タイトルに含まれる地域名から地域を判定し、見つからなければ全国とします。</summary>
    private static string DetectRegion(string title) =>
        Regions.FirstOrDefault(title.Contains) ?? "全国";
}
