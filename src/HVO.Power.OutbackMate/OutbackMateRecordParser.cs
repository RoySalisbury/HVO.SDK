using System;

namespace HVO.Power.OutbackMate
{
    /// <summary>
    /// Parses raw CSV data from an Outback Mate controller into strongly-typed
    /// <see cref="IOutbackMateRecord"/> instances.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Supports both Mate1 and Mate2 protocols. The two protocols use different CSV
    /// field layouts:
    /// </para>
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// <b>Mate1</b>: The first field is a character that identifies both the device type
    /// and hub port — 'A'–'K' for charge controllers, 'a'–'j' for FlexNet DC monitors,
    /// '0'–'9' or ';' for inverter/chargers.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <b>Mate2</b>: The first field is the numeric hub port address and the second field
    /// is the device type identifier — 2 = inverter/charger, 3 = charge controller,
    /// 4 = FlexNet DC.
    /// </description>
    /// </item>
    /// </list>
    /// <para>
    /// Use <see cref="Parse"/> for auto-detection, or <see cref="ParseMate1(DateTimeOffset, string)"/> /
    /// <see cref="ParseMate2(DateTimeOffset, string)"/> when the protocol version is known.
    /// </para>
    /// </remarks>
    public static class OutbackMateRecordParser
    {
        private const string Mate1ChargeControllerChars = "ABCDEFGHIJK";
        private const string Mate1FlexNetChars = "abcdefghij";
        private const string Mate1InverterChargerChars = "0123456789;";

        /// <summary>
        /// Status flags embedded in the FlexNet DC status field.
        /// </summary>
        [Flags]
        private enum FlexNetStatusFlags
        {
            ChargeParamsMet = 1,
            RelayStateOpen = 2,
            RelayModeAutomatic = 4,
            ShuntAValueNegative = 8,
            ShuntBValueNegative = 16,
            ShuntCValueNegative = 32
        }

        /// <summary>
        /// Internal enum mapping raw extra-info type codes (0-based) to
        /// <see cref="OutbackMateFlexNetExtraValueType"/> values.
        /// </summary>
        private enum FlexNetExtraInfoType
        {
            AccumulatedShuntAAmpHours = 0,
            AccumulatedShuntAWattHours = 1,
            AccumulatedShuntBAmpHours = 2,
            AccumulatedShuntBWattHours = 3,
            AccumulatedShuntCAmpHours = 4,
            AccumulatedShuntCWattHours = 5,
            DaysSinceFull = 6,
            TodaysMinimumSOC = 7,
            TodaysNetInputAmpHours = 8,
            TodaysNetOutputAmpHours = 9,
            TodaysNetInputWattHours = 10,
            TodaysNetOutputWattHours = 11,
            ChargeFactorCorrectedNetAmpHours = 12,
            ChargeFactorCorrectedNetWattHours = 13
        }

        /// <summary>
        /// Parses a raw CSV record from an Outback Mate controller, auto-detecting
        /// the protocol version (Mate1 or Mate2).
        /// </summary>
        /// <param name="recordDateTime">The date and time the record was received.</param>
        /// <param name="rawRecord">The raw CSV data from the Mate controller.</param>
        /// <returns>
        /// A parsed <see cref="IOutbackMateRecord"/> instance, or <c>null</c> if the
        /// record cannot be recognized.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="rawRecord"/> is <c>null</c> or empty.</exception>
        public static IOutbackMateRecord? Parse(DateTimeOffset recordDateTime, string rawRecord)
        {
            if (string.IsNullOrWhiteSpace(rawRecord))
            {
                throw new ArgumentNullException(nameof(rawRecord));
            }

            // Auto-detect protocol version based on the first field.
            // Mate1: first field is a single character (letter or digit/semicolon).
            // Mate2: first field is a numeric hub port, second field is device_id (2, 3, or 4).
            string[] parts = rawRecord.Split(',');
            if (parts.Length < 2)
            {
                return null;
            }

            string firstField = parts[0];

            // If the first character is a letter, it's Mate1.
            if (firstField.Length == 1 && char.IsLetter(firstField[0]))
            {
                return ParseMate1(recordDateTime, rawRecord, parts);
            }

            // If the first field is numeric and the second field is a known device_id, it's Mate2.
            if (int.TryParse(firstField, out _) && parts.Length >= 2)
            {
                string secondField = parts[1];
                if (secondField == "2" || secondField == "3" || secondField == "4")
                {
                    return ParseMate2(recordDateTime, rawRecord, parts);
                }
            }

            // Mate1 inverter/chargers use '0'-'9' or ';' as the first field,
            // which are numeric; check for those explicitly.
            if (firstField.Length == 1 && Mate1InverterChargerChars.IndexOf(firstField[0]) >= 0)
            {
                return ParseMate1(recordDateTime, rawRecord, parts);
            }

            return null;
        }

