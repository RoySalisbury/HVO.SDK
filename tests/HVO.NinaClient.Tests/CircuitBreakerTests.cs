using HVO.NinaClient.Resilience;
using Microsoft.Extensions.Logging.Abstractions;

namespace HVO.NinaClient.Tests;

[TestClass]
public class CircuitBreakerTests
{
    [TestMethod]
    public void Constructor_DefaultState_IsClosed()
    {
        using var breaker = new CircuitBreaker(5, TimeSpan.FromSeconds(30));

        Assert.AreEqual(CircuitBreakerState.Closed, breaker.State);
    }

    [TestMethod]
    public void Constructor_WithLogger_DoesNotThrow()
    {
        using var breaker = new CircuitBreaker(5, TimeSpan.FromSeconds(30), NullLogger.Instance);

        Assert.AreEqual(CircuitBreakerState.Closed, breaker.State);
    }

    [TestMethod]
    public void Dispose_MultipleDispose_DoesNotThrow()
    {
        var breaker = new CircuitBreaker(5, TimeSpan.FromSeconds(30));

        breaker.Dispose();
        breaker.Dispose(); // Should not throw
    }

    [TestMethod]
    public void State_AfterCreation_IsClosed()
    {
        using var breaker = new CircuitBreaker(3, TimeSpan.FromSeconds(10));

        Assert.AreEqual(CircuitBreakerState.Closed, breaker.State);
    }

    [TestMethod]
    public async Task RetryPolicy_UnknownFailure_IsNotRetried()
    {
        var attemptCount = 0;

        var result = await RetryPolicy.ExecuteWithRetryAsync<int>(
            () =>
            {
                attemptCount++;
                return Task.FromResult(HVO.Core.Results.Result<int>.Failure(new Exception("permanent")));
            },
            maxAttempts: 3,
            baseDelay: TimeSpan.Zero);

        Assert.IsFalse(result.IsSuccessful);
        Assert.AreEqual(1, attemptCount);
    }

    [TestMethod]
    public async Task RetryPolicy_ZeroAttempts_StillExecutesOperationOnce()
    {
        var attemptCount = 0;

        var result = await RetryPolicy.ExecuteWithRetryAsync(
            () =>
            {
                attemptCount++;
                return Task.FromResult(HVO.Core.Results.Result<int>.Success(42));
            },
            maxAttempts: 0,
            baseDelay: TimeSpan.Zero);

        Assert.IsTrue(result.IsSuccessful);
        Assert.AreEqual(42, result.Value);
        Assert.AreEqual(1, attemptCount);
    }
}
