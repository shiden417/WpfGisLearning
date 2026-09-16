using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using WpfGisLearning.Converters;

namespace WpfGisLearning.Tests;

[TestClass]
public class ConverterTests
{
    [TestMethod]
    public void PhotoCountVisibilityConverter_ZeroIsVisibleAndNonZeroIsCollapsed()
    {
        var converter = new PhotoCountVisibilityConverter();

        Assert.AreEqual(Visibility.Visible, converter.Convert(0, typeof(Visibility), null!, CultureInfo.InvariantCulture));
        Assert.AreEqual(Visibility.Collapsed, converter.Convert(1, typeof(Visibility), null!, CultureInfo.InvariantCulture));
        Assert.AreEqual(Visibility.Collapsed, converter.Convert("0", typeof(Visibility), null!, CultureInfo.InvariantCulture));
    }

    [TestMethod]
    public void Converters_ConvertBackReturnsDoNothing()
    {
        var culture = CultureInfo.InvariantCulture;
        var photoConverter = new PhotoCountVisibilityConverter();
        var imageConverter = new ImagePathConverter();

        Assert.AreSame(Binding.DoNothing, photoConverter.ConvertBack(Visibility.Visible, typeof(int), null!, culture));
        Assert.AreSame(Binding.DoNothing, imageConverter.ConvertBack(null!, typeof(BitmapImage), null!, culture));
    }

    [TestMethod]
    public void ImagePathConverter_MissingPathReturnsNull()
    {
        var converter = new ImagePathConverter();

        var result = converter.Convert(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "missing.png"), typeof(BitmapImage), null!, CultureInfo.InvariantCulture);

        Assert.IsNull(result);
    }
}
