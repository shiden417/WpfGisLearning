using Microsoft.Win32;
using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.Tiling;
using System.Windows;
using System.Windows.Input;
using WpfGisLearning.Models;
using WpfGisLearning.Services;
using WpfGisLearning.Services.Interfaces;
using WpfGisLearning.ViewModels;

namespace WpfGisLearning.Views;

/// <summary>
/// 店舗登録・編集画面のViewです。
/// ファイルダイアログ、地図操作、WindowのClosingイベントなど、WPF固有のUI処理を担当します。
/// 店舗データの検証や保存処理はShopEditViewModelへ委譲します。
/// </summary>
public partial class ShopEditView : System.Windows.Controls.UserControl
{
    // 編集対象店舗の初期表示ズームです。
    private const long InitialShopLocationResolution = 500;
    // 店舗位置が未設定の場合に表示する日本全体の経度です。
    private const double JapanOverviewLongitude = 138.0;
    // 店舗位置が未設定の場合に表示する日本全体の緯度です。
    private const double JapanOverviewLatitude = 36.0;
    // 日本全体を表示するときのズーム量です。
    private const long JapanOverviewResolution = 6000;
    // 編集画面の地図操作で使用するズーム量です。
    private const long ZoomAmount = 500;

    // 画面状態と入力値を管理するViewModelです。
    private readonly ShopEditViewModel _viewModel;
    // Windowsの現在地取得を担当するサービスです。
    private readonly ICurrentLocationService _currentLocationService;
    // 座標から住所を取得するサービスです。
    private readonly IReverseGeocodingService _reverseGeocodingService;
    // 編集画面専用のMapsui地図です。
    private Mapsui.Map? _map;
    // ユーザーが指定した店舗位置を表示するレイヤーです。
    private MemoryLayer? _locationLayer;
    // 店舗位置マーカーをドラッグ中かどうかを示します。
    private bool _isDraggingLocation;
    // このUserControlを表示しているホストWindowです。
    private Window? _hostWindow;
    // 保存成功後など、確認なしでWindowを閉じてよい状態を示します。
    private bool _allowWindowClose;

