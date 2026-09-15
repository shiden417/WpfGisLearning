using System.Windows.Controls;
using WpfGisLearning.ViewModels;

namespace WpfGisLearning.Views
{
    /// <summary>
    /// MessageView.xaml の相互作用ロジック
    /// </summary>
    public partial class MessageView : UserControl
    {
        public MessageView(MessageViewModel viewModel)
        {
            InitializeComponent();

            this.DataContext = viewModel;
        }
    }
}