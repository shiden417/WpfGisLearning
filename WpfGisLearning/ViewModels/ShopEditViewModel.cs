using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.IO;
using WpfGisLearning.Models;
using WpfGisLearning.Services;
using WpfGisLearning.Services.Interfaces;
using WpfGisLearning.Validation;

namespace WpfGisLearning.ViewModels;

/// <summary>
/// 店舗登録・編集画面の入力状態、入力検証、写真管理、保存処理を担当するViewModelです。
/// WPFのコントロール操作はViewへ任せ、データの編集ルールをここに集約します。
/// </summary>
public partial class ShopEditViewModel : ObservableObject
{
    /// <summary>1店舗へ登録できる写真の最大枚数です。</summary>
    public const int MaxPhotoCount = 10;

    /// <summary>店舗データの取得・保存を担当するサービスです。</summary>
    private readonly IShopService _shopService;

    /// <summary>写真ファイルの保存・削除を担当するサービスです。</summary>
    private readonly IPhotoService _photoService;

    /// <summary>編集画面を開いた時点で存在していた写真を保持し、削除対象を判定します。</summary>
    private readonly List<ShopPhoto> _originalPhotos = [];

    /// <summary>現在の画面が新規登録か編集かを示します。</summary>
    private bool _isEdit;

    /// <summary>初期値設定中にIsDirty更新を発生させないためのフラグです。</summary>
    private bool _isLoading;

    /// <summary>変更前の画面状態を文字列化して保持し、IsDirtyを判定します。</summary>
    private string _initialState = string.Empty;

    /// <summary>編集対象の店舗IDです。新規登録ではnullです。</summary>
    public int? ShopId { get; private set; }

    /// <summary>新規登録・編集に応じた画面タイトルです。</summary>
    public string ScreenTitle => _isEdit ? "店舗を編集" : "店舗を登録";

    /// <summary>画面で選択できるラーメン種別一覧です。</summary>
    public string[] RamenTypes { get; } = [.. ShopValidation.RamenTypes];

    /// <summary>営業時間モードの選択肢です。</summary>
    public string[] OpeningHoursModes { get; } = [.. ShopValidation.OpeningHoursModes];

    /// <summary>開始・終了時刻として選択できる一覧です。</summary>
    public string[] TimeOptions { get; } = [.. ShopValidation.TimeOptions];

    /// <summary>編集中の写真一覧です。</summary>
    public ObservableCollection<ShopPhoto> Photos { get; } = new();

    /// <summary>店舗名入力値です。</summary>
    [ObservableProperty] private string shopName = string.Empty;

    /// <summary>価格入力値です。</summary>
    [ObservableProperty] private decimal shopPrice;

    /// <summary>住所入力値です。</summary>
    [ObservableProperty] private string shopAddress = string.Empty;

    /// <summary>店舗位置の緯度です。</summary>
    [ObservableProperty] private double? shopLatitude;

    /// <summary>店舗位置の経度です。</summary>
    [ObservableProperty] private double? shopLongitude;

    /// <summary>選択中のラーメン種別です。</summary>
    [ObservableProperty] private string ramenType = "醤油";

    /// <summary>保存対象となる営業時間文字列です。</summary>
    [ObservableProperty] private string openingHours = string.Empty;

    /// <summary>営業時間の入力モードです。</summary>
    [ObservableProperty] private string openingHoursMode = "未設定";

    /// <summary>営業時間の開始時刻です。</summary>
    [ObservableProperty] private string openingTime = "11:00";

    /// <summary>営業時間の終了時刻です。</summary>
    [ObservableProperty] private string closingTime = "21:00";

    /// <summary>保存対象となる定休日文字列です。</summary>
    [ObservableProperty] private string closedDay = string.Empty;

    /// <summary>日曜日を定休日にするかを示します。</summary>
    [ObservableProperty] private bool closedSunday;

    /// <summary>月曜日を定休日にするかを示します。</summary>
    [ObservableProperty] private bool closedMonday;

    /// <summary>火曜日を定休日にするかを示します。</summary>
    [ObservableProperty] private bool closedTuesday;

    /// <summary>水曜日を定休日にするかを示します。</summary>
    [ObservableProperty] private bool closedWednesday;

    /// <summary>木曜日を定休日にするかを示します。</summary>
    [ObservableProperty] private bool closedThursday;

