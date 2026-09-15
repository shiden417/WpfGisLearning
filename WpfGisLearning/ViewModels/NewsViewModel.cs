using CommunityToolkit.Mvvm.ComponentModel;
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

    public IEnumerable<NewsItem> FeaturedNews => News.Where(x => x.IsFeatured).Take(2);

    public NewsViewModel(NewsService newsService)
    {
        _newsService = newsService;
        foreach (var item in _newsService.GetNews()) News.Add(item);
        NewsView = CollectionViewSource.GetDefaultView(News);
        NewsView.Filter = FilterNews;
    }

    partial void OnSelectedCategoryChanged(string value) => NewsView.Refresh();

    private bool FilterNews(object item) => item is NewsItem news && (SelectedCategory == "すべて" || news.Category == SelectedCategory);
}
