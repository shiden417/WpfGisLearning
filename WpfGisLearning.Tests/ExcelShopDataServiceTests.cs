using ClosedXML.Excel;
using WpfGisLearning.Models;
using WpfGisLearning.Services;

namespace WpfGisLearning.Tests;

[TestClass]
public class ExcelShopDataServiceTests
{
    private readonly ExcelShopDataService _service = new();

    [TestMethod]
    public void ExportAndImport_RoundTripsShopData()
    {
        var path = CreateTempPath();
        try
        {
            var shops = new[]
            {
                new Shop
                {
                    Id = 7,
                    Name = "テスト店",
                    Price = 1200,
                    Address = "東京都千代田区",
                    Latitude = 35.681236,
                    Longitude = 139.767125,
                    RamenType = "醤油",
                    OpeningHours = "11:00-21:00",
                    ClosedDay = "火曜日",
                    Rating = 4.3
                }
            };

            _service.Export(path, shops);
            var imported = _service.Import(path);

            Assert.HasCount(1, imported);
            var shop = imported.Single();
            Assert.AreEqual(7, shop.Id);
            Assert.AreEqual("テスト店", shop.Name);
            Assert.AreEqual(1200m, shop.Price);
            Assert.AreEqual("醤油", shop.RamenType);
            Assert.AreEqual("11:00-21:00", shop.OpeningHours);
            Assert.AreEqual(4.3, shop.Rating, 0.0001);
        }
        finally
        {
            DeleteTempFile(path);
        }
    }

    [TestMethod]
    public void Import_BlankIdKeepsZeroForMergeStep()
    {
        var path = CreateWorkbookWithRows(
            [
                [1, "既存", 1000, "住所", 35.0, 139.0, "醤油", "未設定", "", "", "", 4.0],
                [null, "新規", 1100, "住所", 35.1, 139.1, "塩", "未設定", "", "", "", 3.5]
            ]);
        try
        {
            var imported = _service.Import(path);

            Assert.IsTrue(imported.Select(x => x.Id).SequenceEqual([1, 0]));
        }
        finally
        {
            DeleteTempFile(path);
        }
    }

    [TestMethod]
    public void Import_DuplicateExplicitIdThrows()
    {
        var path = CreateWorkbookWithRows(
            [
                [1, "A", 1000, "住所", 35.0, 139.0, "醤油", "未設定", "", "", "", 4.0],
                [1, "B", 1100, "住所", 35.1, 139.1, "塩", "未設定", "", "", "", 3.5]
            ]);
        try
        {
            Assert.ThrowsExactly<InvalidDataException>(() => _service.Import(path));
        }
        finally
        {
            DeleteTempFile(path);
        }
    }

    [TestMethod]
    public void Import_InvalidPriceThrows()
    {
        var path = CreateWorkbookWithRows(
            [[1, "A", 0, "住所", 35.0, 139.0, "醤油", "未設定", "", "", "", 4.0]]);
        try
        {
            Assert.ThrowsExactly<InvalidDataException>(() => _service.Import(path));
        }
        finally
        {
            DeleteTempFile(path);
        }
    }

    [TestMethod]
    public void Import_InvalidRatingThrows()
    {
        var path = CreateWorkbookWithRows(
            [[1, "A", 1000, "住所", 35.0, 139.0, "醤油", "未設定", "", "", "", 5.1]]);
        try
        {
            Assert.ThrowsExactly<InvalidDataException>(() => _service.Import(path));
        }
        finally
        {
            DeleteTempFile(path);
        }
    }

    [TestMethod]
    public void Import_InvalidCoordinatesThrows()
    {
        var path = CreateWorkbookWithRows(
            [[1, "A", 1000, "住所", 95.0, 139.0, "醤油", "未設定", "", "", "", 4.0]]);
        try
        {
            Assert.ThrowsExactly<InvalidDataException>(() => _service.Import(path));
        }
        finally
        {
            DeleteTempFile(path);
        }
    }

    [TestMethod]
    public void Import_InvalidRamenTypeThrows()
    {
        var path = CreateWorkbookWithRows(
            [[1, "A", 1000, "住所", 35.0, 139.0, "カレー", "未設定", "", "", "", 4.0]]);
        try
        {
            Assert.ThrowsExactly<InvalidDataException>(() => _service.Import(path));
        }
        finally
        {
            DeleteTempFile(path);
        }
    }

    [TestMethod]
    public void Import_TimeSpecifiedOpeningHoursRequiresDifferentTimes()
    {
        var path = CreateWorkbookWithRows(
            [[1, "A", 1000, "住所", 35.0, 139.0, "醤油", "時間指定", "11:00", "11:00", "", 4.0]]);
        try
        {
            Assert.ThrowsExactly<InvalidDataException>(() => _service.Import(path));
        }
        finally
        {
            DeleteTempFile(path);
        }
    }

    [TestMethod]
    public void CreateImportTemplate_CreatesExpectedSheetsAndHeaders()
    {
        var path = CreateTempPath();
        try
        {
            _service.CreateImportTemplate(path);
            using var workbook = new XLWorkbook(path);

            Assert.AreEqual("店舗入力", workbook.Worksheet(1).Name);
            Assert.AreEqual("入力ルール", workbook.Worksheet(2).Name);
            Assert.AreEqual("ID", workbook.Worksheet(1).Cell(1, 1).GetString());
            Assert.AreEqual("評価", workbook.Worksheet(1).Cell(1, 12).GetString());
            Assert.AreEqual("入力項目", workbook.Worksheet(2).Cell(1, 1).GetString());
        }
        finally
        {
            DeleteTempFile(path);
        }
    }

    private static string CreateWorkbookWithRows(List<object?[]> rows)
    {
        var path = CreateTempPath();
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("店舗データ");
        var headers = new[] { "ID", "店舗名", "価格", "住所", "緯度", "経度", "ラーメンの種類", "営業時間モード", "開始時刻", "終了時刻", "定休日", "評価" };
        for (var i = 0; i < headers.Length; i++) sheet.Cell(1, i + 1).Value = headers[i];

        for (var row = 0; row < rows.Count; row++)
        {
            for (var column = 0; column < rows[row].Length; column++)
            {
                var value = rows[row][column];
                if (value is not null)
                    sheet.Cell(row + 2, column + 1).Value = XLCellValue.FromObject(value);
            }
        }

        workbook.SaveAs(path);
        return path;
    }

    private static string CreateTempPath() =>
        Path.Combine(Path.GetTempPath(), $"ramenia-test-{Guid.NewGuid():N}.xlsx");

    private static void DeleteTempFile(string path)
    {
        if (File.Exists(path)) File.Delete(path);
    }
}
