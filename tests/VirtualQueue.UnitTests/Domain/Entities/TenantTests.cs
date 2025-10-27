using FluentAssertions;
using VirtualQueue.Domain.Entities;
using Xunit;

namespace VirtualQueue.UnitTests.Domain.Entities;

public class TenantTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateTenant()
    {
        // Arrange
        var name = "Test Tenant";
        var domain = "test.com";

        // Act
        var tenant = new Tenant(name, domain);

        // Assert
        tenant.Name.Should().Be(name);
        tenant.Domain.Should().Be(domain);
        tenant.IsActive.Should().BeTrue();
        tenant.ApiKey.Should().NotBeNullOrEmpty();
        tenant.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public void Constructor_WithEmptyName_ShouldThrowArgumentException()
    {
        // Arrange
        var name = "";
        var domain = "test.com";

        // Act & Assert
        var action = () => new Tenant(name, domain);
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateName_WithValidName_ShouldUpdateName()
    {
        // Arrange
        var tenant = new Tenant("Old Name", "test.com");
        var newName = "New Name";

        // Act
        tenant.UpdateName(newName);

        // Assert
        tenant.Name.Should().Be(newName);
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var tenant = new Tenant("Test Tenant", "test.com");

        // Act
        tenant.Deactivate();

        // Assert
        tenant.IsActive.Should().BeFalse();
        tenant.DomainEvents.Should().HaveCount(2); // Created + Deactivated
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var tenant = new Tenant("Test Tenant", "test.com");
        tenant.Deactivate();

        // Act
        tenant.Activate();

        // Assert
        tenant.IsActive.Should().BeTrue();
    }
}
