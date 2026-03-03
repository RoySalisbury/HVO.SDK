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
}
