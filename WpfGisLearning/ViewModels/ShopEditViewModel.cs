using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WpfGisLearning.Models;
using WpfGisLearning.Services;

namespace WpfGisLearning.ViewModels;

public partial class ShopEditViewModel : ObservableObject
{
    private readonly IShopService _shopService;
    private readonly bool _isEdit;

    public int? ShopId { get; }
    public string ScreenTitle => _isEdit ? "店舗を編集" : "店舗を登録";

    [ObservableProperty]
    private string shopName = string.Empty;

    [ObservableProperty]
    private decimal shopPrice;

    [ObservableProperty]
    private string shopAddress = string.Empty;

    [ObservableProperty]
    private double? shopLatitude;

    [ObservableProperty]
    private double? shopLongitude;

    public ShopEditViewModel(IShopService shopService, int? shopId = null)
    {
        _shopService = shopService;
        ShopId = shopId;
        _isEdit = shopId.HasValue;

        if (shopId.HasValue)
        {
            var shop = _shopService.GetShops().FirstOrDefault(x => x.Id == shopId.Value);
            if (shop is not null)
            {
                ShopName = shop.Name;
                ShopPrice = shop.Price;
                ShopAddress = shop.Address;
                ShopLatitude = shop.Latitude;
                ShopLongitude = shop.Longitude;
            }
        }
    }

    public bool TrySetLocation(double latitude, double longitude)
    {
        if (latitude < -90 || latitude > 90 || longitude < -180 || longitude > 180)
            return false;

        ShopLatitude = latitude;
        ShopLongitude = longitude;
        return true;
    }

    [RelayCommand]
    private void Save()
    {
        if (string.IsNullOrWhiteSpace(ShopName) ||
            ShopPrice <= 0 ||
            !ShopLatitude.HasValue ||
            !ShopLongitude.HasValue)
        {
            return;
        }

        if (_isEdit && ShopId.HasValue)
        {
            _shopService.UpdateShop(new Shop
            {
                Id = ShopId.Value,
                Name = ShopName.Trim(),
                Price = ShopPrice,
                Address = ShopAddress.Trim(),
                Latitude = ShopLatitude.Value,
                Longitude = ShopLongitude.Value
            });
        }
        else
        {
            var nextId = _shopService.GetShops().Any()
                ? _shopService.GetShops().Max(x => x.Id) + 1
                : 1;

            _shopService.AddShop(new Shop
            {
                Id = nextId,
                Name = ShopName.Trim(),
                Price = ShopPrice,
                Address = ShopAddress.Trim(),
                Latitude = ShopLatitude.Value,
                Longitude = ShopLongitude.Value
            });
        }

        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void Cancel()
    {
        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? RequestClose;
}
