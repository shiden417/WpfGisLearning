namespace WpfGisLearning.Map;

/// <summary>
/// 緯度・経度が地理座標として利用可能かを検証するヘルパーです。
/// 地図表示や逆ジオコーディングの前に共通して利用します。
/// </summary>
public static class MapCoordinateValidator
{
    /// <summary>
    /// 緯度・経度が数値として有限で、通常の地理座標の範囲内にあるかを判定します。
    /// (0, 0) は未設定値として扱うため無効とします。
    /// </summary>
    /// <param name="latitude">緯度です。</param>
    /// <param name="longitude">経度です。</param>
    /// <returns>利用可能な座標なら true、それ以外なら false です。</returns>
    public static bool IsValid(double latitude, double longitude)
    {
        return !double.IsNaN(latitude)
            && !double.IsNaN(longitude)
            && !double.IsInfinity(latitude)
            && !double.IsInfinity(longitude)
            && latitude >= -90
            && latitude <= 90
            && longitude >= -180
            && longitude <= 180
            && !(latitude == 0 && longitude == 0);
    }
}
