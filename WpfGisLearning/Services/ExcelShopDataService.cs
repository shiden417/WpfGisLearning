using ClosedXML.Excel;
using System.IO;
using WpfGisLearning.Models;
using WpfGisLearning.Services.Interfaces;

namespace WpfGisLearning.Services;

public sealed class ExcelShopDataService : IExcelShopDataService
{
    private static readonly string[] Headers =
    [
        "ID", "店舗名", "価格", "住所", "緯度", "経度", "ラーメンの種類",
        "営業時間モード", "開始時刻", "終了時刻", "定休日", "評価"
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

        double[] widths = [10, 26, 12, 34, 13, 13, 16, 18, 12, 12, 18, 10];
        for (var i = 0; i < widths.Length; i++) sheet.Column(i + 1).Width = widths[i];

        var inputRange = sheet.Range(2, 1, TemplateRows + 1, Headers.Length);
        inputRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        inputRange.Style.Border.SetInsideBorder(XLBorderStyleValues.Hair);
        inputRange.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);

        sheet.Range(2, 1, TemplateRows + 1, 1).Style.NumberFormat.Format = "0";
        sheet.Range(2, 3, TemplateRows + 1, 3).Style.NumberFormat.Format = "#,##0";
        sheet.Range(2, 5, TemplateRows + 1, 6).Style.NumberFormat.Format = "0.000000";
        sheet.Range(2, 12, TemplateRows + 1, 12).Style.NumberFormat.Format = "0.0";

        var ramenValidation = sheet.Range(2, 7, TemplateRows + 1, 7).CreateDataValidation();
        ramenValidation.List($"\"{string.Join(",", RamenTypes)}\"");
        ramenValidation.ErrorTitle = "入力値が不正です";
        ramenValidation.ErrorMessage = "一覧からラーメンの種類を選択してください。";
        ramenValidation.ShowErrorMessage = true;
        ramenValidation.ShowInputMessage = true;
        ramenValidation.InputTitle = "ラーメンの種類";
        ramenValidation.InputMessage = "プルダウンから選択してください。";

        var hoursValidation = sheet.Range(2, 8, TemplateRows + 1, 8).CreateDataValidation();
        hoursValidation.List($"\"{string.Join(",", OpeningHoursModes)}\"");
        hoursValidation.ErrorTitle = "入力値が不正です";
        hoursValidation.ErrorMessage = "未設定・時間指定・24時間営業から選択してください。";
        hoursValidation.ShowErrorMessage = true;

        var priceValidation = sheet.Range(2, 3, TemplateRows + 1, 3).CreateDataValidation();
        priceValidation.Decimal.Between(1, 999999999);
        priceValidation.ErrorTitle = "価格が不正です";
        priceValidation.ErrorMessage = "価格は1円以上で入力してください。";
        priceValidation.ShowErrorMessage = true;

        var ratingValidation = sheet.Range(2, 12, TemplateRows + 1, 12).CreateDataValidation();
        ratingValidation.Custom("=AND(L2>=0,L2<=5,ROUND(L2,1)=L2)");
        ratingValidation.ErrorTitle = "評価が不正です";
        ratingValidation.ErrorMessage = "評価は0～5の範囲で、小数第1位まで入力してください。";
        ratingValidation.ShowErrorMessage = true;

        var idValidation = sheet.Range(2, 1, TemplateRows + 1, 1).CreateDataValidation();
        idValidation.WholeNumber.Between(1, int.MaxValue);
        idValidation.ErrorTitle = "IDが不正です";
        idValidation.ErrorMessage = "IDは1以上で入力してください。新規店舗は空欄にしてください。";
        idValidation.ShowErrorMessage = true;

        var latitudeValidation = sheet.Range(2, 5, TemplateRows + 1, 5).CreateDataValidation();
        latitudeValidation.Decimal.Between(-90, 90);
        latitudeValidation.ErrorMessage = "緯度は-90～90で入力してください。";
        latitudeValidation.ShowErrorMessage = true;

        var longitudeValidation = sheet.Range(2, 6, TemplateRows + 1, 6).CreateDataValidation();
        longitudeValidation.Decimal.Between(-180, 180);
        longitudeValidation.ErrorMessage = "経度は-180～180で入力してください。";
        longitudeValidation.ShowErrorMessage = true;

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

