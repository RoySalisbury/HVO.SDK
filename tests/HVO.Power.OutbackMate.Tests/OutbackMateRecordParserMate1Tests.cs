namespace HVO.Power.OutbackMate.Tests;

/// <summary>
/// Tests for <see cref="OutbackMateRecordParser"/> using the Mate1 protocol.
/// </summary>
[TestClass]
public class OutbackMateRecordParserMate1Tests
{
    #region Charge Controller (Mate1)

    [TestMethod]
    public void ParseMate1_ChargeController_PortA_ParsesCorrectly()
    {
        // Mate1 CC: port_char, unused, charge_amps_whole, pv_amps, pv_volts,
        //           daily_kwh, charge_tenths, aux_mode, error_modes, charge_mode,
        //           batt_volts*10, daily_ah, unused, crc
        string raw = "A,00,15,12,180,25,5,0,0,2,560,10,00,12345";
        var dt = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);

        var result = OutbackMateRecordParser.ParseMate1(dt, raw);

        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(OutbackMateChargeControllerRecord));

        var cc = (OutbackMateChargeControllerRecord)result;
        Assert.AreEqual(OutbackMateRecordType.ChargeController, cc.RecordType);
        Assert.AreEqual(dt, cc.RecordDateTime);
        Assert.AreEqual((byte)0, cc.HubPort); // 'A' - 'A' = 0
        Assert.AreEqual((short)12, cc.PVAmps);
        Assert.AreEqual((short)180, cc.PVVoltage);
        Assert.AreEqual(15.5m, cc.ChargerAmps); // 15 + 5/10
        Assert.AreEqual(56.0m, cc.ChargerVoltage); // 560/10
        Assert.AreEqual((short)10, cc.DailyAmpHoursProduced);
        Assert.AreEqual(2500, cc.DailyWattHoursProduced); // 25 * 100
        Assert.AreEqual(OutbackMateChargeControllerMode.Bulk, cc.Mode);
        Assert.AreEqual(OutbackMateChargeControllerAuxRelayMode.Disabled, cc.AuxRelayMode);
        Assert.AreEqual(OutbackMateChargeControllerErrorMode.None, cc.ErrorMode);
        Assert.AreEqual(raw, cc.RawData);
    }

    [TestMethod]
    public void ParseMate1_ChargeController_PortK_ParsesHubPort()
    {
        string raw = "K,00,20,18,200,30,3,2,128,4,480,15,00,99999";
        var dt = DateTimeOffset.UtcNow;

        var result = OutbackMateRecordParser.ParseMate1(dt, raw);

        Assert.IsNotNull(result);
        var cc = (OutbackMateChargeControllerRecord)result;
        Assert.AreEqual((byte)10, cc.HubPort); // 'K' - 'A' = 10
        Assert.AreEqual(OutbackMateChargeControllerMode.Equalizing, cc.Mode);
        Assert.AreEqual(OutbackMateChargeControllerAuxRelayMode.Remote, cc.AuxRelayMode);
        Assert.AreEqual(OutbackMateChargeControllerErrorMode.HighVOC, cc.ErrorMode);
    }

    [TestMethod]
    public void ParseMate1_ChargeController_ChargerAmpsIncludesTenths()
    {
        // chargerAmps = parts[2] + parts[6]/10 = 22 + 7/10 = 22.7
        string raw = "B,00,22,10,150,20,7,0,0,1,555,8,00,11111";
        var dt = DateTimeOffset.UtcNow;

        var result = OutbackMateRecordParser.ParseMate1(dt, raw);
        var cc = (OutbackMateChargeControllerRecord)result!;

        Assert.AreEqual(22.7m, cc.ChargerAmps);
    }

    #endregion

    #region FlexNet DC (Mate1)

    [TestMethod]
    public void ParseMate1_FlexNet_ParsesCorrectly()
    {
        // Mate1 FlexNet: port_char, shuntA*10, shuntB*10, shuntC*10,
        //                extra_info_type, extra_value, batt_volts*10, soc,
        //                shunt_enabled, status_flags, batt_temp, crc
        // shunt_enabled "000" = all enabled
        // status_flags: 0 = no flags
        // batt_temp: 30 → 30-10 = 20°C
        string raw = "a,150,50,0,0,500,480,85,000,0,30,99999";
        var dt = DateTimeOffset.UtcNow;

        var result = OutbackMateRecordParser.ParseMate1(dt, raw);

        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(OutbackMateFlexNetRecord));

        var fn = (OutbackMateFlexNetRecord)result;
        Assert.AreEqual(OutbackMateRecordType.FlexNetDC, fn.RecordType);
        Assert.AreEqual((byte)0, fn.HubPort); // 'a' - 'a' = 0
        Assert.AreEqual(48.0m, fn.BatteryVoltage); // 480/10
        Assert.AreEqual((byte)85, fn.BatteryStateOfCharge);
        Assert.AreEqual((short)20, fn.BatteryTemperatureC); // 30 - 10
        Assert.IsTrue(fn.ShuntAEnabled);
        Assert.IsTrue(fn.ShuntBEnabled);
        Assert.IsTrue(fn.ShuntCEnabled);
        Assert.AreEqual(15.0m, fn.ShuntAAmps); // 150/10, no negative flag
        Assert.AreEqual(5.0m, fn.ShuntBAmps); // 50/10
        Assert.AreEqual(0.0m, fn.ShuntCAmps);
        Assert.AreEqual(OutbackMateFlexNetRelayState.Closed, fn.RelayState);
        Assert.AreEqual(OutbackMateFlexNetRelayMode.Manual, fn.RelayMode);
        Assert.IsFalse(fn.ChargeParamsMet);
        Assert.AreEqual(OutbackMateFlexNetExtraValueType.AccumulatedShuntAAmpHours, fn.ExtraValueType);
        Assert.AreEqual(500m, fn.ExtraValue);
    }

    [TestMethod]
    public void ParseMate1_FlexNet_BatteryTempNull_WhenRaw99()
    {
        // batt_temp = 99 → null
        string raw = "b,100,0,0,0,100,500,90,000,0,99,99999";
        var dt = DateTimeOffset.UtcNow;

        var fn = (OutbackMateFlexNetRecord)OutbackMateRecordParser.ParseMate1(dt, raw)!;
        Assert.IsNull(fn.BatteryTemperatureC);
    }

    [TestMethod]
    public void ParseMate1_FlexNet_ShuntNegativeFlags()
    {
        // status_flags: 8 = ShuntANegative, 16 = ShuntBNegative → 8+16 = 24
        string raw = "a,200,300,0,0,100,480,50,000,24,25,99999";
        var dt = DateTimeOffset.UtcNow;

        var fn = (OutbackMateFlexNetRecord)OutbackMateRecordParser.ParseMate1(dt, raw)!;
        Assert.AreEqual(-20.0m, fn.ShuntAAmps); // 200/10 * -1
        Assert.AreEqual(-30.0m, fn.ShuntBAmps); // 300/10 * -1
        Assert.AreEqual(0.0m, fn.ShuntCAmps); // no negative flag
    }

    [TestMethod]
    public void ParseMate1_FlexNet_RelayOpenAutomatic_ChargeParamsMet()
    {
        // status_flags: 1 (ChargeParamsMet) + 2 (RelayOpen) + 4 (RelayAutomatic) = 7
        string raw = "a,0,0,0,0,0,480,100,000,7,25,99999";
        var dt = DateTimeOffset.UtcNow;

        var fn = (OutbackMateFlexNetRecord)OutbackMateRecordParser.ParseMate1(dt, raw)!;
        Assert.IsTrue(fn.ChargeParamsMet);
        Assert.AreEqual(OutbackMateFlexNetRelayState.Open, fn.RelayState);
        Assert.AreEqual(OutbackMateFlexNetRelayMode.Automatic, fn.RelayMode);
    }

    [TestMethod]
    public void ParseMate1_FlexNet_ShuntDisabled()
    {
        // shunt_enabled "111" = all disabled (0=enabled, 1=disabled)
        string raw = "a,0,0,0,0,0,480,50,111,0,25,99999";
        var dt = DateTimeOffset.UtcNow;

        var fn = (OutbackMateFlexNetRecord)OutbackMateRecordParser.ParseMate1(dt, raw)!;
        Assert.IsFalse(fn.ShuntAEnabled);
        Assert.IsFalse(fn.ShuntBEnabled);
        Assert.IsFalse(fn.ShuntCEnabled);
    }

    [TestMethod]
    [DataRow(0, OutbackMateFlexNetExtraValueType.AccumulatedShuntAAmpHours, 100, 100)]
    [DataRow(1, OutbackMateFlexNetExtraValueType.AccumulatedShuntAWattHours, 50, 500)]
    [DataRow(6, OutbackMateFlexNetExtraValueType.DaysSinceFull, 35, 3.5)]
    [DataRow(7, OutbackMateFlexNetExtraValueType.TodaysMinimumSOC, 75, 75)]
    [DataRow(10, OutbackMateFlexNetExtraValueType.TodaysNetInputWattHours, 200, 2000)]
    public void ParseMate1_FlexNet_ExtraValueTypes(
        int extraInfoType, OutbackMateFlexNetExtraValueType expectedType,
        int rawValue, double expectedValue)
    {
        string raw = $"a,0,0,0,{extraInfoType},{rawValue},480,50,000,0,25,99999";
        var dt = DateTimeOffset.UtcNow;

        var fn = (OutbackMateFlexNetRecord)OutbackMateRecordParser.ParseMate1(dt, raw)!;
        Assert.AreEqual(expectedType, fn.ExtraValueType);
        Assert.AreEqual((decimal)expectedValue, fn.ExtraValue);
    }

    [TestMethod]
    public void ParseMate1_FlexNet_NegativeExtraValue()
    {
        // extra_info_type with bit 0x40 set = negative
        // 0x40 | 0 = 64 → AccumulatedShuntAAmpHours, negative
        string raw = "a,0,0,0,64,250,480,50,000,0,25,99999";
        var dt = DateTimeOffset.UtcNow;

        var fn = (OutbackMateFlexNetRecord)OutbackMateRecordParser.ParseMate1(dt, raw)!;
        Assert.AreEqual(OutbackMateFlexNetExtraValueType.AccumulatedShuntAAmpHours, fn.ExtraValueType);
        Assert.AreEqual(-250m, fn.ExtraValue);
    }

    #endregion

    #region Inverter/Charger (Mate1)

    [TestMethod]
    public void ParseMate1_InverterCharger_ParsesCorrectly()
    {
        // Mate1 Inverter/Charger: port_char, inverter_amps, charger_amps, buy_amps,
        //                          ac_input_volts, ac_output_volts, sell_amps,
        //                          fx_op_mode, error_mode, ac_input_mode,
        //                          batt_volts*10, misc, warning_mode
        string raw = "0,25,10,5,120,121,3,2,0,2,480,0,0";
        var dt = DateTimeOffset.UtcNow;

        var result = OutbackMateRecordParser.ParseMate1(dt, raw);

        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(OutbackMateInverterChargerRecord));

        var ic = (OutbackMateInverterChargerRecord)result;
        Assert.AreEqual(OutbackMateRecordType.InverterCharger, ic.RecordType);
        Assert.AreEqual((byte)0, ic.HubPort); // '0' - '0' = 0
        Assert.AreEqual((byte)25, ic.InverterCurrent);
        Assert.AreEqual((byte)10, ic.ChargerCurrent);
        Assert.AreEqual((byte)5, ic.BuyCurrent);
        Assert.AreEqual((ushort)120, ic.ACInputVoltage);
        Assert.AreEqual((ushort)121, ic.ACOutputVoltage);
        Assert.AreEqual((byte)3, ic.SellCurrent);
        Assert.AreEqual(OutbackFxInverterChargerOperationalMode.InverterOn, ic.OperationalMode);
        Assert.AreEqual(OutbackFxInverterChargerErrorMode.None, ic.ErrorMode);
        Assert.AreEqual(OutbackFxInverterChargerACInputMode.Use, ic.ACInputMode);
        Assert.AreEqual(48.0m, ic.BatteryVoltage);
        Assert.AreEqual(OutbackFxInverterChargerMisc.None, ic.Misc);
        Assert.AreEqual(OutbackFxInverterChargerWarningMode.None, ic.WarningMode);
    }

    [TestMethod]
    public void ParseMate1_InverterCharger_SemicolonPort()
    {
        // ';' is ASCII 59, '0' is ASCII 48, so hubPort = 59-48 = 11
        string raw = ";,0,0,0,120,120,0,0,0,0,480,0,0";
        var dt = DateTimeOffset.UtcNow;

        var ic = (OutbackMateInverterChargerRecord)OutbackMateRecordParser.ParseMate1(dt, raw)!;
        Assert.AreEqual((byte)11, ic.HubPort);
    }

    [TestMethod]
    public void ParseMate1_InverterCharger_ErrorAndWarningFlags()
    {
        // error_mode: 4 (OverTemperature) + 8 (LowBattery) = 12
        // warning_mode: 32 (TemperatureSensorFailed) + 128 (FanFailure) = 160
        // misc: 129 (AC240 + AuxOutputOn)
        string raw = "1,0,0,0,120,120,0,3,12,2,480,129,160";
        var dt = DateTimeOffset.UtcNow;

        var ic = (OutbackMateInverterChargerRecord)OutbackMateRecordParser.ParseMate1(dt, raw)!;
        Assert.AreEqual(OutbackFxInverterChargerErrorMode.OverTemperature | OutbackFxInverterChargerErrorMode.LowBattery, ic.ErrorMode);
        Assert.AreEqual(OutbackFxInverterChargerWarningMode.TemperatureSensorFailed | OutbackFxInverterChargerWarningMode.FanFailure, ic.WarningMode);
        Assert.AreEqual(OutbackFxInverterChargerMisc.AC240 | OutbackFxInverterChargerMisc.AuxOutputOn, ic.Misc);
    }

    #endregion

    #region Edge Cases

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ParseMate1_NullInput_ThrowsArgumentNullException()
    {
        OutbackMateRecordParser.ParseMate1(DateTimeOffset.UtcNow, null!);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ParseMate1_EmptyInput_ThrowsArgumentNullException()
    {
        OutbackMateRecordParser.ParseMate1(DateTimeOffset.UtcNow, "");
    }

    [TestMethod]
    public void ParseMate1_UnrecognizedPrefix_ReturnsNull()
    {
        var result = OutbackMateRecordParser.ParseMate1(DateTimeOffset.UtcNow, "Z,1,2,3,4,5");
        Assert.IsNull(result);
    }

    [TestMethod]
    public void ParseMate1_TooFewFields_ReturnsNull()
    {
        // CC needs 14 fields, only providing 5
        var result = OutbackMateRecordParser.ParseMate1(DateTimeOffset.UtcNow, "A,00,15,12,180");
        Assert.IsNull(result);
    }

    #endregion
}
