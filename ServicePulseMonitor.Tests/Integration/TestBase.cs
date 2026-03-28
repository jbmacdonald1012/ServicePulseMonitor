using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using ServicePulseMonitor.Data;
using ServicePulseMonitor.Data.DTOs;
using ServicePulseMonitor.Services;

namespace ServicePulseMonitor.Tests.Integration;

/// <summary>
/// Base class for integration tests. Spins up the full ASP.NET Core pipeline against an isolated
/// in-memory database. Each fixture class gets its own database so tests do not share state
/// across fixtures. Tests within the same fixture share one <see cref="HttpClient"/> instance
/// and should use unique service names (e.g. <c>Guid.NewGuid()</c> suffix) to avoid collisions.
/// </summary>
[TestFixture]
public abstract class TestBase
{
    /// <summary>The <see cref="WebApplicationFactory{TEntryPoint}"/> that hosts the test server.</summary>
    protected WebApplicationFactory<Program> Factory { get; private set; } = null!;

    /// <summary>An <see cref="HttpClient"/> pre-configured to call the test server.</summary>
    protected HttpClient Client { get; private set; } = null!;

    [OneTimeSetUp]
    public void BaseOneTimeSetUp()
    {
        Factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // Replace Npgsql DbContext with InMemory
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ServicePulseDbContext>));
                if (descriptor is not null)
                    services.Remove(descriptor);

                services.AddDbContext<ServicePulseDbContext, InMemoryServicePulseDbContext>(opts =>
                    opts.UseInMemoryDatabase(Guid.NewGuid().ToString())
                        .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning)));

                // Remove the background health-collector so it does not make HTTP calls during tests
                var hcs = services.SingleOrDefault(
                    d => d.ImplementationType == typeof(HealthCollectorService));
                if (hcs is not null)
                    services.Remove(hcs);
            });
        });

        Client = Factory.CreateClient();
    }

    [OneTimeTearDown]
    public void BaseOneTimeTearDown()
    {
        Client.Dispose();
        Factory.Dispose();
    }

    /// <summary>
    /// Resolves the <see cref="ServicePulseDbContext"/> from the test server's DI container.
    /// Creates a new scope each call — dispose the context when done, or use it within a single operation.
    /// </summary>
    protected ServicePulseDbContext GetDbContext()
    {
        var scope = Factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<ServicePulseDbContext>();
    }

    /// <summary>Registers a new service via <c>POST /api/services</c> and returns the created DTO.</summary>
    /// <param name="name">Service name. Defaults to a unique GUID-based name if not provided.</param>
    /// <param name="baseUrl">Base URL of the service. Defaults to a unique localhost port.</param>
    protected async Task<ServiceDto> RegisterServiceAsync(string? name = null, string? baseUrl = null)
    {
        var payload = new CreateServiceDto
        {
            ServiceName = name ?? $"TestSvc-{Guid.NewGuid():N}",
            BaseUrl = baseUrl ?? $"http://localhost:{Random.Shared.Next(10000, 60000)}"
        };

        var response = await Client.PostAsJsonAsync("/api/services", payload);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ServiceDto>())!;
    }

    /// <summary>
    /// Submits a health check for a service via <c>POST /api/services/{serviceId}/health</c>
    /// and returns the created DTO.
    /// </summary>
    /// <param name="serviceId">The target service ID.</param>
    /// <param name="status">Health status: <c>Healthy</c>, <c>Degraded</c>, or <c>Unhealthy</c>.</param>
    /// <param name="responseTimeMs">Simulated response time in milliseconds.</param>
    protected async Task<HealthCheckDto> SubmitHealthCheckAsync(
        long serviceId, string status, int responseTimeMs = 100)
    {
        var payload = new CreateHealthCheckDto { Status = status, ResponseTimeMs = responseTimeMs };
        var response = await Client.PostAsJsonAsync($"/api/services/{serviceId}/health", payload);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<HealthCheckDto>())!;
    }
}
