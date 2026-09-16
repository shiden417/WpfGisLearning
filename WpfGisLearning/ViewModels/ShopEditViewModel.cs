using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.IO;
using WpfGisLearning.Models;
using WpfGisLearning.Services;
using WpfGisLearning.Services.Interfaces;

namespace WpfGisLearning.ViewModels;

public partial class ShopEditViewModel : ObservableObject
{
    private readonly IShopService _shopService;
    private readonly IPhotoService _photoService;
    private readonly List<ShopPhoto> _originalPhotos = [];
    private bool _isEdit;

    public int? ShopId { get; private set; }
    public string ScreenTitle => _isEdit ? "店舗を編集" : "店舗を登録";
    public string[] RamenTypes { get; } = ["醤油", "塩", "味噌", "豚骨", "家系", "二郎系", "つけ麺", "その他"];
    public string[] OpeningHoursModes { get; } = ["未設定", "時間指定", BusinessHoursStatusCalculator.Open24Hours];
    public string[] TimeOptions { get; } = CreateTimeOptions();
    public ObservableCollection<ShopPhoto> Photos { get; } = new();

    [ObservableProperty] private string shopName = string.Empty;
    [ObservableProperty] private decimal shopPrice;
    [ObservableProperty] private string shopAddress = string.Empty;
    [ObservableProperty] private double? shopLatitude;
    [ObservableProperty] private double? shopLongitude;
    [ObservableProperty] private string ramenType = "醤油";
    [ObservableProperty] private string openingHours = string.Empty;
    [ObservableProperty] private string openingHoursMode = "未設定";
    [ObservableProperty] private string openingTime = "11:00";
    [ObservableProperty] private string closingTime = "21:00";
    [ObservableProperty] private bool closedSunday;
    [ObservableProperty] private bool closedMonday;
    [ObservableProperty] private bool closedTuesday;
    [ObservableProperty] private bool closedWednesday;
    [ObservableProperty] private bool closedThursday;
    [ObservableProperty] private bool closedFriday;
    [ObservableProperty] private bool closedSaturday;
    [ObservableProperty] private double rating;
    [ObservableProperty] private string errorMessage = string.Empty;
    [ObservableProperty] private string shopNameError = string.Empty;
    [ObservableProperty] private string shopPriceError = string.Empty;
    [ObservableProperty] private string ratingError = string.Empty;
    [ObservableProperty] private string locationError = string.Empty;

    public ShopEditViewModel(IShopService shopService, IPhotoService photoService)
    {
        _shopService = shopService;
        _photoService = photoService;
    }

    partial void OnOpeningHoursModeChanged(string value) => UpdateOpeningHours();
    partial void OnOpeningTimeChanged(string value) => UpdateOpeningHours();
    partial void OnClosingTimeChanged(string value) => UpdateOpeningHours();
    partial void OnClosedSundayChanged(bool value) => UpdateClosedDay();
    partial void OnClosedMondayChanged(bool value) => UpdateClosedDay();
    partial void OnClosedTuesdayChanged(bool value) => UpdateClosedDay();
    partial void OnClosedWednesdayChanged(bool value) => UpdateClosedDay();
    partial void OnClosedThursdayChanged(bool value) => UpdateClosedDay();
    partial void OnClosedFridayChanged(bool value) => UpdateClosedDay();
    partial void OnClosedSaturdayChanged(bool value) => UpdateClosedDay();

    public void Load(int? shopId)
    {
        ShopId = shopId;
        _isEdit = shopId.HasValue;
        ClearErrors();
        Photos.Clear();
        _originalPhotos.Clear();
        OnPropertyChanged(nameof(ScreenTitle));

        if (!shopId.HasValue)
        {
            ShopLatitude = null;
            ShopLongitude = null;
            OpeningHoursMode = "未設定";
            OpeningTime = "11:00";
            ClosingTime = "21:00";
            ClearClosedDays();
            return;
        }

        var shop = _shopService.GetShops().FirstOrDefault(x => x.Id == shopId.Value);
        if (shop is null)
        {
            ErrorMessage = "編集対象の店舗が見つかりません。";
            return;
        }

        ShopName = shop.Name;
        ShopPrice = shop.Price;
        ShopAddress = shop.Address;
        ShopLatitude = shop.Latitude;
        ShopLongitude = shop.Longitude;
        RamenType = shop.RamenType;
        SetOpeningHoursFromStoredValue(shop.OpeningHours);
        SetClosedDaysFromStoredValue(shop.ClosedDay);
        Rating = shop.Rating;

        foreach (var photo in shop.Photos.OrderBy(x => x.SortOrder))
        {
            Photos.Add(new ShopPhoto
            {
                Id = photo.Id,
                FileName = photo.FileName,
                IsMain = photo.IsMain,
                SortOrder = photo.SortOrder,
                SourcePath = _photoService.GetPhotoPath(shop, photo)
            });

            _originalPhotos.Add(new ShopPhoto
            {
                Id = photo.Id,
                FileName = photo.FileName,
                IsMain = photo.IsMain,
                SortOrder = photo.SortOrder
            });
        }
    }