        /// <summary>
        /// Parses a raw CSV record using the Mate1 protocol.
        /// </summary>
        /// <param name="recordDateTime">The date and time the record was received.</param>
        /// <param name="rawRecord">The raw CSV data from the Mate1 controller.</param>
        /// <returns>
        /// A parsed <see cref="IOutbackMateRecord"/> instance, or <c>null</c> if the
        /// record cannot be recognized.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="rawRecord"/> is <c>null</c> or empty.</exception>
        public static IOutbackMateRecord? ParseMate1(DateTimeOffset recordDateTime, string rawRecord)
        {
            if (string.IsNullOrWhiteSpace(rawRecord))
            {
                throw new ArgumentNullException(nameof(rawRecord));
            }

            return ParseMate1(recordDateTime, rawRecord, rawRecord.Split(','));
        }

        /// <summary>
        /// Parses a raw CSV record using the Mate2 protocol.
        /// </summary>
        /// <param name="recordDateTime">The date and time the record was received.</param>
        /// <param name="rawRecord">The raw CSV data from the Mate2 controller.</param>
        /// <returns>
        /// A parsed <see cref="IOutbackMateRecord"/> instance, or <c>null</c> if the
        /// record cannot be recognized.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="rawRecord"/> is <c>null</c> or empty.</exception>
        public static IOutbackMateRecord? ParseMate2(DateTimeOffset recordDateTime, string rawRecord)
        {
            if (string.IsNullOrWhiteSpace(rawRecord))
            {
                throw new ArgumentNullException(nameof(rawRecord));
            }

            return ParseMate2(recordDateTime, rawRecord, rawRecord.Split(','));
        }

        #region Mate1 Parsing

        private static IOutbackMateRecord? ParseMate1(DateTimeOffset recordDateTime, string rawRecord, string[] parts)
        {
            if (parts.Length < 2)
            {
                return null;
            }

            string firstField = parts[0];
            if (firstField.Length == 0)
            {
                return null;
            }

            char typeChar = firstField[0];

            if (Mate1ChargeControllerChars.IndexOf(typeChar) >= 0)
            {
                return ParseMate1ChargeController(recordDateTime, rawRecord, parts);
            }

            if (Mate1FlexNetChars.IndexOf(typeChar) >= 0)
            {
                return ParseMate1FlexNet(recordDateTime, rawRecord, parts);
            }

            if (Mate1InverterChargerChars.IndexOf(typeChar) >= 0)
            {
                return ParseMate1InverterCharger(recordDateTime, rawRecord, parts);
            }

            return null;
        }

        private static OutbackMateChargeControllerRecord? ParseMate1ChargeController(
            DateTimeOffset recordDateTime, string rawRecord, string[] parts)
        {
            if (parts.Length < 14)
            {
                return null;
            }

            // Mate1 CC field layout:
            // [0]=port_char, [1]=unused, [2]=charge_amps_whole, [3]=pv_amps, [4]=pv_volts,
            // [5]=daily_kwh, [6]=charge_tenths, [7]=aux_mode, [8]=error_modes, [9]=charge_mode,
            // [10]=battery_volts*10, [11]=daily_ah, [12]=unused, [13]=crc
            byte hubPort = (byte)(parts[0][0] - 'A');

            short pvAmps = Convert.ToInt16(parts[3]);
            short pvVoltage = Convert.ToInt16(parts[4]);

            decimal chargerAmps = Convert.ToDecimal(parts[2]) + (Convert.ToDecimal(parts[6]) / 10m);
            decimal chargerVoltage = Convert.ToDecimal(parts[10]) / 10m;

            short dailyAmpHoursProduced = Convert.ToInt16(parts[11]);
            int dailyWattHoursProduced = Convert.ToInt32(parts[5]) * 100;

            byte chargerAuxRelayMode = Convert.ToByte(parts[7]);
            short chargerErrorMode = Convert.ToInt16(parts[8]);
            byte chargerMode = Convert.ToByte(parts[9]);

            return new OutbackMateChargeControllerRecord(
                recordDateTime, hubPort, pvAmps, pvVoltage, chargerAmps, chargerVoltage,
                dailyAmpHoursProduced, dailyWattHoursProduced,
                (OutbackMateChargeControllerMode)chargerMode,
                (OutbackMateChargeControllerAuxRelayMode)chargerAuxRelayMode,
                (OutbackMateChargeControllerErrorMode)chargerErrorMode,
                rawRecord);
        }