    /// <summary>金曜日を定休日にするかを示します。</summary>
    [ObservableProperty] private bool closedFriday;

    /// <summary>土曜日を定休日にするかを示します。</summary>
    [ObservableProperty] private bool closedSaturday;

    /// <summary>店舗評価です。</summary>
    [ObservableProperty] private double rating;

    /// <summary>画面全体のエラーメッセージです。</summary>
    [ObservableProperty] private string errorMessage = string.Empty;

    /// <summary>店舗名入力に関するエラーメッセージです。</summary>
    [ObservableProperty] private string shopNameError = string.Empty;

    /// <summary>価格入力に関するエラーメッセージです。</summary>
    [ObservableProperty] private string shopPriceError = string.Empty;

    /// <summary>評価入力に関するエラーメッセージです。</summary>
    [ObservableProperty] private string ratingError = string.Empty;

    /// <summary>営業時間入力に関するエラーメッセージです。</summary>
    [ObservableProperty] private string openingHoursError = string.Empty;

    /// <summary>位置情報入力に関するエラーメッセージです。</summary>
    [ObservableProperty] private string locationError = string.Empty;

    /// <summary>写真枚数に関するエラーメッセージです。</summary>
    [ObservableProperty] private string photoCountError = string.Empty;

    /// <summary>現在保存処理中かを示し、二重保存や入力を防止します。</summary>
    [ObservableProperty] private bool isSaving;

    /// <summary>保存成功時に表示するメッセージです。</summary>
    [ObservableProperty] private string saveSuccessMessage = string.Empty;

    /// <summary>画面を開いた時点の状態と現在の状態が異なるかを示します。</summary>
    public bool IsDirty => !string.Equals(_initialState, CreateStateFingerprint(), StringComparison.Ordinal);

    /// <summary>保存中でなければ編集可能であることを示します。</summary>
    public bool CanEdit => !IsSaving;

    /// <summary>必要なサービスをDIから受け取ってViewModelを生成します。</summary>
    public ShopEditViewModel(IShopService shopService, IPhotoService photoService)
    {
        _shopService = shopService;
        _photoService = photoService;
        _initialState = CreateStateFingerprint();
    }

    /// <summary>店舗名変更時に未保存状態を更新します。</summary>
    partial void OnShopNameChanged(string value) => MarkDirty();

    /// <summary>価格変更時に未保存状態を更新します。</summary>
    partial void OnShopPriceChanged(decimal value) => MarkDirty();

    /// <summary>住所変更時に未保存状態を更新します。</summary>
    partial void OnShopAddressChanged(string value) => MarkDirty();

    /// <summary>緯度変更時に未保存状態を更新します。</summary>
    partial void OnShopLatitudeChanged(double? value) => MarkDirty();

    /// <summary>経度変更時に未保存状態を更新します。</summary>
    partial void OnShopLongitudeChanged(double? value) => MarkDirty();

    /// <summary>ラーメン種別変更時に未保存状態を更新します。</summary>
    partial void OnRamenTypeChanged(string value) => MarkDirty();

    /// <summary>営業時間モード変更時に保存用文字列を更新します。</summary>
    partial void OnOpeningHoursModeChanged(string value) { UpdateOpeningHours(); MarkDirty(); }

    /// <summary>開始時刻変更時に保存用文字列を更新します。</summary>
    partial void OnOpeningTimeChanged(string value) { UpdateOpeningHours(); MarkDirty(); }

    /// <summary>終了時刻変更時に保存用文字列を更新します。</summary>
    partial void OnClosingTimeChanged(string value) { UpdateOpeningHours(); MarkDirty(); }

    /// <summary>日曜定休日変更時に保存用文字列を更新します。</summary>
    partial void OnClosedSundayChanged(bool value) { UpdateClosedDay(); MarkDirty(); }

    /// <summary>月曜定休日変更時に保存用文字列を更新します。</summary>
    partial void OnClosedMondayChanged(bool value) { UpdateClosedDay(); MarkDirty(); }

    /// <summary>火曜定休日変更時に保存用文字列を更新します。</summary>
    partial void OnClosedTuesdayChanged(bool value) { UpdateClosedDay(); MarkDirty(); }

    /// <summary>水曜定休日変更時に保存用文字列を更新します。</summary>
    partial void OnClosedWednesdayChanged(bool value) { UpdateClosedDay(); MarkDirty(); }

