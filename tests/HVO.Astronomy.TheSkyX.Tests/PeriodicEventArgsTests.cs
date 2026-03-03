using System;
using HVO.Astronomy.TheSkyX;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HVO.Astronomy.TheSkyX.Tests;

[TestClass]
public class PeriodicEventArgsTests
{
    #region PeriodicCameraStatusEventArgs

    [TestMethod]
    public void CameraStatusEventArgs_Constructor_SetsRequiredProperties()
    {
        var now = DateTimeOffset.UtcNow;
        var args = new PeriodicCameraStatusEventArgs(now, true, HardwareInstanceType.Primary);

        Assert.AreEqual(now, args.EventDateTime);
        Assert.IsTrue(args.IsConnected);
        Assert.AreEqual(HardwareInstanceType.Primary, args.HardwareInstanceType);
    }

    [TestMethod]
    public void CameraStatusEventArgs_OptionalProperties_DefaultToNull()
    {
        var args = new PeriodicCameraStatusEventArgs(DateTimeOffset.UtcNow, false, HardwareInstanceType.Autoguider);

        Assert.IsNull(args.Status);
        Assert.IsNull(args.State);
        Assert.IsNull(args.BinX);
        Assert.IsNull(args.BinY);
        Assert.IsNull(args.TEC);
    }

    [TestMethod]
    public void CameraStatusEventArgs_TECProperty_IsNullByDefault()
    {
        var now = DateTimeOffset.UtcNow;
        var args = new PeriodicCameraStatusEventArgs(now, true, HardwareInstanceType.Primary);

        Assert.IsNull(args.TEC);
    }

    #endregion

    #region PeriodicMountStatusEventArgs

    [TestMethod]
    public void MountStatusEventArgs_Constructor_SetsRequiredProperties()
    {
        var now = DateTimeOffset.UtcNow;
        var args = new PeriodicMountStatusEventArgs(now, true);

        Assert.AreEqual(now, args.EventDateTime);
        Assert.IsTrue(args.IsConnected);
    }

    [TestMethod]
    public void MountStatusEventArgs_OptionalProperties_DefaultToNull()
    {
        var args = new PeriodicMountStatusEventArgs(DateTimeOffset.UtcNow, false);

        Assert.IsNull(args.IsParked);
        Assert.IsNull(args.IsTracking);
        Assert.IsNull(args.IsSlewComplete);
        Assert.IsNull(args.RightAscension);
        Assert.IsNull(args.Declination);
        Assert.IsNull(args.Altitude);
        Assert.IsNull(args.Azimuth);
    }

    [TestMethod]
    public void MountStatusEventArgs_CanSetRightAscension()
    {
        var args = new PeriodicMountStatusEventArgs(DateTimeOffset.UtcNow, true);
        var ra = new RightAscension(12, 30, 45);
        args.RightAscension = ra;

        Assert.IsNotNull(args.RightAscension);
        Assert.AreEqual(12, args.RightAscension.Value.Hours);
        Assert.AreEqual(30, args.RightAscension.Value.Minutes);
        Assert.AreEqual(45, args.RightAscension.Value.Seconds);
    }

    #endregion

    #region PeriodicObservatoryStatusEventArgs

    [TestMethod]
    public void ObservatoryStatusEventArgs_Constructor_SetsLocalSiderealTime()
    {
        var lst = TimeSpan.FromHours(18.5);
        var args = new PeriodicObservatoryStatusEventArgs(lst);

        Assert.AreEqual(lst, args.LocalSiderealTime);
    }

    #endregion

    #region PeriodicFocuserStatusEventArgs

    [TestMethod]
    public void FocuserStatusEventArgs_Constructor_SetsRequiredProperties()
    {
        var now = DateTimeOffset.UtcNow;
        var args = new PeriodicFocuserStatusEventArgs(now, true, HardwareInstanceType.Primary);

        Assert.AreEqual(now, args.EventDateTime);
        Assert.IsTrue(args.IsConnected);
        Assert.AreEqual(HardwareInstanceType.Primary, args.HardwareInstanceType);
    }

    [TestMethod]
    public void FocuserStatusEventArgs_OptionalProperties_DefaultToNull()
    {
        var args = new PeriodicFocuserStatusEventArgs(DateTimeOffset.UtcNow, false, HardwareInstanceType.Autoguider);

        Assert.IsNull(args.Position);
        Assert.IsNull(args.TemperatureC);
    }

    [TestMethod]
    public void FocuserStatusEventArgs_CanSetOptionalProperties()
    {
        var args = new PeriodicFocuserStatusEventArgs(DateTimeOffset.UtcNow, true, HardwareInstanceType.Primary);
        args.Position = 12500;
        args.TemperatureC = 22.3;

        Assert.AreEqual(12500, args.Position);
        Assert.AreEqual(22.3, args.TemperatureC);
    }

    #endregion

    #region PeriodicFilterWheelStatusEventArgs

    [TestMethod]
    public void FilterWheelStatusEventArgs_Constructor_SetsRequiredProperties()
    {
        var now = DateTimeOffset.UtcNow;
        var args = new PeriodicFilterWheelStatusEventArgs(now, true, 3, HardwareInstanceType.Primary);

        Assert.AreEqual(now, args.EventDateTime);
        Assert.IsTrue(args.IsConnected);
        Assert.AreEqual(3, args.CurrentIndex);
        Assert.AreEqual(HardwareInstanceType.Primary, args.HardwareInstanceType);
    }

    #endregion
}