        private static OutbackMateFlexNetRecord? ParseMate1FlexNet(
            DateTimeOffset recordDateTime, string rawRecord, string[] parts)
        {
            if (parts.Length < 12)
            {
                return null;
            }

            // Mate1 FlexNet field layout:
            // [0]=port_char, [1]=shuntA*10, [2]=shuntB*10, [3]=shuntC*10,
            // [4]=extra_info_type, [5]=extra_value, [6]=batt_volts*10, [7]=soc,
            // [8]=shunt_enabled, [9]=status_flags, [10]=batt_temp, [11]=crc
            byte hubPort = (byte)(parts[0][0] - 'a');

            return ParseFlexNetCommon(recordDateTime, rawRecord, hubPort,
                parts[1], parts[2], parts[3],     // shunt amps * 10
                parts[6],                          // battery voltage * 10
                parts[7],                          // SOC
                parts[8],                          // shunt enabled flags
                parts[10],                         // battery temp
                parts[4],                          // extra info type
                parts[5],                          // extra value
                parts[9]);                         // status flags
        }

        private static OutbackMateInverterChargerRecord? ParseMate1InverterCharger(
            DateTimeOffset recordDateTime, string rawRecord, string[] parts)
        {
            if (parts.Length < 13)
            {
                return null;
            }

            // Mate1 Inverter/Charger field layout:
            // [0]=port_char, [1]=inverter_amps, [2]=charger_amps, [3]=buy_amps,
            // [4]=ac_input_volts, [5]=ac_output_volts, [6]=sell_amps,
            // [7]=fx_op_mode, [8]=error_mode, [9]=ac_input_mode,
            // [10]=batt_volts*10, [11]=misc, [12]=warning_mode
            byte hubPort = (byte)(parts[0][0] - '0');

            return ParseInverterChargerCommon(recordDateTime, rawRecord, hubPort,
                parts[1], parts[2], parts[3],
                parts[4], parts[5], parts[6],
                parts[7], parts[8], parts[9],
                parts[10], parts[11], parts[12]);
        }

        #endregion

        #region Mate2 Parsing

        private static IOutbackMateRecord? ParseMate2(DateTimeOffset recordDateTime, string rawRecord, string[] parts)
        {
            if (parts.Length < 2)
            {
                return null;
            }

            string deviceId = parts[1];

            switch (deviceId)
            {
                case "3":
                    return ParseMate2ChargeController(recordDateTime, rawRecord, parts);
                case "4":
                    return ParseMate2FlexNet(recordDateTime, rawRecord, parts);
                case "2":
                    return ParseMate2InverterCharger(recordDateTime, rawRecord, parts);
                default:
                    return null;
            }
        }

