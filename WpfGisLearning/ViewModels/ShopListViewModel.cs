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

/// <summary>
/// 店舗一覧画面の表示データ、検索・フィルター・ソート状態、選択状態を管理するViewModelです。
/// WPFのICollectionViewを利用して、元データを変更せず表示対象だけを絞り込みます。
/// </summary>
public partial class ShopListViewModel : ObservableObject
{
    /// <summary>サービスから取得した全店舗を保持するコレクションです。</summary>
    public ObservableCollection<Shop> Shops { get; } = new();

    /// <summary>
    /// ShopsをWPFのCollectionViewとして公開します。FilterやSortDescriptionsをここへ設定します。
    /// </summary>
    public ICollectionView ShopsView { get; }

    /// <summary>現在の検索・フィルター条件に一致する店舗件数です。</summary>
    public int FilteredShopCount => ShopsView.Cast<Shop>().Count();

    /// <summary>店舗の取得・更新・削除などを担当するサービスです。</summary>
    private readonly IShopService _shopService;

    /// <summary>店舗詳細・編集画面への遷移を担当するサービスです。</summary>
    private readonly INavigationService _navigationService;

    /// <summary>現在地検索で使用する基準地点の緯度です。</summary>
    private double? _nearbyLatitude;

    /// <summary>現在地検索で使用する基準地点の経度です。</summary>
    private double? _nearbyLongitude;

    /// <summary>検索条件をまとめて初期化するとき、個別の再描画を抑制するフラグです。</summary>
    private bool _isClearingSearch;

    /// <summary>キーワード検索文字列です。</summary>
    [ObservableProperty] private string searchKeyword = string.Empty;

    /// <summary>選択中のラーメン種別フィルターです。</summary>
    [ObservableProperty] private string selectedRamenType = ShopFilter.AllFilter;

    /// <summary>選択中の価格上限フィルターです。</summary>
    [ObservableProperty] private string selectedPriceFilter = ShopFilter.AllFilter;

    /// <summary>現在地から検索する場合の距離範囲です。</summary>
    [ObservableProperty] private string selectedNearbyRadius = "5km";

    /// <summary>お気に入りだけを表示するかを示します。</summary>
    [ObservableProperty] private bool favoriteOnly;

    /// <summary>現在地周辺だけを表示するかを示します。</summary>
    [ObservableProperty] private bool nearbyOnly;

    /// <summary>価格昇順ソートを有効にするかを示します。</summary>
    [ObservableProperty] private bool sortByPrice;

    /// <summary>評価降順ソートを有効にするかを示します。</summary>
    [ObservableProperty] private bool sortByRating;

    /// <summary>現在選択されている店舗です。</summary>
    [ObservableProperty] private Shop? selectedShop;

    /// <summary>画面のラーメン種別ComboBoxに表示する選択肢です。</summary>
    public string[] RamenTypeOptions { get; } =
        [ShopFilter.AllFilter, "醤油", "塩", "味噌", "豚骨", "家系", "二郎系", "つけ麺", "その他"];

    /// <summary>画面の価格フィルターComboBoxに表示する選択肢です。</summary>
    public string[] PriceFilterOptions { get; } =
        [ShopFilter.AllFilter, ShopFilter.PriceFilter1000, ShopFilter.PriceFilter1500, ShopFilter.PriceFilter2000];

    /// <summary>現在地検索の距離選択肢です。</summary>
    public string[] NearbyRadiusOptions { get; } = ["1km", "3km", "5km", "10km", "20km"];

    /// <summary>選択店舗が変更されたときに発生するイベントです。MapやInfoCardとの同期に利用します。</summary>
    public event EventHandler<Shop?>? SelectedShopChanged;

    /// <summary>店舗一覧やフィルター結果が変化したときに発生するイベントです。</summary>
    public event EventHandler? ShopsChanged;

    /// <summary>サービスから店舗を読み込み、WPFのCollectionViewを初期化します。</summary>
    public ShopListViewModel(IShopService shopService, INavigationService navigationService)
    {
        _shopService = shopService;
        _navigationService = navigationService;
        ReloadShops();
        ShopsView = CollectionViewSource.GetDefaultView(Shops);
        ShopsView.Filter = FilterShop;
    }

    /// <summary>検索キーワード変更時に一覧を再評価します。</summary>
    partial void OnSearchKeywordChanged(string value)
    {
        if (!_isClearingSearch)
            RefreshFilteredShops();
    }

    /// <summary>ラーメン種別変更時に一覧を再評価します。</summary>
    partial void OnSelectedRamenTypeChanged(string value)
    {
        if (!_isClearingSearch)
            RefreshFilteredShops();
    }

    /// <summary>価格フィルター変更時に一覧を再評価します。</summary>
    partial void OnSelectedPriceFilterChanged(string value)
    {
        if (!_isClearingSearch)
            RefreshFilteredShops();
    }

    /// <summary>距離範囲変更時に一覧を再評価します。</summary>
    partial void OnSelectedNearbyRadiusChanged(string value)
    {
        if (!_isClearingSearch)
            RefreshFilteredShops();
    }

