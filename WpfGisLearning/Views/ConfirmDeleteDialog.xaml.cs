using System.Windows;

namespace WpfGisLearning.Views
{
    public partial class ConfirmDeleteDialog : Window
    {
        public ConfirmDeleteDialog()
        {
            InitializeComponent();
        }

        private void Yes_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void No_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
