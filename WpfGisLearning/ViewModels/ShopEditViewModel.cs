using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.IO;
using WpfGisLearning.Models;
using WpfGisLearning.Services;
using WpfGisLearning.Services.Interfaces;
using WpfGisLearning.Validation;

namespace WpfGisLearning.ViewModels;

public partial class ShopEditViewModel : ObservableObject
{
    public const int MaxPhotoCount = 10;

    private readonly IShopService _shopService;
    private readonly IPhotoService _photoService;
    private readonly List<ShopPhoto> _originalPhotos = [];
    private bool _isEdit;
    private bool _isLoading;
    private string _initialState = string.Empty;

    public int? ShopId { get; private set; }
    public string ScreenTitle => _isEdit ? "店舗を編集" : "店舗を登録";
    public string[] RamenTypes { get; } = [.. ShopValidation.RamenTypes];
    public string[] OpeningHoursModes { get; } = [.. ShopValidation.OpeningHoursModes];
    public string[] TimeOptions { get; } = [.. ShopValidation.TimeOptions];
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
    [ObservableProperty] private string closedDay = string.Empty;
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
    [ObservableProperty] private string openingHoursError = string.Empty;
    [ObservableProperty] private string locationError = string.Empty;
    [ObservableProperty] private string photoCountError = string.Empty;
    [ObservableProperty] private bool isSaving;
    [ObservableProperty] private string saveSuccessMessage = string.Empty;

    public bool IsDirty => !string.Equals(_initialState, CreateStateFingerprint(), StringComparison.Ordinal);
    public bool CanEdit => !IsSaving;

    public ShopEditViewModel(IShopService shopService, IPhotoService photoService)
    {
        _shopService = shopService;
        _photoService = photoService;
        _initialState = CreateStateFingerprint();
    }

    partial void OnShopNameChanged(string value) => MarkDirty();
    partial void OnShopPriceChanged(decimal value) => MarkDirty();
    partial void OnShopAddressChanged(string value) => MarkDirty();
    partial void OnShopLatitudeChanged(double? value) => MarkDirty();
    partial void OnShopLongitudeChanged(double? value) => MarkDirty();
    partial void OnRamenTypeChanged(string value) => MarkDirty();
    partial void OnOpeningHoursModeChanged(string value) { UpdateOpeningHours(); MarkDirty(); }
    partial void OnOpeningTimeChanged(string value) { UpdateOpeningHours(); MarkDirty(); }
    partial void OnClosingTimeChanged(string value) { UpdateOpeningHours(); MarkDirty(); }
    partial void OnClosedSundayChanged(bool value) { UpdateClosedDay(); MarkDirty(); }
    partial void OnClosedMondayChanged(bool value) { UpdateClosedDay(); MarkDirty(); }
    partial void OnClosedTuesdayChanged(bool value) { UpdateClosedDay(); MarkDirty(); }
    partial void OnClosedWednesdayChanged(bool value) { UpdateClosedDay(); MarkDirty(); }
    partial void OnClosedThursdayChanged(bool value) { UpdateClosedDay(); MarkDirty(); }
    partial void OnClosedFridayChanged(bool value) { UpdateClosedDay(); MarkDirty(); }
    partial void OnClosedSaturdayChanged(bool value) { UpdateClosedDay(); MarkDirty(); }
    partial void OnRatingChanged(double value) => MarkDirty();
    partial void OnIsSavingChanged(bool value) => OnPropertyChanged(nameof(CanEdit));