    /// <summary>ViewModelと位置情報関連サービスを受け取り、イベントを購読します。</summary>
    public ShopEditView(ShopEditViewModel viewModel, ICurrentLocationService currentLocationService, IReverseGeocodingService reverseGeocodingService)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _currentLocationService = currentLocationService;
        _reverseGeocodingService = reverseGeocodingService;
        DataContext = viewModel;
        viewModel.RequestClose += ViewModel_RequestClose;
        Loaded += ShopEditView_Loaded;
        EditMapControl.PreviewMouseLeftButtonDown += EditMapControl_PreviewMouseLeftButtonDown;
        EditMapControl.PreviewMouseMove += EditMapControl_PreviewMouseMove;
        EditMapControl.PreviewMouseLeftButtonUp += EditMapControl_PreviewMouseLeftButtonUp;
    }

    /// <summary>WPFのOpenFileDialogで複数の店舗写真を選択し、ViewModelへ追加します。</summary>
    private void AddPhotoButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Title = "店舗写真を選択", Filter = "画像ファイル|*.jpg;*.jpeg;*.png;*.webp;*.bmp|すべてのファイル|*.*", Multiselect = true };
        if (dialog.ShowDialog() != true) return;
        foreach (var file in dialog.FileNames) _viewModel.AddPhoto(file);
    }

    /// <summary>指定された写真を一覧から削除します。</summary>
    private void RemovePhotoButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button { Tag: ShopPhoto photo }) _viewModel.RemovePhoto(photo);
    }

    /// <summary>指定された写真をメイン写真に変更します。</summary>
    private void SetMainPhotoButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button { Tag: ShopPhoto photo }) _viewModel.SetMainPhoto(photo);
    }

    /// <summary>入力エラーがないことを確認してからViewModelの保存コマンドを実行します。</summary>
    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (System.Windows.Controls.Validation.GetHasError(ShopPriceTextBox) ||
            System.Windows.Controls.Validation.GetHasError(RatingTextBox))
            return;

        _viewModel.SaveCommand.Execute(null);
    }

    /// <summary>画面初回表示時にホストWindowと地図を初期化します。</summary>
    private void ShopEditView_Loaded(object sender, RoutedEventArgs e)
    {
        AttachHostWindow();
        if (_map is not null) return;
        _map = new Mapsui.Map();
        _map.Layers.Add(OpenStreetMap.CreateTileLayer());
        EditMapControl.Map = _map;

        if (_viewModel.ShopId.HasValue && _viewModel.ShopLatitude.HasValue && _viewModel.ShopLongitude.HasValue)
        {
            var latitude = _viewModel.ShopLatitude.Value;
            var longitude = _viewModel.ShopLongitude.Value;
            ShowLocation(latitude, longitude);
            var (x, y) = SphericalMercator.FromLonLat(longitude, latitude);
            _map.Navigator.CenterOnAndZoomTo(new MPoint(x, y), InitialShopLocationResolution);
            return;
        }
        SetJapanOverview();
    }

    /// <summary>このUserControlを表示しているWindowを取得し、Closingイベントへ接続します。</summary>
    private void AttachHostWindow()
    {
        var window = Window.GetWindow(this);
        if (window is null || ReferenceEquals(_hostWindow, window)) return;
        _hostWindow = window;
        _hostWindow.Closing += HostWindow_Closing;
    }

    /// <summary>未保存変更がある場合に、Windowを閉じてよいか確認します。</summary>
    private void HostWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (_allowWindowClose) return;
        if (_viewModel.IsSaving)
        {
            e.Cancel = true;
            return;
        }
        if (!_viewModel.IsDirty) return;

        var result = MessageBox.Show(
            "変更内容が保存されていません。保存せずに閉じますか？",
            "未保存の変更",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
        {
            e.Cancel = true;
            return;
        }

        _allowWindowClose = true;
    }

    /// <summary>ViewModelから閉じる要求を受け取り、必要なら未保存確認を行ってWindowを閉じます。</summary>
    private void ViewModel_RequestClose(object? sender, EventArgs e)
    {
        if (_hostWindow is null)
        {
            _hostWindow = Window.GetWindow(this);
            if (_hostWindow is null) return;
        }

        if (_viewModel.IsDirty)
        {
            var result = MessageBox.Show(
                "変更内容が保存されていません。保存せずに閉じますか？",
                "未保存の変更",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;
        }

        _allowWindowClose = true;
        _hostWindow.Close();
    }

    /// <summary>店舗位置が未設定の場合に、日本付近へ地図を移動します。</summary>
    private void SetJapanOverview()
    {
        var (x, y) = SphericalMercator.FromLonLat(JapanOverviewLongitude, JapanOverviewLatitude);
        _map?.Navigator.CenterOnAndZoomTo(new MPoint(x, y), JapanOverviewResolution);
    }

    /// <summary>地図上のダブルクリックで位置を設定し、既存マーカーのドラッグを開始します。</summary>
    private async void EditMapControl_PreviewMouseLeftButtonDown(object? sender, MouseButtonEventArgs e)
    {
        if (_map is null || _viewModel.IsSaving) return;
        var position = e.GetPosition(EditMapControl);
        if (e.ClickCount == 2)
        {
            if (SetLocationFromScreen(position.X, position.Y, false)) await UpdateAddressAsync();
            e.Handled = true;
            return;
        }
        if (HasLocationAt(position.X, position.Y))
        {
            _isDraggingLocation = true;
            EditMapControl.CaptureMouse();
            e.Handled = true;
        }
    }

    /// <summary>店舗位置マーカーをドラッグしている間、マウス位置に座標を追従させます。</summary>
    private void EditMapControl_PreviewMouseMove(object? sender, MouseEventArgs e)
    {
        if (!_isDraggingLocation || _map is null || _viewModel.IsSaving) return;
        var position = e.GetPosition(EditMapControl);
        SetLocationFromScreen(position.X, position.Y, false);
        e.Handled = true;
    }

    /// <summary>店舗位置のドラッグを終了し、確定した座標から住所を再取得します。</summary>
    private async void EditMapControl_PreviewMouseLeftButtonUp(object? sender, MouseButtonEventArgs e)
    {
        if (!_isDraggingLocation) return;
        _isDraggingLocation = false;
        EditMapControl.ReleaseMouseCapture();
        if (_viewModel.IsSaving) return;
        var position = e.GetPosition(EditMapControl);
        if (SetLocationFromScreen(position.X, position.Y, false)) await UpdateAddressAsync();
        e.Handled = true;
    }

    /// <summary>指定された画面座標に店舗位置レイヤーのマーカーが存在するか確認します。</summary>
    private bool HasLocationAt(double x, double y)
    {
        if (_locationLayer is null) return false;
        var mapInfo = EditMapControl.GetMapInfo(new Mapsui.Manipulations.ScreenPosition((int)x, (int)y), new[] { _locationLayer });
        return mapInfo?.Feature is not null;
    }

    /// <summary>画面座標を地理座標へ変換し、ViewModelへ位置を設定します。</summary>
    private bool SetLocationFromScreen(double x, double y, bool recenter)
    {
        if (_map is null) return false;
        var screenPosition = new Mapsui.Manipulations.ScreenPosition((int)x, (int)y);
        var worldPosition = _map.Navigator.Viewport.ScreenToWorldXY(screenPosition.X, screenPosition.Y);
        var (lon, lat) = SphericalMercator.ToLonLat(worldPosition.worldX, worldPosition.worldY);
        if (!_viewModel.TrySetLocation(lat, lon)) return false;
        ShowLocation(lat, lon, recenter);
        return true;
    }

    /// <summary>指定座標に店舗位置マーカーを持つレイヤーを再生成します。</summary>
    private void ShowLocation(double latitude, double longitude, bool recenter = false)
    {
        if (_map is null) return;
        var result = ShopLocationLayerBuilder.Build(latitude, longitude, "SelectedLocation");

        if (_locationLayer is not null) _map.Layers.Remove(_locationLayer);
        _locationLayer = result.Layer;
        _map.Layers.Add(_locationLayer);
        if (recenter) _map.Navigator.CenterOn(result.Point);
        EditMapControl.Refresh();
    }

    /// <summary>現在設定されている座標からNominatim等のサービスで住所を取得します。</summary>
    private async Task UpdateAddressAsync()
    {
        if (!_viewModel.ShopLatitude.HasValue || !_viewModel.ShopLongitude.HasValue) return;
        try
        {
            var address = await _reverseGeocodingService.GetAddressAsync(_viewModel.ShopLatitude.Value, _viewModel.ShopLongitude.Value);
            if (!string.IsNullOrWhiteSpace(address))
            {
                _viewModel.ShopAddress = address;
                _viewModel.ErrorMessage = string.Empty;
            }
        }
        catch (Exception ex)
        {
            _viewModel.ErrorMessage = $"住所を自動取得できませんでした。住所は手入力できます。\n{ex.Message}";
        }
    }

    /// <summary>Windowsの現在地を店舗位置として設定します。</summary>
    private async void CurrentLocationButton_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.IsSaving) return;
        try
        {
            var location = await _currentLocationService.GetCurrentLocationAsync();
            if (location is null)
            {
                _viewModel.ErrorMessage = "現在地を取得できませんでした。現在地へのアクセス許可とWindowsの位置情報設定を確認してください。";
                return;
            }
            if (_viewModel.TrySetLocation(location.Latitude, location.Longitude))
            {
                ShowLocation(location.Latitude, location.Longitude, false);
                await UpdateAddressAsync();
            }
        }
        catch (Exception ex)
        {
            _viewModel.ErrorMessage = $"現在地を取得できませんでした。\n{ex.Message}";
        }
    }

    /// <summary>編集画面の地図を拡大します。</summary>
    private void ZoomInButton_Click(object sender, RoutedEventArgs e) => _map?.Navigator.ZoomIn(ZoomAmount);

    /// <summary>編集画面の地図を縮小します。</summary>
    private void ZoomOutButton_Click(object sender, RoutedEventArgs e) => _map?.Navigator.ZoomOut(ZoomAmount);
}
