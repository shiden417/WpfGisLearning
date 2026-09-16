using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfGisLearning.Utilities;

public static class RameniaIconFactory
{
    private const int IconSize = 64;
    private const int IconFontSize = 48;

    public static ImageSource Create()
    {
        var visual = new DrawingVisual();
        using (var context = visual.RenderOpen())
        {
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

        var bitmap = new RenderTargetBitmap(IconSize, IconSize, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(visual);
        bitmap.Freeze();
        return bitmap;
    }
}
