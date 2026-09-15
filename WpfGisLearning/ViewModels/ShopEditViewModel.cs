using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WpfGisLearning.Models;
using WpfGisLearning.Services;

namespace WpfGisLearning.ViewModels;

public partial class ShopEditViewModel : ObservableObject
{
    private readonly IShopService _shopService;
    private bool _isEdit;

    public int? ShopId { get; private set; }
    public string ScreenTitle => _isEdit ? "店舗を編集" : "店舗を登録";
    public string[] RamenTypes { get; } = ["醤油", "塩", "味噌", "豚骨", "家系", "二郎系", "つけ麺", "その他"];

    [ObservableProperty] private string shopName = string.Empty;
    [ObservableProperty] private decimal shopPrice;
    [ObservableProperty] private string shopAddress = string.Empty;
    [ObservableProperty] private double? shopLatitude;
    [ObservableProperty] private double? shopLongitude;
    [ObservableProperty] private string ramenType = "醤油";
    [ObservableProperty] private string openingHours = string.Empty;
    [ObservableProperty] private string closedDay = string.Empty;
    [ObservableProperty] private double rating;
    [ObservableProperty] private string errorMessage = string.Empty;
    [ObservableProperty] private string shopNameError = string.Empty;
    [ObservableProperty] private string shopPriceError = string.Empty;
    [ObservableProperty] private string locationError = string.Empty;

    public ShopEditViewModel(IShopService shopService) => _shopService = shopService;

    public void Load(int? shopId)
    {
        ShopId = shopId;
        _isEdit = shopId.HasValue;
        ClearErrors();
        OnPropertyChanged(nameof(ScreenTitle));

        if (!shopId.HasValue)
        {
            ShopLatitude = null;
            ShopLongitude = null;
            return;
        }

        var shop = _shopService.GetShops().FirstOrDefault(x => x.Id == shopId.Value);
        if (shop is null)
        {
            ErrorMessage = "編集対象の店舗が見つかりません。";
            return;
        }

        ShopName = shop.Name;
        ShopPrice = shop.Price;
        ShopAddress = shop.Address;
        ShopLatitude = shop.Latitude;
        ShopLongitude = shop.Longitude;
        RamenType = shop.RamenType;
        OpeningHours = shop.OpeningHours;
        ClosedDay = shop.ClosedDay;
        Rating = shop.Rating;
    }

    public bool TrySetLocation(double latitude, double longitude)
    {
        if (latitude < -90 || latitude > 90 || longitude < -180 || longitude > 180)
            return false;
        ShopLatitude = latitude;
        ShopLongitude = longitude;
        LocationError = string.Empty;
        ErrorMessage = string.Empty;
        return true;
    }

    [RelayCommand]
    private void Save()
    {
        ClearErrors();

        if (string.IsNullOrWhiteSpace(ShopName))
            ShopNameError = "店舗名を入力してください。";

        if (ShopPrice <= 0)
            ShopPriceError = "価格は1円以上で入力してください。";

        if (!ShopLatitude.HasValue || !ShopLongitude.HasValue)
            LocationError = "地図上で店舗位置を指定してください。ダブルクリックで設定できます。";

        if (!string.IsNullOrEmpty(ShopNameError) || !string.IsNullOrEmpty(ShopPriceError) || !string.IsNullOrEmpty(LocationError))
            return;

        var existing = ShopId.HasValue ? _shopService.GetShops().FirstOrDefault(x => x.Id == ShopId.Value) : null;
        if (ShopId.HasValue && existing is null)
        {
            ErrorMessage = "編集対象の店舗が見つかりません。画面を閉じてもう一度お試しください。";
            return;
        }

        var shop = new Shop
        {
            Id = ShopId ?? (_shopService.GetShops().Any() ? _shopService.GetShops().Max(x => x.Id) + 1 : 1),
            Name = ShopName.Trim(),
            Price = ShopPrice,
            Address = ShopAddress.Trim(),
            Latitude = ShopLatitude.Value,
            Longitude = ShopLongitude.Value,
            RamenType = RamenType,
            OpeningHours = OpeningHours.Trim(),
            ClosedDay = ClosedDay.Trim(),
            Rating = Math.Clamp(Rating, 0, 5),
            IsFavorite = existing?.IsFavorite ?? false
        };

        try
        {
            if (_isEdit)
                _shopService.UpdateShop(shop);
            else
                _shopService.AddShop(shop);

            RequestClose?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"店舗を保存できませんでした。\n{ex.Message}";
        }
    }

    private void ClearErrors()
    {
        ErrorMessage = string.Empty;
        ShopNameError = string.Empty;
        ShopPriceError = string.Empty;
        LocationError = string.Empty;
    }

    [RelayCommand]
    private void Cancel() => RequestClose?.Invoke(this, EventArgs.Empty);

    public event EventHandler? RequestClose;
}
