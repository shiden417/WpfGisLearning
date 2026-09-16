using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace WpfGisLearning.Converters;

/// <summary>
/// 画像ファイルのパスをWPFのImageSourceへ変換する値コンバーターです。
/// BitmapImageをOnLoadで読み込むことで、ファイルストリームを閉じた後も画像を表示できます。
/// </summary>
public sealed class ImagePathConverter : IValueConverter
{
    /// <summary>
    /// 画像ファイルのパスをBitmapImageへ変換します。
    /// </summary>
    /// <param name="value">画像ファイルのパスとして使用する文字列です。</param>
    /// <param name="targetType">Binding先で期待される型です。</param>
    /// <param name="parameter">XAMLから渡される任意のパラメーターです。</param>
    /// <param name="culture">変換時のカルチャです。</param>
    /// <returns>読み込めた場合はFreeze済みのBitmapImage、読み込めない場合はnullです。</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string path || string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return null!;
        }

        // OnLoadを指定すると、BitmapImageがストリームの内容をメモリへ読み込んでからストリームを閉じられる。
        using var stream = File.OpenRead(path);
        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.StreamSource = stream;
        bitmap.EndInit();

        // Freezeすると変更不可の共有可能なオブジェクトになり、WPFのBindingで扱いやすくなる。
        bitmap.Freeze();
        return bitmap;
    }

    /// <summary>BitmapImageからファイルパスへの逆変換は行わないため、Bindingの変更を無視します。</summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;
}
