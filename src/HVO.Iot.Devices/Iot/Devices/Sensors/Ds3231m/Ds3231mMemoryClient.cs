using System;
using HVO.Iot.Devices.Implementation;

namespace HVO.Iot.Devices.Iot.Devices.Sensors.Ds3231m;

/// <summary>
/// In-memory simulation of the DS3231M real-time clock for unit testing.
/// Provides settable date/time and temperature values stored in the simulated register array.
/// </summary>
/// <remarks>
/// The DS3231M stores timekeeping data in BCD format across registers 0x00–0x06,
/// temperature in registers 0x11–0x12, and status/control in registers 0x0E–0x0F.
/// This client writes directly to the backing register array.
/// </remarks>
public class Ds3231mMemoryClient : MemoryI2cRegisterClient
{
    private const byte TimeCalRegister = 0x00;
    private const byte ControlRegister = 0x0E;
    private const byte StatusRegister = 0x0F;
    private const byte TemperatureRegister = 0x11;

    /// <summary>
    /// Initializes a new memory-backed DS3231M client with default values
    /// (2025-01-01 00:00:00 UTC, 25.0°C, OSF cleared).
    /// </summary>
    /// <param name="busId">Simulated bus ID.</param>
    /// <param name="address">Simulated device address (default 0x68).</param>
    public Ds3231mMemoryClient(int busId = 1, int address = Ds3231m.DefaultI2cAddress)
        : base(busId, address, registerCount: 256)
    {
        SetDateTime(new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero));
        SetTemperature(25.0);

        // Set control register to INTCN default
        RegisterSpan[ControlRegister] = 0x04;
    }

    /// <summary>
    /// Sets the simulated date/time (stored as BCD in registers 0x00–0x06).
    /// </summary>
    /// <param name="value">The date/time to store (converted to UTC).</param>
    public void SetDateTime(DateTimeOffset value)
    {
        var utc = value.ToUniversalTime();
        var regs = RegisterSpan;

        regs[TimeCalRegister + 0] = DecToBcd(utc.Second);
        regs[TimeCalRegister + 1] = DecToBcd(utc.Minute);
        regs[TimeCalRegister + 2] = DecToBcd(utc.Hour);
        regs[TimeCalRegister + 3] = (byte)(((int)utc.DayOfWeek + 7) % 7);
        regs[TimeCalRegister + 4] = DecToBcd(utc.Day);
        regs[TimeCalRegister + 5] = utc.Year >= 2000
            ? (byte)(DecToBcd(utc.Month) | 0x80)
            : DecToBcd(utc.Month);
        regs[TimeCalRegister + 6] = DecToBcd(utc.Year % 100);
    }

    /// <summary>
    /// Sets the simulated temperature in degrees Celsius.
    /// </summary>
    /// <param name="celsius">Temperature in degrees Celsius.</param>
    public void SetTemperature(double celsius)
    {
        // MSB is signed integer part, upper 2 bits of LSB are fractional (0.25°C steps)
        var integerPart = (int)Math.Floor(celsius);
        var fractionalPart = (int)Math.Round((celsius - integerPart) / 0.25);

        RegisterSpan[TemperatureRegister] = (byte)integerPart;
        RegisterSpan[TemperatureRegister + 1] = (byte)(fractionalPart << 6);
    }

    /// <summary>
    /// Sets the oscillator stop flag (OSF) to simulate a power loss condition.
    /// </summary>
    /// <param name="flagSet">True to set the OSF bit, false to clear it.</param>
    public void SetOscillatorStopFlag(bool flagSet)
    {
        if (flagSet)
        {
            RegisterSpan[StatusRegister] |= 0x80;
        }
        else
        {
            RegisterSpan[StatusRegister] = (byte)(RegisterSpan[StatusRegister] & ~0x80);
        }
    }

    private static byte DecToBcd(int value) => (byte)((value / 10 * 16) + (value % 10));
}
