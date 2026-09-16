using WpfGisLearning.Models;
using WpfGisLearning.Services.Interfaces;

namespace WpfGisLearning.Services;

/// <summary>
/// 現在地取得のアプリケーション側サービスです。
/// 低レベルの位置情報取得処理と、アプリケーションで利用できる座標かの判定を分離します。
/// </summary>
public sealed class CurrentLocationService : ICurrentLocationService
{
    /// <summary>OSや端末から実際の位置情報を取得するプロバイダーです。</summary>
    private readonly IGeolocationProvider _provider;

    /// <summary>位置情報プロバイダーを受け取ります。</summary>
    /// <param name="provider">位置情報取得処理を担当するプロバイダーです。</param>
    public CurrentLocationService(IGeolocationProvider provider)
    {
        _provider = provider;
    }

    /// <summary>
    /// 現在地を取得し、緯度・経度が有効な場合だけ呼び出し側へ返します。
    /// </summary>
    public async Task<CurrentLocation?> GetCurrentLocationAsync()
    {
        var location = await _provider.GetCurrentLocationAsync();
        if (location is null)
            return null;

        // OSから値が返ってきても、地理座標として不正ならアプリ側では利用しない。
        return MapCoordinateValidator.IsValid(location.Latitude, location.Longitude)
            ? location
            : null;
    }
}
