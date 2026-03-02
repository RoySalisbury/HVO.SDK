using System;
using HVO.Core.Security.Cryptography;

namespace HVO.Weather.DavisVantagePro.Tests;

[TestClass]
public class DavisVantageProConsoleRecordTests
{
    /// <summary>
    /// Creates a minimal valid 99-byte LOOP packet with a correct CRC.
    /// </summary>
    private static byte[] CreateValidLoopPacket(
        short outsideTemp = 720,    // 72.0°F
        byte outsideHumidity = 50,
        byte windSpeed = 5,
        byte avgWindSpeed = 4,
        ushort windDirection = 180,
        byte insideHumidity = 45,
        short insideTemp = 700,     // 70.0°F
        ushort barometer = 29921)   // 29.921 inHg
    {
        var data = new byte[99];

        // LOO header
        data[0] = 0x4C; // L
        data[1] = 0x4F; // O
        data[2] = 0x4F; // O
        data[3] = 0;    // barometer trend = Steady

        // Barometer at offset 7
        BitConverter.GetBytes(barometer).CopyTo(data, 7);

        // Inside temp at offset 9
        BitConverter.GetBytes(insideTemp).CopyTo(data, 9);

        // Inside humidity at offset 11
        data[11] = insideHumidity;

        // Outside temp at offset 12
        BitConverter.GetBytes(outsideTemp).CopyTo(data, 12);

        // Wind speed at offset 14
        data[14] = windSpeed;

        // 10-min avg wind speed at offset 15
        data[15] = avgWindSpeed;

        // Wind direction at offset 16
        BitConverter.GetBytes(windDirection).CopyTo(data, 16);

        // Outside humidity at offset 33
        data[33] = outsideHumidity;

        // Rain rate at offset 41 = 0 (no rain)
        BitConverter.GetBytes((ushort)0).CopyTo(data, 41);

        // UV Index at offset 43 = MaxValue (unavailable)
        data[43] = byte.MaxValue;

        // Solar radiation at offset 44 = MaxValue (unavailable)
        BitConverter.GetBytes(short.MaxValue).CopyTo(data, 44);

        // Storm rain at offset 46 = MaxValue (unavailable)
        BitConverter.GetBytes(short.MaxValue).CopyTo(data, 46);

        // Storm start date at offset 48 = MaxValue (no storm)
        BitConverter.GetBytes(ushort.MaxValue).CopyTo(data, 48);

        // Sunrise at offset 91 = 0600 (6:00 AM)
        BitConverter.GetBytes((ushort)600).CopyTo(data, 91);

        // Sunset at offset 93 = 1800 (6:00 PM)
        BitConverter.GetBytes((ushort)1800).CopyTo(data, 93);

        // Console battery voltage at offset 87
        BitConverter.GetBytes((ushort)512).CopyTo(data, 87);

        // Compute and set the CRC
        using (var crc16 = new Crc16())
        {
            byte[] crcBytes = crc16.ComputeHash(data, 0, 97);
            crcBytes.CopyTo(data, 97);
        }

        return data;
    }

    [TestMethod]
    public void Create_ValidPacket_ParsesTemperatures()
    {
        var data = CreateValidLoopPacket(outsideTemp: 720, insideTemp: 700);
        var record = DavisVantageProConsoleRecord.Create(data, DateTimeOffset.Now);

        Assert.AreEqual(72.0, record.OutsideTemperature!.Fahrenheit, 0.1);
        Assert.AreEqual(70.0, record.InsideTemperature.Fahrenheit, 0.1);
    }

    [TestMethod]
    public void Create_ValidPacket_ParsesWind()
    {
        var data = CreateValidLoopPacket(windSpeed: 10, avgWindSpeed: 8, windDirection: 270);
        var record = DavisVantageProConsoleRecord.Create(data, DateTimeOffset.Now);

        Assert.AreEqual((byte)10, record.WindSpeed);
        Assert.AreEqual((byte)8, record.TenMinuteWindSpeedAverage);
        Assert.AreEqual((ushort)270, record.WindDirection);
    }

    [TestMethod]
    public void Create_ValidPacket_ParsesHumidity()
    {
        var data = CreateValidLoopPacket(outsideHumidity: 65, insideHumidity: 40);
        var record = DavisVantageProConsoleRecord.Create(data, DateTimeOffset.Now);

        Assert.AreEqual((byte)65, record.OutsideHumidity);
        Assert.AreEqual((byte)40, record.InsideHumidity);
    }

    [TestMethod]
    public void Create_UnavailableSensors_ReturnsNull()
    {
        var data = CreateValidLoopPacket();
        // Mark outside temp as unavailable
        BitConverter.GetBytes(short.MaxValue).CopyTo(data, 12);
        // Mark wind speed as unavailable
        data[14] = byte.MaxValue;
        // Mark outside humidity as unavailable
        data[33] = byte.MaxValue;

        // Recompute CRC
        using (var crc16 = new Crc16())
        {
            byte[] crcBytes = crc16.ComputeHash(data, 0, 97);
            crcBytes.CopyTo(data, 97);
        }

        var record = DavisVantageProConsoleRecord.Create(data, DateTimeOffset.Now);

        Assert.IsNull(record.OutsideTemperature);
        Assert.IsNull(record.WindSpeed);
        Assert.IsNull(record.OutsideHumidity);
    }

