namespace WpfGisLearning.Services;

public interface IReverseGeocodingService
{
    Task<string?> GetAddressAsync(double latitude, double longitude);
}
