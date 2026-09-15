namespace WpfGisLearning.Services;

public interface ICurrentLocationService
{
    Task<CurrentLocation?> GetCurrentLocationAsync();
}

public sealed record CurrentLocation(double Latitude, double Longitude);
