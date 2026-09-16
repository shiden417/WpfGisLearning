using System.Windows;
using System.Windows.Input;
using WpfGisLearning.Map;
using WpfGisLearning.Models;

namespace WpfGisLearning;

/// <summary>
/// MainWindowのうち、地図・店舗選択・現在地に関するコードをまとめたpartial部分です。
/// MainWindow本体から地図関連のWPFイベント処理を分離しています。
/// </summary>
public partial class MainWindow
{
    /// <summary>Mapsuiの地図を初期化し、店舗一覧とWPFイベントを接続します。</summary>
    private void InitializeMap()
    {
        try
        {
            _mapController.Initialize();
            RebuildShopLayer();
            MapControl.MouseLeftButtonUp += MapControl_MouseLeftButtonUp;
            MapControl.Loaded += MapControl_Loaded;
            MapStatusText.Visibility = Visibility.Collapsed;
        }
        catch (Exception ex)
        {
            ShowMapError("地図を初期化できませんでした。", ex);
        }
    }

    /// <summary>現在のフィルター結果を店舗レイヤーへ反映します。</summary>
    private void RebuildShopLayer()
    {
        var filteredShops = _shopListViewModel.ShopsView.Cast<Shop>().ToList();
        _mapController.RebuildShopLayer(filteredShops, _selectedShopId);

        // 選択中店舗がフィルターで消えた場合は、地図と情報カードの選択も解除する。
        if (_selectedShopId.HasValue && !filteredShops.Any(shop => shop.Id == _selectedShopId.Value))
        {
            _selectedShopId = null;
            _shopListViewModel.SelectedShop = null;
            _infoCardPresenter.Hide();
        }
    }

    /// <summary>MapControlのLoaded時に初期表示位置を設定します。</summary>
    private void MapControl_Loaded(object sender, RoutedEventArgs e)
    {
        if (_mapController.InitialMapPositionSet) return;
        SetInitialMapPosition();
    }

    /// <summary>店舗全体が見える初期地図位置をMapControllerへ依頼します。</summary>
    private void SetInitialMapPosition()
    {
        try
        {
            _mapController.SetInitialMapPosition();
        }
        catch (Exception ex)
        {
            ShowMapError("地図の表示位置を設定できませんでした。", ex);
        }
    }

    /// <summary>店舗一覧が変更されたら地図レイヤーを再構築します。</summary>
    private void ShopListViewModel_ShopsChanged(object? sender, EventArgs e) => RebuildShopLayer();

    /// <summary>
    /// 一覧で店舗が選択された際に、地図・情報カード・選択状態を同期します。
    /// </summary>
    private void ShopListViewModel_SelectedShopChanged(object? sender, Shop? shop)
    {
        _selectedShopId = shop?.Id;

        if (shop is null)
        {
            _infoCardPresenter.Hide();
            RebuildShopLayer();
            return;
        }

        RebuildShopLayer();

        if (!MapCoordinateValidator.IsValid(shop.Latitude, shop.Longitude))
        {
            _infoCardPresenter.Hide();
            return;
        }

        // フィルター結果に存在しない店舗は地図上で選択状態にしない。
        if (!_shopListViewModel.ShopsView.Cast<Shop>().Any(filteredShop => filteredShop.Id == shop.Id))
        {
            _selectedShopId = null;
            _infoCardPresenter.Hide();
            return;
        }

        _infoCardPresenter.Show(shop);
        _mapController.CenterOnShop(shop);
    }

    /// <summary>地図クリック位置から店舗IDを取得し、一覧側の選択へ反映します。</summary>
    private void MapControl_MouseLeftButtonUp(object? sender, MouseButtonEventArgs e)
    {
        try
        {
            var position = e.GetPosition(MapControl);
            var shopId = _mapController.GetShopIdAt(position);
            if (shopId is not null)
                _shopListViewModel.SelectShopById(shopId.Value);
        }
        catch (Exception ex)
        {
            ShowMapError("地図上の店舗情報を取得できませんでした。", ex);
        }
    }

    /// <summary>情報カードの閉じるボタンからカード表示だけを消します。</summary>
    private void InfoCardClose_Click(object sender, RoutedEventArgs e) => _infoCardPresenter.Hide();

    /// <summary>現在地ボタンから現在地取得処理を開始します。</summary>
    private async void CurrentLocationButton_Click(object sender, RoutedEventArgs e) =>
        await TryShowCurrentLocationAsync(showMessageOnFailure: true);

    /// <summary>
    /// 現在地を取得し、近隣フィルター・現在地マーカー・地図中心をまとめて更新します。
    /// </summary>
    private async Task TryShowCurrentLocationAsync(bool showMessageOnFailure)
    {
        try
        {
            var location = await _currentLocationService.GetCurrentLocationAsync();
            if (location is null)
            {
                if (showMessageOnFailure)
                {
                    MessageBox.Show(
                        "現在地を取得できませんでした。位置情報の利用を許可しているか確認してください。",
                        "現在地",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                return;
            }

            _shopListViewModel.SetNearbyLocation(location.Latitude, location.Longitude);
            _mapController.ShowCurrentLocation(location.Latitude, location.Longitude);
            _mapController.CenterOn(location.Latitude, location.Longitude);
        }
        catch (Exception ex)
        {
            ShowMapError("現在地を取得できませんでした。", ex);
            if (showMessageOnFailure)
            {
                MessageBox.Show(
                    $"現在地を取得できませんでした。\n{ex.Message}",
                    "現在地",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }
    }

    /// <summary>検索条件を解除して全店舗を再表示し、地図を初期位置へ戻します。</summary>
    private void ShowAllShopsButton_Click(object sender, RoutedEventArgs e)
    {
        _selectedShopId = null;
        _shopListViewModel.ClearSearchCommand.Execute(null);
        _infoCardPresenter.Hide();
        RebuildShopLayer();
        _mapController.InitialMapPositionSet = false;
        SetInitialMapPosition();
    }

    /// <summary>地図を1段階拡大します。</summary>
    private void ZoomInButton_Click(object sender, RoutedEventArgs e) => _mapController.ZoomIn();

    /// <summary>地図を1段階縮小します。</summary>
    private void ZoomOutButton_Click(object sender, RoutedEventArgs e) => _mapController.ZoomOut();

    /// <summary>ダブルクリック時の既定の地図操作を抑制するWPFイベントハンドラーです。</summary>
    private void MapControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount != 2) return;
        e.Handled = true;
    }

    /// <summary>地図処理中の例外メッセージを画面上のステータス表示へ設定します。</summary>
    private void ShowMapError(string message, Exception exception)
    {
        MapStatusText.Text = $"{message}\n{exception.Message}";
        MapStatusText.Visibility = Visibility.Visible;
    }
}
