using Microsoft.Win32;
using System.Windows;
using WpfGisLearning.Services.Interfaces;

namespace WpfGisLearning.Views;

public sealed class ExcelShopDataController
{
    private readonly IExcelShopDataService _excelShopDataService;
    private readonly IShopService _shopService;
    private readonly Action _afterImport;

    public ExcelShopDataController(
        IExcelShopDataService excelShopDataService,
        IShopService shopService,
        Action afterImport)
    {
        _excelShopDataService = excelShopDataService;
        _shopService = shopService;
        _afterImport = afterImport;
    }

    public void DownloadTemplate()
    {
        var dialog = new SaveFileDialog
        {
            Title = "Excel入力テンプレートを保存",
            Filter = "Excelファイル|*.xlsx",
            FileName = "ramenia-shop-import-template.xlsx",
            AddExtension = true,
            OverwritePrompt = true
        };

        if (dialog.ShowDialog() != true) return;

        try
        {
            _excelShopDataService.CreateImportTemplate(dialog.FileName);
            MessageBox.Show(
                $"Excel入力テンプレートを保存しました。\n\n保存先：\n{dialog.FileName}",
                "Excelテンプレート",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Excelテンプレートの作成に失敗しました。\n\n{ex.Message}",
                "Excelテンプレート",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    public void Export()
    {
        var dialog = new SaveFileDialog
        {
            Title = "店舗データをExcelで保存",
            Filter = "Excelファイル|*.xlsx",
            FileName = $"ramenia-shops-{DateTime.Now:yyyyMMdd-HHmmss}.xlsx",
            AddExtension = true,
            OverwritePrompt = true
        };

        if (dialog.ShowDialog() != true) return;

        try
        {
            _excelShopDataService.Export(dialog.FileName, _shopService.GetShops());
            MessageBox.Show(
                $"店舗データをExcelで保存しました。\n\n保存先：\n{dialog.FileName}",
                "Excelエクスポート",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Excelの書き出しに失敗しました。\n\n{ex.Message}",
                "Excelエクスポート",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    public void Import()
    {
        var dialog = new OpenFileDialog
        {
            Title = "店舗データExcelを選択",
            Filter = "Excelファイル|*.xlsx|すべてのファイル|*.*",
            Multiselect = false
        };

        if (dialog.ShowDialog() != true) return;

        try
        {
            var shops = _excelShopDataService.Import(dialog.FileName);
            var result = MessageBox.Show(
                $"Excelから{shops.Count}件の店舗データを読み込みます。\nIDが一致する店舗は更新し、IDが空欄だった店舗は新規登録します。\n\n実行しますか？",
                "Excelインポートの確認",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            _shopService.ReplaceAll(shops);
            _afterImport();

            MessageBox.Show(
                $"Excelから{shops.Count}件の店舗データを読み込みました。",
                "Excelインポート",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Excelの読み込みに失敗しました。\n\n{ex.Message}",
                "Excelインポート",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}
