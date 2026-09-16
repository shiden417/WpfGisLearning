using System.Globalization;
using WpfGisLearning.Models;

namespace WpfGisLearning.Services;

public static class ShopInfoFormatter
{
    public static string Format(Shop shop)
    {
        ArgumentNullException.ThrowIfNull(shop);

        return $"店舗名：{shop.Name}\n" +
               $"ジャンル：{shop.RamenType}\n" +
               $"住所：{(string.IsNullOrWhiteSpace(shop.Address) ? "未登録" : shop.Address)}\n" +
               $"価格：{shop.Price.ToString("N0", CultureInfo.InvariantCulture)}円\n" +
               $"評価：★ {shop.Rating:F1}\n" +
               $"営業時間：{(string.IsNullOrWhiteSpace(shop.OpeningHours) ? "未登録" : shop.OpeningHours)}\n" +
               $"定休日：{(string.IsNullOrWhiteSpace(shop.ClosedDay) ? "未登録" : shop.ClosedDay)}";
    }
}
