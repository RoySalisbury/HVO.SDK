namespace HVO.Power.OutbackMate.Tests;

/// <summary>
/// Tests for event arguments and record data types.
/// </summary>
[TestClass]
public class EventArgsAndRecordTests
{
    #region OutbackMateCommunicationsErrorEventArgs Tests

    [TestMethod]
    public void CommunicationsErrorEventArgs_StoresException()
    {
        var ex = new InvalidOperationException("Serial port failure");
        var args = new OutbackMateCommunicationsErrorEventArgs(ex);

        Assert.AreSame(ex, args.Exception);
    }

    [TestMethod]
    public void CommunicationsErrorEventArgs_NullException_StoresNull()
    {
        var args = new OutbackMateCommunicationsErrorEventArgs(null!);
        Assert.IsNull(args.Exception);
    }

    #endregion

    #region OutbackMateRecordReceivedEventArgs Tests

    [TestMethod]
    public void RecordReceivedEventArgs_StoresDateTimeAndRecord()
    {
        var now = DateTimeOffset.UtcNow;
        string raw = "A,00,15,12,180,25,5,0,0,2,560,10,00,12345";

        var args = new OutbackMateRecordReceivedEventArgs(now, raw);

        Assert.AreEqual(now, args.RecordDateTime);
        Assert.AreEqual(raw, args.DataRecord);
    }

    #endregion

    #region IOutbackMateRecord Implementation Tests

    [TestMethod]
    public void ChargeControllerRecord_ImplementsInterface()
    {
        string raw = "A,00,15,12,180,25,5,0,0,2,560,10,00,12345";
        var now = DateTimeOffset.UtcNow;
        var record = OutbackMateRecordParser.ParseMate1(now, raw);

        Assert.IsNotNull(record);
        Assert.IsInstanceOfType(record, typeof(IOutbackMateRecord));
        Assert.AreEqual(OutbackMateRecordType.ChargeController, record!.RecordType);
        Assert.AreEqual(now, record.RecordDateTime);
        Assert.AreEqual(raw, record.RawData);
    }

    [TestMethod]
    public void FlexNetRecord_ImplementsInterface()
    {
        string raw = "a,150,50,0,0,500,480,85,000,0,30,99999";
        var now = DateTimeOffset.UtcNow;
        var record = OutbackMateRecordParser.ParseMate1(now, raw);

        Assert.IsNotNull(record);
        Assert.IsInstanceOfType(record, typeof(IOutbackMateRecord));
        Assert.AreEqual(OutbackMateRecordType.FlexNetDC, record!.RecordType);
    }

    [TestMethod]
    public void InverterChargerRecord_ImplementsInterface()
    {
        string raw = "0,25,10,5,120,121,3,2,0,2,480,0,0";
        var now = DateTimeOffset.UtcNow;
        var record = OutbackMateRecordParser.ParseMate1(now, raw);

        Assert.IsNotNull(record);
        Assert.IsInstanceOfType(record, typeof(IOutbackMateRecord));
        Assert.AreEqual(OutbackMateRecordType.InverterCharger, record!.RecordType);
    }

    #endregion

    #region OutbackMateProtocolVersion Enum Tests

    [TestMethod]
    public void ProtocolVersion_HasExpectedValues()
    {
        Assert.AreEqual(0, (int)OutbackMateProtocolVersion.Mate1);
        Assert.AreEqual(1, (int)OutbackMateProtocolVersion.Mate2);
    }

    #endregion

    #region OutbackFxInverterChargerOperationalMode Enum Tests

    [TestMethod]
    public void OperationalMode_SellEnabled_IsNotDuplicateOfSupport()
    {
        // Verify the fix: SellEnabled was =8 (duplicate of Support=8), now =9
        Assert.AreNotEqual(
            (int)OutbackFxInverterChargerOperationalMode.Support,
            (int)OutbackFxInverterChargerOperationalMode.SellEnabled);
    }

    [TestMethod]
    public void OperationalMode_HasExpectedValues()
    {
        Assert.AreEqual(0, (int)OutbackFxInverterChargerOperationalMode.InverterOff);
        Assert.AreEqual(1, (int)OutbackFxInverterChargerOperationalMode.Search);
        Assert.AreEqual(2, (int)OutbackFxInverterChargerOperationalMode.InverterOn);
        Assert.AreEqual(3, (int)OutbackFxInverterChargerOperationalMode.Charge);
        Assert.AreEqual(4, (int)OutbackFxInverterChargerOperationalMode.Silent);
        Assert.AreEqual(5, (int)OutbackFxInverterChargerOperationalMode.Float);
        Assert.AreEqual(6, (int)OutbackFxInverterChargerOperationalMode.Equalize);
        Assert.AreEqual(7, (int)OutbackFxInverterChargerOperationalMode.ChargerOff);
        Assert.AreEqual(8, (int)OutbackFxInverterChargerOperationalMode.Support);
        Assert.AreEqual(9, (int)OutbackFxInverterChargerOperationalMode.SellEnabled);
        Assert.AreEqual(10, (int)OutbackFxInverterChargerOperationalMode.PassThru);
    }

    #endregion
}
