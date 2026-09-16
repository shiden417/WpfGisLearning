using WpfGisLearning.Models;

namespace WpfGisLearning.Services.Interfaces;

public interface INewsService
{
    Task<IReadOnlyList<NewsItem>> GetNewsAsync(DateTime date, CancellationToken cancellationToken = default);
}
