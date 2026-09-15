using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using WpfGisLearning.Models;
using WpfGisLearning.Services;

namespace WpfGisLearning.ViewModels;

public partial class ShopListViewModel : ObservableObject
{
    public ObservableCollection<Shop> Shops { get; } = new();
    public ICollectionView ShopsView { get; }

    private readonly IShopService _shopService;
    private readonly INavigationService _navigationService;
    private double? _nearbyLatitude;
    private double? _nearbyLongitude;

    [ObservableProperty] private string searchKeyword = string.Empty;
    [ObservableProperty] private string selectedSort = "おすすめ";
    [ObservableProperty] private string selectedRamenType = "すべて";
    [ObservableProperty] private string selectedPriceFilter = "すべて";
    [ObservableProperty] private string selectedNearbyRadius = "5km";
    [ObservableProperty] private bool favoriteOnly;
    [ObservableProperty] private bool nearbyOnly;
    [ObservableProperty] private Shop? selectedShop;

    public string[] SortOptions { get; } = [
        "おすすめ", "店舗名: A → Z", "価格: 安い順", "価格: 高い順", "評価: 高い順"
    ];

    public string[] RamenTypeOptions { get; } = [
        "すべて", "醤油", "塩", "味噌", "豚骨", "家系", "二郎系", "つけ麺", "その他"
    ];

    public string[] PriceFilterOptions { get; } = [
        "すべて", "1000円以下", "1500円以下", "2000円以下"
    ];

    public string[] NearbyRadiusOptions { get; } = ["1km", "3km", "5km", "10km", "20km"];

    public event EventHandler<Shop?>? SelectedShopChanged;
    public event EventHandler? ShopsChanged;

    public ShopListViewModel(IShopService shopService, INavigationService navigationService)
    {
        _shopService = shopService;
        _navigationService = navigationService;
        ReloadShops();
        ShopsView = CollectionViewSource.GetDefaultView(Shops);
        ShopsView.Filter = FilterShop;
    }

    partial void OnSearchKeywordChanged(string value) => ShopsView.Refresh();
    partial void OnSelectedRamenTypeChanged(string value) => ShopsView.Refresh();
    partial void OnSelectedPriceFilterChanged(string value) => ShopsView.Refresh();
    partial void OnSelectedNearbyRadiusChanged(string value) => ShopsView.Refresh();
    partial void OnFavoriteOnlyChanged(bool value) => ShopsView.Refresh();
    partial void OnNearbyOnlyChanged(bool value) => ShopsView.Refresh();

    partial void OnSelectedSortChanged(string value)
    {
        ShopsView.SortDescriptions.Clear();
        switch (value)
        {
            case "店舗名: A → Z":
                ShopsView.SortDescriptions.Add(new SortDescription(nameof(Shop.Name), ListSortDirection.Ascending));
                break;
            case "価格: 安い順":
                ShopsView.SortDescriptions.Add(new SortDescription(nameof(Shop.Price), ListSortDirection.Ascending));
                break;
            case "価格: 高い順":
                ShopsView.SortDescriptions.Add(new SortDescription(nameof(Shop.Price), ListSortDirection.Descending));
                break;
            case "評価: 高い順":
                ShopsView.SortDescriptions.Add(new SortDescription(nameof(Shop.Rating), ListSortDirection.Descending));
                break;
        }
        ShopsView.Refresh();
    }

    private bool FilterShop(object item)
    {
        if (item is not Shop shop)
            return false;

        var keyword = SearchKeyword.Trim();
        if (!string.IsNullOrWhiteSpace(keyword) &&
            !shop.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) &&
            !shop.Address.Contains(keyword, StringComparison.OrdinalIgnoreCase) &&
            !shop.RamenType.Contains(keyword, StringComparison.OrdinalIgnoreCase) &&
            !shop.RecommendedMenu.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            return false;

        if (SelectedRamenType != "すべて" && shop.RamenType != SelectedRamenType)
            return false;

        if (FavoriteOnly && !shop.IsFavorite)
            return false;

        if (SelectedPriceFilter != "すべて")
        {
            var maxPrice = SelectedPriceFilter switch
            {
                "1000円以下" => 1000m,
                "1500円以下" => 1500m,
                "2000円以下" => 2000m,
                _ => decimal.MaxValue
            };
            if (shop.Price > maxPrice)
                return false;
        }

        if (NearbyOnly)
        {
            if (!_nearbyLatitude.HasValue || !_nearbyLongitude.HasValue)
                return false;

            var radiusKm = double.Parse(SelectedNearbyRadius.Replace("km", ""));
            if (DistanceKm(_nearbyLatitude.Value, _nearbyLongitude.Value, shop.Latitude, shop.Longitude) > radiusKm)
                return false;
        }

        return true;
    }

    private static double DistanceKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double earthRadiusKm = 6371.0;
        var dLat = DegreesToRadians(lat2 - lat1);
        var dLon = DegreesToRadians(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return earthRadiusKm * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180.0;

    private void ReloadShops()
    {
        Shops.Clear();
        foreach (var shop in _shopService.GetShops())
            Shops.Add(shop);
    }

    [RelayCommand]
    private void NewShop()
    {
        _navigationService.NavigateToShopEdit();
        RefreshFromService();
    }

    [RelayCommand]
    private void OpenSelectedShop()
    {
        if (SelectedShop is not null)
            _navigationService.NavigateToDetail(SelectedShop.Id);
    }

    [RelayCommand]
    private void EditSelectedShop()
    {
        if (SelectedShop is null)
            return;

        _navigationService.NavigateToShopEdit(SelectedShop.Id);
        RefreshFromService();
    }

    [RelayCommand]
    private void ToggleFavorite(Shop? shop)
    {
        shop ??= SelectedShop;
        if (shop is null)
            return;

        _shopService.ToggleFavorite(shop.Id);
        ShopsView.Refresh();
        ShopsChanged?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void DeleteSelectedShop()
    {
        if (SelectedShop is null)
            return;

        var result = MessageBox.Show($"「{SelectedShop.Name}」を削除しますか？", "店舗削除の確認", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (result != MessageBoxResult.Yes)
            return;

        _shopService.DeleteShop(SelectedShop.Id);
        SelectedShop = null;
        RefreshFromService();
    }

    [RelayCommand]
    private void ClearSearch()
    {
        SearchKeyword = string.Empty;
        SelectedRamenType = "すべて";
        SelectedPriceFilter = "すべて";
        FavoriteOnly = false;
        NearbyOnly = false;
    }

    public void SetNearbyLocation(double latitude, double longitude)
    {
        _nearbyLatitude = latitude;
        _nearbyLongitude = longitude;
        NearbyOnly = true;
        ShopsView.Refresh();
    }

    public void SelectShopById(int shopId)
    {
        var shop = Shops.FirstOrDefault(s => s.Id == shopId);
        if (shop is not null)
            SelectedShop = shop;
    }

    partial void OnSelectedShopChanged(Shop? value) => SelectedShopChanged?.Invoke(this, value);

    public void RefreshFromService()
    {
        ReloadShops();
        ShopsView.Refresh();
        ShopsChanged?.Invoke(this, EventArgs.Empty);
    }
}
