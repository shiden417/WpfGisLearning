using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using WpfGisLearning.Models;
using WpfGisLearning.Services;

namespace WpfGisLearning.ViewModels;

public partial class NewsViewModel : ObservableObject
{
    private readonly NewsService _newsService;

    public ObservableCollection<NewsItem> News { get; } = new();
    public ICollectionView NewsView { get; }
    public string[] Categories { get; } = ["すべて", "新店", "限定", "イベント", "特集"];

    [ObservableProperty] private string selectedCategory = "すべて";
    [ObservableProperty] private NewsItem? selectedNews;
    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private string statusMessage = "";
    [ObservableProperty] private DateTime selectedDate = DateTime.Today;

    public IEnumerable<NewsItem> FeaturedNews => News.Where(x => x.IsFeatured).Take(2);

    public NewsViewModel(NewsService newsService)
    {
        _newsService = newsService;
        NewsView = CollectionViewSource.GetDefaultView(News);
        NewsView.Filter = FilterNews;
    }

    partial void OnSelectedCategoryChanged(string value) => NewsView.Refresh();

    [RelayCommand]
    private async Task RefreshAsync()
    {
        if (IsLoading) return;

        IsLoading = true;
        StatusMessage = $"{SelectedDate:yyyy/MM/dd} のニュースを取得しています…";

        try
        {
            var items = await _newsService.GetNewsAsync(SelectedDate);
            News.Clear();

            var index = 0;
            foreach (var item in items)
            {
                item.IsFeatured = index < 2;
                News.Add(item);
                index++;
            }

            OnPropertyChanged(nameof(FeaturedNews));
            NewsView.Refresh();
            StatusMessage = News.Count == 0
                ? $"{SelectedDate:yyyy/MM/dd} のニュースが見つかりませんでした。"
                : $"{SelectedDate:yyyy/MM/dd}：{News.Count}件のニュースを取得しました。";
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

    private bool FilterNews(object item)
        => item is NewsItem news && (SelectedCategory == "すべて" || news.Category == SelectedCategory);
}
