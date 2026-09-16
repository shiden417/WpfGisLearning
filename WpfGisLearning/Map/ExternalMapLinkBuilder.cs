using WpfGisLearning.Models;

namespace WpfGisLearning.Map;

public static class ExternalMapLinkBuilder
{
    public static string CreateGoogleMapsUrl(Shop shop)
    {
        ArgumentNullException.ThrowIfNull(shop);

        return $"https://www.google.com/maps/search/?api=1&query={shop.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)},{shop.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
    }
}