    public void AddPhoto(string sourcePath)
    {
        if (string.IsNullOrWhiteSpace(sourcePath)
            || !File.Exists(sourcePath)
            || Photos.Any(x => string.Equals(x.SourcePath, sourcePath, StringComparison.OrdinalIgnoreCase))) return;

        Photos.Add(new ShopPhoto { SourcePath = sourcePath, SortOrder = Photos.Count, IsMain = Photos.Count == 0 });
        OnPropertyChanged(nameof(Photos));
    }

    public void RemovePhoto(ShopPhoto? photo)
    {
        if (photo is null) return;
        var wasMain = photo.IsMain;
        Photos.Remove(photo);
        if (wasMain && Photos.Count > 0) Photos[0].IsMain = true;
        NormalizePhotoOrder();
    }

    public void SetMainPhoto(ShopPhoto? photo)
    {
        if (photo is null) return;
        foreach (var item in Photos) item.IsMain = item == photo;
        RefreshPhotoCollection();
    }

    private void RefreshPhotoCollection()
    {
        var photos = Photos.ToList();
        Photos.Clear();
        foreach (var photo in photos) Photos.Add(photo);
    }

    public bool TrySetLocation(double latitude, double longitude)
    {
        if (!MapCoordinateValidator.IsValid(latitude, longitude)) return false;
        ShopLatitude = latitude;
        ShopLongitude = longitude;
        LocationError = string.Empty;
        ErrorMessage = string.Empty;
        return true;
    }

    [RelayCommand]
    private void Save()
    {
        ClearErrors();
        ValidateRequiredFields();
        if (!TryGetValidLocation(out var latitude, out var longitude))
            LocationError = "有効な緯度・経度を地図上で指定してください。";
        if (HasValidationErrors()) return;

        var existing = FindExistingShop();
        if (ShopId.HasValue && existing is null)
        {
            ErrorMessage = "編集対象の店舗が見つかりません。画面を閉じてもう一度お試しください。";
            return;
        }

        var shop = CreateShop(latitude, longitude, existing);
        try
        {
            SaveShop(shop);
            SavePhotos(shop);
            UpdatePhotoFileNames(shop);
            _shopService.UpdateShop(shop);
            RequestClose?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"店舗を保存できませんでした。\n{ex.Message}";
        }
    }

    private void ValidateRequiredFields()
    {
        if (string.IsNullOrWhiteSpace(ShopName)) ShopNameError = "店舗名を入力してください。";
        if (ShopPrice <= 0) ShopPriceError = "価格は1円以上で入力してください。";
        if (Rating < 0 || Rating > 5) RatingError = "評価は0～5の範囲で入力してください。";
    }

    private bool TryGetValidLocation(out double latitude, out double longitude)
    {
        latitude = ShopLatitude.GetValueOrDefault();
        longitude = ShopLongitude.GetValueOrDefault();
        return ShopLatitude.HasValue && ShopLongitude.HasValue && MapCoordinateValidator.IsValid(latitude, longitude);
    }

    private bool HasValidationErrors() => !string.IsNullOrEmpty(ShopNameError)
        || !string.IsNullOrEmpty(ShopPriceError)
        || !string.IsNullOrEmpty(RatingError)
        || !string.IsNullOrEmpty(LocationError);

    private Shop? FindExistingShop() => ShopId.HasValue
        ? _shopService.GetShops().FirstOrDefault(x => x.Id == ShopId.Value)
        : null;

    private Shop CreateShop(double latitude, double longitude, Shop? existing) => new()
    {
        Id = GetShopId(),
        Name = ShopName.Trim(),
        Price = ShopPrice,
        Address = ShopAddress.Trim(),
        Latitude = latitude,
        Longitude = longitude,
        RamenType = RamenType,
        OpeningHours = OpeningHours.Trim(),
        ClosedDay = ClosedDay.Trim(),
        Rating = Rating,
        IsFavorite = existing?.IsFavorite ?? false,
        Photos = Photos.Select((photo, index) => new ShopPhoto
        {
            Id = string.IsNullOrWhiteSpace(photo.Id) ? Guid.NewGuid().ToString("N") : photo.Id,
            FileName = photo.FileName,
            IsMain = photo.IsMain,
            SortOrder = index
        }).ToList()
    };

    private int GetShopId()
    {
        if (ShopId.HasValue) return ShopId.Value;
        var shops = _shopService.GetShops().ToList();
        return shops.Count > 0 ? shops.Max(x => x.Id) + 1 : 1;
    }

    private void SaveShop(Shop shop)
    {
        if (_isEdit) { DeleteRemovedPhotos(shop); return; }
        _shopService.AddShop(shop);
    }

