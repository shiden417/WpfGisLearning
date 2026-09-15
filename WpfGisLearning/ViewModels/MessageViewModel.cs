using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading;
using System.Threading.Tasks;

namespace WpfGisLearning.ViewModels;

public partial class MessageViewModel : ObservableObject
{
    [ObservableProperty]
    private string message = "これはViewModelから表示しいます";

    [RelayCommand]
    private void ChangeMessage()
    {
        Message = "MessageViewのCommandが実行されました!";
    }

    // async/await 学習用サンプル
    [RelayCommand]
    private async Task RunAsyncTest()
    {
        Message = "処理中...";

        // 重い処理を模擬するため Task.Run を使用してバックグラウンドで待機
        await Task.Run(() => Thread.Sleep(3000));

        Message = "処理完了";
    }
}