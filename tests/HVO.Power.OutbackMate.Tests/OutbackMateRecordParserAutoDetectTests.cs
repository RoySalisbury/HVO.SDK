namespace HVO.Power.OutbackMate.Tests;

/// <summary>
/// Tests for the auto-detection <see cref="OutbackMateRecordParser.Parse"/> method.
/// </summary>
[TestClass]
public class OutbackMateRecordParserAutoDetectTests
{
    [TestMethod]
    public void Parse_Mate1ChargeController_AutoDetects()
    {
        string raw = "A,00,15,12,180,25,5,0,0,2,560,10,00,12345";
        var result = OutbackMateRecordParser.Parse(DateTimeOffset.UtcNow, raw);

        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(OutbackMateChargeControllerRecord));
    }

    [TestMethod]
    public void Parse_Mate1FlexNet_AutoDetects()
    {
        string raw = "a,150,50,0,0,500,480,85,000,0,30,99999";
        var result = OutbackMateRecordParser.Parse(DateTimeOffset.UtcNow, raw);

        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(OutbackMateFlexNetRecord));
    }

    [TestMethod]
    public void Parse_Mate1InverterCharger_AutoDetects()
    {
        // '0' followed by non-device-id field: auto-detect falls through to Mate1
        string raw = "0,25,10,5,120,121,3,2,0,2,480,0,0";
        var result = OutbackMateRecordParser.Parse(DateTimeOffset.UtcNow, raw);

        Assert.IsNotNull(result);
        // Note: '0' is numeric and parts[1]="25" is not 2/3/4, so it's not Mate2.
        // Then it falls to the Mate1 inverter/charger check.
        Assert.IsInstanceOfType(result, typeof(OutbackMateInverterChargerRecord));
    }

    [TestMethod]
    public void Parse_Mate2ChargeController_AutoDetects()
    {
        string raw = "1,3,00,15,12,180,25,5,0,0,2,560,10,00";
        var result = OutbackMateRecordParser.Parse(DateTimeOffset.UtcNow, raw);

        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(OutbackMateChargeControllerRecord));
    }

    [TestMethod]
    public void Parse_Mate2FlexNet_AutoDetects()
    {
        string raw = "2,4,150,50,0,0,500,480,85,000,0,30";
        var result = OutbackMateRecordParser.Parse(DateTimeOffset.UtcNow, raw);

        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(OutbackMateFlexNetRecord));
    }

    [TestMethod]
    public void Parse_Mate2InverterCharger_AutoDetects()
    {
        string raw = "3,2,25,10,5,120,121,3,2,0,2,480,0,0";
        var result = OutbackMateRecordParser.Parse(DateTimeOffset.UtcNow, raw);

        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(OutbackMateInverterChargerRecord));
    }

    [TestMethod]
    public void Parse_NullInput_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            OutbackMateRecordParser.Parse(DateTimeOffset.UtcNow, null!));
    }

    [TestMethod]
    public void Parse_UnrecognizedData_ReturnsNull()
    {
        var result = OutbackMateRecordParser.Parse(DateTimeOffset.UtcNow, "INVALID_DATA");
        Assert.IsNull(result);
    }

    [TestMethod]
    public void Parse_SingleField_ReturnsNull()
    {
        var result = OutbackMateRecordParser.Parse(DateTimeOffset.UtcNow, "ALONE");
        Assert.IsNull(result);
    }
}
