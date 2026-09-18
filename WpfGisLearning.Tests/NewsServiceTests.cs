using System.Net;
using System.Net.Http;
using WpfGisLearning.Services;

namespace WpfGisLearning.Tests;

[TestClass]
public class NewsServiceTests
{
    [TestMethod]
    public async Task GetNewsAsync_ParsesFeedAndArticleImage()
    {
        var handler = new QueueHandler(
        [
            CreateResponse("<rss><channel><item><title>東京の新店 - Example</title><link>https://example.com/news/1</link><pubDate>Wed, 16 Sep 2026 00:00:00 GMT</pubDate><description>&lt;p&gt;新しいラーメン店がオープン&lt;/p&gt;</description><source>Example</source></item></channel></rss>"),
            CreateResponse("<html><head><meta property=\"og:image\" content=\"https://example.com/images/1.jpg\" /></head></html>")
        ]);
        using var client = new HttpClient(handler);
        var service = new NewsService(client);

        var items = await service.GetNewsAsync(new DateTime(2026, 9, 16));

        Assert.HasCount(1, items);
        Assert.AreEqual("東京の新店", items[0].Title);
        Assert.AreEqual("新しいラーメン店がオープン", items[0].Summary);
        Assert.AreEqual("新店", items[0].Category);
        Assert.AreEqual("東京", items[0].Region);
        Assert.AreEqual("https://example.com/images/1.jpg", items[0].ImageUrl);
        Assert.HasCount(2, handler.Requests);
    }


    [TestMethod]
    public async Task GetNewsAsync_UsesCanonicalArticlePageWhenRedirectPageHasNoImage()
    {
        var handler = new QueueHandler(
        [
            CreateResponse("<rss><channel><item><title>ニュース</title><link>https://news.google.com/rss/articles/test</link><pubDate>Wed, 16 Sep 2026 00:00:00 GMT</pubDate></item></channel></rss>"),
            CreateResponse("<html><head><link rel=\"canonical\" href=\"https://example.com/news/1\"></head></html>"),
            CreateResponse("<html><head><meta property=\"og:image\" content=\"https://example.com/images/1.jpg\"></head></html>")
        ]);
        using var client = new HttpClient(handler);
        var service = new NewsService(client);

        var items = await service.GetNewsAsync(new DateTime(2026, 9, 16));

        Assert.HasCount(1, items);
        Assert.AreEqual("https://example.com/images/1.jpg", items[0].ImageUrl);
        Assert.HasCount(3, handler.Requests);
    }

    [TestMethod]
    public async Task GetNewsAsync_PagesFeedUntilRequestedDateIsReached()
    {
        var currentFeed = "<rss><channel>" +
            "<item><title>新しいラーメン</title><link>https://example.com/new</link><pubDate>Wed, 16 Sep 2026 12:00:00 GMT</pubDate></item>" +
            "</channel></rss>";
        var historicalFeed = "<rss><channel>" +
            "<item><title>過去のラーメン</title><link>https://example.com/old</link><pubDate>Mon, 1 Sep 2026 12:00:00 GMT</pubDate></item>" +
            "</channel></rss>";

        var handler = new QueueHandler(
        [
            CreateResponse(currentFeed),
            CreateResponse(historicalFeed),
            CreateResponse("<html><head></head></html>")
        ]);
        using var client = new HttpClient(handler);
        var service = new NewsService(client);

        var items = await service.GetNewsAsync(new DateTime(2026, 9, 1));

        Assert.HasCount(1, items);
        Assert.AreEqual("過去のラーメン", items[0].Title);
        Assert.HasCount(3, handler.Requests);
        Assert.Contains("first=1", handler.Requests[0].RequestUri!.Query);
        Assert.Contains("first=11", handler.Requests[1].RequestUri!.Query);
    }

    [TestMethod]
    public async Task GetNewsAsync_ExcludesItemsAfterRequestedDate()
    {
        var feed = "<rss><channel>" +
            "<item><title>昨日のラーメン</title><link>https://example.com/old</link><pubDate>Tue, 15 Sep 2026 12:00:00 GMT</pubDate></item>" +
            "<item><title>未来のラーメン</title><link>https://example.com/future</link><pubDate>Thu, 17 Sep 2026 12:00:00 GMT</pubDate></item>" +
            "</channel></rss>";
        var handler = new QueueHandler(
        [
            CreateResponse(feed),
            CreateResponse("<html><head></head></html>")
        ]);
        using var client = new HttpClient(handler);
        var service = new NewsService(client);

        var items = await service.GetNewsAsync(new DateTime(2026, 9, 16));

        Assert.HasCount(1, items);
        Assert.AreEqual("昨日のラーメン", items[0].Title);
        Assert.HasCount(2, handler.Requests);
    }

    [TestMethod]
    public async Task GetNewsAsync_ReturnsEmptyListWhenFeedHasNoValidItems()
    {
        var handler = new QueueHandler([CreateResponse("<rss><channel><item><title></title><link></link></item></channel></rss>")]);
        using var client = new HttpClient(handler);
        var service = new NewsService(client);

        var items = await service.GetNewsAsync(new DateTime(2026, 9, 16));

        Assert.IsEmpty(items);
        Assert.HasCount(1, handler.Requests);
    }

    [TestMethod]
    public async Task GetNewsAsync_KeepsNewsItemWhenArticleImageRequestFails()
    {
        var handler = new QueueHandler(
        [
            CreateResponse("<rss><channel><item><title>ニュース</title><link>https://example.com/news</link><pubDate>Wed, 16 Sep 2026 00:00:00 GMT</pubDate></item></channel></rss>"),
            new HttpResponseMessage(HttpStatusCode.NotFound)
        ]);
        using var client = new HttpClient(handler);
        var service = new NewsService(client);

        var items = await service.GetNewsAsync(new DateTime(2026, 9, 16));

        Assert.HasCount(1, items);
        Assert.AreEqual(string.Empty, items[0].ImageUrl);
        Assert.IsFalse(items[0].HasImage);
    }

    private static HttpResponseMessage CreateResponse(string content) =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent(content)
        };

    private sealed class QueueHandler(IEnumerable<HttpResponseMessage> responses) : HttpMessageHandler
    {
        private readonly Queue<HttpResponseMessage> _responses = new(responses);
        public List<HttpRequestMessage> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            return Task.FromResult(_responses.Dequeue());
        }
    }
}
