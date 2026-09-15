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
using WpfGisLearning.ViewModels;

namespace WpfGisLearning.Views;

public partial class ShopEditView : System.Windows.Controls.UserControl
{
    private const long InitialShopLocationResolution = 500;
    private const double JapanOverviewLongitude = 138.0;
    private const double JapanOverviewLatitude = 36.0;
    private const long JapanOverviewResolution = 6000;
    private const double LocationMarkerScale = 1.15;
    private const long ZoomAmount = 500;

    private readonly ShopEditViewModel _viewModel;
    private readonly ICurrentLocationService _currentLocationService;
    private Mapsui.Map? _map;
    private MemoryLayer? _locationLayer;
    private bool _isDraggingLocation;

    public ShopEditView(ShopEditViewModel viewModel, ICurrentLocationService currentLocationService)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _currentLocationService = currentLocationService;
        DataContext = viewModel;
        viewModel.RequestClose += ViewModel_RequestClose;
        Loaded += ShopEditView_Loaded;
        EditMapControl.PreviewMouseLeftButtonDown += EditMapControl_PreviewMouseLeftButtonDown;
        EditMapControl.PreviewMouseMove += EditMapControl_PreviewMouseMove;
        EditMapControl.PreviewMouseLeftButtonUp += EditMapControl_PreviewMouseLeftButtonUp;
    }

    private void AddPhotoButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "店舗写真を選択",
            Filter = "画像ファイル|*.jpg;*.jpeg;*.png;*.webp;*.bmp|すべてのファイル|*.*",
            Multiselect = true
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        foreach (var file in dialog.FileNames)
        {
            _viewModel.AddPhoto(file);
        }
    }

    private void RemovePhotoButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button { Tag: ShopPhoto photo })
        {
            _viewModel.RemovePhoto(photo);
        }
    }

    private void SetMainPhotoButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button { Tag: ShopPhoto photo })
        {
            _viewModel.SetMainPhoto(photo);
        }
    }

    private void ShopEditView_Loaded(object sender, RoutedEventArgs e)
    {
        if (_map is not null)
        {
            return;
        }

        _map = new Mapsui.Map();
        _map.Layers.Add(OpenStreetMap.CreateTileLayer());
        EditMapControl.Map = _map;

        if (_viewModel.ShopId.HasValue
            && _viewModel.ShopLatitude.HasValue
            && _viewModel.ShopLongitude.HasValue)
        {
            var latitude = _viewModel.ShopLatitude.Value;
            var longitude = _viewModel.ShopLongitude.Value;
            ShowLocation(latitude, longitude);

            var point = SphericalMercator
                .FromLonLat(longitude, latitude)
                .ToMPoint();
            _map.Navigator.CenterOnAndZoomTo(point, InitialShopLocationResolution);
            return;
        }

        SetJapanOverview();
    }

    private void SetJapanOverview()
    {
        var point = SphericalMercator
            .FromLonLat(JapanOverviewLongitude, JapanOverviewLatitude)
            .ToMPoint();
        _map?.Navigator.CenterOnAndZoomTo(point, JapanOverviewResolution);
    }

    private void EditMapControl_PreviewMouseLeftButtonDown(object? sender, MouseButtonEventArgs e)
    {
        if (_map is null)
        {
            return;
        }

        var position = e.GetPosition(EditMapControl);
        if (e.ClickCount == 2)
        {
            SetLocationFromScreen(position.X, position.Y, false);
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
        if (!_isDraggingLocation || _map is null)
        {
            return;
        }

        var position = e.GetPosition(EditMapControl);
        SetLocationFromScreen(position.X, position.Y, false);
        e.Handled = true;
    }

    private void EditMapControl_PreviewMouseLeftButtonUp(object? sender, MouseButtonEventArgs e)
    {
        if (!_isDraggingLocation)
        {
            return;
        }

        _isDraggingLocation = false;
        EditMapControl.ReleaseMouseCapture();

        var position = e.GetPosition(EditMapControl);
        SetLocationFromScreen(position.X, position.Y, false);
        e.Handled = true;
    }

    private bool HasLocationAt(double x, double y)
    {
        if (_locationLayer is null)
        {
            return false;
        }

        var mapInfo = EditMapControl.GetMapInfo(
            new Mapsui.Manipulations.ScreenPosition((int)x, (int)y),
            new[] { _locationLayer });

        return mapInfo?.Feature is not null;
    }

    private void SetLocationFromScreen(double x, double y, bool recenter)
    {
        if (_map is null)
        {
            return;
        }

        var screenPosition = new Mapsui.Manipulations.ScreenPosition((int)x, (int)y);
        var worldPosition = _map.Navigator.Viewport.ScreenToWorld(screenPosition);
        var lonLat = SphericalMercator.ToLonLat(worldPosition);

        if (_viewModel.TrySetLocation(lonLat.Y, lonLat.X))
        {
            ShowLocation(lonLat.Y, lonLat.X, recenter);
        }
    }

    private void ShowLocation(double latitude, double longitude, bool recenter = false)
    {
        if (_map is null)
        {
            return;
        }

        var point = SphericalMercator
            .FromLonLat(longitude, latitude)
            .ToMPoint();
        var feature = new PointFeature(point);
        feature.Styles.Add(ImageStyles.CreatePinStyle(
            Mapsui.Styles.Color.FromString("#C56B4D"),
            Mapsui.Styles.Color.FromString("#343A40"),
            LocationMarkerScale));

        if (_locationLayer is not null)
        {
            _map.Layers.Remove(_locationLayer);
        }

        _locationLayer = new MemoryLayer
        {
            Name = "SelectedLocation",
            Features = new[] { feature }
        };
        _map.Layers.Add(_locationLayer);

        if (recenter)
        {
            _map.Navigator.CenterOn(point);
        }

        EditMapControl.Refresh();
    }

    private async void CurrentLocationButton_Click(object sender, RoutedEventArgs e)
    {
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
            }
        }
        catch (Exception ex)
        {
            _viewModel.ErrorMessage = $"現在地を取得できませんでした。\n{ex.Message}";
        }
    }

    private void ZoomInButton_Click(object sender, RoutedEventArgs e)
    {
        _map?.Navigator.ZoomIn(ZoomAmount);
    }

    private void ZoomOutButton_Click(object sender, RoutedEventArgs e)
    {
        _map?.Navigator.ZoomOut(ZoomAmount);
    }

    private void ViewModel_RequestClose(object? sender, EventArgs e)
    {
        Window.GetWindow(this)?.Close();
    }
}
