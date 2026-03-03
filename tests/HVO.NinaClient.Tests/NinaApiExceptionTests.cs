using HVO.NinaClient.Exceptions;
using System.Net;

namespace HVO.NinaClient.Tests;

[TestClass]
public class NinaApiExceptionTests
{
    [TestMethod]
    public void Constructor_WithMessage_SetsMessage()
    {
        var ex = new NinaApiException("Test error");

        Assert.AreEqual("Test error", ex.Message);
        Assert.IsNull(ex.StatusCode);
        Assert.IsNull(ex.ApiError);
        Assert.IsNull(ex.Endpoint);
    }

    [TestMethod]
    public void Constructor_WithInnerException_SetsInnerException()
    {
        var inner = new InvalidOperationException("inner");
        var ex = new NinaApiException("outer", inner);

        Assert.AreEqual("outer", ex.Message);
        Assert.AreSame(inner, ex.InnerException);
    }

    [TestMethod]
    public void Constructor_WithStatusCode_SetsAllProperties()
    {
        var ex = new NinaApiException(
            "Not Found",
            HttpStatusCode.NotFound,
            apiError: "Camera not connected",
            endpoint: "/v2/api/camera/info");

        Assert.AreEqual(HttpStatusCode.NotFound, ex.StatusCode);
        Assert.AreEqual("Camera not connected", ex.ApiError);
        Assert.AreEqual("/v2/api/camera/info", ex.Endpoint);
    }

    [TestMethod]
    public void Constructor_WithStatusCodeAndInnerException_SetsAll()
    {
        var inner = new HttpRequestException("connection failed");
        var ex = new NinaApiException(
            "Request failed",
            HttpStatusCode.ServiceUnavailable,
            inner,
            apiError: "NINA not responding",
            endpoint: "/v2/api/version");

        Assert.AreEqual(HttpStatusCode.ServiceUnavailable, ex.StatusCode);
        Assert.AreSame(inner, ex.InnerException);
        Assert.AreEqual("NINA not responding", ex.ApiError);
        Assert.AreEqual("/v2/api/version", ex.Endpoint);
    }
}
