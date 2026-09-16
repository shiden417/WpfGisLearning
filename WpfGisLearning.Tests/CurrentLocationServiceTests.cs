using WpfGisLearning.Models;
using WpfGisLearning.Services;
using WpfGisLearning.Services.Interfaces;

namespace WpfGisLearning.Tests;

[TestClass]
public class CurrentLocationServiceTests
{
    [TestMethod]
    public async Task GetCurrentLocationAsync_ReturnsValidLocation()
    {
        var expected = new CurrentLocation(35.681236, 139.767125);
        var service = new CurrentLocationService(new FakeGeolocationProvider(expected));

        var result = await service.GetCurrentLocationAsync();

        Assert.IsNotNull(result);
        Assert.AreEqual(expected.Latitude, result.Latitude, 0.000001);
        Assert.AreEqual(expected.Longitude, result.Longitude, 0.000001);
    }

    [TestMethod]
    public async Task GetCurrentLocationAsync_ReturnsNull_WhenProviderReturnsNull()
    {
        var service = new CurrentLocationService(new FakeGeolocationProvider(null));

        var result = await service.GetCurrentLocationAsync();

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetCurrentLocationAsync_ReturnsNull_WhenCoordinatesAreInvalid()
    {
        var service = new CurrentLocationService(new FakeGeolocationProvider(new CurrentLocation(91, 139)));

        var result = await service.GetCurrentLocationAsync();

        Assert.IsNull(result);
    }

    private sealed class FakeGeolocationProvider(CurrentLocation? location) : IGeolocationProvider
    {
        public Task<CurrentLocation?> GetCurrentLocationAsync() => Task.FromResult(location);
    }
}