    public void Load(int? shopId)
    {
        _isLoading = true;
        ShopId = shopId;
        _isEdit = shopId.HasValue;
        ClearErrors();
        SaveSuccessMessage = string.Empty;
        Photos.Clear();
        _originalPhotos.Clear();
        OnPropertyChanged(nameof(ScreenTitle));

        if (!shopId.HasValue)
        {
            ShopLatitude = null;
            ShopLongitude = null;
            OpeningHoursMode = ShopValidation.UnsetOpeningHoursMode;
            OpeningTime = "11:00";
            ClosingTime = "21:00";
            ClearClosedDays();
            _isLoading = false;
            CaptureInitialState();
            return;
        }

        var shop = _shopService.GetShops().FirstOrDefault(x => x.Id == shopId.Value);
        if (shop is null)
        {
            ErrorMessage = "編集対象の店舗が見つかりません。";
            _isLoading = false;
            CaptureInitialState();
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

        _isLoading = false;
        CaptureInitialState();
    }

    public void AddPhoto(string sourcePath)
    {
        if (Photos.Count >= MaxPhotoCount)
        {
            PhotoCountError = $"写真は最大{MaxPhotoCount}枚まで登録できます。";
            return;
        }

        if (string.IsNullOrWhiteSpace(sourcePath)
            || !File.Exists(sourcePath)
            || Photos.Any(x => string.Equals(x.SourcePath, sourcePath, StringComparison.OrdinalIgnoreCase))) return;

        Photos.Add(new ShopPhoto { SourcePath = sourcePath, SortOrder = Photos.Count, IsMain = Photos.Count == 0 });
        PhotoCountError = string.Empty;
        MarkDirty();
    }

    public void RemovePhoto(ShopPhoto? photo)
    {
        if (photo is null) return;
        var wasMain = photo.IsMain;
        Photos.Remove(photo);
        if (wasMain && Photos.Count > 0) Photos[0].IsMain = true;
        if (Photos.Count < MaxPhotoCount) PhotoCountError = string.Empty;
        NormalizePhotoOrder();
        MarkDirty();
    }

    public void SetMainPhoto(ShopPhoto? photo)
    {
        if (photo is null) return;
        foreach (var item in Photos) item.IsMain = item == photo;
        RefreshPhotoCollection();
        MarkDirty();
    }

    private void RefreshPhotoCollection()
    {
        var photos = Photos.ToList();
        Photos.Clear();
        foreach (var photo in photos) Photos.Add(photo);
    }

    public bool TrySetLocation(double latitude, double longitude)
    {
        if (ShopValidation.ValidateLocation(latitude, longitude) is not null) return false;
        ShopLatitude = latitude;
        ShopLongitude = longitude;
        LocationError = string.Empty;
        ErrorMessage = string.Empty;
        return true;
    }

    [RelayCommand]
    private async Task Save()
    {
        if (IsSaving) return;

        ClearErrors();
        ValidateRequiredFields();
        ValidateOpeningHours();
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
            IsSaving = true;
            SaveSuccessMessage = string.Empty;
            await Task.Run(() =>
            {
                SaveShop(shop);
                SavePhotos(shop);
                UpdatePhotoFileNames(shop);
                _shopService.UpdateShop(shop);
            });

            _isLoading = true;
            CaptureInitialState();
            _isLoading = false;
            SaveSuccessMessage = "店舗情報を保存しました。";
            RequestClose?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"店舗を保存できませんでした。\n{ex.Message}";
        }
        finally
        {
            IsSaving = false;
        }
    }

    private void ValidateRequiredFields()
    {
        ShopNameError = ShopValidation.ValidateName(ShopName) ?? string.Empty;
        ShopPriceError = ShopValidation.ValidatePrice(ShopPrice) ?? string.Empty;
        RatingError = ShopValidation.ValidateRating(Rating) ?? string.Empty;
    }

    private void ValidateOpeningHours()
    {
        OpeningHoursError = ShopValidation.ValidateOpeningHours(OpeningHoursMode, OpeningTime, ClosingTime) ?? string.Empty;
    }

    private bool TryGetValidLocation(out double latitude, out double longitude)
    {
        latitude = ShopLatitude.GetValueOrDefault();
        longitude = ShopLongitude.GetValueOrDefault();
        return ShopLatitude.HasValue
            && ShopLongitude.HasValue
            && ShopValidation.ValidateLocation(latitude, longitude) is null;
    }

    private bool HasValidationErrors() => !string.IsNullOrEmpty(ShopNameError)
        || !string.IsNullOrEmpty(ShopPriceError)
        || !string.IsNullOrEmpty(RatingError)
        || !string.IsNullOrEmpty(OpeningHoursError)
        || !string.IsNullOrEmpty(LocationError)
        || !string.IsNullOrEmpty(PhotoCountError);

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
            ShopValidation.SpecifiedOpeningHoursMode => $"{OpeningTime}-{ClosingTime}",
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
            OpeningHoursMode = ShopValidation.SpecifiedOpeningHoursMode;
            return;
        }

        OpeningHoursMode = string.IsNullOrWhiteSpace(value) ? ShopValidation.UnsetOpeningHoursMode : ShopValidation.SpecifiedOpeningHoursMode;
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

    private void ClearErrors()
    {
        ErrorMessage = string.Empty;
        ShopNameError = string.Empty;
        ShopPriceError = string.Empty;
        RatingError = string.Empty;
        OpeningHoursError = string.Empty;
        LocationError = string.Empty;
        PhotoCountError = string.Empty;
    }

    private string CreateStateFingerprint() => string.Join("|",
        ShopName.Trim(),
        ShopPrice,
        ShopAddress.Trim(),
        ShopLatitude,
        ShopLongitude,
        RamenType,
        OpeningHours,
        ClosedDay,
        Rating,
        string.Join(",", Photos.Select(x => $"{x.Id}:{x.FileName}:{x.IsMain}:{x.SortOrder}:{x.SourcePath}")));

    private void CaptureInitialState()
    {
        _initialState = CreateStateFingerprint();
        OnPropertyChanged(nameof(IsDirty));
    }

    private void MarkDirty()
    {
        if (_isLoading) return;
        OnPropertyChanged(nameof(IsDirty));
    }

    [RelayCommand]
    private void Cancel()
    {
        CaptureInitialState();
        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? RequestClose;
}
