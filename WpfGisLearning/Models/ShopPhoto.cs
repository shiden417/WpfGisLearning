using System.Text.Json.Serialization;

namespace WpfGisLearning.Models;

public class ShopPhoto
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string FileName { get; set; } = string.Empty;
    public bool IsMain { get; set; }
    public int SortOrder { get; set; }

    [JsonIgnore]
    public string SourcePath { get; set; } = string.Empty;
}