    private void DeleteRemovedPhotos(Shop shop)
    {
        foreach (var original in _originalPhotos.Where(photo => Photos.All(x => x.Id != photo.Id)))
            _photoService.DeletePhoto(shop, original);
    }

    private void SavePhotos(Shop shop)
    {
        var newPhotos = Photos
            .Where(photo => File.Exists(photo.SourcePath)
                && !string.Equals(Path.GetFullPath(photo.SourcePath), Path.GetFullPath(_photoService.GetPhotoPath(shop, photo)), StringComparison.OrdinalIgnoreCase))
            .Select(photo => (photo.SourcePath, Photo: photo))
            .ToList();
        _photoService.SavePhotos(shop, newPhotos);
    }

    private void UpdatePhotoFileNames(Shop shop)
    {
        foreach (var photo in shop.Photos)
        {
            var source = Photos.FirstOrDefault(x => x.Id == photo.Id);
            if (source is not null) photo.FileName = source.FileName;
        }
    }

    private void NormalizePhotoOrder()
    {
        for (var i = 0; i < Photos.Count; i++) Photos[i].SortOrder = i;
        OnPropertyChanged(nameof(Photos));
    }

    private void UpdateOpeningHours()
    {
        OpeningHours = OpeningHoursMode switch
        {
            BusinessHoursStatusCalculator.Open24Hours => BusinessHoursStatusCalculator.Open24Hours,
            "時間指定" => OpeningTime == ClosingTime ? OpeningTime + "-" + ClosingTime : $"{OpeningTime}-{ClosingTime}",
            _ => string.Empty
        };
    }

    private void SetOpeningHoursFromStoredValue(string? value)
    {
        if (string.Equals(value?.Trim(), BusinessHoursStatusCalculator.Open24Hours, StringComparison.Ordinal))
        {
            OpeningHoursMode = BusinessHoursStatusCalculator.Open24Hours;
            return;
        }

        var match = System.Text.RegularExpressions.Regex.Match(
            value ?? string.Empty,
            @"^(?<start>\d{1,2}:\d{2})\s*(?:-|ー|−|–|〜|~)\s*(?<end>\d{1,2}:\d{2})$");

        if (match.Success && TimeOptions.Contains(match.Groups["start"].Value) && TimeOptions.Contains(match.Groups["end"].Value))
        {
            OpeningTime = match.Groups["start"].Value;
            ClosingTime = match.Groups["end"].Value;
            OpeningHoursMode = "時間指定";
            return;
        }

        OpeningHoursMode = string.IsNullOrWhiteSpace(value) ? "未設定" : "時間指定";
        if (!string.IsNullOrWhiteSpace(value) && !match.Success)
        {
            OpeningTime = "11:00";
            ClosingTime = "21:00";
        }
    }

    private void UpdateClosedDay()
    {
        ClosedDay = string.Join("・", new[]
        {
            (Enabled: ClosedSunday, Name: "日"),
            (Enabled: ClosedMonday, Name: "月"),
            (Enabled: ClosedTuesday, Name: "火"),
            (Enabled: ClosedWednesday, Name: "水"),
            (Enabled: ClosedThursday, Name: "木"),
            (Enabled: ClosedFriday, Name: "金"),
            (Enabled: ClosedSaturday, Name: "土")
        }.Where(x => x.Enabled).Select(x => x.Name));
    }

    private void SetClosedDaysFromStoredValue(string? value)
    {
        ClearClosedDays();
        var normalized = value ?? string.Empty;
        ClosedSunday = normalized.Contains("日", StringComparison.Ordinal);
        ClosedMonday = normalized.Contains("月", StringComparison.Ordinal);
        ClosedTuesday = normalized.Contains("火", StringComparison.Ordinal);
        ClosedWednesday = normalized.Contains("水", StringComparison.Ordinal);
        ClosedThursday = normalized.Contains("木", StringComparison.Ordinal);
        ClosedFriday = normalized.Contains("金", StringComparison.Ordinal);
        ClosedSaturday = normalized.Contains("土", StringComparison.Ordinal);
        UpdateClosedDay();
    }

    private void ClearClosedDays()
    {
        ClosedSunday = false;
        ClosedMonday = false;
        ClosedTuesday = false;
        ClosedWednesday = false;
        ClosedThursday = false;
        ClosedFriday = false;
        ClosedSaturday = false;
        ClosedDay = string.Empty;
    }

    private static string[] CreateTimeOptions()
    {
        var values = new List<string>();
        for (var hour = 0; hour < 24; hour++)
        {
            values.Add($"{hour:00}:00");
            values.Add($"{hour:00}:30");
        }
        return [.. values];
    }

    private void ClearErrors()
    {
        ErrorMessage = string.Empty;
        ShopNameError = string.Empty;
        ShopPriceError = string.Empty;
        RatingError = string.Empty;
        LocationError = string.Empty;
    }

    [RelayCommand]
    private void Cancel() => RequestClose?.Invoke(this, EventArgs.Empty);

    public event EventHandler? RequestClose;
}
