using System.Windows.Input;

namespace WpfGisLearning.Commands;

/// <summary>
/// アプリケーション全体で利用するRoutedCommandを定義するクラスです。
/// WPFのコマンドは、処理そのものとキーボード操作などの入力を分離できます。
/// </summary>
public static class AppCommands
{
    /// <summary>
    /// 保存を表すRoutedUICommandです。
    /// Ctrl+Sを入力ジェスチャとして登録し、CommandBinding側で実際の保存処理を接続します。
    /// </summary>
    public static readonly RoutedUICommand SaveCommand = new(
        "Save",
        "Save",
        typeof(AppCommands),
        new InputGestureCollection { new KeyGesture(Key.S, ModifierKeys.Control) }
    );
}
