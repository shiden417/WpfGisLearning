using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using WpfGisLearning.Models;
using WpfGisLearning.Services;
using WpfGisLearning.Services.Interfaces;

namespace WpfGisLearning.ViewModels;

public partial class ShopListViewModel : ObservableObject
{
    public ObservableCollection<Shop> Shops { get; } = new();
    public ICollectionView ShopsView { get; }
    public int FilteredShopCount => ShopsView.Cast<Shop>().Count();

    private readonly IShopService _shopService;
    private readonly INavigationService _navigationService;
    private double? _nearbyLatitude;
    private double? _nearbyLongitude;
    private bool _isClearingSearch;

    [ObservableProperty] private string searchKeyword = string.Empty;
    [ObservableProperty] private string selectedRamenType = ShopFilter.AllFilter;
    [ObservableProperty] private string selectedPriceFilter = ShopFilter.AllFilter;
    [ObservableProperty] private string selectedNearbyRadius = "5km";
    [ObservableProperty] private bool favoriteOnly;
    [ObservableProperty] private bool nearbyOnly;
    [ObservableProperty] private bool sortByPrice;
    [ObservableProperty] private bool sortByRating;
    [ObservableProperty] private Shop? selectedShop;

    public string[] RamenTypeOptions { get; } =
        [ShopFilter.AllFilter, "醤油", "塩", "味噌", "豚骨", "家系", "二郎系", "つけ麺", "その他"];

    public string[] PriceFilterOptions { get; } =
        [ShopFilter.AllFilter, ShopFilter.PriceFilter1000, ShopFilter.PriceFilter1500, ShopFilter.PriceFilter2000];

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

    partial void OnSearchKeywordChanged(string value)
    {
        if (!_isClearingSearch)
            RefreshFilteredShops();
    }

    partial void OnSelectedRamenTypeChanged(string value)
    {
        if (!_isClearingSearch)
            RefreshFilteredShops();
    }

    partial void OnSelectedPriceFilterChanged(string value)
    {
        if (!_isClearingSearch)
            RefreshFilteredShops();
    }

    partial void OnSelectedNearbyRadiusChanged(string value)
    {
        if (!_isClearingSearch)
            RefreshFilteredShops();
    }

    partial void OnFavoriteOnlyChanged(bool value)
    {
        if (!_isClearingSearch)
            RefreshFilteredShops();
    }

    partial void OnNearbyOnlyChanged(bool value)
    {
        if (!_isClearingSearch)
            RefreshFilteredShops();
    }

    partial void OnSortByPriceChanged(bool value)
    {
        if (!_isClearingSearch)
            ApplySort();
    }

    partial void OnSortByRatingChanged(bool value)
    {
        if (!_isClearingSearch)
            ApplySort();
    }

    private void RefreshFilteredShops()
    {
        ShopsView.Refresh();
        OnPropertyChanged(nameof(FilteredShopCount));

        if (SelectedShop is not null &&
            !ShopsView.Cast<Shop>().Any(shop => shop.Id == SelectedShop.Id))
        {
            SelectedShop = null;
        }

        ShopsChanged?.Invoke(this, EventArgs.Empty);
    }

    private void ApplySort()
    {
        ShopsView.SortDescriptions.Clear();
        if (SortByRating)
            ShopsView.SortDescriptions.Add(new SortDescription(nameof(Shop.Rating), ListSortDirection.Descending));
        if (SortByPrice)
            ShopsView.SortDescriptions.Add(new SortDescription(nameof(Shop.Price), ListSortDirection.Ascending));

        RefreshFilteredShops();
    }

    private bool FilterShop(object item)
    {
        return item is Shop shop
            && ShopFilter.Matches(
                shop,
                SearchKeyword,
                SelectedRamenType,
                SelectedPriceFilter,
                FavoriteOnly,
                NearbyOnly,
                _nearbyLatitude,
                _nearbyLongitude,
                SelectedNearbyRadius);
    }

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
    private void Search() => RefreshFilteredShops();

    [RelayCommand]
    private void ClearSearch()
    {
        _isClearingSearch = true;
        try
        {
            SearchKeyword = string.Empty;
            SelectedRamenType = ShopFilter.AllFilter;
            SelectedPriceFilter = ShopFilter.AllFilter;
            FavoriteOnly = false;
            NearbyOnly = false;
            SortByPrice = false;
            SortByRating = false;
        }
        finally
        {
            _isClearingSearch = false;
        }

        ApplySort();
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
    }
}
