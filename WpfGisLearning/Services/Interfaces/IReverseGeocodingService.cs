namespace WpfGisLearning.Services.Interfaces;

public interface IReverseGeocodingService
{
    Task<string?> GetAddressAsync(double latitude, double longitude);
}