    [TestMethod]
    public void Create_NullData_ThrowsArgumentOutOfRangeException()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            DavisVantageProConsoleRecord.Create(null!, DateTimeOffset.Now));
    }

    [TestMethod]
    public void Create_ShortData_ThrowsArgumentOutOfRangeException()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            DavisVantageProConsoleRecord.Create(new byte[50], DateTimeOffset.Now));
    }

    [TestMethod]
    public void Create_InvalidCrc_ThrowsInvalidOperationException()
    {
        var data = CreateValidLoopPacket();
        // Corrupt a byte to invalidate CRC
        data[10] = 0xFF;

        Assert.ThrowsException<InvalidOperationException>(() =>
            DavisVantageProConsoleRecord.Create(data, DateTimeOffset.Now));
    }

    [TestMethod]
    public void Create_InvalidHeader_ThrowsArgumentException()
    {
        var data = CreateValidLoopPacket();
        // Corrupt the "LOO" header
        data[0] = 0x00;

        // Recompute CRC
        using (var crc16 = new Crc16())
        {
            byte[] crcBytes = crc16.ComputeHash(data, 0, 97);
            crcBytes.CopyTo(data, 97);
        }

        Assert.ThrowsException<ArgumentException>(() =>
            DavisVantageProConsoleRecord.Create(data, DateTimeOffset.Now));
    }

    [TestMethod]
    public void Create_SkipCrcValidation_DoesNotThrow()
    {
        var data = CreateValidLoopPacket();
        // Corrupt a byte but skip CRC validation
        data[10] = 0xFF;

        var record = DavisVantageProConsoleRecord.Create(data, DateTimeOffset.Now, validateCrc: false);

        Assert.IsNotNull(record);
    }

    [TestMethod]
    public void ValidatePacketCrc_ValidPacket_ReturnsTrue()
    {
        var data = CreateValidLoopPacket();
        Assert.IsTrue(DavisVantageProConsoleRecord.ValidatePacketCrc(data));
    }

    [TestMethod]
    public void ValidatePacketCrc_CorruptedPacket_ReturnsFalse()
    {
        var data = CreateValidLoopPacket();
        data[50] = (byte)(data[50] ^ 0xFF); // flip bits in a data byte

        Assert.IsFalse(DavisVantageProConsoleRecord.ValidatePacketCrc(data));
    }

    [TestMethod]
    public void Create_ParsesSunriseAndSunset()
    {
        var data = CreateValidLoopPacket();
        var record = DavisVantageProConsoleRecord.Create(data, DateTimeOffset.Now);

        Assert.AreEqual(new TimeSpan(6, 0, 0), record.SunriseTime);
        Assert.AreEqual(new TimeSpan(18, 0, 0), record.SunsetTime);
    }

    [TestMethod]
    public void OutsideDewpoint_BugFix_ValidConditions_ReturnsNonNull()
    {
        // BUG REGRESSION TEST: Legacy code used || instead of && in the humidity guard.
        // With ||, any non-null humidity (including 0) would attempt to compute dew point.
        var data = CreateValidLoopPacket(outsideTemp: 750, outsideHumidity: 60);
        var record = DavisVantageProConsoleRecord.Create(data, DateTimeOffset.Now);

        Assert.IsNotNull(record.OutsideDewpoint, "Dew point should be computed when temp and humidity are available");
        Assert.IsTrue(record.OutsideDewpoint!.Celsius < record.OutsideTemperature!.Celsius,
            "Dew point should be lower than ambient temperature at less than 100% humidity");
    }

    [TestMethod]
    public void OutsideDewpoint_ZeroHumidity_ReturnsNull()
    {
        // With the (|| → &&) bug fix, zero humidity should return null since the guard fails.
        var data = CreateValidLoopPacket(outsideTemp: 750, outsideHumidity: 0);
        var record = DavisVantageProConsoleRecord.Create(data, DateTimeOffset.Now);

        Assert.IsNull(record.OutsideDewpoint, "Dew point should be null when humidity is 0");
    }

    [TestMethod]
    public void OutsideHeatIndex_HighTempAndHumidity_ReturnsValue()
    {
        var data = CreateValidLoopPacket(outsideTemp: 950, outsideHumidity: 60);
        var record = DavisVantageProConsoleRecord.Create(data, DateTimeOffset.Now);

        Assert.IsNotNull(record.OutsideHeatIndex);
    }

    [TestMethod]
    public void OutsideWindChill_WithWind_ReturnsValue()
    {
        var data = CreateValidLoopPacket(outsideTemp: 350, avgWindSpeed: 10);
        var record = DavisVantageProConsoleRecord.Create(data, DateTimeOffset.Now);

        Assert.IsNotNull(record.OutsideWindChill);
    }
}
