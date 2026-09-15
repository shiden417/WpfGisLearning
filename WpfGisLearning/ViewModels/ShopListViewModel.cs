using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows;
using WpfGisLearning.Models;
using WpfGisLearning.Services;

namespace WpfGisLearning.ViewModels;

public partial class ShopListViewModel : ObservableObject
{
    public ObservableCollection<Shop> Shops { get; } = new();
    public ICollectionView ShopsView { get; }

    private readonly IShopService _shopService;
    private readonly INavigationService _navigationService;

    [ObservableProperty] private string searchKeyword = string.Empty;
    [ObservableProperty] private string selectedSort = "おすすめ";
    [ObservableProperty] private string selectedRamenType = "すべて";
    [ObservableProperty] private string selectedPriceFilter = "すべて";
    [ObservableProperty] private bool favoriteOnly;
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
    partial void OnFavoriteOnlyChanged(bool value) => ShopsView.Refresh();

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

        return true;
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
    private void ToggleSelectedFavorite()
    {
        if (SelectedShop is null)
            return;

        _shopService.ToggleFavorite(SelectedShop.Id);
        ShopsView.Refresh();
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
        SelectedRamenType = "すべて";
        SelectedPriceFilter = "すべて";
        FavoriteOnly = false;
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
