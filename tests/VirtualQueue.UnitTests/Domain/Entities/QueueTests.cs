using FluentAssertions;
using VirtualQueue.Domain.Entities;
using Xunit;

namespace VirtualQueue.UnitTests.Domain.Entities;

public class QueueTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateQueue()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var name = "Test Queue";
        var description = "Test Description";
        var maxConcurrentUsers = 100;
        var releaseRatePerMinute = 10;

        // Act
        var queue = new Queue(tenantId, name, description, maxConcurrentUsers, releaseRatePerMinute);

        // Assert
        queue.TenantId.Should().Be(tenantId);
        queue.Name.Should().Be(name);
        queue.Description.Should().Be(description);
        queue.MaxConcurrentUsers.Should().Be(maxConcurrentUsers);
        queue.ReleaseRatePerMinute.Should().Be(releaseRatePerMinute);
        queue.IsActive.Should().BeTrue();
        queue.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public void EnqueueUser_WithValidUser_ShouldAddUserToQueue()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var queue = new Queue(tenantId, "Test Queue", "Description", 100, 10);
        var userIdentifier = "user123";

        // Act
        var userSession = queue.EnqueueUser(userIdentifier);

        // Assert
        userSession.Should().NotBeNull();
        userSession.UserIdentifier.Should().Be(userIdentifier);
        userSession.QueueId.Should().Be(queue.Id);
        queue.Users.Should().HaveCount(1);
        queue.DomainEvents.Should().HaveCount(2); // Created + UserEnqueued
    }

    [Fact]
    public void ReleaseUsers_WithWaitingUsers_ShouldReleaseUsers()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var queue = new Queue(tenantId, "Test Queue", "Description", 100, 10);
        queue.EnqueueUser("user1");
        queue.EnqueueUser("user2");
        queue.EnqueueUser("user3");

        // Act
        queue.ReleaseUsers(2);

        // Assert
        var releasedUsers = queue.Users.Where(u => u.Status == Domain.Enums.QueueStatus.Released);
        releasedUsers.Should().HaveCount(2);
        queue.LastReleaseAt.Should().NotBeNull();
    }

    [Fact]
    public void GetWaitingUsersCount_ShouldReturnCorrectCount()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var queue = new Queue(tenantId, "Test Queue", "Description", 100, 10);
        queue.EnqueueUser("user1");
        queue.EnqueueUser("user2");

        // Act
        var waitingCount = queue.GetWaitingUsersCount();

        // Assert
        waitingCount.Should().Be(2);
    }
}
