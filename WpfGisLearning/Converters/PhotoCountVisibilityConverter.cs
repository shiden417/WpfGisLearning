using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WpfGisLearning.Converters;

/// <summary>
/// 写真件数に応じてWPF要素の表示・非表示を切り替える値コンバーターです。
/// XAMLのBindingで「写真が0件なら表示」のようなUI条件を記述するために使用します。
/// </summary>
public sealed class PhotoCountVisibilityConverter : IValueConverter
{
    /// <summary>写真件数をVisibilityへ変換します。</summary>
    /// <param name="value">Binding元から渡される写真件数です。</param>
    /// <param name="targetType">Binding先で期待される型です。</param>
    /// <param name="parameter">XAMLから渡される任意のパラメーターです。</param>
    /// <param name="culture">変換時のカルチャです。</param>
    /// <returns>件数が0ならVisible、それ以外ならCollapsedです。</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is int count && count == 0 ? Visibility.Visible : Visibility.Collapsed;

    /// <summary>Visibilityから写真件数への逆変換は行わないため、Bindingの変更を無視します。</summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;
}
