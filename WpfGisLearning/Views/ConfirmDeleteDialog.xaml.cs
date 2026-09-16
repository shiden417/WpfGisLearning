using System.Windows;

namespace WpfGisLearning.Views
{
    /// <summary>
    /// 店舗削除の確認を行うためのモーダルダイアログです。
    /// </summary>
    public partial class ConfirmDeleteDialog : Window
    {
        /// <summary>ダイアログを生成してXAMLで定義されたUIを初期化します。</summary>
        public ConfirmDeleteDialog()
        {
            InitializeComponent();
        }

        /// <summary>削除を確定し、呼び出し元へtrueを返してダイアログを閉じます。</summary>
        private void Yes_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        /// <summary>削除をキャンセルし、呼び出し元へfalseを返してダイアログを閉じます。</summary>
        private void No_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
