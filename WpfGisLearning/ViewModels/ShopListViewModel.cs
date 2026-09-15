using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

    [ObservableProperty]
    private string searchKeyword = string.Empty;

    [ObservableProperty]
    private string selectedSort = "おすすめ";

    [ObservableProperty]
    private Shop? selectedShop;

    public string[] SortOptions { get; } =
    [
        "おすすめ",
        "店舗名: A → Z",
        "価格: 安い順",
        "価格: 高い順"
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
        }

        ShopsView.Refresh();
    }

    private bool FilterShop(object item)
    {
        if (item is not Shop shop)
            return false;

        if (string.IsNullOrWhiteSpace(SearchKeyword))
            return true;

        var keyword = SearchKeyword.Trim();
        return shop.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
               shop.Address.Contains(keyword, StringComparison.OrdinalIgnoreCase);
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
    private void ClearSearch() => SearchKeyword = string.Empty;

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
