using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using WpfGisLearning.Models;
using WpfGisLearning.Services;

namespace WpfGisLearning.ViewModels;

public partial class ShopListViewModel : ObservableObject, INotifyDataErrorInfo
{
    public ObservableCollection<Shop> Shops { get; } = new();

    private readonly IShopService _shopService;
    private readonly INavigationService _navigationService;

    public ICollectionView ShopsView { get; }

    public ShopListViewModel(
        IShopService shopService,
        INavigationService navigationService)
    {
        _shopService = shopService;
        _navigationService = navigationService;

        foreach (var s in _shopService.GetShops())
        {
            Shops.Add(s);
        }

        ShopsView = CollectionViewSource.GetDefaultView(Shops);
    }

    private readonly Dictionary<string, List<string>> _errors = new();

    public bool HasErrors => _errors.Any();

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    public System.Collections.IEnumerable? GetErrors(string? propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
        {
            return null;
        }

        if (_errors.TryGetValue(propertyName, out var list))
        {
            return list;
        }

        return null;
    }

    private void AddError(string propertyName, string error)
    {
        if (!_errors.TryGetValue(propertyName, out var list))
        {
            list = new List<string>();
            _errors[propertyName] = list;
        }

        if (!list.Contains(error))
        {
            list.Add(error);
            ErrorsChanged?.Invoke(
                this,
                new DataErrorsChangedEventArgs(propertyName));
        }
    }

    private void ClearErrors(string propertyName)
    {
        if (_errors.Remove(propertyName))
        {
            ErrorsChanged?.Invoke(
                this,
                new DataErrorsChangedEventArgs(propertyName));
        }
    }

    [ObservableProperty]
    private string newShopName = string.Empty;

    [ObservableProperty]
    private decimal newShopPrice = 0m;

    partial void OnNewShopNameChanged(string value)
    {
        ValidateNewShopName();
    }

    partial void OnNewShopPriceChanged(decimal value)
    {
        ValidateNewShopPrice();
    }

    private void ValidateNewShopName()
    {
        ClearErrors(nameof(NewShopName));

        if (string.IsNullOrWhiteSpace(NewShopName))
        {
            AddError(nameof(NewShopName), "店舗名は必須です。");
        }
    }

    private void ValidateNewShopPrice()
    {
        ClearErrors(nameof(NewShopPrice));

        if (NewShopPrice <= 0m)
        {
            AddError(nameof(NewShopPrice), "価格は0より大きい値を入力してください。");
        }
    }

    [RelayCommand]
    private void Register()
    {
        ValidateNewShopName();
        ValidateNewShopPrice();

        if (HasErrors)
        {
            return;
        }

        var nextId = Shops.Any()
            ? Shops.Max(s => s.Id) + 1
            : 1;

        Shops.Add(new Shop
        {
            Id = nextId,
            Name = NewShopName,
            Price = NewShopPrice
        });

        NewShopName = string.Empty;
        NewShopPrice = 0m;
    }

    [ObservableProperty]
    private Shop? selectedShop;

    public event EventHandler<Shop?>? SelectedShopChanged;

    partial void OnSelectedShopChanged(Shop? value)
    {
        SelectedShopChanged?.Invoke(this, value);
    }

    // 地図など外部のUIから店舗を選択するための入口
    public void SelectShopById(int shopId)
    {
        var shop = Shops.FirstOrDefault(s => s.Id == shopId);

        if (shop is not null)
        {
            SelectedShop = shop;
        }
    }

    [RelayCommand]
    private void OpenSelectedShop()
    {
        if (SelectedShop is null)
        {
            return;
        }

        _navigationService.NavigateToDetail(SelectedShop.Id);
    }

    [RelayCommand]
    private void SortPriceAsc()
    {
        if (ShopsView == null)
        {
            return;
        }

        ShopsView.SortDescriptions.Clear();
        ShopsView.SortDescriptions.Add(
            new SortDescription(nameof(Shop.Price), ListSortDirection.Ascending));
        ShopsView.Refresh();
    }

    [RelayCommand]
    private void SortPriceDesc()
    {
        if (ShopsView == null)
        {
            return;
        }

        ShopsView.SortDescriptions.Clear();
        ShopsView.SortDescriptions.Add(
            new SortDescription(nameof(Shop.Price), ListSortDirection.Descending));
        ShopsView.Refresh();
    }
}