    /// <summary>木曜定休日変更時に保存用文字列を更新します。</summary>
    partial void OnClosedThursdayChanged(bool value) { UpdateClosedDay(); MarkDirty(); }

    /// <summary>金曜定休日変更時に保存用文字列を更新します。</summary>
    partial void OnClosedFridayChanged(bool value) { UpdateClosedDay(); MarkDirty(); }

    /// <summary>土曜定休日変更時に保存用文字列を更新します。</summary>
    partial void OnClosedSaturdayChanged(bool value) { UpdateClosedDay(); MarkDirty(); }

    /// <summary>評価変更時に未保存状態を更新します。</summary>
    partial void OnRatingChanged(double value) => MarkDirty();

    /// <summary>保存状態変更時に編集可否の変更を通知します。</summary>
    partial void OnIsSavingChanged(bool value) => OnPropertyChanged(nameof(CanEdit));

    /// <summary>
    /// 新規登録または既存店舗の編集データを画面へ読み込み、初期状態を記録します。
    /// </summary>
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

    /// <summary>新しい写真を一覧へ追加し、枚数上限・重複・ファイル存在を確認します。</summary>
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

    /// <summary>写真を一覧から削除し、必要なら別の写真をメインにします。</summary>
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

    /// <summary>指定写真だけをメイン写真として設定します。</summary>
    public void SetMainPhoto(ShopPhoto? photo)
    {
        if (photo is null) return;
        foreach (var item in Photos) item.IsMain = item == photo;
        RefreshPhotoCollection();
        MarkDirty();
    }

    /// <summary>ObservableCollectionを再構築して、画像の表示更新をWPFへ通知します。</summary>
    private void RefreshPhotoCollection()
    {
        var photos = Photos.ToList();
        Photos.Clear();
        foreach (var photo in photos) Photos.Add(photo);
    }

    /// <summary>指定座標が有効ならViewModelへ設定し、関連するエラーを消します。</summary>
    public bool TrySetLocation(double latitude, double longitude)
    {
        if (ShopValidation.ValidateLocation(latitude, longitude) is not null) return false;
        ShopLatitude = latitude;
        ShopLongitude = longitude;
        LocationError = string.Empty;
        ErrorMessage = string.Empty;
        return true;
    }

    /// <summary>
    /// 入力検証後に店舗と写真を保存します。
    /// RelayCommandによりSaveCommandが自動生成されます。
    /// </summary>
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

            // ファイルコピーなどUIスレッドを長時間占有する処理をバックグラウンドへ移す。
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

    /// <summary>店舗名・価格・評価の必須入力をまとめて検証します。</summary>
    private void ValidateRequiredFields()
    {
        ShopNameError = ShopValidation.ValidateName(ShopName) ?? string.Empty;
        ShopPriceError = ShopValidation.ValidatePrice(ShopPrice) ?? string.Empty;
        RatingError = ShopValidation.ValidateRating(Rating) ?? string.Empty;
    }

    /// <summary>営業時間モードと時刻の組み合わせを検証します。</summary>
    private void ValidateOpeningHours()
    {
        OpeningHoursError = ShopValidation.ValidateOpeningHours(OpeningHoursMode, OpeningTime, ClosingTime) ?? string.Empty;
    }

    /// <summary>現在の緯度・経度を検証し、out引数へ数値を返します。</summary>
    private bool TryGetValidLocation(out double latitude, out double longitude)
    {
        latitude = ShopLatitude.GetValueOrDefault();
        longitude = ShopLongitude.GetValueOrDefault();
        return ShopLatitude.HasValue
            && ShopLongitude.HasValue
            && ShopValidation.ValidateLocation(latitude, longitude) is null;
    }

    /// <summary>現在の各検証エラーのどれかが設定されているかを判定します。</summary>
    private bool HasValidationErrors() => !string.IsNullOrEmpty(ShopNameError)
        || !string.IsNullOrEmpty(ShopPriceError)
        || !string.IsNullOrEmpty(RatingError)
        || !string.IsNullOrEmpty(OpeningHoursError)
        || !string.IsNullOrEmpty(LocationError)
        || !string.IsNullOrEmpty(PhotoCountError);

    /// <summary>編集対象IDに一致する現在の店舗をサービスから取得します。</summary>
    private Shop? FindExistingShop() => ShopId.HasValue
        ? _shopService.GetShops().FirstOrDefault(x => x.Id == ShopId.Value)
        : null;

