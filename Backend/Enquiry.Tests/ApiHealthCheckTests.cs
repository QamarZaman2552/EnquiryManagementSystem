using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;

namespace Enquiry.Tests;

public class ApiHealthCheckTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ApiHealthCheckTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsOk()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsHealthy()
    {
        var client = _factory.CreateClient();
        var result = await client.GetFromJsonAsync<HealthResponse>("/health");
        Assert.NotNull(result);
    }

    [Fact]
    public async Task ServiceEndpoint_ReturnsUnauthorized_WithoutToken()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/service");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task EnquiryEndpoint_ReturnsUnauthorized_WithoutToken()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/Enquiry");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}

public class HealthResponse
{
    public string? Status { get; set; }
}
