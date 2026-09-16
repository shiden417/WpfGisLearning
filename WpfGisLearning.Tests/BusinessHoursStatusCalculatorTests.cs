using WpfGisLearning.Services;

namespace WpfGisLearning.Tests;

[TestClass]
public class BusinessHoursStatusCalculatorTests
{
    [TestMethod]
    public void IsOpen_ReturnsTrue_InsideBusinessHours()
    {
        var result = BusinessHoursStatusCalculator.IsOpen("11:00-21:00", "月曜", new DateTime(2026, 9, 16, 12, 30, 0));
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void IsOpen_ReturnsFalse_OutsideBusinessHours()
    {
        var result = BusinessHoursStatusCalculator.IsOpen("11:00-21:00", "月曜", new DateTime(2026, 9, 16, 22, 0, 0));
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsOpen_ReturnsFalse_OnClosedDay()
    {
        var result = BusinessHoursStatusCalculator.IsOpen("11:00-21:00", "水曜", new DateTime(2026, 9, 16, 12, 30, 0));
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsOpen_SupportsOvernightHours()
    {
        var result = BusinessHoursStatusCalculator.IsOpen("22:00-02:00", "", new DateTime(2026, 9, 16, 23, 30, 0));
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void IsOpen_ReturnsFalse_WhenHoursAreNotSpecified()
    {
        var result = BusinessHoursStatusCalculator.IsOpen(string.Empty, string.Empty, new DateTime(2026, 9, 16, 12, 30, 0));
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsOpen_ReturnsTrue_For24HourBusiness()
    {
        var result = BusinessHoursStatusCalculator.IsOpen(
            BusinessHoursStatusCalculator.Open24Hours,
            string.Empty,
            new DateTime(2026, 9, 16, 3, 15, 0));
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void IsOpen_ReturnsFalse_OnClosedDay_EvenFor24HourBusiness()
    {
        var result = BusinessHoursStatusCalculator.IsOpen(
            BusinessHoursStatusCalculator.Open24Hours,
            "水",
            new DateTime(2026, 9, 16, 12, 0, 0));
        Assert.IsFalse(result);
    }
}
