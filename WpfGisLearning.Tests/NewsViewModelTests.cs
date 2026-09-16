using WpfGisLearning.Models;
using WpfGisLearning.Services.Interfaces;
using WpfGisLearning.ViewModels;

namespace WpfGisLearning.Tests;

[TestClass]
public class NewsViewModelTests
{
    [TestMethod]
    public async Task RefreshAsync_LoadsItemsAndMarksFirstTwoAsFeatured()
    {
        var service = new FakeNewsService(
            new NewsItem { Id = "1", Title = "First" },
            new NewsItem { Id = "2", Title = "Second" },
            new NewsItem { Id = "3", Title = "Third" });
        var viewModel = new NewsViewModel(service);

        await viewModel.RefreshCommand.ExecuteAsync(null);

        Assert.HasCount(3, viewModel.News);
        Assert.IsTrue(viewModel.News[0].IsFeatured);
        Assert.IsTrue(viewModel.News[1].IsFeatured);
        Assert.IsFalse(viewModel.News[2].IsFeatured);
        Assert.HasCount(2, viewModel.FeaturedNews);
        Assert.AreEqual($"{viewModel.SelectedDate:yyyy/MM/dd}以前：3件のニュースを取得しました。", viewModel.StatusMessage);
        Assert.IsFalse(viewModel.IsLoading);
        Assert.AreEqual(viewModel.SelectedDate.Date, service.RequestedDate.Date);
    }

    [TestMethod]
    public async Task RefreshAsync_WhenNoItemsSetsEmptyMessage()
    {
        var viewModel = new NewsViewModel(new FakeNewsService());

        await viewModel.RefreshCommand.ExecuteAsync(null);

        Assert.HasCount(0, viewModel.News);
        Assert.HasCount(0, viewModel.FeaturedNews);
        Assert.AreEqual($"{viewModel.SelectedDate:yyyy/MM/dd}以前のニュースが見つかりませんでした。", viewModel.StatusMessage);
        Assert.IsFalse(viewModel.IsLoading);
    }

    [TestMethod]
    public async Task RefreshAsync_WhenServiceFailsSetsErrorMessageAndClearsLoading()
    {
        var viewModel = new NewsViewModel(new FakeNewsService(exception: new InvalidOperationException("network error")));

        await viewModel.RefreshCommand.ExecuteAsync(null);

        StringAssert.Contains(viewModel.StatusMessage, "ニュースを取得できませんでした。network error");
        Assert.IsFalse(viewModel.IsLoading);
    }

    private sealed class FakeNewsService : INewsService
    {
        private readonly IReadOnlyList<NewsItem> _items;
        private readonly Exception? _exception;
        public DateTime RequestedDate { get; private set; }

        public FakeNewsService(params NewsItem[] items)
            : this(items, null)
        {
        }

        public FakeNewsService(IReadOnlyList<NewsItem> items, Exception? exception)
        {
            _items = items;
            _exception = exception;
        }

        public Task<IReadOnlyList<NewsItem>> GetNewsAsync(DateTime date, CancellationToken cancellationToken = default)
        {
            RequestedDate = date;
            if (_exception is not null) throw _exception;
            return Task.FromResult(_items);
        }
    }
}
