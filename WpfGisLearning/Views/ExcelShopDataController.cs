using Microsoft.Win32;
using System.Windows;
using WpfGisLearning.Services.Interfaces;

namespace WpfGisLearning.Views;

/// <summary>
/// Excelのファイル選択ダイアログと店舗データサービスの橋渡しを担当するUIコントローラーです。
/// Excelの具体的な読み書きはIExcelShopDataServiceへ委譲します。
/// </summary>
public sealed class ExcelShopDataController
{
    /// <summary>Excelファイルの作成・読込・出力を担当するサービスです。</summary>
    private readonly IExcelShopDataService _excelShopDataService;

    /// <summary>現在の店舗データを取得・置換するサービスです。</summary>
    private readonly IShopService _shopService;

    /// <summary>Excelインポート完了後に画面データを再同期するコールバックです。</summary>
    private readonly Action _afterImport;

    /// <summary>必要なサービスとインポート後処理を受け取ります。</summary>
    public ExcelShopDataController(
        IExcelShopDataService excelShopDataService,
        IShopService shopService,
        Action afterImport)
    {
        _excelShopDataService = excelShopDataService;
        _shopService = shopService;
        _afterImport = afterImport;
    }

    /// <summary>保存先を選択して、店舗入力用Excelテンプレートを作成します。</summary>
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

    /// <summary>保存先を選択して、現在の店舗一覧をExcelへ出力します。</summary>
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

    /// <summary>
    /// Excelファイルを選択して検証済み店舗データを読み込み、ユーザー確認後に全店舗を置換します。
    /// </summary>
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
