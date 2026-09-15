using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WpfGisLearning.Models;
using WpfGisLearning.Services;

namespace WpfGisLearning.ViewModels;

public partial class DetailViewModel : ObservableObject
{
    private readonly IShopService _shopService;

    public int Id { get; }
    public string ServiceInstanceId { get; }
    public Shop? Shop { get; private set; }

    [ObservableProperty]
    private bool isFavorite;

    public DetailViewModel(int id, IShopService shopService)
    {
        Id = id;
        _shopService = shopService;
        ServiceInstanceId = shopService.GetHashCode().ToString();
        Shop = shopService.GetShops().FirstOrDefault(shop => shop.Id == id);
        IsFavorite = Shop?.IsFavorite ?? false;
    }

    [RelayCommand]
    private void ToggleFavorite()
    {
        if (Shop is null)
            return;

        _shopService.ToggleFavorite(Shop.Id);
        IsFavorite = Shop.IsFavorite;
    }
}
