using Mapsui;
using Mapsui.Layers;
using Mapsui.UI.Wpf;

namespace WpfGisLearning.Map;

/// <summary>
/// Mapsui.WPFのMapControlをIMapControlAdapterとして公開する実装です。
/// WPF依存をこのクラスに閉じ込め、地図ロジックのテストをしやすくします。
/// </summary>
public sealed class MapControlAdapter : IMapControlAdapter
{
    /// <summary>実際のWPF MapControlです。</summary>
    private readonly MapControl _mapControl;

    /// <summary>
    /// MapControlを受け取り、アダプターとして保持します。
    /// </summary>
    /// <param name="mapControl">操作対象のWPF MapControlです。</param>
    public MapControlAdapter(MapControl mapControl)
    {
        _mapControl = mapControl;
    }

    /// <summary>MapControlに設定されているMapsuiの地図です。</summary>
    public Mapsui.Map? Map
    {
        get => _mapControl.Map;
        set
        {
            if (value is not null)
                _mapControl.Map = value;
        }
    }

    /// <summary>MapControlの現在の表示幅です。</summary>
    public double ActualWidth => _mapControl.ActualWidth;

    /// <summary>MapControlの現在の表示高さです。</summary>
    public double ActualHeight => _mapControl.ActualHeight;

    /// <summary>MapControlに地図の再描画を要求します。</summary>
    public void Refresh() => _mapControl.Refresh();

    /// <summary>指定した画面位置にある地図要素を取得します。</summary>
    public MapInfo? GetMapInfo(Mapsui.Manipulations.ScreenPosition position, IEnumerable<ILayer> layers) =>
        _mapControl.GetMapInfo(position, layers);
}
