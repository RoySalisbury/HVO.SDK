using System;

namespace HVO.Power.OutbackMate
{
    /// <summary>
    /// Represents a parsed record from an Outback FX-series inverter/charger.
    /// </summary>
    public sealed class OutbackMateInverterChargerRecord : IOutbackMateRecord
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OutbackMateInverterChargerRecord"/> class.
        /// </summary>
        /// <param name="recordDateTime">The date and time the record was received.</param>
        /// <param name="hubPort">The hub port number (0-based).</param>
        /// <param name="inverterCurrent">Inverter output current in amps.</param>
        /// <param name="chargerCurrent">Charger input current in amps.</param>
        /// <param name="buyCurrent">Buy (import from grid) current in amps.</param>
        /// <param name="acInputVoltage">AC input voltage in volts.</param>
        /// <param name="acOutputVoltage">AC output voltage in volts.</param>
        /// <param name="sellCurrent">Sell (export to grid) current in amps.</param>
        /// <param name="operationalMode">Current operational mode.</param>
        /// <param name="errorMode">Active error flags.</param>
        /// <param name="acInputMode">AC input qualification mode.</param>
        /// <param name="batteryVoltage">Battery voltage in volts.</param>
        /// <param name="misc">Miscellaneous status flags.</param>
        /// <param name="warningMode">Active warning flags.</param>
        /// <param name="rawData">Original raw CSV data.</param>
        internal OutbackMateInverterChargerRecord(
            DateTimeOffset recordDateTime,
            byte hubPort,
            byte inverterCurrent,
            byte chargerCurrent,
            byte buyCurrent,
            ushort acInputVoltage,
            ushort acOutputVoltage,
            byte sellCurrent,
            OutbackFxInverterChargerOperationalMode operationalMode,
            OutbackFxInverterChargerErrorMode errorMode,
            OutbackFxInverterChargerACInputMode acInputMode,
            decimal batteryVoltage,
            OutbackFxInverterChargerMisc misc,
            OutbackFxInverterChargerWarningMode warningMode,
            string rawData)
        {
            RecordDateTime = recordDateTime;
            HubPort = hubPort;
            InverterCurrent = inverterCurrent;
            ChargerCurrent = chargerCurrent;
            BuyCurrent = buyCurrent;
            ACInputVoltage = acInputVoltage;
            ACOutputVoltage = acOutputVoltage;
            SellCurrent = sellCurrent;
            OperationalMode = operationalMode;
            ErrorMode = errorMode;
            ACInputMode = acInputMode;
            BatteryVoltage = batteryVoltage;
            Misc = misc;
            WarningMode = warningMode;
            RawData = rawData;
        }

        /// <inheritdoc />
        public OutbackMateRecordType RecordType => OutbackMateRecordType.InverterCharger;

        /// <inheritdoc />
        public DateTimeOffset RecordDateTime { get; }

        /// <inheritdoc />
        public byte HubPort { get; }

        /// <inheritdoc />
        public string RawData { get; }

        /// <summary>Gets the inverter output current in amps.</summary>
        public byte InverterCurrent { get; }

        /// <summary>Gets the charger input current in amps.</summary>
        public byte ChargerCurrent { get; }

        /// <summary>Gets the buy (import from grid) current in amps.</summary>
        public byte BuyCurrent { get; }

        /// <summary>Gets the AC input voltage in volts.</summary>
        public ushort ACInputVoltage { get; }

        /// <summary>Gets the AC output voltage in volts.</summary>
        public ushort ACOutputVoltage { get; }

        /// <summary>Gets the sell (export to grid) current in amps.</summary>
        public byte SellCurrent { get; }

        /// <summary>Gets the current operational mode.</summary>
        public OutbackFxInverterChargerOperationalMode OperationalMode { get; }

        /// <summary>Gets the active error flags.</summary>
        public OutbackFxInverterChargerErrorMode ErrorMode { get; }

        /// <summary>Gets the AC input qualification mode.</summary>
        public OutbackFxInverterChargerACInputMode ACInputMode { get; }

        /// <summary>Gets the battery voltage in volts.</summary>
        public decimal BatteryVoltage { get; }

        /// <summary>Gets the miscellaneous status flags.</summary>
        public OutbackFxInverterChargerMisc Misc { get; }

        /// <summary>Gets the active warning flags.</summary>
        public OutbackFxInverterChargerWarningMode WarningMode { get; }
    }
}