    /// <summary>お気に入り限定条件変更時に一覧を再評価します。</summary>
    partial void OnFavoriteOnlyChanged(bool value)
    {
        if (!_isClearingSearch)
            RefreshFilteredShops();
    }

    /// <summary>現在地周辺限定条件変更時に一覧を再評価します。</summary>
    partial void OnNearbyOnlyChanged(bool value)
    {
        if (!_isClearingSearch)
            RefreshFilteredShops();
    }

    /// <summary>価格ソート条件変更時にソート順を再構築します。</summary>
    partial void OnSortByPriceChanged(bool value)
    {
        if (!_isClearingSearch)
            ApplySort();
    }

    /// <summary>評価ソート条件変更時にソート順を再構築します。</summary>
    partial void OnSortByRatingChanged(bool value)
    {
        if (!_isClearingSearch)
            ApplySort();
    }

    /// <summary>
    /// CollectionViewへFilterを再適用し、表示件数と地図同期用イベントを更新します。
    /// </summary>
    private void RefreshFilteredShops()
    {
        ShopsView.Refresh();
        OnPropertyChanged(nameof(FilteredShopCount));

        // 現在選択中の店舗がフィルター結果から消えた場合は選択も解除する。
        if (SelectedShop is not null &&
            !ShopsView.Cast<Shop>().Any(shop => shop.Id == SelectedShop.Id))
        {
            SelectedShop = null;
        }

        ShopsChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// 現在の価格・評価ソート条件からSortDescriptionsを作り直します。
    /// 複数条件が有効な場合、評価→価格の順に優先されます。
    /// </summary>
    private void ApplySort()
    {
        ShopsView.SortDescriptions.Clear();
        if (SortByRating)
            ShopsView.SortDescriptions.Add(new SortDescription(nameof(Shop.Rating), ListSortDirection.Descending));
        if (SortByPrice)
            ShopsView.SortDescriptions.Add(new SortDescription(nameof(Shop.Price), ListSortDirection.Ascending));

        RefreshFilteredShops();
    }

    /// <summary>WPFのCollectionViewから呼ばれ、店舗1件が現在の検索条件を満たすか判定します。</summary>
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

    /// <summary>店舗サービスから最新一覧を読み込み、ObservableCollectionを更新します。</summary>
    private void ReloadShops()
    {
        Shops.Clear();
        foreach (var shop in _shopService.GetShops())
            Shops.Add(shop);
    }

    /// <summary>新規店舗登録画面を開き、戻ってきたら店舗一覧を更新します。</summary>
    [RelayCommand]
    private void NewShop()
    {
        _navigationService.NavigateToShopEdit();
        RefreshFromService();
    }

    /// <summary>現在選択中の店舗詳細画面を開きます。</summary>
    [RelayCommand]
    private void OpenSelectedShop()
    {
        if (SelectedShop is not null)
            _navigationService.NavigateToDetail(SelectedShop.Id);
    }

    /// <summary>現在選択中の店舗編集画面を開き、戻ってきたら一覧を更新します。</summary>
    [RelayCommand]
    private void EditSelectedShop()
    {
        if (SelectedShop is null)
            return;

        _navigationService.NavigateToShopEdit(SelectedShop.Id);
        RefreshFromService();
    }

    /// <summary>指定店舗、または選択中店舗のお気に入り状態を切り替えます。</summary>
    /// <param name="shop">切り替え対象。nullならSelectedShopを使用します。</param>
    [RelayCommand]
    private void ToggleFavorite(Shop? shop)
    {
        shop ??= SelectedShop;
        if (shop is null)
            return;

        _shopService.ToggleFavorite(shop.Id);
        RefreshFilteredShops();
    }

    /// <summary>選択中店舗を確認後に削除し、一覧を再読み込みします。</summary>
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

    /// <summary>現在の検索・フィルター条件を再適用するコマンドです。</summary>
    [RelayCommand]
    private void Search() => RefreshFilteredShops();

    /// <summary>検索条件とソート条件をまとめて初期状態へ戻します。</summary>
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

        // 各プロパティ変更時の個別更新を抑え、最後に1回だけ一覧を更新する。
        ApplySort();
    }

    /// <summary>
    /// 現在地検索の基準座標を設定し、近隣店舗フィルターを有効にします。
    /// </summary>
    public void SetNearbyLocation(double latitude, double longitude)
    {
        _nearbyLatitude = latitude;
        _nearbyLongitude = longitude;
        NearbyOnly = true;
        RefreshFilteredShops();
    }

    /// <summary>店舗IDに一致する店舗を一覧から選択します。</summary>
    public void SelectShopById(int shopId)
    {
        var shop = Shops.FirstOrDefault(s => s.Id == shopId);
        if (shop is not null)
            SelectedShop = shop;
    }

    /// <summary>選択店舗変更をView側へ通知するCommunityToolkitの部分メソッドです。</summary>
    partial void OnSelectedShopChanged(Shop? value) => SelectedShopChanged?.Invoke(this, value);

    /// <summary>サービスから店舗を再読み込みし、現在のフィルター・ソート条件を維持します。</summary>
    public void RefreshFromService()
    {
        ReloadShops();
        ApplySort();
    }
}
