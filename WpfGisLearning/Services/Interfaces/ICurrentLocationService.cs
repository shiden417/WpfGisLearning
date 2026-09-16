using WpfGisLearning.Models;

namespace WpfGisLearning.Services.Interfaces;

/// <summary>
/// アプリケーション側から現在地を取得するサービスの契約です。
/// 実際のWindows APIへのアクセスは実装クラスに隠蔽します。
/// </summary>
public interface ICurrentLocationService
{
    /// <summary>現在地を非同期で取得します。取得できない場合はnullを返します。</summary>
    Task<CurrentLocation?> GetCurrentLocationAsync();
}
