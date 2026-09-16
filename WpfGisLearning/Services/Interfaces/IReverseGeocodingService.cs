namespace WpfGisLearning.Services.Interfaces;

/// <summary>
/// 緯度・経度から人が読める住所へ変換する逆ジオコーディングサービスの契約です。
/// </summary>
public interface IReverseGeocodingService
{
    /// <summary>指定した座標の住所を非同期で取得します。</summary>
    /// <param name="latitude">対象地点の緯度です。</param>
    /// <param name="longitude">対象地点の経度です。</param>
    /// <returns>取得した住所。取得できない場合はnullです。</returns>
    Task<string?> GetAddressAsync(double latitude, double longitude);
}
