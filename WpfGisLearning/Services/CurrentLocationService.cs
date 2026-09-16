using WpfGisLearning.Models;
using WpfGisLearning.Services.Interfaces;

namespace WpfGisLearning.Services;

public sealed class CurrentLocationService : ICurrentLocationService
{
    private readonly IGeolocationProvider _provider;

    public CurrentLocationService(IGeolocationProvider provider)
    {
        _provider = provider;
    }

    public async Task<CurrentLocation?> GetCurrentLocationAsync()
    {
        var location = await _provider.GetCurrentLocationAsync();
        if (location is null)
            return null;

        return MapCoordinateValidator.IsValid(location.Latitude, location.Longitude)
            ? location
            : null;
    }
}
