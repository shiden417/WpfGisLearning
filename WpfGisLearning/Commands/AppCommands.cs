using System.Windows.Input;

namespace WpfGisLearning.Commands;

public static class AppCommands
{
    // 学習用の Save コマンド。Ctrl+S をジェスチャとして持つ。
    public static readonly RoutedUICommand SaveCommand = new(
        "Save",
        "Save",
        typeof(AppCommands),
        new InputGestureCollection { new KeyGesture(Key.S, ModifierKeys.Control) }
    );
}