        sheet.Cell("A2").GetComment().AddText("既存店舗を更新する場合はIDを入力してください。新規店舗は空欄で構いません。");
        sheet.Cell("B2").GetComment().AddText("店舗名は必須です。");
        sheet.Cell("C2").GetComment().AddText("画面の店舗編集と同じく1円以上です。");
        sheet.Cell("L2").GetComment().AddText("画面の店舗編集と同じく0～5、小数第1位までです。");

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
        var dataLastRow = Math.Max(2, row - 1);
        sheet.Range(2, 3, dataLastRow, 3).Style.NumberFormat.Format = "#,##0";
        sheet.Range(2, 5, dataLastRow, 6).Style.NumberFormat.Format = "0.000000";
        sheet.Range(2, 12, dataLastRow, 12).Style.NumberFormat.Format = "0.0";

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
            if (sheet.Range(row, 1, row, Headers.Length).Cells().All(cell => cell.IsEmpty())) continue;
            var id = ReadInt(sheet.Cell(row, 1), 0);
            if (id > 0 && !explicitIds.Add(id))
                throw new InvalidDataException($"{row}行目の店舗IDが重複しています。ID: {id}");
        }

        var shops = new List<Shop>();
        var nextId = explicitIds.Count == 0 ? 1 : explicitIds.Max() + 1;
        for (var row = 2; row <= lastRow; row++)
        {
            if (sheet.Range(row, 1, row, Headers.Length).Cells().All(cell => cell.IsEmpty())) continue;

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

            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidDataException($"{row}行目の店舗名を入力してください。");
            if (price <= 0)
                throw new InvalidDataException($"{row}行目の価格は1円以上で入力してください。");
            if (!MapCoordinateValidator.IsValid(latitude, longitude))
                throw new InvalidDataException($"{row}行目の緯度・経度が不正です。");
            if (string.IsNullOrWhiteSpace(ramenType)) ramenType = "醤油";
            if (!RamenTypes.Contains(ramenType, StringComparer.Ordinal))
                throw new InvalidDataException($"{row}行目のラーメンの種類が不正です。");
            if (rating < 0 || rating > 5 || Math.Abs(rating * 10 - Math.Round(rating * 10)) > 1e-9)
                throw new InvalidDataException($"{row}行目の評価は0～5、小数第1位までで入力してください。");

            var openingHours = hoursMode switch
            {
                "未設定" or "" => string.Empty,
                BusinessHoursStatusCalculator.Open24Hours => BusinessHoursStatusCalculator.Open24Hours,
                "時間指定" => BuildOpeningHours(openingTime, closingTime, row),
                _ => throw new InvalidDataException($"{row}行目の営業時間モードが不正です。")
            };

            shops.Add(new Shop
            {
                Id = id, Name = name, Price = price, Address = address,
                Latitude = latitude, Longitude = longitude, RamenType = ramenType,
                OpeningHours = openingHours, ClosedDay = closedDay, Rating = rating
            });
        }

        return shops;
    }

    private static string BuildOpeningHours(string openingTime, string closingTime, int row)
    {
        if (!TimeSpan.TryParseExact(openingTime, @"hh\:mm", null, out _) ||
            !TimeSpan.TryParseExact(closingTime, @"hh\:mm", null, out _))
            throw new InvalidDataException($"{row}行目の開始時刻・終了時刻が不正です。30分単位で入力してください。");

        if (!IsHalfHour(openingTime) || !IsHalfHour(closingTime))
            throw new InvalidDataException($"{row}行目の開始時刻・終了時刻は30分単位で入力してください。");
        if (string.Equals(openingTime, closingTime, StringComparison.Ordinal))
            throw new InvalidDataException($"{row}行目の開始時刻と終了時刻は異なる時刻を選択してください。");

        return $"{openingTime}-{closingTime}";
    }

    private static bool IsHalfHour(string value) =>
        TimeSpan.TryParseExact(value, @"hh\:mm", null, out var time) && time.Minutes is 0 or 30 && time.Hours < 24;

    private static void WriteHeaders(IXLWorksheet sheet)
    {
        for (var i = 0; i < Headers.Length; i++) sheet.Cell(1, i + 1).Value = Headers[i];
    }

    private static int ReadInt(IXLCell cell, int fallback) => cell.IsEmpty() ? fallback : cell.GetValue<int>();
    private static decimal ReadDecimal(IXLCell cell) => cell.IsEmpty() ? 0 : cell.GetValue<decimal>();
    private static double ReadDouble(IXLCell cell) => cell.IsEmpty() ? 0 : cell.GetValue<double>();

    private static (string Mode, string Start, string End) SplitOpeningHours(string? openingHours)
    {
        if (string.IsNullOrWhiteSpace(openingHours)) return ("未設定", string.Empty, string.Empty);
        if (string.Equals(openingHours.Trim(), BusinessHoursStatusCalculator.Open24Hours, StringComparison.Ordinal))
            return (BusinessHoursStatusCalculator.Open24Hours, string.Empty, string.Empty);

        var parts = openingHours.Replace("〜", "-").Replace("~", "-").Split('-', 2);
        return parts.Length == 2 ? ("時間指定", parts[0].Trim(), parts[1].Trim()) : ("時間指定", string.Empty, string.Empty);
    }
}
