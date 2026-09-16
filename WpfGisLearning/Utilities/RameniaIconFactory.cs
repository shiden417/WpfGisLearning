using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfGisLearning.Utilities;

/// <summary>
/// WPFのDrawingVisualを使ってアプリケーションアイコンを生成するヘルパーです。
/// 外部画像ファイルを用意せず、実行時に絵文字からアイコンを生成します。
/// </summary>
public static class RameniaIconFactory
{
    /// <summary>生成するアイコン画像の縦横サイズです。</summary>
    private const int IconSize = 64;

    /// <summary>アイコン内に描画する絵文字のフォントサイズです。</summary>
    private const int IconFontSize = 48;

    /// <summary>
    /// ラーメン絵文字を描画したImageSourceを生成します。
    /// RenderTargetBitmapへ描画してFreezeすることで、Window.Iconなどへ設定できます。
    /// </summary>
    public static ImageSource Create()
    {
        var visual = new DrawingVisual();
        using (var context = visual.RenderOpen())
        {
            // WPF標準のSegoe UI Emojiでラーメン絵文字を描画する。
            var formattedText = new FormattedText(
                "🍜",
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(new FontFamily("Segoe UI Emoji"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
                IconFontSize,
                Brushes.Black,
                1.0);

            context.DrawText(
                formattedText,
                new Point((IconSize - formattedText.Width) / 2, (IconSize - formattedText.Height) / 2));
        }

        // DrawingVisualの内容をビットマップへ変換し、WPFで共有できる画像にする。
        var bitmap = new RenderTargetBitmap(IconSize, IconSize, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(visual);
        bitmap.Freeze();
        return bitmap;
    }
}
