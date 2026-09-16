namespace WpfGisLearning.Models;

/// <summary>
/// 現在地として取得した緯度・経度をまとめて扱う不変データです。
/// record を使うことで、位置情報のような単純な値オブジェクトを表現しています。
/// </summary>
/// <param name="Latitude">緯度です。</param>
/// <param name="Longitude">経度です。</param>
public sealed record CurrentLocation(double Latitude, double Longitude);
