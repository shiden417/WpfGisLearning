namespace WpfGisLearning.Services;

public static class MapCoordinateValidator
{
    public static bool IsValid(double latitude, double longitude)
    {
        return !double.IsNaN(latitude)
            && !double.IsNaN(longitude)
            && !double.IsInfinity(latitude)
            && !double.IsInfinity(longitude)
            && latitude >= -90
            && latitude <= 90
            && longitude >= -180
            && longitude <= 180
            && !(latitude == 0 && longitude == 0);
    }
}
