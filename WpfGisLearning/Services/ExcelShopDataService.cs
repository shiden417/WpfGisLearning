using ClosedXML.Excel;
using WpfGisLearning.Models;
using WpfGisLearning.Services.Interfaces;

namespace WpfGisLearning.Services;

public sealed class ExcelShopDataService : IExcelShopDataService
{
    private static readonly string[] Headers =
    [
        "ID",
        "店舗名",
        "価格",
        "住所",
        "緯度",
        "経度",
        "ラーメンの種類",
        "営業時間モード",
        "開始時刻",
        "終了時刻",
        "定休日",
        "評価"
    ];

    private static readonly string[] RamenTypes = ["醤油", "塩", "味噌", "豚骨", "家系", "二郎系", "つけ麺", "その他"];
    private static readonly string[] OpeningHoursModes = ["未設定", "時間指定", BusinessHoursStatusCalculator.Open24Hours];
    private const int TemplateRows = 200;

    public void CreateImportTemplate(string filePath)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("店舗入力");
        var rules = workbook.Worksheets.Add("入力ルール");

        WriteHeaders(sheet);
        sheet.Row(1).Style.Font.Bold = true;
        sheet.Row(1).Style.Font.FontColor = XLColor.White;
        sheet.Row(1).Style.Fill.BackgroundColor = XLColor.FromHtml("#20252A");
        sheet.Row(1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        sheet.Row(1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        sheet.SheetView.FreezeRows(1);

        sheet.Column(1).Width = 10;
        sheet.Column(2).Width = 26;
        sheet.Column(3).Width = 12;
        sheet.Column(4).Width = 34;
        sheet.Column(5).Width = 13;
        sheet.Column(6).Width = 13;
        sheet.Column(7).Width = 16;
        sheet.Column(8).Width = 18;
        sheet.Column(9).Width = 12;
        sheet.Column(10).Width = 12;
        sheet.Column(11).Width = 18;
        sheet.Column(12).Width = 10;

        var inputRange = sheet.Range(2, 1, TemplateRows + 1, Headers.Length);
        inputRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        inputRange.Style.Border.SetInsideBorder(XLBorderStyleValues.Hair);
        inputRange.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);

        sheet.Range(2, 1, TemplateRows + 1, 1).Style.NumberFormat.Format = "0";
        sheet.Range(2, 3, TemplateRows + 1, 3).Style.NumberFormat.Format = "#,##0";
        sheet.Range(2, 5, TemplateRows + 1, 6).Style.NumberFormat.Format = "0.000000";
        sheet.Range(2, 12, TemplateRows + 1, 12).Style.NumberFormat.Format = "0.0";

        var ramenValidation = sheet.Range(2, 7, TemplateRows + 1, 7).DataValidation;
        ramenValidation.List($"\"{string.Join(",", RamenTypes)}\"");
        ramenValidation.ErrorTitle = "入力値が不正です";
        ramenValidation.ErrorMessage = "一覧からラーメンの種類を選択してください。";
        ramenValidation.ShowErrorMessage = true;

        var hoursValidation = sheet.Range(2, 8, TemplateRows + 1, 8).DataValidation;
        hoursValidation.List($"\"{string.Join(",", OpeningHoursModes)}\"");
        hoursValidation.ErrorTitle = "入力値が不正です";
        hoursValidation.ErrorMessage = "未設定・時間指定・24時間営業から選択してください。";
        hoursValidation.ShowErrorMessage = true;

        var priceValidation = sheet.Range(2, 3, TemplateRows + 1, 3).DataValidation;
        priceValidation.Decimal.Between(1, 999999999);
        priceValidation.ErrorTitle = "価格が不正です";
        priceValidation.ErrorMessage = "価格は1円以上で入力してください。";
        priceValidation.ShowErrorMessage = true;

        var ratingValidation = sheet.Range(2, 12, TemplateRows + 1, 12).DataValidation;
        ratingValidation.Custom("=AND(L2>=0,L2<=5,ROUND(L2,1)=L2)");
        ratingValidation.ErrorTitle = "評価が不正です";
        ratingValidation.ErrorMessage = "評価は0～5の範囲で、小数第1位まで入力してください。";
        ratingValidation.ShowErrorMessage = true;

        sheet.Range(2, 1, TemplateRows + 1, 1).DataValidation.WholeNumber.Between(1, 2147483647);
        sheet.Range(2, 5, TemplateRows + 1, 5).DataValidation.Decimal.Between(-90, 90);
        sheet.Range(2, 6, TemplateRows + 1, 6).DataValidation.Decimal.Between(-180, 180);

        rules.Cell("A1").Value = "入力項目";
        rules.Cell("B1").Value = "入力ルール";
        rules.Range("A1:B1").Style.Font.Bold = true;
        rules.Range("A1:B1").Style.Font.FontColor = XLColor.White;
        rules.Range("A1:B1").Style.Fill.BackgroundColor = XLColor.FromHtml("#20252A");
        var ruleRows = new (string Item, string Rule)[]
        {
            ("ID", "1以上。空欄の場合は新規店舗として自動採番"),
            ("店舗名", "必須"),
            ("価格", "1円以上"),
            ("住所", "任意"),
            ("緯度", "-90～90"),
            ("経度", "-180～180"),
            ("ラーメンの種類", string.Join(" / ", RamenTypes)),
            ("営業時間モード", string.Join(" / ", OpeningHoursModes)),
            ("開始時刻", "営業時間モードが時間指定の場合に入力。30分単位（例：11:00）"),
            ("終了時刻", "営業時間モードが時間指定の場合に入力。30分単位（例：21:00）"),
            ("定休日", "複数指定は「日・月・火」のように区切る"),
            ("評価", "0～5、小数第1位まで")
        };
        for (var i = 0; i < ruleRows.Length; i++)
        {
            rules.Cell(i + 2, 1).Value = ruleRows[i].Item;
            rules.Cell(i + 2, 2).Value = ruleRows[i].Rule;
        }
        rules.Range("A1:B13").Style.Border.SetInsideBorder(XLBorderStyleValues.Hair);
        rules.Range("A1:B13").Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
        rules.Column(1).Width = 24;
        rules.Column(2).Width = 66;
        rules.Range("A1:B13").Style.Alignment.WrapText = true;
        rules.Row(1).Height = 24;

        sheet.Cell("A2").Comment.AddText("既存店舗を更新する場合はIDを入力してください。新規店舗は空欄で構いません。");
        sheet.Cell("B2").Comment.AddText("店舗名は必須です。");
        sheet.Cell("C2").Comment.AddText("画面の店舗編集と同じく1円以上です。");
        sheet.Cell("L2").Comment.AddText("画面の店舗編集と同じく0～5、小数第1位までです。");

        workbook.SaveAs(filePath);
    }

