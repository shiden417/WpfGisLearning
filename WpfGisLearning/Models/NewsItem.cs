namespace WpfGisLearning.Models;

public class NewsItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Category { get; set; } = "特集";
    public string Region { get; set; } = "全国";
    public DateTime PublishedAt { get; set; }
    public string ImagePath { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string SourceName { get; set; } = "Ramenia編集部";
    public string SourceUrl { get; set; } = string.Empty;
    public bool IsFeatured { get; set; }
    public bool HasImage => !string.IsNullOrWhiteSpace(ImageUrl);
}
