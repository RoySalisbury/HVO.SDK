using System;
using HVO.Weather;

namespace HVO.Weather.Tests;

[TestClass]
public class DirectionTests
{
    [TestMethod]
    public void Constructor_ValidDegrees_SetsCorrectly()
    {
        var dir = new Direction(180);

        Assert.AreEqual(180, dir.Degree);
        Assert.AreEqual(CompassPoint.S, dir.CardinalPoint);
    }

    [TestMethod]
    public void Constructor_360Degrees_NormalizesToZero()
    {
        var dir = new Direction(360);

        Assert.AreEqual(0, dir.Degree);
        Assert.AreEqual(CompassPoint.N, dir.CardinalPoint);
    }

    [TestMethod]
    public void Constructor_NegativeDegrees_ThrowsException()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new Direction(-1));
    }

    [TestMethod]
    public void Constructor_Over360Degrees_ThrowsException()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new Direction(361));
    }

    [DataTestMethod]
    [DataRow(0, CompassPoint.N)]
    [DataRow(45, CompassPoint.NE)]
    [DataRow(90, CompassPoint.E)]
    [DataRow(135, CompassPoint.SE)]
    [DataRow(180, CompassPoint.S)]
    [DataRow(225, CompassPoint.SW)]
    [DataRow(270, CompassPoint.W)]
    [DataRow(315, CompassPoint.NW)]
    public void CardinalPoint_ExactCardinalDirections_MapsCorrectly(int degrees, CompassPoint expected)
    {
        var dir = new Direction((short)degrees);
        Assert.AreEqual(expected, dir.CardinalPoint);
    }

    [DataTestMethod]
    [DataRow(22, CompassPoint.NNE)]
    [DataRow(68, CompassPoint.ENE)]
    [DataRow(112, CompassPoint.ESE)]
    [DataRow(158, CompassPoint.SSE)]
    [DataRow(202, CompassPoint.SSW)]
    [DataRow(248, CompassPoint.WSW)]
    [DataRow(292, CompassPoint.WNW)]
    [DataRow(338, CompassPoint.NNW)]
    public void CardinalPoint_IntercardinalDirections_MapsCorrectly(int degrees, CompassPoint expected)
    {
        var dir = new Direction((short)degrees);
        Assert.AreEqual(expected, dir.CardinalPoint);
    }

    [TestMethod]
    public void CardinalPoint_359Degrees_MapsToNorth()
    {
        var dir = new Direction(359);
        Assert.AreEqual(CompassPoint.NNW, dir.CardinalPoint);
    }
}
