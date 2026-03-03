namespace HVO.Power.OutbackMate.Tests;

/// <summary>
/// Tests for <see cref="OutbackMateRecordParser"/> using the Mate2 protocol.
/// </summary>
[TestClass]
public class OutbackMateRecordParserMate2Tests
{
    #region Charge Controller (Mate2)

    [TestMethod]
    public void ParseMate2_ChargeController_ParsesCorrectly()
    {
        // Mate2 CC field layout:
        // address, device_id(3), unused, charge_current, pv_current, pv_voltage,
        // daily_kwh, charge_tenths, aux_mode, error_modes, charge_mode,
        // battery_volts*10, daily_ah, unused
        string raw = "1,3,00,15,12,180,25,5,0,0,2,560,10,00";
        var dt = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);

        var result = OutbackMateRecordParser.ParseMate2(dt, raw);

        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(OutbackMateChargeControllerRecord));

        var cc = (OutbackMateChargeControllerRecord)result;
        Assert.AreEqual(OutbackMateRecordType.ChargeController, cc.RecordType);
        Assert.AreEqual(dt, cc.RecordDateTime);
        Assert.AreEqual((byte)1, cc.HubPort);
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
    public void ParseMate2_ChargeController_DifferentPort()
    {
        string raw = "5,3,00,20,18,200,30,3,2,128,4,480,15,00";
        var dt = DateTimeOffset.UtcNow;

        var cc = (OutbackMateChargeControllerRecord)OutbackMateRecordParser.ParseMate2(dt, raw)!;
        Assert.AreEqual((byte)5, cc.HubPort);
        Assert.AreEqual(OutbackMateChargeControllerMode.Equalizing, cc.Mode);
        Assert.AreEqual(OutbackMateChargeControllerAuxRelayMode.Remote, cc.AuxRelayMode);
        Assert.AreEqual(OutbackMateChargeControllerErrorMode.HighVOC, cc.ErrorMode);
    }

    #endregion

    #region FlexNet DC (Mate2)

    [TestMethod]
    public void ParseMate2_FlexNet_ParsesCorrectly()
    {
        // Mate2 FlexNet field layout:
        // address, device_id(4), shuntA*10, shuntB*10, shuntC*10,
        // extra_info_type, extra_value, batt_volts*10, soc,
        // shunt_enabled, status_flags, batt_temp
        string raw = "2,4,150,50,0,0,500,480,85,000,0,30";
        var dt = DateTimeOffset.UtcNow;

        var result = OutbackMateRecordParser.ParseMate2(dt, raw);

        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(OutbackMateFlexNetRecord));

        var fn = (OutbackMateFlexNetRecord)result;
        Assert.AreEqual(OutbackMateRecordType.FlexNetDC, fn.RecordType);
        Assert.AreEqual((byte)2, fn.HubPort);
        Assert.AreEqual(48.0m, fn.BatteryVoltage);
        Assert.AreEqual((byte)85, fn.BatteryStateOfCharge);
        Assert.AreEqual((short)20, fn.BatteryTemperatureC); // 30 - 10
        Assert.IsTrue(fn.ShuntAEnabled);
        Assert.IsTrue(fn.ShuntBEnabled);
        Assert.IsTrue(fn.ShuntCEnabled);
        Assert.AreEqual(15.0m, fn.ShuntAAmps);
        Assert.AreEqual(5.0m, fn.ShuntBAmps);
        Assert.AreEqual(0.0m, fn.ShuntCAmps);
    }

    [TestMethod]
    public void ParseMate2_FlexNet_NullTemperature()
    {
        string raw = "0,4,0,0,0,0,0,480,50,000,0,99";
        var dt = DateTimeOffset.UtcNow;

        var fn = (OutbackMateFlexNetRecord)OutbackMateRecordParser.ParseMate2(dt, raw)!;
        Assert.IsNull(fn.BatteryTemperatureC);
    }

    [TestMethod]
    public void ParseMate2_FlexNet_NegativeShunts()
    {
        // status_flags: 8 + 32 = 40 (shuntA negative + shuntC negative)
        string raw = "0,4,200,100,300,0,0,480,50,000,40,25";
        var dt = DateTimeOffset.UtcNow;

        var fn = (OutbackMateFlexNetRecord)OutbackMateRecordParser.ParseMate2(dt, raw)!;
        Assert.AreEqual(-20.0m, fn.ShuntAAmps);
        Assert.AreEqual(10.0m, fn.ShuntBAmps); // not negative
        Assert.AreEqual(-30.0m, fn.ShuntCAmps);
    }

    #endregion

    #region Inverter/Charger (Mate2)

    [TestMethod]
    public void ParseMate2_InverterCharger_ParsesCorrectly()
    {
        // Mate2 Inverter/Charger field layout:
        // address, device_id(2), inverter_amps, charger_amps, buy_amps,
        // ac_input_volts, ac_output_volts, sell_amps,
        // fx_op_mode, error_mode, ac_input_mode,
        // batt_volts*10, misc, warning_mode
        string raw = "3,2,25,10,5,120,121,3,2,0,2,480,0,0";
        var dt = DateTimeOffset.UtcNow;

        var result = OutbackMateRecordParser.ParseMate2(dt, raw);

        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(OutbackMateInverterChargerRecord));

        var ic = (OutbackMateInverterChargerRecord)result;
        Assert.AreEqual(OutbackMateRecordType.InverterCharger, ic.RecordType);
        Assert.AreEqual((byte)3, ic.HubPort);
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
    }

    [TestMethod]
    public void ParseMate2_InverterCharger_WithErrors()
    {
        // error_mode: 12 (OverTemp + LowBattery), warning_mode: 160 (TempSensor + Fan)
        string raw = "0,2,0,0,0,120,120,0,3,12,2,480,129,160";
        var dt = DateTimeOffset.UtcNow;

        var ic = (OutbackMateInverterChargerRecord)OutbackMateRecordParser.ParseMate2(dt, raw)!;
        Assert.AreEqual(OutbackFxInverterChargerErrorMode.OverTemperature | OutbackFxInverterChargerErrorMode.LowBattery, ic.ErrorMode);
        Assert.AreEqual(OutbackFxInverterChargerWarningMode.TemperatureSensorFailed | OutbackFxInverterChargerWarningMode.FanFailure, ic.WarningMode);
    }

    #endregion

    #region Edge Cases

    [TestMethod]
    public void ParseMate2_NullInput_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            OutbackMateRecordParser.ParseMate2(DateTimeOffset.UtcNow, null!));
    }

    [TestMethod]
    public void ParseMate2_EmptyInput_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            OutbackMateRecordParser.ParseMate2(DateTimeOffset.UtcNow, ""));
    }

    [TestMethod]
    public void ParseMate2_UnknownDeviceId_ReturnsNull()
    {
        var result = OutbackMateRecordParser.ParseMate2(DateTimeOffset.UtcNow, "0,99,1,2,3,4,5,6,7,8,9,10,11,12");
        Assert.IsNull(result);
    }

    [TestMethod]
    public void ParseMate2_TooFewFields_ReturnsNull()
    {
        var result = OutbackMateRecordParser.ParseMate2(DateTimeOffset.UtcNow, "0,3,15");
        Assert.IsNull(result);
    }

    #endregion
}
