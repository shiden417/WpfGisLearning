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
    private const string AllFilter = "すべて";
    private const string PriceFilter1000 = "1000円以下";
    private const string PriceFilter1500 = "1500円以下";
    private const string PriceFilter2000 = "2000円以下";
    private const double EarthRadiusKm = 6371.0;

    public ObservableCollection<Shop> Shops { get; } = new();
    public ICollectionView ShopsView { get; }
    public int FilteredShopCount => ShopsView.Cast<Shop>().Count();

    private readonly IShopService _shopService;
    private readonly INavigationService _navigationService;
    private double? _nearbyLatitude;
    private double? _nearbyLongitude;

    [ObservableProperty] private string searchKeyword = string.Empty;
    [ObservableProperty] private string selectedRamenType = AllFilter;
    [ObservableProperty] private string selectedPriceFilter = AllFilter;
    [ObservableProperty] private string selectedNearbyRadius = "5km";
    [ObservableProperty] private bool favoriteOnly;
    [ObservableProperty] private bool nearbyOnly;
    [ObservableProperty] private bool sortByName;
    [ObservableProperty] private bool sortByPrice;
    [ObservableProperty] private bool sortByRating;
    [ObservableProperty] private Shop? selectedShop;

    public string[] RamenTypeOptions { get; } = [AllFilter, "醤油", "塩", "味噌", "豚骨", "家系", "二郎系", "つけ麺", "その他"];
    public string[] PriceFilterOptions { get; } = [AllFilter, PriceFilter1000, PriceFilter1500, PriceFilter2000];
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

    partial void OnSearchKeywordChanged(string value) => RefreshFilteredShops();
    partial void OnSelectedRamenTypeChanged(string value) => RefreshFilteredShops();
    partial void OnSelectedPriceFilterChanged(string value) => RefreshFilteredShops();
    partial void OnSelectedNearbyRadiusChanged(string value) => RefreshFilteredShops();
    partial void OnFavoriteOnlyChanged(bool value) => RefreshFilteredShops();
    partial void OnNearbyOnlyChanged(bool value) => RefreshFilteredShops();
    partial void OnSortByNameChanged(bool value) => ApplySort();
    partial void OnSortByPriceChanged(bool value) => ApplySort();
    partial void OnSortByRatingChanged(bool value) => ApplySort();

    private void RefreshFilteredShops()
    {
        ShopsView.Refresh();
        OnPropertyChanged(nameof(FilteredShopCount));
    }

    private void ApplySort()
    {
        ShopsView.SortDescriptions.Clear();
        if (SortByRating)
            ShopsView.SortDescriptions.Add(new SortDescription(nameof(Shop.Rating), ListSortDirection.Descending));
        if (SortByPrice)
            ShopsView.SortDescriptions.Add(new SortDescription(nameof(Shop.Price), ListSortDirection.Ascending));
        if (SortByName)
            ShopsView.SortDescriptions.Add(new SortDescription(nameof(Shop.Name), ListSortDirection.Ascending));

        RefreshFilteredShops();
    }

    private bool FilterShop(object item)
    {
        if (item is not Shop shop)
            return false;

        return MatchesKeyword(shop)
            && MatchesRamenType(shop)
            && MatchesFavorite(shop)
            && MatchesPrice(shop)
            && MatchesNearby(shop);
    }

    private bool MatchesKeyword(Shop shop)
    {
        var keyword = SearchKeyword.Trim();
        if (string.IsNullOrWhiteSpace(keyword))
            return true;

        return shop.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase)
            || shop.Address.Contains(keyword, StringComparison.OrdinalIgnoreCase)
            || shop.RamenType.Contains(keyword, StringComparison.OrdinalIgnoreCase);
    }

    private bool MatchesRamenType(Shop shop)
    {
        return SelectedRamenType == AllFilter || shop.RamenType == SelectedRamenType;
    }

    private bool MatchesFavorite(Shop shop)
    {
        return !FavoriteOnly || shop.IsFavorite;
    }

    private bool MatchesPrice(Shop shop)
    {
        if (SelectedPriceFilter == AllFilter)
            return true;

        var maxPrice = SelectedPriceFilter switch
        {
            PriceFilter1000 => 1000m,
            PriceFilter1500 => 1500m,
            PriceFilter2000 => 2000m,
            _ => decimal.MaxValue
        };

        return shop.Price <= maxPrice;
    }

    private bool MatchesNearby(Shop shop)
    {
        if (!NearbyOnly)
            return true;

        if (!_nearbyLatitude.HasValue || !_nearbyLongitude.HasValue)
            return false;

        var radiusKm = double.Parse(SelectedNearbyRadius.Replace("km", ""));
        return DistanceKm(_nearbyLatitude.Value, _nearbyLongitude.Value, shop.Latitude, shop.Longitude) <= radiusKm;
    }

    private static double DistanceKm(double lat1, double lon1, double lat2, double lon2)
    {
        var dLat = DegreesToRadians(lat2 - lat1);
        var dLon = DegreesToRadians(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
            + Math.Cos(DegreesToRadians(lat1))
            * Math.Cos(DegreesToRadians(lat2))
            * Math.Sin(dLon / 2)
            * Math.Sin(dLon / 2);

        return EarthRadiusKm * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
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
        RefreshFilteredShops();
        ShopsChanged?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void DeleteSelectedShop()
    {
        if (SelectedShop is null)
            return;

        var result = MessageBox.Show(
            $"「{SelectedShop.Name}」を削除しますか？",
            "店舗削除の確認",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

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
        SelectedRamenType = AllFilter;
        SelectedPriceFilter = AllFilter;
        FavoriteOnly = false;
        NearbyOnly = false;
        SortByName = false;
        SortByPrice = false;
        SortByRating = false;
    }

    public void SetNearbyLocation(double latitude, double longitude)
    {
        _nearbyLatitude = latitude;
        _nearbyLongitude = longitude;
        NearbyOnly = true;
        RefreshFilteredShops();
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
        ApplySort();
        ShopsChanged?.Invoke(this, EventArgs.Empty);
    }
}
