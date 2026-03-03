using HVO.NinaClient.Models;
using System.Text.Json;

namespace HVO.NinaClient.Tests;

[TestClass]
public class ModelSerializationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [TestMethod]
    public void DeviceInfo_Deserialize_SetsAllProperties()
    {
        var json = """
        {
            "Name": "ZWO ASI294MC Pro",
            "DisplayName": "ASI294MC Pro",
            "Description": "ZWO Color Camera",
            "DeviceId": "ASI294MC Pro #0",
            "DriverInfo": "ZWO",
            "DriverVersion": "3.24",
            "Connected": true
        }
        """;

        var device = JsonSerializer.Deserialize<DeviceInfo>(json, JsonOptions)!;

        Assert.AreEqual("ZWO ASI294MC Pro", device.Name);
        Assert.AreEqual("ASI294MC Pro", device.DisplayName);
        Assert.AreEqual("ZWO Color Camera", device.Description);
        Assert.AreEqual("ASI294MC Pro #0", device.DeviceId);
        Assert.IsTrue(device.Connected);
    }

    [TestMethod]
    public void DeviceInfo_DefaultValues_AreNull()
    {
        var device = new DeviceInfo();

        Assert.IsNull(device.Name);
        Assert.IsNull(device.DisplayName);
        Assert.IsNull(device.Description);
        Assert.IsNull(device.DeviceId);
        Assert.IsFalse(device.Connected);
    }

    [TestMethod]
    public void CameraInfo_Deserialize_IncludesBaseAndCameraProperties()
    {
        var json = """
        {
            "Name": "ZWO ASI294MC Pro",
            "Connected": true,
            "TargetTemp": -10.0,
            "AtTargetTemp": true,
            "CanSetTemperature": true,
            "Temperature": -10.2,
            "CoolerOn": true,
            "CoolerPower": 65.5,
            "Gain": 120,
            "Offset": 30,
            "XSize": 4144,
            "YSize": 2822,
            "BinX": 1,
            "BinY": 1
        }
        """;

        var camera = JsonSerializer.Deserialize<CameraInfo>(json, JsonOptions)!;

        Assert.AreEqual("ZWO ASI294MC Pro", camera.Name);
        Assert.IsTrue(camera.Connected);
        Assert.AreEqual(-10.0, camera.TargetTemp);
        Assert.IsTrue(camera.AtTargetTemp);
        Assert.IsTrue(camera.CanSetTemperature);
    }

    [TestMethod]
    public void NinaApiResponse_Deserialize_Success()
    {
        var json = """
        {
            "Response": "1.0.0",
            "Success": true,
            "Type": "API",
            "Error": ""
        }
        """;

        var response = JsonSerializer.Deserialize<NinaApiResponse<string>>(json, JsonOptions)!;

        Assert.IsTrue(response.Success);
        Assert.AreEqual("1.0.0", response.Response);
        Assert.AreEqual("", response.Error);
    }

    [TestMethod]
    public void NinaApiResponse_Deserialize_Error()
    {
        var json = """
        {
            "Response": null,
            "Success": false,
            "Type": "API",
            "Error": "Camera not connected"
        }
        """;

        var response = JsonSerializer.Deserialize<NinaApiResponse<string>>(json, JsonOptions)!;

        Assert.IsFalse(response.Success);
        Assert.AreEqual("Camera not connected", response.Error);
    }

    [TestMethod]
    public void LogEntry_Deserialize_SetsProperties()
    {
        var json = """
        {
            "Timestamp": "2024-01-15T22:30:00Z",
            "Level": "INFO",
            "Source": "Camera",
            "Member": "Connect",
            "Line": "42",
            "Message": "Camera connected successfully"
        }
        """;

        var entry = JsonSerializer.Deserialize<LogEntry>(json, JsonOptions)!;

        Assert.AreEqual("INFO", entry.Level);
        Assert.AreEqual("Camera", entry.Source);
        Assert.AreEqual("Camera connected successfully", entry.Message);
    }

    [TestMethod]
    public void EventEntry_Deserialize_SetsProperties()
    {
        var json = """
        {
            "Time": "2024-01-15T22:31:00Z",
            "Event": "CameraConnected"
        }
        """;

        var entry = JsonSerializer.Deserialize<EventEntry>(json, JsonOptions)!;

        Assert.AreEqual("CameraConnected", entry.Event);
        Assert.AreEqual("2024-01-15T22:31:00Z", entry.Time);
    }
}