    /// <summary>画面入力から保存用Shopモデルを生成します。編集時はお気に入り状態を引き継ぎます。</summary>
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

    /// <summary>新規店舗用IDを最大ID+1で生成し、編集時は既存IDを返します。</summary>
    private int GetShopId()
    {
        if (ShopId.HasValue) return ShopId.Value;
        var shops = _shopService.GetShops().ToList();
        return shops.Count > 0 ? shops.Max(x => x.Id) + 1 : 1;
    }

    /// <summary>新規登録か編集かに応じて店舗保存前の写真削除・店舗登録を実行します。</summary>
    private void SaveShop(Shop shop)
    {
        if (_isEdit) { DeleteRemovedPhotos(shop); return; }
        _shopService.AddShop(shop);
    }

    /// <summary>編集前には存在したが、現在の写真一覧から消えた写真ファイルを削除します。</summary>
    private void DeleteRemovedPhotos(Shop shop)
    {
        foreach (var original in _originalPhotos.Where(photo => Photos.All(x => x.Id != photo.Id)))
            _photoService.DeletePhoto(shop, original);
    }

    /// <summary>新しく選択された写真だけを写真サービスへ渡してファイルコピーします。</summary>
    private void SavePhotos(Shop shop)
    {
        var newPhotos = Photos
            .Where(photo => File.Exists(photo.SourcePath)
                && !string.Equals(Path.GetFullPath(photo.SourcePath), Path.GetFullPath(_photoService.GetPhotoPath(shop, photo)), StringComparison.OrdinalIgnoreCase))
            .Select(photo => (photo.SourcePath, Photo: photo))
            .ToList();
        _photoService.SavePhotos(shop, newPhotos);
    }

    /// <summary>写真サービスが決定したファイル名を保存用Shopモデルへ反映します。</summary>
    private void UpdatePhotoFileNames(Shop shop)
    {
        foreach (var photo in shop.Photos)
        {
            var source = Photos.FirstOrDefault(x => x.Id == photo.Id);
            if (source is not null) photo.FileName = source.FileName;
        }
    }

    /// <summary>写真のSortOrderを0始まりで振り直します。</summary>
    private void NormalizePhotoOrder()
    {
        for (var i = 0; i < Photos.Count; i++) Photos[i].SortOrder = i;
        OnPropertyChanged(nameof(Photos));
    }

    /// <summary>営業時間の入力項目から保存用文字列を組み立てます。</summary>
    private void UpdateOpeningHours()
    {
        OpeningHours = OpeningHoursMode switch
        {
            BusinessHoursStatusCalculator.Open24Hours => BusinessHoursStatusCalculator.Open24Hours,
            ShopValidation.SpecifiedOpeningHoursMode => $"{OpeningTime}-{ClosingTime}",
            _ => string.Empty
        };
    }

    /// <summary>保存済み営業時間を画面のモード・開始・終了時刻へ分解します。</summary>
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

    /// <summary>曜日チェックボックスの状態から保存用の定休日文字列を組み立てます。</summary>
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

    /// <summary>保存済みの定休日文字列から各曜日チェックボックスを復元します。</summary>
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

    /// <summary>全曜日の定休日チェック状態と保存用文字列を初期化します。</summary>
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

    /// <summary>画面上に表示しているすべての入力エラーを消します。</summary>
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

    /// <summary>
    /// 現在の入力値を比較可能な文字列へ変換します。
    /// ObservablePropertyを含む画面状態全体をIsDirty判定に利用します。
    /// </summary>
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

    /// <summary>現在の入力状態を「保存済みの初期状態」として記録します。</summary>
    private void CaptureInitialState()
    {
        _initialState = CreateStateFingerprint();
        OnPropertyChanged(nameof(IsDirty));
    }

    /// <summary>
    /// 入力値変更時にIsDirtyを通知します。
    /// Load中は初期値設定による変更通知を無視します。
    /// </summary>
    private void MarkDirty()
    {
        if (_isLoading) return;
        OnPropertyChanged(nameof(IsDirty));
    }

    /// <summary>変更を破棄して画面を閉じる要求を発生させます。</summary>
    [RelayCommand]
    private void Cancel()
    {
        CaptureInitialState();
        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Viewへ画面を閉じるよう通知するイベントです。</summary>
    public event EventHandler? RequestClose;
}