    public void Export(string filePath, IEnumerable<Shop> shops)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("店舗データ");
        WriteHeaders(sheet);

        var row = 2;
        foreach (var shop in shops.OrderBy(x => x.Id))
        {
            var (mode, start, end) = SplitOpeningHours(shop.OpeningHours);
            sheet.Cell(row, 1).Value = shop.Id;
            sheet.Cell(row, 2).Value = shop.Name;
            sheet.Cell(row, 3).Value = shop.Price;
            sheet.Cell(row, 4).Value = shop.Address;
            sheet.Cell(row, 5).Value = shop.Latitude;
            sheet.Cell(row, 6).Value = shop.Longitude;
            sheet.Cell(row, 7).Value = shop.RamenType;
            sheet.Cell(row, 8).Value = mode;
            sheet.Cell(row, 9).Value = start;
            sheet.Cell(row, 10).Value = end;
            sheet.Cell(row, 11).Value = shop.ClosedDay;
            sheet.Cell(row, 12).Value = shop.Rating;
            row++;
        }

        sheet.Row(1).Style.Font.Bold = true;
        sheet.Row(1).Style.Font.FontColor = XLColor.White;
        sheet.Row(1).Style.Fill.BackgroundColor = XLColor.FromHtml("#20252A");
        sheet.SheetView.FreezeRows(1);
        sheet.Columns(1, Headers.Length).AdjustToContents();
        sheet.Column(2).Width = Math.Min(sheet.Column(2).Width, 28);
        sheet.Column(4).Width = Math.Min(sheet.Column(4).Width, 34);
        sheet.Column(11).Width = Math.Min(sheet.Column(11).Width, 18);
        sheet.Range(2, 3, Math.Max(2, row - 1), 3).Style.NumberFormat.Format = "#,##0";
        sheet.Range(2, 5, Math.Max(2, row - 1), 6).Style.NumberFormat.Format = "0.000000";
        sheet.Range(2, 12, Math.Max(2, row - 1), 12).Style.NumberFormat.Format = "0.0";

