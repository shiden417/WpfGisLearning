using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using WpfGisLearning.Models;
using WpfGisLearning.Services;

namespace WpfGisLearning.ViewModels;

public partial class NewsViewModel : ObservableObject
{
    private readonly NewsService _newsService;

    public ObservableCollection<NewsItem> News { get; } = new();

    [ObservableProperty] private NewsItem? selectedNews;
    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private string statusMessage = "";
    [ObservableProperty] private DateTime selectedDate = DateTime.Today;

    public IEnumerable<NewsItem> FeaturedNews => News.Where(x => x.IsFeatured).Take(2);

    public NewsViewModel(NewsService newsService)
    {
        _newsService = newsService;
    }

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
                item.IsFeatured = index < 2;
                News.Add(item);
                index++;
            }

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
