using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace WpfGisLearning.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly Services.INavigationService _navigationService;
    private readonly Services.IShopService _shopService;

    public MainViewModel(Services.INavigationService navigationService, Services.IShopService shopService)
    {
        _navigationService = navigationService;
        _shopService = shopService;
    }

    [ObservableProperty]
    private string message = "Hello WPF!";

    [RelayCommand]
    private void ChangeMessage()
    {
        Message = "ボタンが押されました！";
    }

    // MainWindow のボタンから呼び出すコマンド。ナビゲーションは ViewModel の責務とする。
    [RelayCommand]
    private void OpenDetail()
    {
        _navigationService.NavigateToDetail(42);
    }

    [RelayCommand]
    private void ShowShopList()
    {
        _navigationService.NavigateToShopList();
    }

    [RelayCommand]
    private void NavigateToShopPageFrame()
    {
        _navigationService.NavigateToShopPageFrame();
    }

    // DI経由のサービスを使うサンプルコマンド
    [RelayCommand]
    private void UseShop()
    {
        Message = _shopService.GetWelcomeMessage();
    }
}
