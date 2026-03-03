using System.Text.Json;
using HVO.Astronomy.TheSkyX.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HVO.Astronomy.TheSkyX.Tests;

[TestClass]
public class ModelSerializationTests
{
    #region TheSkyXObservatoryInformation

    [TestMethod]
    public void ObservatoryInformation_DefaultValues()
    {
        var info = new TheSkyXObservatoryInformation();

        Assert.AreEqual(0.0, info.Latitude);
        Assert.AreEqual(0.0, info.Longitude);
        Assert.AreEqual(0, info.TimeZoneOffset);
        Assert.AreEqual(0, info.ElevationInMeters);
        Assert.AreEqual(string.Empty, info.LocationName);
    }

    [TestMethod]
    public void ObservatoryInformation_JsonDeserialization()
    {
        var json = "{\"latitude\":35.65,\"longitude\":-113.78,\"timeZoneOffset\":-7,\"elevation\":1200,\"locationName\":\"HVO\"}";
        var info = JsonSerializer.Deserialize<TheSkyXObservatoryInformation>(json);

        Assert.IsNotNull(info);
        Assert.AreEqual(35.65, info.Latitude, 0.001);
        Assert.AreEqual(-113.78, info.Longitude, 0.001);
        Assert.AreEqual(-7, info.TimeZoneOffset);
        Assert.AreEqual(1200, info.ElevationInMeters);
        Assert.AreEqual("HVO", info.LocationName);
    }

    [TestMethod]
    public void ObservatoryInformation_JsonRoundTrip()
    {
        var original = new TheSkyXObservatoryInformation
        {
            Latitude = 35.65,
            Longitude = -113.78,
            TimeZoneOffset = -7,
            ElevationInMeters = 1200,
            LocationName = "HVO"
        };

        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<TheSkyXObservatoryInformation>(json);

        Assert.IsNotNull(deserialized);
        Assert.AreEqual(original.Latitude, deserialized.Latitude);
        Assert.AreEqual(original.Longitude, deserialized.Longitude);
        Assert.AreEqual(original.TimeZoneOffset, deserialized.TimeZoneOffset);
        Assert.AreEqual(original.ElevationInMeters, deserialized.ElevationInMeters);
        Assert.AreEqual(original.LocationName, deserialized.LocationName);
    }

    #endregion

    #region TheSkyXSelectedHardware

    [TestMethod]
    public void SelectedHardware_DefaultValues()
    {
        var hw = new TheSkyXSelectedHardware();

        Assert.IsNull(hw.Mount);
        Assert.IsNull(hw.PrimaryCamera);
        Assert.IsNull(hw.AutoGuider);
    }

    [TestMethod]
    public void SelectedHardware_JsonDeserialization_CaseInsensitive()
    {
        var json = "{\"Mount\":{\"Model\":\"Paramount MX+\"},\"PrimaryCamera\":{\"Model\":\"ZWO ASI2600MM\"}}";
        var hw = JsonSerializer.Deserialize<TheSkyXSelectedHardware>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.IsNotNull(hw);
        Assert.IsNotNull(hw.Mount);
        Assert.AreEqual("Paramount MX+", hw.Mount!.Model);
        Assert.IsNotNull(hw.PrimaryCamera);
        Assert.AreEqual("ZWO ASI2600MM", hw.PrimaryCamera!.Model);
    }

    #endregion

    #region TheSkyXOperatingSystem Enum

    [TestMethod]
    public void OperatingSystem_HasExpectedValues()
    {
        Assert.AreEqual(0, (int)TheSkyXOperatingSystem.osUnknown);
        Assert.AreEqual(1, (int)TheSkyXOperatingSystem.osWindows);
        Assert.AreEqual(2, (int)TheSkyXOperatingSystem.osMac);
        Assert.AreEqual(3, (int)TheSkyXOperatingSystem.osLinux);
    }

    #endregion
}
