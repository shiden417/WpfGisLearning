using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using WpfGisLearning.Models;
using WpfGisLearning.Services.Interfaces;

namespace WpfGisLearning.ViewModels;

/// <summary>
/// ニュース画面の状態と再取得操作を管理するViewModelです。
/// HTTP通信そのものはINewsServiceへ委譲します。
/// </summary>
public partial class NewsViewModel : ObservableObject
{
    /// <summary>ニュースデータの取得を担当するサービスです。</summary>
    private readonly INewsService _newsService;

    /// <summary>画面へ表示するニュース一覧です。</summary>
    public ObservableCollection<NewsItem> News { get; } = new();

    /// <summary>現在選択されているニュース記事です。</summary>
    [ObservableProperty] private NewsItem? selectedNews;

    /// <summary>ニュース取得中かどうかを示します。二重取得防止にも使用します。</summary>
    [ObservableProperty] private bool isLoading;

    /// <summary>取得状態やエラーを画面へ表示するメッセージです。</summary>
    [ObservableProperty] private string statusMessage = "";

    /// <summary>ニュース取得の基準日です。</summary>
    [ObservableProperty] private DateTime selectedDate = DateTime.Today;

    /// <summary>一覧の先頭2件を注目記事として返します。</summary>
    public IEnumerable<NewsItem> FeaturedNews => News.Where(x => x.IsFeatured).Take(2);

    /// <summary>ニュース取得サービスを受け取ってViewModelを生成します。</summary>
    public NewsViewModel(INewsService newsService) => _newsService = newsService;

    /// <summary>
    /// 指定日のニュースを再取得し、一覧と注目記事を更新します。
    /// RelayCommandによりRefreshCommandが自動生成されます。
    /// </summary>
    [RelayCommand]
    private async Task RefreshAsync()
    {
        if (IsLoading) return;

        IsLoading = true;
        StatusMessage = $"{SelectedDate:yyyy/MM/dd}以前のニュースを取得しています…";

        try
        {
            var items = await _newsService.GetNewsAsync(SelectedDate);
            News.Clear();

            var index = 0;
            foreach (var item in items)
            {
                // 取得順の先頭2件だけを注目記事として扱う。
                item.IsFeatured = index < 2;
                News.Add(item);
                index++;
            }

            // FeaturedNewsは計算プロパティなので、元コレクションを変更したタイミングで明示通知する。
            OnPropertyChanged(nameof(FeaturedNews));
            StatusMessage = News.Count == 0
                ? $"{SelectedDate:yyyy/MM/dd}以前のニュースが見つかりませんでした。"
                : $"{SelectedDate:yyyy/MM/dd}以前：{News.Count}件のニュースを取得しました。";
        }
        catch (Exception ex)
        {
            StatusMessage = $"ニュースを取得できませんでした。{ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
