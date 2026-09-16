using WpfGisLearning.Models;

namespace WpfGisLearning.Services.Interfaces;

/// <summary>
/// 店舗データをExcelファイルへ出力・入力するサービスの契約です。
/// ClosedXMLなど具体的なExcelライブラリへの依存を利用側から隠します。
/// </summary>
public interface IExcelShopDataService
{
    /// <summary>店舗データ入力用のExcelテンプレートを作成します。</summary>
    void CreateImportTemplate(string filePath);

    /// <summary>指定した店舗一覧をExcelファイルへ出力します。</summary>
    void Export(string filePath, IEnumerable<Shop> shops);

    /// <summary>Excelファイルを読み込み、店舗一覧へ変換します。</summary>
    List<Shop> Import(string filePath);
}
