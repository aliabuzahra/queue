using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using VirtualQueue.Infrastructure.Data;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace VirtualQueue.IntegrationTests.Controllers;

public class TenantsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public TenantsControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace database with in-memory for testing
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<VirtualQueueDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddDbContext<VirtualQueueDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                });
            });
        });
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task CreateTenant_WithValidRequest_ShouldReturnCreatedTenant()
    {
        // Arrange
        var request = new
        {
            Name = "Test Tenant",
            Domain = "test.com"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/tenants", request);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Test Tenant");
        content.Should().Contain("test.com");
    }

    [Fact]
    public async Task CreateTenant_WithInvalidRequest_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new
        {
            Name = "", // Invalid empty name
            Domain = "test.com"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/tenants", request);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetTenant_WithValidId_ShouldReturnTenant()
    {
        // Arrange
        var createRequest = new
        {
            Name = "Test Tenant",
            Domain = "test.com"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/v1/tenants", createRequest);
        var createdTenant = await createResponse.Content.ReadFromJsonAsync<dynamic>();
        var tenantId = (Guid)createdTenant!.id;

        // Act
        var response = await _client.GetAsync($"/api/v1/tenants/{tenantId}");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Test Tenant");
    }

    [Fact]
    public async Task GetTenant_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/v1/tenants/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }
}
