using WpfGisLearning.Models;
using WpfGisLearning.Services.Interfaces;

namespace WpfGisLearning.Services;

public sealed class WindowsGeolocationProvider : IGeolocationProvider
{
    private const uint DesiredAccuracyMeters = 50;

    public async Task<CurrentLocation?> GetCurrentLocationAsync()
    {
        var access = await Windows.Devices.Geolocation.Geolocator.RequestAccessAsync();
        if (access != Windows.Devices.Geolocation.GeolocationAccessStatus.Allowed)
            return null;

        var geolocator = new Windows.Devices.Geolocation.Geolocator
        {
            DesiredAccuracyInMeters = DesiredAccuracyMeters
        };
        var position = await geolocator.GetGeopositionAsync();
        return new CurrentLocation(
            position.Coordinate.Point.Position.Latitude,
            position.Coordinate.Point.Position.Longitude);
    }
}