        private static OutbackMateChargeControllerRecord? ParseMate2ChargeController(
            DateTimeOffset recordDateTime, string rawRecord, string[] parts)
        {
            if (parts.Length < 14)
            {
                return null;
            }

            // Mate2 CC field layout:
            // [0]=address, [1]=device_id(3), [2]=unused, [3]=charge_current,
            // [4]=pv_current, [5]=pv_voltage, [6]=daily_kwh,
            // [7]=charge_tenths, [8]=aux_mode, [9]=error_modes, [10]=charge_mode,
            // [11]=battery_volts*10, [12]=daily_ah, [13]=unused
            byte hubPort = Convert.ToByte(parts[0]);

            decimal chargerAmps = Convert.ToDecimal(parts[3]) + (Convert.ToDecimal(parts[7]) / 10m);
            short pvAmps = Convert.ToInt16(parts[4]);
            short pvVoltage = Convert.ToInt16(parts[5]);
            int dailyWattHoursProduced = Convert.ToInt32(parts[6]) * 100;
            byte chargerAuxRelayMode = Convert.ToByte(parts[8]);
            short chargerErrorMode = Convert.ToInt16(parts[9]);
            byte chargerMode = Convert.ToByte(parts[10]);
            decimal chargerVoltage = Convert.ToDecimal(parts[11]) / 10m;
            short dailyAmpHoursProduced = Convert.ToInt16(parts[12]);

            return new OutbackMateChargeControllerRecord(
                recordDateTime, hubPort, pvAmps, pvVoltage, chargerAmps, chargerVoltage,
                dailyAmpHoursProduced, dailyWattHoursProduced,
                (OutbackMateChargeControllerMode)chargerMode,
                (OutbackMateChargeControllerAuxRelayMode)chargerAuxRelayMode,
                (OutbackMateChargeControllerErrorMode)chargerErrorMode,
                rawRecord);
        }

        private static OutbackMateFlexNetRecord? ParseMate2FlexNet(
            DateTimeOffset recordDateTime, string rawRecord, string[] parts)
        {
            if (parts.Length < 12)
            {
                return null;
            }

            // Mate2 FlexNet field layout:
            // [0]=address, [1]=device_id(4), [2]=shuntA*10, [3]=shuntB*10,
            // [4]=shuntC*10, [5]=extra_info_type, [6]=extra_value,
            // [7]=batt_volts*10, [8]=soc, [9]=shunt_enabled,
            // [10]=status_flags, [11]=batt_temp
            byte hubPort = Convert.ToByte(parts[0]);

            return ParseFlexNetCommon(recordDateTime, rawRecord, hubPort,
                parts[2], parts[3], parts[4],     // shunt amps * 10
                parts[7],                          // battery voltage * 10
                parts[8],                          // SOC
                parts[9],                          // shunt enabled flags
                parts[11],                         // battery temp
                parts[5],                          // extra info type
                parts[6],                          // extra value
                parts[10]);                        // status flags
        }

        private static OutbackMateInverterChargerRecord? ParseMate2InverterCharger(
            DateTimeOffset recordDateTime, string rawRecord, string[] parts)
        {
            if (parts.Length < 14)
            {
                return null;
            }

            // Mate2 Inverter/Charger field layout:
            // [0]=address, [1]=device_id(2), [2]=inverter_amps, [3]=charger_amps,
            // [4]=buy_amps, [5]=ac_input_volts, [6]=ac_output_volts, [7]=sell_amps,
            // [8]=fx_op_mode, [9]=error_mode, [10]=ac_input_mode,
            // [11]=batt_volts*10, [12]=misc, [13]=warning_mode
            byte hubPort = Convert.ToByte(parts[0]);

            return ParseInverterChargerCommon(recordDateTime, rawRecord, hubPort,
                parts[2], parts[3], parts[4],
                parts[5], parts[6], parts[7],
                parts[8], parts[9], parts[10],
                parts[11], parts[12], parts[13]);
        }

        #endregion

        #region Shared Parsing Helpers

        private static OutbackMateInverterChargerRecord ParseInverterChargerCommon(
            DateTimeOffset recordDateTime, string rawRecord, byte hubPort,
            string inverterCurrentStr, string chargerCurrentStr, string buyCurrentStr,
            string acInputVoltageStr, string acOutputVoltageStr, string sellCurrentStr,
            string opModeStr, string errorModeStr, string acInputModeStr,
            string batteryVoltageStr, string miscStr, string warningModeStr)
        {
            byte inverterCurrent = Convert.ToByte(inverterCurrentStr);
            byte chargerCurrent = Convert.ToByte(chargerCurrentStr);
            byte buyCurrent = Convert.ToByte(buyCurrentStr);

            ushort acInputVoltage = Convert.ToUInt16(acInputVoltageStr);
            ushort acOutputVoltage = Convert.ToUInt16(acOutputVoltageStr);
            byte sellCurrent = Convert.ToByte(sellCurrentStr);

            int fxOperationalMode = Convert.ToInt32(opModeStr);
            int errorMode = Convert.ToInt32(errorModeStr);
            int acInputMode = Convert.ToInt32(acInputModeStr);

            decimal batteryVoltage = Convert.ToDecimal(batteryVoltageStr) / 10m;
            int misc = Convert.ToInt32(miscStr);
            int warningMode = Convert.ToInt32(warningModeStr);

            return new OutbackMateInverterChargerRecord(
                recordDateTime, hubPort, inverterCurrent, chargerCurrent, buyCurrent,
                acInputVoltage, acOutputVoltage, sellCurrent,
                (OutbackFxInverterChargerOperationalMode)fxOperationalMode,
                (OutbackFxInverterChargerErrorMode)errorMode,
                (OutbackFxInverterChargerACInputMode)acInputMode,
                batteryVoltage,
                (OutbackFxInverterChargerMisc)misc,
                (OutbackFxInverterChargerWarningMode)warningMode,
                rawRecord);
        }

