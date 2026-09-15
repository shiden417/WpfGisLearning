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
    [ObservableProperty] private string tags = string.Empty;
    [ObservableProperty] private string recommendedMenu = string.Empty;
    [ObservableProperty] private string openingHours = string.Empty;
    [ObservableProperty] private string closedDay = string.Empty;
    [ObservableProperty] private double rating;

    public ShopEditViewModel(IShopService shopService) => _shopService = shopService;

    public void Load(int? shopId)
    {
        ShopId = shopId;
        _isEdit = shopId.HasValue;
        OnPropertyChanged(nameof(ScreenTitle));

        if (!shopId.HasValue)
        {
            // 新規登録は日本全体を見渡せる初期地図の中央付近にマーカーを置く。
            ShopLatitude = 36.0;
            ShopLongitude = 138.0;
            return;
        }

        var shop = _shopService.GetShops().FirstOrDefault(x => x.Id == shopId.Value);
        if (shop is null)
            return;

        ShopName = shop.Name;
        ShopPrice = shop.Price;
        ShopAddress = shop.Address;
        ShopLatitude = shop.Latitude;
        ShopLongitude = shop.Longitude;
        RamenType = shop.RamenType;
        Tags = shop.Tags;
        RecommendedMenu = shop.RecommendedMenu;
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
        return true;
    }

    [RelayCommand]
    private void Save()
    {
        if (string.IsNullOrWhiteSpace(ShopName) || ShopPrice <= 0 || !ShopLatitude.HasValue || !ShopLongitude.HasValue)
            return;

        var existing = ShopId.HasValue ? _shopService.GetShops().FirstOrDefault(x => x.Id == ShopId.Value) : null;
        var shop = new Shop
        {
            Id = ShopId ?? (_shopService.GetShops().Any() ? _shopService.GetShops().Max(x => x.Id) + 1 : 1),
            Name = ShopName.Trim(),
            Price = ShopPrice,
            Address = ShopAddress.Trim(),
            Latitude = ShopLatitude.Value,
            Longitude = ShopLongitude.Value,
            RamenType = RamenType,
            Tags = Tags.Trim(),
            RecommendedMenu = RecommendedMenu.Trim(),
            OpeningHours = OpeningHours.Trim(),
            ClosedDay = ClosedDay.Trim(),
            Rating = Math.Clamp(Rating, 0, 5),
            IsFavorite = existing?.IsFavorite ?? false
        };

        if (_isEdit)
            _shopService.UpdateShop(shop);
        else
            _shopService.AddShop(shop);

        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void Cancel() => RequestClose?.Invoke(this, EventArgs.Empty);

    public event EventHandler? RequestClose;
}
