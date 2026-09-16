using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Serialization;

namespace WpfGisLearning.Models;

/// <summary>
/// ラーメン店1店舗分の基本情報と、お気に入り状態を表すモデルです。
/// ObservableObject を継承しているため、変更通知を WPF の DataBinding に伝えられます。
/// </summary>
public partial class Shop : ObservableObject
{
    /// <summary>店舗を一意に識別するIDです。</summary>
    public int Id { get; set; }

    /// <summary>店舗名です。</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>店舗の代表価格です。</summary>
    public decimal Price { get; set; }

    /// <summary>店舗の住所です。</summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>店舗の緯度です。</summary>
    public double Latitude { get; set; }

    /// <summary>店舗の経度です。</summary>
    public double Longitude { get; set; }

    /// <summary>ラーメンの種類です。</summary>
    public string RamenType { get; set; } = "醤油";

    /// <summary>保存されている営業時間文字列です。</summary>
    public string OpeningHours { get; set; } = string.Empty;

    /// <summary>定休日の文字列表現です。</summary>
    public string ClosedDay { get; set; } = string.Empty;

    /// <summary>0～5の範囲で表す店舗評価です。</summary>
    public double Rating { get; set; }

    /// <summary>店舗に紐づく写真一覧です。</summary>
    public List<ShopPhoto> Photos { get; set; } = [];

    /// <summary>
    /// 画面表示用のメイン写真パスです。
    /// JSONには保存せず、サービス側で写真ファイルの実パスを設定します。
    /// </summary>
    [JsonIgnore]
    public string MainPhotoPath { get; set; } = string.Empty;

    /// <summary>
    /// メイン写真を優先し、次に表示順で選択した写真です。
    /// </summary>
    public ShopPhoto? MainPhoto => Photos.OrderByDescending(x => x.IsMain).ThenBy(x => x.SortOrder).FirstOrDefault();

    /// <summary>
    /// CommunityToolkit.Mvvm が IsFavorite / IsFavoriteChanged などの通知用コードを自動生成する対象です。
    /// </summary>
    [ObservableProperty]
    private bool isFavorite;
}
