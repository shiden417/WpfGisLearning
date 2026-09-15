using Microsoft.VisualStudio.TestTools.UnitTesting;
using WpfGisLearning.Services;

namespace WpfGisLearning.Tests;

[TestClass]
public class CoordinateValidationTests
{
    [TestMethod]
    [DataRow(35.681236, 139.767125)]
    [DataRow(-90.0, -180.0)]
    [DataRow(90.0, 180.0)]
    public void IsValidCoordinate_ValidCoordinates_ReturnsTrue(double latitude, double longitude)
    {
        Assert.IsTrue(MapCoordinateValidator.IsValid(latitude, longitude));
    }

    [TestMethod]
    [DataRow(-90.1, 139.0)]
    [DataRow(90.1, 139.0)]
    [DataRow(35.0, -180.1)]
    [DataRow(35.0, 180.1)]
    [DataRow(0.0, 0.0)]
    public void IsValidCoordinate_InvalidCoordinates_ReturnsFalse(double latitude, double longitude)
    {
        Assert.IsFalse(MapCoordinateValidator.IsValid(latitude, longitude));
    }

    [TestMethod]
    public void IsValidCoordinate_NaN_ReturnsFalse()
    {
        Assert.IsFalse(MapCoordinateValidator.IsValid(double.NaN, 139.0));
        Assert.IsFalse(MapCoordinateValidator.IsValid(35.0, double.NaN));
    }

    [TestMethod]
    public void IsValidCoordinate_Infinity_ReturnsFalse()
    {
        Assert.IsFalse(MapCoordinateValidator.IsValid(double.PositiveInfinity, 139.0));
        Assert.IsFalse(MapCoordinateValidator.IsValid(35.0, double.NegativeInfinity));
    }
}
