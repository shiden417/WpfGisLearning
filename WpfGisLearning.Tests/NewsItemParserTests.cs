using System.Xml.Linq;
using WpfGisLearning.Services;

namespace WpfGisLearning.Tests;

[TestClass]
public class NewsItemParserTests
{
    [TestMethod]
    public void Parse_ConvertsRssItemToNewsItem()
    {
        var item = XElement.Parse("""
            <item>
              <title>東京の新店ラーメン - Example News</title>
              <link>https://example.com/news/1</link>
              <pubDate>Tue, 15 Sep 2026 09:00:00 +0900</pubDate>
              <description>&lt;p&gt;新しい&lt;b&gt;ラーメン店&lt;/b&gt;がオープンしました。&lt;/p&gt;</description>
              <source>Example News</source>
            </item>
            """);

        var result = NewsItemParser.Parse(item);

        Assert.IsNotNull(result);
        Assert.AreEqual("https://example.com/news/1", result!.Id);
        Assert.AreEqual("東京の新店ラーメン", result.Title);
        Assert.AreEqual("新しい ラーメン店 がオープンしました。", result.Summary);
        Assert.AreEqual("新店", result.Category);
        Assert.AreEqual("東京", result.Region);
        Assert.AreEqual("Example News", result.SourceName);
        Assert.AreEqual("https://example.com/news/1", result.SourceUrl);
    }


    [TestMethod]
    public void Parse_UsesImageFromRssMediaContent()
    {
        var item = XElement.Parse("""
            <item xmlns:media="http://search.yahoo.com/mrss/">
              <title>ラーメンニュース</title>
              <link>https://example.com/news/1</link>
              <pubDate>Wed, 16 Sep 2026 09:00:00 GMT</pubDate>
              <media:content url="https://example.com/images/1.jpg" type="image/jpeg" />
            </item>
            """);

        var result = NewsItemParser.Parse(item);

        Assert.IsNotNull(result);
        Assert.AreEqual("https://example.com/images/1.jpg", result!.ImageUrl);
        Assert.IsTrue(result.HasImage);
    }

    [TestMethod]
    public void Parse_ReturnsNullWhenRequiredFieldsAreMissing()
    {
        var item = XElement.Parse("""
            <item>
              <title>タイトルのみ</title>
              <pubDate>Tue, 15 Sep 2026 09:00:00 +0900</pubDate>
            </item>
            """);

        Assert.IsNull(NewsItemParser.Parse(item));
    }

    [TestMethod]
    public void Parse_ReturnsNullWhenPublishedDateIsInvalid()
    {
        var item = XElement.Parse("""
            <item>
              <title>タイトル</title>
              <link>https://example.com/news/1</link>
              <pubDate>invalid date</pubDate>
            </item>
            """);

        Assert.IsNull(NewsItemParser.Parse(item));
    }

    [TestMethod]
    public void Parse_DetectsDefaultCategoryAndRegion()
    {
        var item = XElement.Parse("""
            <item>
              <title>全国のラーメン特集</title>
              <link>https://example.com/news/1</link>
              <pubDate>Tue, 15 Sep 2026 09:00:00 +0900</pubDate>
            </item>
            """);

        var result = NewsItemParser.Parse(item);

        Assert.IsNotNull(result);
        Assert.AreEqual("特集", result!.Category);
        Assert.AreEqual("全国", result.Region);
        Assert.AreEqual("Google ニュース", result.SourceName);
    }

    [TestMethod]
    public void FindArticleImageUrl_ReturnsValidOgImage()
    {
        const string html = "<meta property=\"og:image\" content=\"https://example.com/image.jpg\">";

        var result = NewsItemParser.FindArticleImageUrl(html);

        Assert.AreEqual("https://example.com/image.jpg", result);
    }


    [TestMethod]
    public void FindArticleImageUrl_UsesTwitterAndJsonLdFallbacks()
    {
        const string html = """
            <meta name="twitter:image" content="/images/twitter.jpg">
            """;

        var result = NewsItemParser.FindArticleImageUrl(
            html,
            new Uri("https://example.com/articles/1"));

        Assert.AreEqual("https://example.com/images/twitter.jpg", result);
    }

    [TestMethod]
    public void FindArticleImageUrl_UsesJsonLdWhenMetaImageIsMissing()
    {
        const string html = """
            <script type="application/ld+json">
            {"@type":"NewsArticle","image":{"url":"https://example.com/images/article.jpg"}}
            </script>
            """;

        var result = NewsItemParser.FindArticleImageUrl(html);

        Assert.AreEqual("https://example.com/images/article.jpg", result);
    }

    [TestMethod]
    public void FindCanonicalUrl_ReturnsCanonicalArticleUrl()
    {
        const string html = """
            <link rel="canonical" href="/articles/1">
            """;

        var result = NewsItemParser.FindCanonicalUrl(
            html,
            new Uri("https://example.com/news"));

        Assert.AreEqual("https://example.com/articles/1", result);
    }

    [TestMethod]
    public void FindArticleImageUrl_RejectsGoogleAndLogoImages()
    {
        const string html = "<meta property=\"og:image\" content=\"https://news.google.com/logo.png\">";

        Assert.IsNull(NewsItemParser.FindArticleImageUrl(html));
    }
}
