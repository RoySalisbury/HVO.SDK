using System;
using System.Text.Json;
using HVO.Core.OneOf;

namespace HVO.Core.SourceGenerators.Tests;

// ────────────────────────────────────────────────────────────────
//  Test partial types that the source generator will augment
// ────────────────────────────────────────────────────────────────

[NamedOneOf("Text", typeof(string), "Number", typeof(int))]
public partial class StringOrInt;

[NamedOneOf("Success", typeof(string), "ErrorCode", typeof(int), "Exception", typeof(Exception))]
public partial class TripleUnion;

// ────────────────────────────────────────────────────────────────
//  Tests
// ────────────────────────────────────────────────────────────────

[TestClass]
public class NamedOneOfGeneratorTests
{
    // ── Two-case: Is/As properties ──────────────────────────────

    [TestMethod]
    public void TwoCase_StringValue_IsTextTrue()
    {
        var soi = new StringOrInt("hello");

        Assert.IsTrue(soi.IsText);
        Assert.IsFalse(soi.IsNumber);
    }

    [TestMethod]
    public void TwoCase_StringValue_AsTextReturnsValue()
    {
        var soi = new StringOrInt("hello");

        Assert.AreEqual("hello", soi.AsText);
    }

    [TestMethod]
    public void TwoCase_IntValue_IsNumberTrue()
    {
        var soi = new StringOrInt(42);

        Assert.IsFalse(soi.IsText);
        Assert.IsTrue(soi.IsNumber);
    }

    [TestMethod]
    public void TwoCase_IntValue_AsNumberReturnsValue()
    {
        var soi = new StringOrInt(42);

        Assert.AreEqual(42, soi.AsNumber);
    }

    [TestMethod]
    public void TwoCase_AsWrongCase_ThrowsInvalidOperation()
    {
        var soi = new StringOrInt("hello");

        Assert.ThrowsExactly<InvalidOperationException>(() => soi.AsNumber);
    }

    // ── Implicit operators ──────────────────────────────────────

    [TestMethod]
    public void TwoCase_ImplicitFromString_Works()
    {
        StringOrInt soi = "implicit";

        Assert.IsTrue(soi.IsText);
        Assert.AreEqual("implicit", soi.AsText);
    }

    [TestMethod]
    public void TwoCase_ImplicitFromInt_Works()
    {
        StringOrInt soi = 99;

        Assert.IsTrue(soi.IsNumber);
        Assert.AreEqual(99, soi.AsNumber);
    }

    // ── IOneOf interface ────────────────────────────────────────

    [TestMethod]
    public void TwoCase_IOneOf_ValueReturnsUnderlyingValue()
    {
        IOneOf oneOf = new StringOrInt("test");

        Assert.AreEqual("test", oneOf.Value);
    }

    [TestMethod]
    public void TwoCase_IOneOf_ValueTypeReturnsCorrectType()
    {
        IOneOf oneOf = new StringOrInt(42);

        Assert.AreEqual(typeof(int), oneOf.ValueType);
    }

    // ── ToString ────────────────────────────────────────────────

    [TestMethod]
    public void TwoCase_ToString_ReturnsValueString()
    {
        var soi = new StringOrInt("hello");

        Assert.AreEqual("hello", soi.ToString());
    }

    [TestMethod]
    public void TwoCase_ToString_IntReturnsIntString()
    {
        var soi = new StringOrInt(42);

        Assert.AreEqual("42", soi.ToString());
    }

    // ── Default constructor ─────────────────────────────────────

    [TestMethod]
    public void TwoCase_DefaultConstructor_NeitherCaseIsTrue()
    {
        var soi = new StringOrInt();

        Assert.IsFalse(soi.IsText);
        Assert.IsFalse(soi.IsNumber);
    }

    [TestMethod]
    public void TwoCase_DefaultConstructor_ValueIsNull()
    {
        IOneOf oneOf = new StringOrInt();

        Assert.IsNull(oneOf.Value);
    }

    // ── Three-case: TripleUnion ─────────────────────────────────

    [TestMethod]
    public void ThreeCase_SuccessCase_Works()
    {
        var union = new TripleUnion("ok");

        Assert.IsTrue(union.IsSuccess);
        Assert.IsFalse(union.IsErrorCode);
        Assert.IsFalse(union.IsException);
        Assert.AreEqual("ok", union.AsSuccess);
    }

    [TestMethod]
    public void ThreeCase_ErrorCodeCase_Works()
    {
        var union = new TripleUnion(404);

        Assert.IsFalse(union.IsSuccess);
        Assert.IsTrue(union.IsErrorCode);
        Assert.IsFalse(union.IsException);
        Assert.AreEqual(404, union.AsErrorCode);
    }

    [TestMethod]
    public void ThreeCase_ExceptionCase_Works()
    {
        var ex = new InvalidOperationException("test error");
        var union = new TripleUnion(ex);

        Assert.IsFalse(union.IsSuccess);
        Assert.IsFalse(union.IsErrorCode);
        Assert.IsTrue(union.IsException);
        Assert.AreSame(ex, union.AsException);
    }

    // ── JSON round-trip ─────────────────────────────────────────

    [TestMethod]
    public void TwoCase_JsonRoundTrip_StringValue()
    {
        StringOrInt original = "json-test";

        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<StringOrInt>(json);

        Assert.IsNotNull(deserialized);
        Assert.IsTrue(deserialized.IsText);
        Assert.AreEqual("json-test", deserialized.AsText);
    }

    [TestMethod]
    public void TwoCase_JsonRoundTrip_IntValue()
    {
        StringOrInt original = 123;

        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<StringOrInt>(json);

        Assert.IsNotNull(deserialized);
        Assert.IsTrue(deserialized.IsNumber);
        Assert.AreEqual(123, deserialized.AsNumber);
    }

    [TestMethod]
    public void TwoCase_JsonDeserialize_UnknownShape_ReturnsFallback()
    {
        // A JSON object that doesn't match string or int directly
        var json = "{\"unexpected\": true}";

        var deserialized = JsonSerializer.Deserialize<StringOrInt>(json);

        Assert.IsNotNull(deserialized);
        // Neither case should match — falls back to default with RawJson
        Assert.IsNotNull(((IOneOf)deserialized).RawJson);
    }
}
