using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace WpfGisLearning.Services;

public sealed class NominatimReverseGeocodingService : IReverseGeocodingService
{
    private static readonly HttpClient HttpClient = CreateHttpClient();

    public async Task<string?> GetAddressAsync(double latitude, double longitude)
    {
        if (!MapCoordinateValidator.IsValid(latitude, longitude))
        {
            return null;
        }

        var url = $"https://nominatim.openstreetmap.org/reverse?lat={latitude.ToString(CultureInfo.InvariantCulture)}&lon={longitude.ToString(CultureInfo.InvariantCulture)}&format=jsonv2&accept-language=ja";
        var result = await HttpClient.GetFromJsonAsync<ReverseGeocodingResult>(url);
        return string.IsNullOrWhiteSpace(result?.DisplayName)
            ? null
            : result.DisplayName;
    }

    private static HttpClient CreateHttpClient()
    {
        var client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(10)
        };
        client.DefaultRequestHeaders.UserAgent.ParseAdd("WpfGisLearning/1.0");
        return client;
    }

    private sealed class ReverseGeocodingResult
    {
        [JsonPropertyName("display_name")]
        public string? DisplayName { get; set; }
    }
}
