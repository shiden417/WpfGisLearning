using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.IO;
using WpfGisLearning.Models;
using WpfGisLearning.Services;

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
    public ObservableCollection<ShopPhoto> Photos { get; } = new();
    [ObservableProperty] private string shopName = string.Empty;
    [ObservableProperty] private decimal shopPrice;
    [ObservableProperty] private string shopAddress = string.Empty;
    [ObservableProperty] private double? shopLatitude;
    [ObservableProperty] private double? shopLongitude;
    [ObservableProperty] private string ramenType = "醤油";
    [ObservableProperty] private string openingHours = string.Empty;
    [ObservableProperty] private string closedDay = string.Empty;
    [ObservableProperty] private double rating;
    [ObservableProperty] private string errorMessage = string.Empty;
    [ObservableProperty] private string shopNameError = string.Empty;
    [ObservableProperty] private string shopPriceError = string.Empty;
    [ObservableProperty] private string ratingError = string.Empty;
    [ObservableProperty] private string locationError = string.Empty;

    public ShopEditViewModel(IShopService shopService, IPhotoService photoService) { _shopService = shopService; _photoService = photoService; }
    public void Load(int? shopId)
    {
        ShopId = shopId; _isEdit = shopId.HasValue; ClearErrors(); Photos.Clear(); _originalPhotos.Clear(); OnPropertyChanged(nameof(ScreenTitle));
        if (!shopId.HasValue) { ShopLatitude = null; ShopLongitude = null; return; }
        var shop = _shopService.GetShops().FirstOrDefault(x => x.Id == shopId.Value); if (shop is null) { ErrorMessage = "編集対象の店舗が見つかりません。"; return; }
        ShopName = shop.Name; ShopPrice = shop.Price; ShopAddress = shop.Address; ShopLatitude = shop.Latitude; ShopLongitude = shop.Longitude; RamenType = shop.RamenType; OpeningHours = shop.OpeningHours; ClosedDay = shop.ClosedDay; Rating = shop.Rating;
        foreach (var photo in shop.Photos.OrderBy(x => x.SortOrder)) { Photos.Add(new ShopPhoto { Id = photo.Id, FileName = photo.FileName, IsMain = photo.IsMain, SortOrder = photo.SortOrder, SourcePath = _photoService.GetPhotoPath(shop, photo) }); _originalPhotos.Add(new ShopPhoto { Id = photo.Id, FileName = photo.FileName, IsMain = photo.IsMain, SortOrder = photo.SortOrder }); }
    }
    public void AddPhoto(string sourcePath) { if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath) || Photos.Any(x => string.Equals(x.SourcePath, sourcePath, StringComparison.OrdinalIgnoreCase))) return; Photos.Add(new ShopPhoto { SourcePath = sourcePath, SortOrder = Photos.Count, IsMain = Photos.Count == 0 }); OnPropertyChanged(nameof(Photos)); }
    public void RemovePhoto(ShopPhoto? photo) { if (photo is null) return; var wasMain = photo.IsMain; Photos.Remove(photo); if (wasMain && Photos.Count > 0) Photos[0].IsMain = true; NormalizePhotoOrder(); }
    public void SetMainPhoto(ShopPhoto? photo) { if (photo is null) return; foreach (var item in Photos) item.IsMain = item == photo; OnPropertyChanged(nameof(Photos)); }
    public bool TrySetLocation(double latitude, double longitude) { if (!IsValidCoordinate(latitude, longitude)) return false; ShopLatitude = latitude; ShopLongitude = longitude; LocationError = string.Empty; ErrorMessage = string.Empty; return true; }

    [RelayCommand]
    private void Save()
    {
        ClearErrors();
        if (string.IsNullOrWhiteSpace(ShopName)) ShopNameError = "店舗名を入力してください。";
        if (ShopPrice <= 0) ShopPriceError = "価格は1円以上で入力してください。";
        if (Rating < 0 || Rating > 5) RatingError = "評価は0～5の範囲で入力してください。";
        if (!ShopLatitude.HasValue || !ShopLongitude.HasValue || !IsValidCoordinate(ShopLatitude.Value, ShopLongitude.Value)) LocationError = "有効な緯度・経度を地図上で指定してください。";
        if (!string.IsNullOrEmpty(ShopNameError) || !string.IsNullOrEmpty(ShopPriceError) || !string.IsNullOrEmpty(RatingError) || !string.IsNullOrEmpty(LocationError)) return;

        var existing = ShopId.HasValue ? _shopService.GetShops().FirstOrDefault(x => x.Id == ShopId.Value) : null;
        if (ShopId.HasValue && existing is null) { ErrorMessage = "編集対象の店舗が見つかりません。画面を閉じてもう一度お試しください。"; return; }
        var shop = new Shop { Id = ShopId ?? (_shopService.GetShops().Any() ? _shopService.GetShops().Max(x => x.Id) + 1 : 1), Name = ShopName.Trim(), Price = ShopPrice, Address = ShopAddress.Trim(), Latitude = ShopLatitude.Value, Longitude = ShopLongitude.Value, RamenType = RamenType, OpeningHours = OpeningHours.Trim(), ClosedDay = ClosedDay.Trim(), Rating = Rating, IsFavorite = existing?.IsFavorite ?? false, Photos = Photos.Select((p, index) => new ShopPhoto { Id = string.IsNullOrWhiteSpace(p.Id) ? Guid.NewGuid().ToString("N") : p.Id, FileName = p.FileName, IsMain = p.IsMain, SortOrder = index }).ToList() };
        try
        {
            if (_isEdit) { foreach (var original in _originalPhotos.Where(p => Photos.All(x => x.Id != p.Id))) _photoService.DeletePhoto(shop, original); _shopService.UpdateShop(shop); } else _shopService.AddShop(shop);
            var newPhotos = Photos.Where(p => File.Exists(p.SourcePath) && !string.Equals(Path.GetFullPath(p.SourcePath), Path.GetFullPath(_photoService.GetPhotoPath(shop, p)), StringComparison.OrdinalIgnoreCase)).Select(p => (p.SourcePath, p)).ToList();
            _photoService.SavePhotos(shop, newPhotos);
            foreach (var photo in shop.Photos) { var source = Photos.FirstOrDefault(x => x.Id == photo.Id); if (source is not null) photo.FileName = source.FileName; }
            _shopService.UpdateShop(shop); RequestClose?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex) { ErrorMessage = $"店舗を保存できませんでした。\n{ex.Message}"; }
    }
    private static bool IsValidCoordinate(double latitude, double longitude) => !double.IsNaN(latitude) && !double.IsNaN(longitude) && !double.IsInfinity(latitude) && !double.IsInfinity(longitude) && latitude >= -90 && latitude <= 90 && longitude >= -180 && longitude <= 180 && !(latitude == 0 && longitude == 0);
    private void NormalizePhotoOrder() { for (var i = 0; i < Photos.Count; i++) Photos[i].SortOrder = i; OnPropertyChanged(nameof(Photos)); }
    private void ClearErrors() { ErrorMessage = string.Empty; ShopNameError = string.Empty; ShopPriceError = string.Empty; RatingError = string.Empty; LocationError = string.Empty; }
    [RelayCommand] private void Cancel() => RequestClose?.Invoke(this, EventArgs.Empty);
    public event EventHandler? RequestClose;
}
