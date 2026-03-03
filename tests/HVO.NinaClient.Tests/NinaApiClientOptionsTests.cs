using HVO.NinaClient;
using System.ComponentModel.DataAnnotations;

namespace HVO.NinaClient.Tests;

[TestClass]
public class NinaApiClientOptionsTests
{
    [TestMethod]
    public void DefaultOptions_HasValidDefaults()
    {
        var options = new NinaApiClientOptions();

        Assert.AreEqual("http://localhost:1888", options.BaseUrl);
        Assert.AreEqual(300, options.TimeoutSeconds);
        Assert.AreEqual(3, options.MaxRetryAttempts);
        Assert.AreEqual(1000, options.RetryDelayMs);
        Assert.IsTrue(options.EnableCircuitBreaker);
        Assert.AreEqual(5, options.CircuitBreakerFailureThreshold);
        Assert.AreEqual(30, options.CircuitBreakerTimeoutSeconds);
        Assert.IsNull(options.ApiKey);
    }

    [TestMethod]
    public void Validate_WithValidDefaults_ReturnsSuccess()
    {
        var options = new NinaApiClientOptions();

        var result = options.Validate();

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void Validate_WithCustomHttpUrl_ReturnsSuccess()
    {
        var options = new NinaApiClientOptions
        {
            BaseUrl = "http://192.168.1.100:1888"
        };

        var result = options.Validate();

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void Validate_WithCustomHttpsUrl_ReturnsSuccess()
    {
        var options = new NinaApiClientOptions
        {
            BaseUrl = "https://nina.example.com:1888"
        };

        var result = options.Validate();

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void Validate_WithInvalidUrl_ReturnsFailure()
    {
        var options = new NinaApiClientOptions
        {
            BaseUrl = "not-a-url"
        };

        var result = options.Validate();

        Assert.AreNotEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void Validate_WithEmptyUrl_ReturnsFailure()
    {
        var options = new NinaApiClientOptions
        {
            BaseUrl = ""
        };

        var result = options.Validate();

        Assert.AreNotEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void ValidateAndThrow_WithInvalidUrl_ThrowsArgumentException()
    {
        var options = new NinaApiClientOptions
        {
            BaseUrl = "ftp://invalid-scheme"
        };

        Assert.ThrowsExactly<ArgumentException>(() => options.ValidateAndThrow());
    }

    [TestMethod]
    public void ValidateAndThrow_WithValidOptions_DoesNotThrow()
    {
        var options = new NinaApiClientOptions
        {
            BaseUrl = "http://localhost:1888",
            TimeoutSeconds = 60,
            MaxRetryAttempts = 2,
            ApiKey = "test-key"
        };

        options.ValidateAndThrow(); // Should not throw
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(3601)]
    public void Validate_WithOutOfRangeTimeout_ReturnsFailure(int timeout)
    {
        var options = new NinaApiClientOptions
        {
            TimeoutSeconds = timeout
        };

        var result = options.Validate();

        Assert.AreNotEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(21)]
    public void Validate_WithOutOfRangeCircuitBreakerThreshold_ReturnsFailure(int threshold)
    {
        var options = new NinaApiClientOptions
        {
            CircuitBreakerFailureThreshold = threshold
        };

        var result = options.Validate();

        Assert.AreNotEqual(ValidationResult.Success, result);
    }
}
