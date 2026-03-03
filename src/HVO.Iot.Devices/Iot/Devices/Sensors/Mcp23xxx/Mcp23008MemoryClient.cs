using HVO.Iot.Devices.Implementation;

namespace HVO.Iot.Devices.Iot.Devices.Sensors.Mcp23xxx;

/// <summary>
/// In-memory simulation of the MCP23008 I/O expander for unit testing.
/// Provides direct register access to inspect and manipulate simulated GPIO state.
/// </summary>
/// <remarks>
/// <para>
/// All 11 registers (IODIR through OLAT, 0x00–0x0A) are backed by the base class register array.
/// By default, IODIR is set to 0xFF (all pins configured as inputs) to match the hardware power-on default.
/// </para>
/// <para>
/// Write operations to the GPIO register (0x09) are reflected in the OLAT register (0x0A) automatically,
/// mimicking hardware behaviour where writing GPIO updates the output latch.
/// </para>
/// </remarks>
public class Mcp23008MemoryClient : MemoryI2cRegisterClient
{
    /// <summary>
    /// Initializes a new memory-backed MCP23008 client with hardware power-on defaults.
    /// </summary>
    /// <param name="busId">Simulated bus ID.</param>
    /// <param name="address">Simulated device address (default 0x20).</param>
    public Mcp23008MemoryClient(int busId = 1, int address = Mcp23008.DefaultI2cAddress)
        : base(busId, address, registerCount: 256)
    {
        // Match hardware power-on defaults: all pins are inputs
        RegisterSpan[Mcp23008.RegisterIodir] = 0xFF;
    }

    /// <summary>
    /// Sets the simulated value of a specific GPIO pin.
    /// Updates the GPIO register to reflect the pin state.
    /// </summary>
    /// <param name="pin">Pin number (0–7).</param>
    /// <param name="value">True for high, false for low.</param>
    public void SetPinValue(byte pin, bool value)
    {
        var gpio = RegisterSpan[Mcp23008.RegisterGpio];
        RegisterSpan[Mcp23008.RegisterGpio] = Mcp23008.SetBit(gpio, pin, value);
    }

    /// <summary>
    /// Sets the entire GPIO register to a specific byte value.
    /// </summary>
    /// <param name="value">The byte value representing all 8 pin states.</param>
    public void SetAllPins(byte value)
    {
        RegisterSpan[Mcp23008.RegisterGpio] = value;
    }

    /// <summary>
    /// Gets the current value of the IODIR register (pin direction configuration).
    /// </summary>
    /// <returns>Byte where 1 = input, 0 = output for each pin.</returns>
    public byte GetIodir() => RegisterSpan[Mcp23008.RegisterIodir];

    /// <summary>
    /// Gets the current value of the GPPU register (pull-up configuration).
    /// </summary>
    /// <returns>Byte where 1 = pull-up enabled for each pin.</returns>
    public byte GetGppu() => RegisterSpan[Mcp23008.RegisterGppu];

    /// <summary>
    /// Gets the current value of the OLAT register (output latch).
    /// </summary>
    /// <returns>The output latch byte value.</returns>
    public byte GetOlat() => RegisterSpan[Mcp23008.RegisterOlat];

    /// <inheritdoc />
    protected override void OnWrite(byte register, System.ReadOnlySpan<byte> data)
    {
        base.OnWrite(register, data);

        // Mirror GPIO writes to OLAT to simulate hardware latch behaviour
        if (register == Mcp23008.RegisterGpio && data.Length >= 1)
        {
            RegisterSpan[Mcp23008.RegisterOlat] = data[0];
        }
    }
}
