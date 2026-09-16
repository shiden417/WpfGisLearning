using Mapsui;
using Mapsui.Layers;
using Mapsui.UI.Wpf;

namespace WpfGisLearning.Map;

/// <summary>
/// MapsuiのWPF MapControlへアクセスするための抽象化です。
/// MapControllerが具体的なWPFコントロールへ直接依存しないようにするために使用します。
/// </summary>
public interface IMapControlAdapter
{
    /// <summary>地図本体を取得または設定します。</summary>
    Mapsui.Map? Map { get; set; }

    /// <summary>地図コントロールの実際の表示幅です。</summary>
    double ActualWidth { get; }

    /// <summary>地図コントロールの実際の表示高さです。</summary>
    double ActualHeight { get; }

    /// <summary>地図の再描画を要求します。</summary>
    void Refresh();

    /// <summary>
    /// 指定した画面座標から、指定レイヤー上のFeature情報を取得します。
    /// </summary>
    /// <param name="position">クリック位置などの画面座標です。</param>
    /// <param name="layers">ヒット判定の対象にするレイヤー一覧です。</param>
    /// <returns>該当する地図情報があればそのMapInfoです。</returns>
    MapInfo? GetMapInfo(Mapsui.Manipulations.ScreenPosition position, IEnumerable<ILayer> layers);
}
