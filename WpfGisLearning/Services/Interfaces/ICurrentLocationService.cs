using WpfGisLearning.Models;

namespace WpfGisLearning.Services.Interfaces;

public interface ICurrentLocationService
{
    Task<CurrentLocation?> GetCurrentLocationAsync();
}
