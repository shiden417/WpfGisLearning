using WpfGisLearning.Models;
using WpfGisLearning.Map;

namespace WpfGisLearning.Tests;

[TestClass]
public class ExternalMapLinkBuilderTests
{
    [TestMethod]
    public void CreateGoogleMapsUrl_UsesInvariantCoordinates()
    {
        var shop = new Shop
        {
            Id = 1,
            Name = "Test Ramen",
            Latitude = 35.681236,
            Longitude = 139.767125
        };

        var url = ExternalMapLinkBuilder.CreateGoogleMapsUrl(shop);

        Assert.AreEqual(
            "https://www.google.com/maps/search/?api=1&query=35.681236,139.767125",
            url);
    }

    [TestMethod]
    public void CreateGoogleMapsUrl_RejectsNullShop()
    {
        Assert.ThrowsException<ArgumentNullException>(
            () => ExternalMapLinkBuilder.CreateGoogleMapsUrl(null!));
    }
}
