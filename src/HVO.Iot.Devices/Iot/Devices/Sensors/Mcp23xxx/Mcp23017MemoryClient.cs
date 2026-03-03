using HVO.Iot.Devices.Implementation;

namespace HVO.Iot.Devices.Iot.Devices.Sensors.Mcp23xxx;

/// <summary>
/// In-memory simulation of the MCP23017 16-port I/O expander for unit testing.
/// Provides direct register access to inspect and manipulate simulated GPIO state
/// for both port A (pins 0–7) and port B (pins 8–15).
/// </summary>
/// <remarks>
/// <para>
/// All 22 registers (IODIRA through OLATB, 0x00–0x15) are backed by the base class register array.
/// By default, IODIRA and IODIRB are set to 0xFF (all pins configured as inputs) to match
/// the hardware power-on default.
/// </para>
/// <para>
/// Write operations to GPIO registers are mirrored to the corresponding OLAT registers to
/// simulate hardware latch behaviour.
/// </para>
/// </remarks>
public class Mcp23017MemoryClient : MemoryI2cRegisterClient
{
    /// <summary>
    /// Initializes a new memory-backed MCP23017 client with hardware power-on defaults.
    /// </summary>
    /// <param name="busId">Simulated bus ID.</param>
    /// <param name="address">Simulated device address (default 0x20).</param>
    public Mcp23017MemoryClient(int busId = 1, int address = Mcp23017.DefaultI2cAddress)
        : base(busId, address, registerCount: 256)
    {
        // Match hardware power-on defaults: all pins are inputs on both ports
        RegisterSpan[Mcp23017.RegisterIodirA] = 0xFF;
        RegisterSpan[Mcp23017.RegisterIodirB] = 0xFF;
    }

    /// <summary>
    /// Sets the simulated value of a specific GPIO pin.
    /// Pins 0–7 update port A GPIO register; pins 8–15 update port B.
    /// </summary>
    /// <param name="pin">Pin number (0–15).</param>
    /// <param name="value">True for high, false for low.</param>
    public void SetPinValue(byte pin, bool value)
    {
        if (pin < 8)
        {
            var gpio = RegisterSpan[Mcp23017.RegisterGpioA];
            RegisterSpan[Mcp23017.RegisterGpioA] = Mcp23008.SetBit(gpio, pin, value);
        }
        else
        {
            var localPin = (byte)(pin - 8);
            var gpio = RegisterSpan[Mcp23017.RegisterGpioB];
            RegisterSpan[Mcp23017.RegisterGpioB] = Mcp23008.SetBit(gpio, localPin, value);
        }
    }

    /// <summary>
    /// Sets the entire GPIO port A register (pins 0–7) to a specific byte value.
    /// </summary>
    /// <param name="value">The byte value representing all 8 port A pin states.</param>
    public void SetPortA(byte value) => RegisterSpan[Mcp23017.RegisterGpioA] = value;

    /// <summary>
    /// Sets the entire GPIO port B register (pins 8–15) to a specific byte value.
    /// </summary>
    /// <param name="value">The byte value representing all 8 port B pin states.</param>
    public void SetPortB(byte value) => RegisterSpan[Mcp23017.RegisterGpioB] = value;

    /// <summary>
    /// Gets the current value of the IODIRA register (port A pin direction configuration).
    /// </summary>
    /// <returns>Byte where 1 = input, 0 = output for each port A pin.</returns>
    public byte GetIodirA() => RegisterSpan[Mcp23017.RegisterIodirA];

    /// <summary>
    /// Gets the current value of the IODIRB register (port B pin direction configuration).
    /// </summary>
    /// <returns>Byte where 1 = input, 0 = output for each port B pin.</returns>
    public byte GetIodirB() => RegisterSpan[Mcp23017.RegisterIodirB];

    /// <summary>
    /// Gets the current value of the GPPUA register (port A pull-up configuration).
    /// </summary>
    /// <returns>Byte where 1 = pull-up enabled for each port A pin.</returns>
    public byte GetGppuA() => RegisterSpan[Mcp23017.RegisterGppuA];

    /// <summary>
    /// Gets the current value of the GPPUB register (port B pull-up configuration).
    /// </summary>
    /// <returns>Byte where 1 = pull-up enabled for each port B pin.</returns>
    public byte GetGppuB() => RegisterSpan[Mcp23017.RegisterGppuB];

    /// <summary>
    /// Gets the current value of the OLATA register (port A output latch).
    /// </summary>
    public byte GetOlatA() => RegisterSpan[Mcp23017.RegisterOlatA];

    /// <summary>
    /// Gets the current value of the OLATB register (port B output latch).
    /// </summary>
    public byte GetOlatB() => RegisterSpan[Mcp23017.RegisterOlatB];

    /// <inheritdoc />
    protected override void OnWrite(byte register, System.ReadOnlySpan<byte> data)
    {
        base.OnWrite(register, data);

        // Mirror GPIO writes to the corresponding OLAT register
        if (register == Mcp23017.RegisterGpioA && data.Length >= 1)
        {
            RegisterSpan[Mcp23017.RegisterOlatA] = data[0];
        }
        else if (register == Mcp23017.RegisterGpioB && data.Length >= 1)
        {
            RegisterSpan[Mcp23017.RegisterOlatB] = data[0];
        }
    }
}
