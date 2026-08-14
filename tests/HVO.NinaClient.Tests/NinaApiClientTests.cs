using System.Net;
using HVO.NinaClient.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace HVO.NinaClient.Tests;

[TestClass]
public class NinaApiClientTests
{
    [TestMethod]
    public async Task GetGuiderInfoAsync_DeserializesCompleteEnvelope()
    {
        const string json = """
            {
              "Response": { "State": "Guiding", "PixelScale": 1.5 },
              "Error": null,
              "StatusCode": 200,
              "Success": true,
              "Type": "GuiderInfo"
            }
            """;
        using var client = CreateClient((_, _) => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json)
        });

        var result = await client.GetGuiderInfoAsync();

        Assert.IsTrue(result.IsSuccessful);
        Assert.IsNotNull(result.Value.Response);
        Assert.AreEqual("Guiding", result.Value.Response.State);
        Assert.AreEqual(1.5, result.Value.Response.PixelScale);
        Assert.IsTrue(result.Value.Success);
    }

    [TestMethod]
    public async Task OpenCircuit_DoesNotSendAnotherRequest()
    {
        var requestCount = 0;
        using var client = CreateClient((_, _) =>
        {
            Interlocked.Increment(ref requestCount);
            return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
        }, enableCircuitBreaker: true);

        var first = await client.GetVersionAsync();
        var second = await client.GetVersionAsync();

        Assert.IsFalse(first.IsSuccessful);
        Assert.IsFalse(second.IsSuccessful);
        Assert.AreEqual(1, requestCount);
        Assert.AreEqual("Open", client.GetDiagnostics().CircuitBreakerState);
    }

    [TestMethod]
    public async Task GetVersionAsync_CallerCancellationIsPropagated()
    {
        using var client = CreateClient((_, cancellationToken) =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            return new HttpResponseMessage(HttpStatusCode.OK);
        });
        using var cancellationSource = new CancellationTokenSource();
        cancellationSource.Cancel();

        await Assert.ThrowsExactlyAsync<TaskCanceledException>(
            () => client.GetVersionAsync(cancellationSource.Token));
    }

    private static NinaApiClient CreateClient(
        Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> responseFactory,
        bool enableCircuitBreaker = false)
    {
        var handler = new StubHttpMessageHandler(responseFactory);
        var options = Options.Create(new NinaApiClientOptions
        {
            BaseUrl = "http://localhost:1888",
            TimeoutSeconds = 30,
            MaxRetryAttempts = 1,
            RetryDelayMs = 100,
            EnableCircuitBreaker = enableCircuitBreaker,
            CircuitBreakerFailureThreshold = 1,
            CircuitBreakerTimeoutSeconds = 30
        });

        return new NinaApiClient(new HttpClient(handler), NullLogger<NinaApiClient>.Instance, options);
    }

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> responseFactory) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            Task.FromResult(responseFactory(request, cancellationToken));
    }
}
