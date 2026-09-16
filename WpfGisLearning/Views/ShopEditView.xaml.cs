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

public partial class ShopEditView : System.Windows.Controls.UserControl
{
    private const long InitialShopLocationResolution = 500;
    private const double JapanOverviewLongitude = 138.0;
    private const double JapanOverviewLatitude = 36.0;
    private const long JapanOverviewResolution = 6000;
    private const long ZoomAmount = 500;

    private readonly ShopEditViewModel _viewModel;
    private readonly ICurrentLocationService _currentLocationService;
    private readonly IReverseGeocodingService _reverseGeocodingService;
    private Mapsui.Map? _map;
    private MemoryLayer? _locationLayer;
    private bool _isDraggingLocation;
    private Window? _hostWindow;
    private bool _allowWindowClose;

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

    private void AddPhotoButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Title = "店舗写真を選択", Filter = "画像ファイル|*.jpg;*.jpeg;*.png;*.webp;*.bmp|すべてのファイル|*.*", Multiselect = true };
        if (dialog.ShowDialog() != true) return;
        foreach (var file in dialog.FileNames) _viewModel.AddPhoto(file);
    }

    private void RemovePhotoButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button { Tag: ShopPhoto photo }) _viewModel.RemovePhoto(photo);
    }

    private void SetMainPhotoButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button { Tag: ShopPhoto photo }) _viewModel.SetMainPhoto(photo);
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (System.Windows.Controls.Validation.GetHasError(ShopPriceTextBox) ||
            System.Windows.Controls.Validation.GetHasError(RatingTextBox))
            return;

        _viewModel.SaveCommand.Execute(null);
    }

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
            var point = SphericalMercator.FromLonLat(longitude, latitude).ToMPoint();
            _map.Navigator.CenterOnAndZoomTo(point, InitialShopLocationResolution);
            return;
        }
        SetJapanOverview();
    }

    private void AttachHostWindow()
    {
        var window = Window.GetWindow(this);
        if (window is null || ReferenceEquals(_hostWindow, window)) return;
        _hostWindow = window;
        _hostWindow.Closing += HostWindow_Closing;
    }

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

    private void ViewModel_RequestClose(object? sender, EventArgs e)
    {
        if (_hostWindow is null)
        {
            _hostWindow = Window.GetWindow(this);
            if (_hostWindow is null) return;
        }

        if (_viewModel.IsSaving) return;

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

    private void SetJapanOverview()
    {
        var point = SphericalMercator.FromLonLat(JapanOverviewLongitude, JapanOverviewLatitude).ToMPoint();
        _map?.Navigator.CenterOnAndZoomTo(point, JapanOverviewResolution);
    }

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

    private void EditMapControl_PreviewMouseMove(object? sender, MouseEventArgs e)
    {
        if (!_isDraggingLocation || _map is null || _viewModel.IsSaving) return;
        var position = e.GetPosition(EditMapControl);
        SetLocationFromScreen(position.X, position.Y, false);
        e.Handled = true;
    }

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

    private bool HasLocationAt(double x, double y)
    {
        if (_locationLayer is null) return false;
        var mapInfo = EditMapControl.GetMapInfo(new Mapsui.Manipulations.ScreenPosition((int)x, (int)y), new[] { _locationLayer });
        return mapInfo?.Feature is not null;
    }

    private bool SetLocationFromScreen(double x, double y, bool recenter)
    {
        if (_map is null) return false;
        var screenPosition = new Mapsui.Manipulations.ScreenPosition((int)x, (int)y);
        var worldPosition = _map.Navigator.Viewport.ScreenToWorld(screenPosition);
        var lonLat = SphericalMercator.ToLonLat(worldPosition);
        if (!_viewModel.TrySetLocation(lonLat.Y, lonLat.X)) return false;
        ShowLocation(lonLat.Y, lonLat.X, recenter);
        return true;
    }

    private void ShowLocation(double latitude, double longitude, bool recenter = false)
    {
        if (_map is null) return;
        var point = SphericalMercator.FromLonLat(longitude, latitude).ToMPoint();
        var feature = new PointFeature(point);
        feature.Styles.Add(MapMarkerStyleFactory.CreateShopMarker(selected: false));

        if (_locationLayer is not null) _map.Layers.Remove(_locationLayer);
        _locationLayer = new MemoryLayer { Name = "SelectedLocation", Style = null, Features = new[] { feature } };
        _map.Layers.Add(_locationLayer);
        if (recenter) _map.Navigator.CenterOn(point);
        EditMapControl.Refresh();
    }

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

    private void ZoomInButton_Click(object sender, RoutedEventArgs e) => _map?.Navigator.ZoomIn(ZoomAmount);
    private void ZoomOutButton_Click(object sender, RoutedEventArgs e) => _map?.Navigator.ZoomOut(ZoomAmount);
}
