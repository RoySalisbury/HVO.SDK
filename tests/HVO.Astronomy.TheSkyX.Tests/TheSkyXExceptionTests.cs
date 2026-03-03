using System;
using HVO.Astronomy.TheSkyX;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HVO.Astronomy.TheSkyX.Tests;

[TestClass]
public class TheSkyXExceptionTests
{
    [TestMethod]
    public void Constructor_WithMessage_SetsMessage()
    {
        var ex = new TheSkyXException("Test error");

        Assert.AreEqual("Test error", ex.Message);
        Assert.AreEqual(-1, ex.ErrorCode);
    }

    [TestMethod]
    public void Constructor_WithMessageAndErrorCode_SetsBoth()
    {
        var ex = new TheSkyXException("Connection failed", 212);

        Assert.AreEqual("Connection failed", ex.Message);
        Assert.AreEqual(212, ex.ErrorCode);
    }

    [TestMethod]
    public void Constructor_WithInnerException_SetsInnerException()
    {
        var inner = new InvalidOperationException("inner");
        var ex = new TheSkyXException("outer", inner);

        Assert.AreEqual("outer", ex.Message);
        Assert.AreSame(inner, ex.InnerException);
    }

    [TestMethod]
    public void ErrorCode_DefaultsToNegativeOne()
    {
        var ex = new TheSkyXException("test");
        Assert.AreEqual(-1, ex.ErrorCode);
    }

    [TestMethod]
    public void IsException_DerivesFromException()
    {
        var ex = new TheSkyXException("test");
        Assert.IsInstanceOfType(ex, typeof(Exception));
    }
}
