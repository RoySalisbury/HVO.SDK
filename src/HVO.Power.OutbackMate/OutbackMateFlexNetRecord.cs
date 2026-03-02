using System;

namespace HVO.Power.OutbackMate
{
    /// <summary>
    /// Represents a parsed record from an Outback FlexNet DC battery monitor.
    /// </summary>
    /// <remarks>
    /// The FlexNet DC monitors battery current via up to three shunts (A, B, C) and
    /// reports a single rotating extra value each cycle (accumulated amp-hours,
    /// watt-hours, days since full, etc.).
    /// </remarks>
    public sealed class OutbackMateFlexNetRecord : IOutbackMateRecord
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OutbackMateFlexNetRecord"/> class.
        /// </summary>
        /// <param name="recordDateTime">The date and time the record was received.</param>
        /// <param name="hubPort">The hub port number (0-based).</param>
        /// <param name="batteryVoltage">Battery voltage in volts.</param>
        /// <param name="batteryStateOfCharge">Battery state of charge (0–100 percent).</param>
        /// <param name="batteryTemperatureC">Battery temperature in degrees Celsius, or <c>null</c> if the sensor is not connected.</param>
        /// <param name="chargeParamsMet">Whether the charge parameters have been met.</param>
        /// <param name="relayState">Physical state of the relay.</param>
        /// <param name="relayMode">Relay operating mode (manual or automatic).</param>
        /// <param name="shuntAEnabled">Whether shunt A is enabled.</param>
        /// <param name="shuntAAmps">Current through shunt A in amps (negative = discharge).</param>
        /// <param name="shuntBEnabled">Whether shunt B is enabled.</param>
        /// <param name="shuntBAmps">Current through shunt B in amps (negative = discharge).</param>
        /// <param name="shuntCEnabled">Whether shunt C is enabled.</param>
        /// <param name="shuntCAmps">Current through shunt C in amps (negative = discharge).</param>
        /// <param name="extraValueType">The type of the rotating extra value, or <c>null</c> if not recognized.</param>
        /// <param name="extraValue">The rotating extra value, or <c>null</c> if not recognized.</param>
        /// <param name="rawData">Original raw CSV data.</param>
        internal OutbackMateFlexNetRecord(
            DateTimeOffset recordDateTime,
            byte hubPort,
            decimal batteryVoltage,
            byte batteryStateOfCharge,
            short? batteryTemperatureC,
            bool chargeParamsMet,
            OutbackMateFlexNetRelayState relayState,
            OutbackMateFlexNetRelayMode relayMode,
            bool shuntAEnabled,
            decimal shuntAAmps,
            bool shuntBEnabled,
            decimal shuntBAmps,
            bool shuntCEnabled,
            decimal shuntCAmps,
            OutbackMateFlexNetExtraValueType? extraValueType,
            decimal? extraValue,
            string rawData)
        {
            RecordDateTime = recordDateTime;
            HubPort = hubPort;
            BatteryVoltage = batteryVoltage;
            BatteryStateOfCharge = batteryStateOfCharge;
            BatteryTemperatureC = batteryTemperatureC;
            ChargeParamsMet = chargeParamsMet;
            RelayState = relayState;
            RelayMode = relayMode;
            ShuntAEnabled = shuntAEnabled;
            ShuntAAmps = shuntAAmps;
            ShuntBEnabled = shuntBEnabled;
            ShuntBAmps = shuntBAmps;
            ShuntCEnabled = shuntCEnabled;
            ShuntCAmps = shuntCAmps;
            ExtraValueType = extraValueType;
            ExtraValue = extraValue;
            RawData = rawData;
        }

        /// <inheritdoc />
        public OutbackMateRecordType RecordType => OutbackMateRecordType.FlexNetDC;

        /// <inheritdoc />
        public DateTimeOffset RecordDateTime { get; }

        /// <inheritdoc />
        public byte HubPort { get; }

        /// <inheritdoc />
        public string RawData { get; }

        /// <summary>Gets the battery voltage in volts.</summary>
        public decimal BatteryVoltage { get; }

        /// <summary>Gets the battery state of charge (0–100 percent).</summary>
        public byte BatteryStateOfCharge { get; }

        /// <summary>
        /// Gets the battery temperature in degrees Celsius,
        /// or <c>null</c> if the sensor is not connected (raw value 99).
        /// </summary>
        public short? BatteryTemperatureC { get; }

        /// <summary>Gets a value indicating whether the charge parameters have been met.</summary>
        public bool ChargeParamsMet { get; }

        /// <summary>Gets the physical relay state.</summary>
        public OutbackMateFlexNetRelayState RelayState { get; }

        /// <summary>Gets the relay operating mode.</summary>
        public OutbackMateFlexNetRelayMode RelayMode { get; }

        /// <summary>Gets a value indicating whether shunt A is enabled.</summary>
        public bool ShuntAEnabled { get; }

        /// <summary>Gets the current through shunt A in amps (negative = discharge).</summary>
        public decimal ShuntAAmps { get; }

        /// <summary>Gets a value indicating whether shunt B is enabled.</summary>
        public bool ShuntBEnabled { get; }

        /// <summary>Gets the current through shunt B in amps (negative = discharge).</summary>
        public decimal ShuntBAmps { get; }

        /// <summary>Gets a value indicating whether shunt C is enabled.</summary>
        public bool ShuntCEnabled { get; }

        /// <summary>Gets the current through shunt C in amps (negative = discharge).</summary>
        public decimal ShuntCAmps { get; }

        /// <summary>
        /// Gets the type of the rotating extra value, or <c>null</c> if the type was not recognized.
        /// </summary>
        public OutbackMateFlexNetExtraValueType? ExtraValueType { get; }

        /// <summary>
        /// Gets the rotating extra value, or <c>null</c> if the type was not recognized.
        /// </summary>
        public decimal? ExtraValue { get; }
    }
}
