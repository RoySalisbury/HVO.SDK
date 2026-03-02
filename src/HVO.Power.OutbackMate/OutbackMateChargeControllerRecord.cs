using System;

namespace HVO.Power.OutbackMate
{
    /// <summary>
    /// Represents a parsed record from an Outback charge controller (e.g., FM60, FM80).
    /// </summary>
    public sealed class OutbackMateChargeControllerRecord : IOutbackMateRecord
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OutbackMateChargeControllerRecord"/> class.
        /// </summary>
        /// <param name="recordDateTime">The date and time the record was received.</param>
        /// <param name="hubPort">The hub port number (0-based).</param>
        /// <param name="pvAmps">PV array current in amps.</param>
        /// <param name="pvVoltage">PV array voltage in volts.</param>
        /// <param name="chargerAmps">Charge controller output current in amps (includes tenths).</param>
        /// <param name="chargerVoltage">Charge controller output (battery) voltage in volts.</param>
        /// <param name="dailyAmpHoursProduced">Amp-hours produced today.</param>
        /// <param name="dailyWattHoursProduced">Watt-hours produced today.</param>
        /// <param name="mode">Current charge stage.</param>
        /// <param name="auxRelayMode">Auxiliary relay operating mode.</param>
        /// <param name="errorMode">Active error flags.</param>
        /// <param name="rawData">Original raw CSV data.</param>
        internal OutbackMateChargeControllerRecord(
            DateTimeOffset recordDateTime,
            byte hubPort,
            short pvAmps,
            short pvVoltage,
            decimal chargerAmps,
            decimal chargerVoltage,
            short dailyAmpHoursProduced,
            int dailyWattHoursProduced,
            OutbackMateChargeControllerMode mode,
            OutbackMateChargeControllerAuxRelayMode auxRelayMode,
            OutbackMateChargeControllerErrorMode errorMode,
            string rawData)
        {
            RecordDateTime = recordDateTime;
            HubPort = hubPort;
            PVAmps = pvAmps;
            PVVoltage = pvVoltage;
            ChargerAmps = chargerAmps;
            ChargerVoltage = chargerVoltage;
            DailyAmpHoursProduced = dailyAmpHoursProduced;
            DailyWattHoursProduced = dailyWattHoursProduced;
            Mode = mode;
            AuxRelayMode = auxRelayMode;
            ErrorMode = errorMode;
            RawData = rawData;
        }

        /// <inheritdoc />
        public OutbackMateRecordType RecordType => OutbackMateRecordType.ChargeController;

        /// <inheritdoc />
        public DateTimeOffset RecordDateTime { get; }

        /// <inheritdoc />
        public byte HubPort { get; }

        /// <inheritdoc />
        public string RawData { get; }

        /// <summary>Gets the PV array current in amps.</summary>
        public short PVAmps { get; }

        /// <summary>Gets the PV array voltage in volts.</summary>
        public short PVVoltage { get; }

        /// <summary>Gets the charge controller output current in amps (includes tenths).</summary>
        public decimal ChargerAmps { get; }

        /// <summary>Gets the charge controller output (battery) voltage in volts.</summary>
        public decimal ChargerVoltage { get; }

        /// <summary>Gets the amp-hours produced today.</summary>
        public short DailyAmpHoursProduced { get; }

        /// <summary>Gets the watt-hours produced today.</summary>
        public int DailyWattHoursProduced { get; }

        /// <summary>Gets the current charge stage.</summary>
        public OutbackMateChargeControllerMode Mode { get; }

        /// <summary>Gets the auxiliary relay operating mode.</summary>
        public OutbackMateChargeControllerAuxRelayMode AuxRelayMode { get; }

        /// <summary>Gets the active error flags.</summary>
        public OutbackMateChargeControllerErrorMode ErrorMode { get; }
    }
}