        workbook.SaveAs(filePath);
    }

    public List<Shop> Import(string filePath)
    {
        using var workbook = new XLWorkbook(filePath);
        var sheet = workbook.Worksheet(1);
        var lastRow = sheet.LastRowUsed()?.RowNumber() ?? 0;
        if (lastRow < 2)
            return [];

        var explicitIds = new HashSet<int>();
        for (var row = 2; row <= lastRow; row++)
        {
            if (sheet.Range(row, 1, row, Headers.Length).Cells().All(cell => cell.IsEmpty()))
                continue;
            var id = ReadInt(sheet.Cell(row, 1), 0);
            if (id > 0 && !explicitIds.Add(id))
                throw new InvalidDataException($"{row}行目の店舗IDが重複しています。ID: {id}");
        }

        var shops = new List<Shop>();
        var nextId = explicitIds.Count == 0 ? 1 : explicitIds.Max() + 1;
        for (var row = 2; row <= lastRow; row++)
        {
            if (sheet.Range(row, 1, row, Headers.Length).Cells().All(cell => cell.IsEmpty()))
                continue;

            var id = ReadInt(sheet.Cell(row, 1), 0);
            if (id <= 0)
            {
                while (explicitIds.Contains(nextId)) nextId++;
                id = nextId++;
            }

            var name = sheet.Cell(row, 2).GetString().Trim();
            var price = ReadDecimal(sheet.Cell(row, 3));
            var address = sheet.Cell(row, 4).GetString().Trim();
            var latitude = ReadDouble(sheet.Cell(row, 5));
            var longitude = ReadDouble(sheet.Cell(row, 6));
            var ramenType = sheet.Cell(row, 7).GetString().Trim();
            var hoursMode = sheet.Cell(row, 8).GetString().Trim();
            var openingTime = sheet.Cell(row, 9).GetString().Trim();
            var closingTime = sheet.Cell(row, 10).GetString().Trim();
            var closedDay = sheet.Cell(row, 11).GetString().Trim();
            var rating = ReadDouble(sheet.Cell(row, 12));

            if (string.IsNullOrWhiteSpace(ramenType)) ramenType = "醤油";

            var openingHours = hoursMode switch
            {
                "未設定" or "" => string.Empty,
                BusinessHoursStatusCalculator.Open24Hours => BusinessHoursStatusCalculator.Open24Hours,
                "時間指定" => $"{openingTime}-{closingTime}",
                _ => throw new InvalidDataException($"{row}行目の営業時間モードが不正です。")
            };

            shops.Add(new Shop
            {
                Id = id,
                Name = name,
                Price = price,
                Address = address,
                Latitude = latitude,
                Longitude = longitude,
                RamenType = ramenType,
                OpeningHours = openingHours,
                ClosedDay = closedDay,
                Rating = rating
            });
        }

        return shops;
    }

    private static void WriteHeaders(IXLWorksheet sheet)
    {
        for (var i = 0; i < Headers.Length; i++)
            sheet.Cell(1, i + 1).Value = Headers[i];
    }

    private static int ReadInt(IXLCell cell, int fallback) => cell.IsEmpty() ? fallback : cell.GetValue<int>();
    private static decimal ReadDecimal(IXLCell cell) => cell.IsEmpty() ? 0 : cell.GetValue<decimal>();
    private static double ReadDouble(IXLCell cell) => cell.IsEmpty() ? 0 : cell.GetValue<double>();

    private static (string Mode, string Start, string End) SplitOpeningHours(string? openingHours)
    {
        if (string.IsNullOrWhiteSpace(openingHours))
            return ("未設定", string.Empty, string.Empty);
        if (string.Equals(openingHours.Trim(), BusinessHoursStatusCalculator.Open24Hours, StringComparison.Ordinal))
            return (BusinessHoursStatusCalculator.Open24Hours, string.Empty, string.Empty);

        var parts = openingHours.Replace("〜", "-").Replace("~", "-").Split('-', 2);
        return parts.Length == 2
            ? ("時間指定", parts[0].Trim(), parts[1].Trim())
            : ("時間指定", string.Empty, string.Empty);
    }
}
