using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using WpfGisLearning.Services.Interfaces;

namespace WpfGisLearning.Services;

/// <summary>
/// NominatimのReverse Geocoding APIを利用して、緯度・経度を住所へ変換するサービスです。
/// </summary>
public sealed class NominatimReverseGeocodingService : IReverseGeocodingService
{
    /// <summary>Nominatim APIとのHTTP通信に使用するクライアントです。</summary>
    private readonly HttpClient _httpClient;

    /// <summary>既定設定のHTTPクライアントを使ってサービスを生成します。</summary>
    public NominatimReverseGeocodingService() : this(CreateHttpClient())
    {
    }

    /// <summary>テストやDIで差し替え可能なHTTPクライアントを受け取ります。</summary>
    /// <param name="httpClient">Nominatimとの通信に使用するHTTPクライアントです。</param>
    public NominatimReverseGeocodingService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// 指定座標をNominatimへ問い合わせ、取得した表示用住所を返します。
    /// </summary>
    /// <param name="latitude">対象地点の緯度です。</param>
    /// <param name="longitude">対象地点の経度です。</param>
    /// <returns>住所文字列。入力が不正、または住所が取得できない場合はnullです。</returns>
    public async Task<string?> GetAddressAsync(double latitude, double longitude)
    {
        if (!MapCoordinateValidator.IsValid(latitude, longitude))
            return null;

        // 小数点記号をOSのロケールに依存させないためInvariantCultureでURLへ埋め込む。
        var url = $"https://nominatim.openstreetmap.org/reverse?lat={latitude.ToString(CultureInfo.InvariantCulture)}&lon={longitude.ToString(CultureInfo.InvariantCulture)}&format=jsonv2&accept-language=ja";
        var result = await _httpClient.GetFromJsonAsync<ReverseGeocodingResult>(url);
        return string.IsNullOrWhiteSpace(result?.DisplayName) ? null : result.DisplayName;
    }

    /// <summary>Nominatimへ送るタイムアウトとUser-Agentを設定したHTTPクライアントを生成します。</summary>
    private static HttpClient CreateHttpClient()
    {
        var client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        client.DefaultRequestHeaders.UserAgent.ParseAdd("WpfGisLearning/1.0");
        return client;
    }

    /// <summary>Nominatim JSONレスポンスのうち、表示用住所を受け取るための内部DTOです。</summary>
    private sealed class ReverseGeocodingResult
    {
        /// <summary>Nominatimのdisplay_nameプロパティを住所として保持します。</summary>
        [JsonPropertyName("display_name")]
        public string? DisplayName { get; set; }
    }
}
