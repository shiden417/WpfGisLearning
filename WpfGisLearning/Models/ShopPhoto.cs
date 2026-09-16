using System.Text.Json.Serialization;

namespace WpfGisLearning.Models;

/// <summary>
/// 店舗写真のメタデータを表すモデルです。
/// 写真そのものではなく、ファイル名や表示順などを保持します。
/// </summary>
public class ShopPhoto
{
    /// <summary>写真を一意に識別するIDです。</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>店舗の写真フォルダー内で使用するファイル名です。</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>この写真をメイン写真として扱うかを示します。</summary>
    public bool IsMain { get; set; }

    /// <summary>一覧やスライドショーでの表示順です。</summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// 登録時に選択された元画像のパスです。
    /// DBやJSONへの永続化対象ではないため JsonIgnore を付けています。
    /// </summary>
    [JsonIgnore]
    public string SourcePath { get; set; } = string.Empty;
}