        private static OutbackMateFlexNetRecord ParseFlexNetCommon(
            DateTimeOffset recordDateTime, string rawRecord, byte hubPort,
            string shuntAStr, string shuntBStr, string shuntCStr,
            string batteryVoltageStr, string socStr, string shuntEnabledStr,
            string batteryTempStr, string extraInfoTypeStr, string extraValueStr,
            string statusFlagsStr)
        {
            decimal shuntAAmps = Convert.ToDecimal(shuntAStr) / 10m;
            decimal shuntBAmps = Convert.ToDecimal(shuntBStr) / 10m;
            decimal shuntCAmps = Convert.ToDecimal(shuntCStr) / 10m;

            decimal batteryVoltage = Convert.ToDecimal(batteryVoltageStr) / 10m;
            byte batteryStateOfCharge = Convert.ToByte(socStr);

            bool shuntAEnabled = shuntEnabledStr.Length > 0 && shuntEnabledStr[0] == '0';
            bool shuntBEnabled = shuntEnabledStr.Length > 1 && shuntEnabledStr[1] == '0';
            bool shuntCEnabled = shuntEnabledStr.Length > 2 && shuntEnabledStr[2] == '0';

            short rawBatteryTemp = Convert.ToInt16(batteryTempStr);
            short? batteryTemperatureC = rawBatteryTemp != 99
                ? (short)(rawBatteryTemp - 10)
                : (short?)null;

            // Parse extra value (rotating data field)
            bool isExtraValueNegative = (Convert.ToByte(extraInfoTypeStr) & 0x40) == 0x40;
            FlexNetExtraInfoType flexNetExtraInfoType =
                (FlexNetExtraInfoType)(Convert.ToInt32(extraInfoTypeStr) & 0x3F);

            ParseFlexNetExtraValue(flexNetExtraInfoType, extraValueStr, isExtraValueNegative,
                out OutbackMateFlexNetExtraValueType? extraValueType, out decimal? extraValue);

            // Parse status flags
            FlexNetStatusFlags statusFlags = (FlexNetStatusFlags)Convert.ToInt32(statusFlagsStr);

            OutbackMateFlexNetRelayState relayState =
                (statusFlags & FlexNetStatusFlags.RelayStateOpen) == FlexNetStatusFlags.RelayStateOpen
                    ? OutbackMateFlexNetRelayState.Open
                    : OutbackMateFlexNetRelayState.Closed;

            OutbackMateFlexNetRelayMode relayMode =
                (statusFlags & FlexNetStatusFlags.RelayModeAutomatic) == FlexNetStatusFlags.RelayModeAutomatic
                    ? OutbackMateFlexNetRelayMode.Automatic
                    : OutbackMateFlexNetRelayMode.Manual;

            bool chargeParamsMet =
                (statusFlags & FlexNetStatusFlags.ChargeParamsMet) == FlexNetStatusFlags.ChargeParamsMet;

            // Apply sign to shunt values based on status flags
            decimal signedShuntA = shuntAAmps * ((statusFlags & FlexNetStatusFlags.ShuntAValueNegative) == FlexNetStatusFlags.ShuntAValueNegative ? -1 : 1);
            decimal signedShuntB = shuntBAmps * ((statusFlags & FlexNetStatusFlags.ShuntBValueNegative) == FlexNetStatusFlags.ShuntBValueNegative ? -1 : 1);
            decimal signedShuntC = shuntCAmps * ((statusFlags & FlexNetStatusFlags.ShuntCValueNegative) == FlexNetStatusFlags.ShuntCValueNegative ? -1 : 1);

            return new OutbackMateFlexNetRecord(
                recordDateTime, hubPort, batteryVoltage, batteryStateOfCharge, batteryTemperatureC,
                chargeParamsMet, relayState, relayMode,
                shuntAEnabled, signedShuntA,
                shuntBEnabled, signedShuntB,
                shuntCEnabled, signedShuntC,
                extraValueType, extraValue,
                rawRecord);
        }

