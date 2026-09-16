using WpfGisLearning.Models;
using WpfGisLearning.Services.Interfaces;

namespace WpfGisLearning.Services;

/// <summary>
/// Windowsの位置情報APIから現在地を取得するプロバイダーです。
/// OS固有の処理をIGeolocationProviderの実装として分離しています。
/// </summary>
public sealed class WindowsGeolocationProvider : IGeolocationProvider
{
    /// <summary>Windowsへ要求する位置情報の希望精度（メートル）です。</summary>
    private const uint DesiredAccuracyMeters = 50;

    /// <summary>
    /// WindowsのGeolocatorを使って現在地を非同期取得します。
    /// 位置情報へのアクセスが許可されていない場合はnullを返します。
    /// </summary>
    public async Task<CurrentLocation?> GetCurrentLocationAsync()
    {
        // 最初にWindowsへ位置情報アクセスの許可を確認する。
        var access = await Windows.Devices.Geolocation.Geolocator.RequestAccessAsync();
        if (access != Windows.Devices.Geolocation.GeolocationAccessStatus.Allowed)
            return null;

        // 必要な精度を指定して位置情報取得オブジェクトを生成する。
        var geolocator = new Windows.Devices.Geolocation.Geolocator
        {
            DesiredAccuracyInMeters = DesiredAccuracyMeters
        };
        var position = await geolocator.GetGeopositionAsync();

        // Windows APIの結果からアプリ独自のCurrentLocationモデルへ変換する。
        return new CurrentLocation(
            position.Coordinate.Point.Position.Latitude,
            position.Coordinate.Point.Position.Longitude);
    }
}
