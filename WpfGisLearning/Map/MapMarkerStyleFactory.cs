using Mapsui.Styles;

namespace WpfGisLearning.Map;

/// <summary>
/// Mapsuiで使用する店舗・現在地マーカーのスタイルを生成するファクトリです。
/// 色や大きさを1か所に集約することで、画面ごとのスタイル差異を防ぎます。
/// </summary>
public static class MapMarkerStyleFactory
{
    /// <summary>通常の店舗マーカーに使用する色です。</summary>
    private const string NormalShopColor = "#C74B4B";

    /// <summary>選択中の店舗マーカーに使用する色です。</summary>
    private const string SelectedShopColor = "#C56B4D";

    /// <summary>現在地マーカーに使用する色です。</summary>
    private const string CurrentLocationColor = "#4A90E2";

    /// <summary>通常の店舗マーカーの表示倍率です。</summary>
    private const double NormalShopScale = 1.15;

    /// <summary>選択中の店舗マーカーの表示倍率です。</summary>
    private const double SelectedShopScale = 1.4;

    /// <summary>現在地マーカーの表示倍率です。</summary>
    private const double CurrentLocationScale = 0.65;

    /// <summary>
    /// 選択状態に応じた店舗ピンのImageStyleを生成します。
    /// </summary>
    /// <param name="selected">選択中なら強調表示用の色・倍率を使用します。</param>
    /// <returns>店舗表示用のピンスタイルです。</returns>
    public static ImageStyle CreateShopMarker(bool selected)
    {
        return ImageStyles.CreatePinStyle(
            Color.FromString(selected ? SelectedShopColor : NormalShopColor),
            Color.White,
            selected ? SelectedShopScale : NormalShopScale);
    }

    /// <summary>
    /// 現在地を表す円形マーカーのSymbolStyleを生成します。
    /// </summary>
    /// <returns>現在地表示用のシンボルスタイルです。</returns>
    public static SymbolStyle CreateCurrentLocationMarker()
    {
        return new SymbolStyle
        {
            SymbolType = SymbolType.Ellipse,
            SymbolScale = CurrentLocationScale,
            Fill = new Brush(Color.FromString(CurrentLocationColor)),
            Outline = new Pen
            {
                Color = Color.White,
                Width = 3
            }
        };
    }
}
