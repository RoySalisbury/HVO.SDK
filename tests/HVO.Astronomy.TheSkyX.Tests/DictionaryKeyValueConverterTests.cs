using System.Collections.Generic;
using System.Text.Json;
using HVO.Astronomy.TheSkyX.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HVO.Astronomy.TheSkyX.Tests;

[TestClass]
public class DictionaryKeyValueConverterTests
{
    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new DictionaryKeyValueConverter());
        return options;
    }

    [TestMethod]
    public void CanConvert_IntKeyDictionary_ReturnsTrue()
    {
        var converter = new DictionaryKeyValueConverter();
        Assert.IsTrue(converter.CanConvert(typeof(Dictionary<int, string>)));
    }

    [TestMethod]
    public void CanConvert_StringKeyDictionary_ReturnsFalse()
    {
        var converter = new DictionaryKeyValueConverter();
        Assert.IsFalse(converter.CanConvert(typeof(Dictionary<string, string>)));
    }

    [TestMethod]
    public void CanConvert_NonDictionary_ReturnsFalse()
    {
        var converter = new DictionaryKeyValueConverter();
        Assert.IsFalse(converter.CanConvert(typeof(List<int>)));
        Assert.IsFalse(converter.CanConvert(typeof(string)));
    }

    [TestMethod]
    public void Serialize_IntKeyDictionary_ProducesArray()
    {
        var dict = new Dictionary<int, string>
        {
            { 1, "One" },
            { 2, "Two" }
        };

        var json = JsonSerializer.Serialize(dict, CreateOptions());

        Assert.IsTrue(json.StartsWith("["));
        Assert.IsTrue(json.EndsWith("]"));
        Assert.IsTrue(json.Contains("\"Key\""));
        Assert.IsTrue(json.Contains("\"Value\""));
    }

    [TestMethod]
    public void RoundTrip_IntKeyDictionary_PreservesValues()
    {
        var original = new Dictionary<int, string>
        {
            { 0, "Luminance" },
            { 1, "Red" },
            { 2, "Green" },
            { 3, "Blue" }
        };

        var options = CreateOptions();
        var json = JsonSerializer.Serialize(original, options);
        var deserialized = JsonSerializer.Deserialize<Dictionary<int, string>>(json, options);

        Assert.IsNotNull(deserialized);
        Assert.AreEqual(4, deserialized.Count);
        Assert.AreEqual("Luminance", deserialized[0]);
        Assert.AreEqual("Red", deserialized[1]);
        Assert.AreEqual("Green", deserialized[2]);
        Assert.AreEqual("Blue", deserialized[3]);
    }

    [TestMethod]
    public void Deserialize_EmptyArray_ReturnsEmptyDictionary()
    {
        var json = "[]";
        var options = CreateOptions();
        var result = JsonSerializer.Deserialize<Dictionary<int, string>>(json, options);

        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void Serialize_EmptyDictionary_ProducesEmptyArray()
    {
        var dict = new Dictionary<int, string>();
        var json = JsonSerializer.Serialize(dict, CreateOptions());

        Assert.AreEqual("[]", json);
    }
}
