namespace WpfGisLearning.Services;

public sealed class CurrentLocationService : ICurrentLocationService
{
    private const uint DesiredAccuracyMeters = 50;

    public async Task<CurrentLocation?> GetCurrentLocationAsync()
    {
        var access = await Windows.Devices.Geolocation.Geolocator.RequestAccessAsync();
        if (access != Windows.Devices.Geolocation.GeolocationAccessStatus.Allowed)
        {
            return null;
        }

        var geolocator = new Windows.Devices.Geolocation.Geolocator
        {
            DesiredAccuracyInMeters = DesiredAccuracyMeters
        };
        var position = await geolocator.GetGeopositionAsync();
        var latitude = position.Coordinate.Point.Position.Latitude;
        var longitude = position.Coordinate.Point.Position.Longitude;

        return MapCoordinateValidator.IsValid(latitude, longitude)
            ? new CurrentLocation(latitude, longitude)
            : null;
    }
}
