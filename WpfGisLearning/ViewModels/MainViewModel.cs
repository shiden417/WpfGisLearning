using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WpfGisLearning.Services.Interfaces;

namespace WpfGisLearning.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;
    private readonly IShopService _shopService;

    public MainViewModel(INavigationService navigationService, IShopService shopService)
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

    [RelayCommand]
    private void UseShop()
    {
        Message = _shopService.GetWelcomeMessage();
    }
}
