using Microsoft.Extensions.Logging;
using Moq;
using VirtualQueue.Application.Common.Interfaces;
using VirtualQueue.Infrastructure.Services;
using FluentAssertions;

namespace VirtualQueue.Tests.Services;

/// <summary>
/// Unit tests for the JwtAuthenticationService
/// </summary>
public class JwtAuthenticationServiceTests
{
    #region Fields
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<JwtAuthenticationService>> _mockLogger;
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly JwtAuthenticationService _jwtService;
    #endregion

    #region Constructor
    public JwtAuthenticationServiceTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<JwtAuthenticationService>>();
        _mockCacheService = new Mock<ICacheService>();

        // Setup configuration
        _mockConfiguration.Setup(x => x["Jwt:SecretKey"]).Returns("YourSuperSecretKeyThatIsAtLeast32CharactersLong!");
        _mockConfiguration.Setup(x => x["Jwt:Issuer"]).Returns("VirtualQueue");
        _mockConfiguration.Setup(x => x["Jwt:Audience"]).Returns("VirtualQueueUsers");
        _mockConfiguration.Setup(x => x["Jwt:ExpirationMinutes"]).Returns("60");

        _jwtService = new JwtAuthenticationService(_mockConfiguration.Object, _mockLogger.Object, _mockCacheService.Object);
    }
    #endregion

    #region GenerateJwtTokenAsync Tests
    [Fact]
    public async Task GenerateJwtTokenAsync_WithValidUser_ShouldReturnToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var username = "testuser";
        var email = "test@example.com";
        var role = "Customer";

        // Act
        var result = await _jwtService.GenerateJwtTokenAsync(userId, username, email, role);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        result.Should().Contain(".");
    }

    [Fact]
    public async Task GenerateJwtTokenAsync_WithEmptyUsername_ShouldThrowException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var username = "";
        var email = "test@example.com";
        var role = "Customer";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _jwtService.GenerateJwtTokenAsync(userId, username, email, role));
    }

    [Fact]
    public async Task GenerateJwtTokenAsync_WithEmptyEmail_ShouldThrowException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var username = "testuser";
        var email = "";
        var role = "Customer";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _jwtService.GenerateJwtTokenAsync(userId, username, email, role));
    }
    #endregion

    #region ValidateJwtTokenAsync Tests
    [Fact]
    public async Task ValidateJwtTokenAsync_WithValidToken_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var username = "testuser";
        var email = "test@example.com";
        var role = "Customer";

        var token = await _jwtService.GenerateJwtTokenAsync(userId, username, email, role);

        // Act
        var result = await _jwtService.ValidateJwtTokenAsync(token);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateJwtTokenAsync_WithInvalidToken_ShouldReturnFalse()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var result = await _jwtService.ValidateJwtTokenAsync(invalidToken);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateJwtTokenAsync_WithEmptyToken_ShouldReturnFalse()
    {
        // Arrange
        var emptyToken = "";

        // Act
        var result = await _jwtService.ValidateJwtTokenAsync(emptyToken);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateJwtTokenAsync_WithNullToken_ShouldReturnFalse()
    {
        // Arrange
        string? nullToken = null;

        // Act
        var result = await _jwtService.ValidateJwtTokenAsync(nullToken!);

        // Assert
        result.Should().BeFalse();
    }
    #endregion

    #region RefreshTokenAsync Tests
    [Fact]
    public async Task RefreshTokenAsync_WithValidToken_ShouldReturnNewToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var username = "testuser";
        var email = "test@example.com";
        var role = "Customer";

        var originalToken = await _jwtService.GenerateJwtTokenAsync(userId, username, email, role);

        // Act
        var result = await _jwtService.RefreshTokenAsync(originalToken);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        result.Should().NotBe(originalToken);
    }

    [Fact]
    public async Task RefreshTokenAsync_WithInvalidToken_ShouldThrowException()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _jwtService.RefreshTokenAsync(invalidToken));
    }
    #endregion

    #region BlacklistTokenAsync Tests
    [Fact]
    public async Task BlacklistTokenAsync_WithValidToken_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var username = "testuser";
        var email = "test@example.com";
        var role = "Customer";

        var token = await _jwtService.GenerateJwtTokenAsync(userId, username, email, role);

        // Act
        await _jwtService.BlacklistTokenAsync(token);

        // Assert
        _mockCacheService.Verify(x => x.SetAsync(
            It.IsAny<string>(), 
            It.IsAny<string>(), 
            It.IsAny<TimeSpan>(), 
            It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    public async Task BlacklistTokenAsync_WithEmptyToken_ShouldNotThrow()
    {
        // Arrange
        var emptyToken = "";

        // Act & Assert
        await _jwtService.BlacklistTokenAsync(emptyToken);
        // Should not throw exception
    }

    [Fact]
    public async Task BlacklistTokenAsync_WithNullToken_ShouldNotThrow()
    {
        // Arrange
        string? nullToken = null;

        // Act & Assert
        await _jwtService.BlacklistTokenAsync(nullToken!);
        // Should not throw exception
    }
    #endregion
}
