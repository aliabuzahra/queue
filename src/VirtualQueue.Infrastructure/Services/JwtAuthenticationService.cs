using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using VirtualQueue.Application.Common.Interfaces;

namespace VirtualQueue.Infrastructure.Services;

/// <summary>
/// Service for JWT token authentication and management.
/// </summary>
public class JwtAuthenticationService : IAuthenticationService
{
    #region Fields
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtAuthenticationService> _logger;
    private readonly ICacheService _cacheService;
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expirationMinutes;
    #endregion

    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="JwtAuthenticationService"/> class.
    /// </summary>
    /// <param name="configuration">The configuration instance.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="cacheService">The cache service instance.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any of the required parameters are null.
    /// </exception>
    public JwtAuthenticationService(IConfiguration configuration, ILogger<JwtAuthenticationService> logger, ICacheService cacheService)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        
        _secretKey = _configuration["Jwt:SecretKey"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!";
        _issuer = _configuration["Jwt:Issuer"] ?? "VirtualQueue";
        _audience = _configuration["Jwt:Audience"] ?? "VirtualQueueUsers";
        _expirationMinutes = int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "60");
        
        ValidateConfiguration();
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Generates a JWT token for the specified user and tenant.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="userIdentifier">The user identifier.</param>
    /// <param name="roles">The list of user roles.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// A JWT token string.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when any of the required parameters are invalid.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when token generation fails.
    /// </exception>
    public async Task<string> GenerateJwtTokenAsync(Guid tenantId, string userIdentifier, List<string> roles, CancellationToken cancellationToken = default)
    {
        ValidateTokenGenerationParameters(tenantId, userIdentifier, roles);
        
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);
            
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userIdentifier),
                new("tenant_id", tenantId.ToString()),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            // Add roles as claims
            foreach (var role in roles)
            {
                if (!string.IsNullOrWhiteSpace(role))
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_expirationMinutes),
                Issuer = _issuer,
                Audience = _audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            // Cache the token for validation and blacklisting
            var jti = claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
            if (!string.IsNullOrEmpty(jti))
            {
                var cacheKey = $"jwt_token:{tenantId}:{userIdentifier}:{jti}";
                await _cacheService.SetAsync(cacheKey, tokenString, TimeSpan.FromMinutes(_expirationMinutes), cancellationToken);
            }

            _logger.LogInformation("JWT token generated for user {UserIdentifier} in tenant {TenantId}", userIdentifier, tenantId);
            return tokenString;
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogError(ex, "Security token error generating JWT token for user {UserIdentifier}", userIdentifier);
            throw new InvalidOperationException("Failed to generate JWT token due to security token error", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate JWT token for user {UserIdentifier}", userIdentifier);
            throw new InvalidOperationException("Failed to generate JWT token", ex);
        }
    }

    public async Task<bool> ValidateJwtTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(token))
                return false;

            // Check if token is blacklisted
            var jti = GetTokenJti(token);
            if (!string.IsNullOrEmpty(jti))
            {
                var blacklistKey = $"jwt_blacklist:{jti}";
                var isBlacklisted = await _cacheService.ExistsAsync(blacklistKey, cancellationToken);
                if (isBlacklisted)
                {
                    _logger.LogWarning("Token is blacklisted");
                    return false;
                }
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
            return true;
        }
        catch (SecurityTokenExpiredException)
        {
            _logger.LogWarning("JWT token has expired");
            return false;
        }
        catch (SecurityTokenInvalidSignatureException)
        {
            _logger.LogWarning("JWT token has invalid signature");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "JWT token validation failed");
            return false;
        }
    }

    public async Task<Guid?> GetTenantIdFromTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            
            var tenantIdClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == "tenant_id");
            return tenantIdClaim != null ? Guid.Parse(tenantIdClaim.Value) : null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to extract tenant ID from token");
            return null;
        }
    }

    public async Task<string?> GetUserIdentifierFromTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            
            return jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to extract user identifier from token");
            return null;
        }
    }

    public async Task<List<string>> GetRolesFromTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            
            return jwtToken.Claims
                .Where(x => x.Type == ClaimTypes.Role)
                .Select(x => x.Value)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to extract roles from token");
            return new List<string>();
        }
    }

    public async Task<bool> IsTokenExpiredAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            
            return jwtToken.ValidTo < DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to check token expiration");
            return true;
        }
    }

    public async Task<string> RefreshTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Token cannot be null or empty", nameof(token));

            // Validate the current token
            var isValid = await ValidateJwtTokenAsync(token, cancellationToken);
            if (!isValid)
                throw new SecurityTokenException("Invalid token");

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            
            var tenantId = jwtToken.Claims.FirstOrDefault(x => x.Type == "tenant_id")?.Value;
            var userIdentifier = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            var roles = jwtToken.Claims.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value).ToList();

            if (tenantId == null || userIdentifier == null)
            {
                throw new InvalidOperationException("Invalid token for refresh");
            }

            // Generate new token
            var newToken = await GenerateJwtTokenAsync(Guid.Parse(tenantId), userIdentifier, roles, cancellationToken);

            // Blacklist the old token
            await BlacklistTokenAsync(token, cancellationToken);

            _logger.LogInformation("JWT token refreshed successfully for user {UserIdentifier}", userIdentifier);
            return newToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh token");
            throw;
        }
    }

    /// <summary>
    /// Blacklists a JWT token (for logout functionality).
    /// </summary>
    /// <param name="token">The JWT token to blacklist.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task representing the async operation.</returns>
    public async Task BlacklistTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(token))
                return;

            var jti = GetTokenJti(token);
            if (!string.IsNullOrEmpty(jti))
            {
                var cacheKey = $"jwt_blacklist:{jti}";
                var expiration = GetTokenExpiration(token);
                
                if (expiration.HasValue && expiration.Value > DateTime.UtcNow)
                {
                    var timeToLive = expiration.Value.Subtract(DateTime.UtcNow);
                    await _cacheService.SetAsync(cacheKey, "blacklisted", timeToLive, cancellationToken);
                    _logger.LogDebug("Token blacklisted with JTI: {Jti}", jti);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error blacklisting token");
        }
    }

    // New methods required by the interface
    public async Task<AuthenticationResult> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return new AuthenticationResult(false, null, null, new List<string>(), "Username and password are required");
            }

            _logger.LogInformation("Authenticating user {Username}", username);

            // In a real implementation, this would validate credentials against a database
            // For now, we'll use a simple mock authentication
            if (username == "admin" && password == "admin123")
            {
                var tenantId = Guid.NewGuid();
                var roles = new List<string> { "Admin" };
                var token = await GenerateJwtTokenAsync(tenantId, username, roles, cancellationToken);

                return new AuthenticationResult(true, token, username, roles, null);
            }

            return new AuthenticationResult(false, null, null, new List<string>(), "Invalid credentials");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to authenticate user {Username}", username);
            return new AuthenticationResult(false, null, null, new List<string>(), "Authentication failed");
        }
    }

    public async Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            return await ValidateJwtTokenAsync(token, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate token");
            return false;
        }
    }

    public async Task<string?> GetRoleFromTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            var roles = await GetRolesFromTokenAsync(token, cancellationToken);
            return roles.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get role from token");
            return null;
        }
    }

    public async Task<string> GenerateApiKeyAsync(Guid tenantId, string description, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Description cannot be null or empty", nameof(description));

            _logger.LogInformation("Generating API key for tenant {TenantId} with description {Description}", tenantId, description);

            // Generate a secure API key
            var apiKey = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
            
            // Store the API key in cache with metadata
            var cacheKey = $"api_key:{tenantId}:{apiKey}";
            var apiKeyData = new
            {
                TenantId = tenantId,
                Description = description,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _cacheService.SetAsync(cacheKey, apiKeyData, TimeSpan.FromDays(365), cancellationToken);

            _logger.LogInformation("API key generated successfully for tenant {TenantId}", tenantId);
            return apiKey;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate API key for tenant {TenantId}", tenantId);
            throw;
        }
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Validates the JWT configuration settings.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when configuration is invalid.
    /// </exception>
    private void ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_secretKey))
            throw new InvalidOperationException("JWT SecretKey is not configured");
            
        if (_secretKey.Length < 32)
            throw new InvalidOperationException("JWT SecretKey must be at least 32 characters long");
            
        if (string.IsNullOrWhiteSpace(_issuer))
            throw new InvalidOperationException("JWT Issuer is not configured");
            
        if (string.IsNullOrWhiteSpace(_audience))
            throw new InvalidOperationException("JWT Audience is not configured");
            
        if (_expirationMinutes <= 0)
            throw new InvalidOperationException("JWT ExpirationMinutes must be greater than 0");
    }

    /// <summary>
    /// Validates parameters for token generation.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="userIdentifier">The user identifier.</param>
    /// <param name="roles">The list of roles.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when any parameter is invalid.
    /// </exception>
    private static void ValidateTokenGenerationParameters(Guid tenantId, string userIdentifier, List<string> roles)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("Tenant ID cannot be empty", nameof(tenantId));
            
        if (string.IsNullOrWhiteSpace(userIdentifier))
            throw new ArgumentException("User identifier cannot be null or empty", nameof(userIdentifier));
            
        if (roles == null)
            throw new ArgumentException("Roles cannot be null", nameof(roles));
    }

    /// <summary>
    /// Extracts the JTI (JWT ID) from a token.
    /// </summary>
    /// <param name="token">The JWT token.</param>
    /// <returns>The JTI if found, null otherwise.</returns>
    private string? GetTokenJti(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            return jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Extracts the expiration time from a token.
    /// </summary>
    /// <param name="token">The JWT token.</param>
    /// <returns>The expiration time if found, null otherwise.</returns>
    private DateTime? GetTokenExpiration(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            return jwtToken.ValidTo;
        }
        catch
        {
            return null;
        }
    }
    #endregion
}
