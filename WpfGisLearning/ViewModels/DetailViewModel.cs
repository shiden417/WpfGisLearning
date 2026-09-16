using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.IO;
using WpfGisLearning.Models;
using WpfGisLearning.Services;
using WpfGisLearning.Services.Interfaces;

namespace WpfGisLearning.ViewModels;

/// <summary>
/// 店舗詳細画面の表示状態と操作を管理するViewModelです。
/// Viewは表示やWPFイベントを担当し、店舗検索やお気に入り操作はこちらへ委譲します。
/// </summary>
public partial class DetailViewModel : ObservableObject
{
    /// <summary>店舗情報の取得やお気に入り更新を担当するサービスです。</summary>
    private readonly IShopService _shopService;

    /// <summary>店舗写真の保存先パスを解決するサービスです。</summary>
    private readonly IPhotoService _photoService;

    /// <summary>表示対象店舗のIDです。</summary>
    public int Id { get; }

    /// <summary>
    /// 同一サービスインスタンスを確認するための学習用識別子です。
    /// DIのライフタイムを確認する用途で使用します。
    /// </summary>
    public string ServiceInstanceId { get; }

    /// <summary>現在表示対象の店舗です。存在しない場合はnullです。</summary>
    public Shop? Shop { get; private set; }

    /// <summary>実際に存在する店舗写真ファイルのパス一覧です。</summary>
    public ObservableCollection<string> PhotoPaths { get; } = new();

    /// <summary>
    /// 店舗がお気に入りとして登録されているかを保持します。
    /// ObservablePropertyにより、IsFavoriteと変更通知用コードが自動生成されます。
    /// </summary>
    [ObservableProperty]
    private bool isFavorite;

    /// <summary>営業時間自体が登録されているかを判定します。</summary>
    public bool HasOpeningHours => BusinessHoursStatusCalculator.HasOpeningHours(Shop?.OpeningHours);

    /// <summary>現在時刻時点で店舗が営業中かを判定します。</summary>
    public bool IsCurrentlyOpen => HasOpeningHours && Shop is not null &&
        BusinessHoursStatusCalculator.IsOpen(Shop.OpeningHours, Shop.ClosedDay, DateTime.Now);

    /// <summary>営業時間状態を画面表示用の文字列へ変換します。</summary>
    public string BusinessHoursStatus => IsCurrentlyOpen ? "営業中" : "営業時間外";

    /// <summary>
    /// 店舗IDと必要なサービスを受け取り、詳細表示対象を読み込みます。
    /// </summary>
    public DetailViewModel(int id, IShopService shopService, IPhotoService photoService)
    {
        Id = id;
        _shopService = shopService;
        _photoService = photoService;
        ServiceInstanceId = shopService.GetHashCode().ToString();
        Reload();
    }

    /// <summary>
    /// 最新の店舗情報をサービスから取得し、関連する表示プロパティと写真一覧を更新します。
    /// </summary>
    public void Reload()
    {
        Shop = _shopService.GetShops().FirstOrDefault(shop => shop.Id == Id);
        IsFavorite = Shop?.IsFavorite ?? false;

        // 通常のプロパティを変更した場合、計算プロパティも明示的に通知する必要がある。
        OnPropertyChanged(nameof(Shop));
        OnPropertyChanged(nameof(HasOpeningHours));
        OnPropertyChanged(nameof(IsCurrentlyOpen));
        OnPropertyChanged(nameof(BusinessHoursStatus));
        LoadPhotoPaths();
    }

    /// <summary>店舗写真の中から、実際に存在するファイルだけを表示用コレクションへ読み込みます。</summary>
    private void LoadPhotoPaths()
    {
        PhotoPaths.Clear();
        if (Shop is null) return;

        foreach (var photo in Shop.Photos.OrderByDescending(x => x.IsMain).ThenBy(x => x.SortOrder))
        {
            var path = _photoService.GetPhotoPath(Shop, photo);
            if (File.Exists(path)) PhotoPaths.Add(path);
        }
    }

    /// <summary>
    /// 選択中店舗のお気に入り状態をサービス経由で反転し、画面へ通知します。
    /// RelayCommandによりToggleFavoriteCommandが自動生成されます。
    /// </summary>
    [RelayCommand]
    private void ToggleFavorite()
    {
        if (Shop is null) return;
        _shopService.ToggleFavorite(Shop.Id);
        IsFavorite = Shop.IsFavorite;
    }
}
