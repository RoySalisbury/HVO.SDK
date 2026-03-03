using System.Text.Json;
using HVO.Astronomy.TheSkyX;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HVO.Astronomy.TheSkyX.Tests;

[TestClass]
public class ImageSizeTests
{
    [TestMethod]
    public void DefaultValues_AreZero()
    {
        var size = new ImageSize();

        Assert.AreEqual(0, size.Width);
        Assert.AreEqual(0, size.Height);
    }

    [TestMethod]
    public void Properties_CanBeSet()
    {
        var size = new ImageSize { Width = 1920, Height = 1080 };

        Assert.AreEqual(1920, size.Width);
        Assert.AreEqual(1080, size.Height);
    }

    [TestMethod]
    public void JsonSerialization_UsesLowercasePropertyNames()
    {
        var size = new ImageSize { Width = 800, Height = 600 };
        var json = JsonSerializer.Serialize(size);

        Assert.IsTrue(json.Contains("\"width\""));
        Assert.IsTrue(json.Contains("\"height\""));
        Assert.IsTrue(json.Contains("800"));
        Assert.IsTrue(json.Contains("600"));
    }

    [TestMethod]
    public void JsonDeserialization_FromLowercaseProperties()
    {
        var json = "{\"width\":1024,\"height\":768}";
        var size = JsonSerializer.Deserialize<ImageSize>(json);

        Assert.IsNotNull(size);
        Assert.AreEqual(1024, size.Width);
        Assert.AreEqual(768, size.Height);
    }

    [TestMethod]
    public void JsonRoundTrip_PreservesValues()
    {
        var original = new ImageSize { Width = 3840, Height = 2160 };
        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<ImageSize>(json);

        Assert.IsNotNull(deserialized);
        Assert.AreEqual(original.Width, deserialized.Width);
        Assert.AreEqual(original.Height, deserialized.Height);
    }
}
