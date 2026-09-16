using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using WpfGisLearning.Services.Interfaces;

namespace WpfGisLearning.Services;

public sealed class NominatimReverseGeocodingService : IReverseGeocodingService
{
    private readonly HttpClient _httpClient;

    public NominatimReverseGeocodingService() : this(CreateHttpClient())
    {
    }

    public NominatimReverseGeocodingService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string?> GetAddressAsync(double latitude, double longitude)
    {
        if (!MapCoordinateValidator.IsValid(latitude, longitude))
            return null;

        var url = $"https://nominatim.openstreetmap.org/reverse?lat={latitude.ToString(CultureInfo.InvariantCulture)}&lon={longitude.ToString(CultureInfo.InvariantCulture)}&format=jsonv2&accept-language=ja";
        var result = await _httpClient.GetFromJsonAsync<ReverseGeocodingResult>(url);
        return string.IsNullOrWhiteSpace(result?.DisplayName) ? null : result.DisplayName;
    }

    private static HttpClient CreateHttpClient()
    {
        var client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        client.DefaultRequestHeaders.UserAgent.ParseAdd("WpfGisLearning/1.0");
        return client;
    }

    private sealed class ReverseGeocodingResult
    {
        [JsonPropertyName("display_name")]
        public string? DisplayName { get; set; }
    }
}
