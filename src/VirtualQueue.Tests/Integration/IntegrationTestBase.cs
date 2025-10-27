using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VirtualQueue.Api;
using VirtualQueue.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace VirtualQueue.Tests.Integration;

/// <summary>
/// Integration test base class with test web application factory
/// </summary>
public class IntegrationTestBase : IClassFixture<WebApplicationFactory<Program>>
{
    #region Fields
    protected readonly WebApplicationFactory<Program> Factory;
    protected readonly HttpClient Client;
    protected readonly VirtualQueueDbContext Context;
    #endregion

    #region Constructor
    public IntegrationTestBase(WebApplicationFactory<Program> factory)
    {
        Factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the real database
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<VirtualQueueDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add in-memory database
                services.AddDbContext<VirtualQueueDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDatabase");
                });
            });
        });

        Client = Factory.CreateClient();
        
        var scope = Factory.Services.CreateScope();
        Context = scope.ServiceProvider.GetRequiredService<VirtualQueueDbContext>();
    }
    #endregion

    #region Helper Methods
    /// <summary>
    /// Cleans up the test database
    /// </summary>
    protected async Task CleanupDatabaseAsync()
    {
        Context.Database.EnsureDeleted();
        await Context.Database.EnsureCreatedAsync();
    }

    /// <summary>
    /// Seeds test data
    /// </summary>
    protected async Task SeedTestDataAsync()
    {
        // Add test tenant
        var tenant = new VirtualQueue.Domain.Entities.Tenant("Test Tenant", "test.local");
        Context.Tenants.Add(tenant);
        await Context.SaveChangesAsync();

        // Add test user
        var user = new VirtualQueue.Domain.Entities.User(
            tenant.Id,
            "testuser",
            "test@example.com",
            "hashedpassword",
            "Test",
            "User",
            VirtualQueue.Domain.Enums.UserRole.Customer);

        Context.Users.Add(user);
        await Context.SaveChangesAsync();

        // Add test queue
        var queue = new VirtualQueue.Domain.Entities.Queue(
            tenant.Id,
            "Test Queue",
            "Test queue description",
            10,
            5);

        Context.Queues.Add(queue);
        await Context.SaveChangesAsync();
    }
    #endregion
}
