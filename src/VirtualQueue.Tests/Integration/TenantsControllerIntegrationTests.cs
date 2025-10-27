using Microsoft.AspNetCore.Mvc.Testing;
using VirtualQueue.Tests.Integration;
using FluentAssertions;
using System.Net;
using System.Text.Json;

namespace VirtualQueue.Tests.Integration;

/// <summary>
/// Integration tests for the Tenants API endpoints
/// </summary>
public class TenantsControllerIntegrationTests : IntegrationTestBase
{
    #region Constructor
    public TenantsControllerIntegrationTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }
    #endregion

    #region Setup
    protected override async Task SeedTestDataAsync()
    {
        await base.SeedTestDataAsync();
    }
    #endregion

    #region CreateTenant Tests
    [Fact]
    public async Task CreateTenant_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        await CleanupDatabaseAsync();
        
        var createRequest = new
        {
            Name = "New Test Tenant",
            Domain = "newtest.local"
        };

        var json = JsonSerializer.Serialize(createRequest);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PostAsync("/api/v1/tenants", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateTenant_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        await CleanupDatabaseAsync();
        
        var createRequest = new
        {
            Name = "", // Invalid empty name
            Domain = "test.local"
        };

        var json = JsonSerializer.Serialize(createRequest);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PostAsync("/api/v1/tenants", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTenant_WithDuplicateDomain_ShouldReturnConflict()
    {
        // Arrange
        await CleanupDatabaseAsync();
        await SeedTestDataAsync(); // This creates a tenant with domain "test.local"
        
        var createRequest = new
        {
            Name = "Duplicate Tenant",
            Domain = "test.local" // Duplicate domain
        };

        var json = JsonSerializer.Serialize(createRequest);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PostAsync("/api/v1/tenants", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
    #endregion

    #region GetTenant Tests
    [Fact]
    public async Task GetTenant_WithValidId_ShouldReturnOk()
    {
        // Arrange
        await CleanupDatabaseAsync();
        await SeedTestDataAsync();
        
        var tenant = Context.Tenants.First();
        var tenantId = tenant.Id;

        // Act
        var response = await Client.GetAsync($"/api/v1/tenants/{tenantId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetTenant_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        await CleanupDatabaseAsync();
        var invalidId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"/api/v1/tenants/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    #endregion

    #region GetTenants Tests
    [Fact]
    public async Task GetTenants_ShouldReturnOk()
    {
        // Arrange
        await CleanupDatabaseAsync();
        await SeedTestDataAsync();

        // Act
        var response = await Client.GetAsync("/api/v1/tenants");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().NotBeEmpty();
    }
    #endregion

    #region UpdateTenant Tests
    [Fact]
    public async Task UpdateTenant_WithValidData_ShouldReturnOk()
    {
        // Arrange
        await CleanupDatabaseAsync();
        await SeedTestDataAsync();
        
        var tenant = Context.Tenants.First();
        var tenantId = tenant.Id;
        
        var updateRequest = new
        {
            Name = "Updated Tenant Name"
        };

        var json = JsonSerializer.Serialize(updateRequest);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PutAsync($"/api/v1/tenants/{tenantId}", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateTenant_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        await CleanupDatabaseAsync();
        var invalidId = Guid.NewGuid();
        
        var updateRequest = new
        {
            Name = "Updated Tenant Name"
        };

        var json = JsonSerializer.Serialize(updateRequest);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PutAsync($"/api/v1/tenants/{invalidId}", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    #endregion

    #region DeleteTenant Tests
    [Fact]
    public async Task DeleteTenant_WithValidId_ShouldReturnNoContent()
    {
        // Arrange
        await CleanupDatabaseAsync();
        await SeedTestDataAsync();
        
        var tenant = Context.Tenants.First();
        var tenantId = tenant.Id;

        // Act
        var response = await Client.DeleteAsync($"/api/v1/tenants/{tenantId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteTenant_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        await CleanupDatabaseAsync();
        var invalidId = Guid.NewGuid();

        // Act
        var response = await Client.DeleteAsync($"/api/v1/tenants/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    #endregion
}
