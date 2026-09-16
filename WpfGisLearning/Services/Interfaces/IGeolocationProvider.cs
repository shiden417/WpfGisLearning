using WpfGisLearning.Models;

namespace WpfGisLearning.Services.Interfaces;

/// <summary>
/// OSや端末から位置情報を取得する低レベル側の契約です。
/// 現在地サービスとWindows APIを分離するために使用します。
/// </summary>
public interface IGeolocationProvider
{
    /// <summary>OSから現在地を非同期で取得します。</summary>
    Task<CurrentLocation?> GetCurrentLocationAsync();
}
