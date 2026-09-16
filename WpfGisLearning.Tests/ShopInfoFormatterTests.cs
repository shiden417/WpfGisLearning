using WpfGisLearning.Models;
using WpfGisLearning.Services;

namespace WpfGisLearning.Tests;

[TestClass]
public class ShopInfoFormatterTests
{
    [TestMethod]
    public void Format_ContainsMainShopInformation()
    {
        var shop = new Shop
        {
            Id = 1,
            Name = "Test Ramen",
            RamenType = "醤油",
            Address = "東京都千代田区1-1",
            Price = 1200,
            Rating = 4.2,
            OpeningHours = "11:00-21:00",
            ClosedDay = "月曜"
        };

        var result = ShopInfoFormatter.Format(shop);

        StringAssert.Contains(result, "店舗名：Test Ramen");
        StringAssert.Contains(result, "ジャンル：醤油");
        StringAssert.Contains(result, "住所：東京都千代田区1-1");
        StringAssert.Contains(result, "価格：1,200円");
        StringAssert.Contains(result, "評価：★ 4.2");
        StringAssert.Contains(result, "営業時間：11:00-21:00");
        StringAssert.Contains(result, "定休日：月曜");
    }

    [TestMethod]
    public void Format_UsesFallbackForMissingOptionalValues()
    {
        var shop = new Shop { Id = 1, Name = "Test Ramen" };

        var result = ShopInfoFormatter.Format(shop);

        StringAssert.Contains(result, "住所：未登録");
        StringAssert.Contains(result, "営業時間：未登録");
        StringAssert.Contains(result, "定休日：未登録");
    }
}
