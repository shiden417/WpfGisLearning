using System.Net;
using System.Net.Http;
using System.Text;
using WpfGisLearning.Services;

namespace WpfGisLearning.Tests;

[TestClass]
public class NominatimReverseGeocodingServiceTests
{
    [TestMethod]
    public async Task GetAddressAsync_ReturnsDisplayName()
    {
        using var client = CreateClient("{\"display_name\":\"東京都千代田区丸の内1丁目\"}");
        var service = new NominatimReverseGeocodingService(client);

        var result = await service.GetAddressAsync(35.681236, 139.767125);

        Assert.AreEqual("東京都千代田区丸の内1丁目", result);
    }

    [TestMethod]
    public async Task GetAddressAsync_ReturnsNull_WhenDisplayNameIsBlank()
    {
        using var client = CreateClient("{\"display_name\":\" \"}");
        var service = new NominatimReverseGeocodingService(client);

        var result = await service.GetAddressAsync(35.681236, 139.767125);

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetAddressAsync_ReturnsNull_WhenCoordinatesAreInvalid()
    {
        using var client = CreateClient("{\"display_name\":\"should not be called\"}");
        var service = new NominatimReverseGeocodingService(client);

        var result = await service.GetAddressAsync(91, 139);

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetAddressAsync_SendsInvariantCoordinatesAndJapaneseLanguage()
    {
        var handler = new RecordingHandler("{\"display_name\":\"東京都\"}");
        using var client = new HttpClient(handler);
        var service = new NominatimReverseGeocodingService(client);

        await service.GetAddressAsync(35.5, 139.75);

        StringAssert.Contains(handler.RequestUri!.Query, "lat=35.5");
        StringAssert.Contains(handler.RequestUri.Query, "lon=139.75");
        StringAssert.Contains(handler.RequestUri.Query, "accept-language=ja");
    }

    private static HttpClient CreateClient(string json) => new(new RecordingHandler(json));

    private sealed class RecordingHandler(string json) : HttpMessageHandler
    {
        public Uri? RequestUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestUri = request.RequestUri;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
        }
    }
}
