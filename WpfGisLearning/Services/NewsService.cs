using WpfGisLearning.Models;

namespace WpfGisLearning.Services;

public class NewsService
{
    private readonly List<NewsItem> _items =
    [
        new NewsItem { Id = "news-1", Title = "東京・丸の内に話題の新しいラーメン店がオープン", Summary = "駅から徒歩圏内に注目の新店が登場。こだわりの一杯を提供しています。", Category = "新店", Region = "東京", PublishedAt = new DateTime(2026, 9, 15), IsFeatured = true },
        new NewsItem { Id = "news-2", Title = "秋限定の味噌ラーメンが各地で登場", Summary = "秋の食材を使った季節限定メニューをチェック。", Category = "限定", Region = "全国", PublishedAt = new DateTime(2026, 9, 14), IsFeatured = true },
        new NewsItem { Id = "news-3", Title = "今月注目したいラーメン新店5選", Summary = "今月オープンした注目店をエリア別に紹介します。", Category = "特集", Region = "全国", PublishedAt = new DateTime(2026, 9, 13) },
        new NewsItem { Id = "news-4", Title = "全国ラーメンイベント開催情報", Summary = "今秋開催されるラーメン関連イベントをまとめました。", Category = "イベント", Region = "全国", PublishedAt = new DateTime(2026, 9, 12) },
        new NewsItem { Id = "news-5", Title = "大阪で新しい豚骨ラーメン店がオープン", Summary = "濃厚な豚骨スープを楽しめる新店がオープンしました。", Category = "新店", Region = "大阪", PublishedAt = new DateTime(2026, 9, 11) },
        new NewsItem { Id = "news-6", Title = "人気店の期間限定メニューを紹介", Summary = "今しか食べられない限定メニューをピックアップ。", Category = "限定", Region = "全国", PublishedAt = new DateTime(2026, 9, 10) }
    ];

    public IEnumerable<NewsItem> GetNews() => _items.OrderByDescending(x => x.PublishedAt);
}
