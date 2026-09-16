using WpfGisLearning.Models;

namespace WpfGisLearning.Services.Interfaces;

public interface IGeolocationProvider
{
    Task<CurrentLocation?> GetCurrentLocationAsync();
}