        private static void ParseFlexNetExtraValue(
            FlexNetExtraInfoType infoType,
            string rawValueStr,
            bool isNegative,
            out OutbackMateFlexNetExtraValueType? extraValueType,
            out decimal? extraValue)
        {
            extraValueType = null;
            extraValue = null;

            int rawValue = Convert.ToInt32(rawValueStr);
            int signedRawValue = rawValue * (isNegative ? -1 : 1);

            switch (infoType)
            {
                case FlexNetExtraInfoType.AccumulatedShuntAAmpHours:
                    extraValueType = OutbackMateFlexNetExtraValueType.AccumulatedShuntAAmpHours;
                    extraValue = signedRawValue;
                    break;
                case FlexNetExtraInfoType.AccumulatedShuntAWattHours:
                    extraValueType = OutbackMateFlexNetExtraValueType.AccumulatedShuntAWattHours;
                    extraValue = signedRawValue * 10;
                    break;
                case FlexNetExtraInfoType.AccumulatedShuntBAmpHours:
                    extraValueType = OutbackMateFlexNetExtraValueType.AccumulatedShuntBAmpHours;
                    extraValue = signedRawValue;
                    break;
                case FlexNetExtraInfoType.AccumulatedShuntBWattHours:
                    extraValueType = OutbackMateFlexNetExtraValueType.AccumulatedShuntBWattHours;
                    extraValue = signedRawValue * 10;
                    break;
                case FlexNetExtraInfoType.AccumulatedShuntCAmpHours:
                    extraValueType = OutbackMateFlexNetExtraValueType.AccumulatedShuntCAmpHours;
                    extraValue = signedRawValue;
                    break;
                case FlexNetExtraInfoType.AccumulatedShuntCWattHours:
                    extraValueType = OutbackMateFlexNetExtraValueType.AccumulatedShuntCWattHours;
                    extraValue = signedRawValue * 10;
                    break;
                case FlexNetExtraInfoType.DaysSinceFull:
                    extraValueType = OutbackMateFlexNetExtraValueType.DaysSinceFull;
                    extraValue = (decimal)signedRawValue / 10m;
                    break;
                case FlexNetExtraInfoType.TodaysMinimumSOC:
                    extraValueType = OutbackMateFlexNetExtraValueType.TodaysMinimumSOC;
                    extraValue = Convert.ToByte(rawValueStr);
                    break;
                case FlexNetExtraInfoType.TodaysNetInputAmpHours:
                    extraValueType = OutbackMateFlexNetExtraValueType.TodaysNetInputAmpHours;
                    extraValue = signedRawValue;
                    break;
                case FlexNetExtraInfoType.TodaysNetOutputAmpHours:
                    extraValueType = OutbackMateFlexNetExtraValueType.TodaysNetOutputAmpHours;
                    extraValue = signedRawValue;
                    break;
                case FlexNetExtraInfoType.TodaysNetInputWattHours:
                    extraValueType = OutbackMateFlexNetExtraValueType.TodaysNetInputWattHours;
                    extraValue = signedRawValue * 10;
                    break;
                case FlexNetExtraInfoType.TodaysNetOutputWattHours:
                    extraValueType = OutbackMateFlexNetExtraValueType.TodaysNetOutputWattHours;
                    extraValue = signedRawValue * 10;
                    break;
                case FlexNetExtraInfoType.ChargeFactorCorrectedNetAmpHours:
                    extraValueType = OutbackMateFlexNetExtraValueType.ChargeFactorCorrectedNetAmpHours;
                    extraValue = signedRawValue;
                    break;
                case FlexNetExtraInfoType.ChargeFactorCorrectedNetWattHours:
                    extraValueType = OutbackMateFlexNetExtraValueType.ChargeFactorCorrectedNetWattHours;
                    extraValue = signedRawValue * 10;
                    break;
            }
        }

        #endregion
    }
}
