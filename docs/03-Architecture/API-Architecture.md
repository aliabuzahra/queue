# API Architecture - Virtual Queue Management System

**Document Version:** 1.0  
**Date:** January 15, 2024  
**Author:** API Architect  
**Status:** Draft  
**Phase:** 03 - Architecture  
**Priority:** 🔴 Critical  

---

## Executive Summary

This document defines the API architecture for the Virtual Queue Management System. It covers RESTful API design patterns, CQRS implementation, authentication and authorization, rate limiting, error handling, and API versioning strategies. The architecture follows Domain-Driven Design principles and provides a scalable, secure, and maintainable API foundation.

## API Architecture Overview

### **API Design Principles**
- **RESTful Design**: Follow REST conventions and HTTP standards
- **CQRS Pattern**: Separate command and query operations
- **Domain-Driven Design**: API structure reflects domain boundaries
- **Stateless**: All API operations are stateless
- **Idempotent**: Safe operations are idempotent
- **Versioned**: API versioning for backward compatibility

### **Technology Stack**
- **Framework**: ASP.NET Core 8 Web API
- **Authentication**: JWT Bearer tokens
- **Authorization**: Role-based access control (RBAC)
- **Validation**: FluentValidation
- **Documentation**: Swagger/OpenAPI 3.0
- **Rate Limiting**: Redis-based rate limiting
- **Caching**: Redis caching for responses

## API Structure and Organization

### **API Endpoint Structure**
```
Base URL: https://api.virtualqueue.com/v1

Authentication:
POST /auth/login
POST /auth/refresh
POST /auth/logout
POST /auth/register

Tenants:
GET    /tenants
POST   /tenants
GET    /tenants/{id}
PUT    /tenants/{id}
DELETE /tenants/{id}

Users:
GET    /users
POST   /users
GET    /users/{id}
PUT    /users/{id}
DELETE /users/{id}
GET    /users/profile
PUT    /users/profile

Queues:
GET    /queues
POST   /queues
GET    /queues/{id}
PUT    /queues/{id}
DELETE /queues/{id}
GET    /queues/{id}/sessions
POST   /queues/{id}/join
POST   /queues/{id}/leave

User Sessions:
GET    /sessions
POST   /sessions
GET    /sessions/{id}
PUT    /sessions/{id}
DELETE /sessions/{id}
GET    /sessions/{id}/position
POST   /sessions/{id}/complete

Analytics:
GET    /analytics/queues/{id}/stats
GET    /analytics/users/stats
GET    /analytics/system/stats
GET    /analytics/reports/{type}

Notifications:
GET    /notifications
POST   /notifications
GET    /notifications/{id}
PUT    /notifications/{id}
DELETE /notifications/{id}
```

### **CQRS Implementation**

#### **Command Endpoints**
```csharp
// Commands for write operations
POST /api/v1/tenants                    // CreateTenantCommand
PUT  /api/v1/tenants/{id}              // UpdateTenantCommand
DELETE /api/v1/tenants/{id}            // DeleteTenantCommand

POST /api/v1/users                      // CreateUserCommand
PUT  /api/v1/users/{id}                // UpdateUserCommand
DELETE /api/v1/users/{id}               // DeleteUserCommand

POST /api/v1/queues                     // CreateQueueCommand
PUT  /api/v1/queues/{id}               // UpdateQueueCommand
DELETE /api/v1/queues/{id}              // DeleteQueueCommand

POST /api/v1/queues/{id}/join          // JoinQueueCommand
POST /api/v1/queues/{id}/leave         // LeaveQueueCommand
POST /api/v1/sessions/{id}/complete    // CompleteSessionCommand
```

#### **Query Endpoints**
```csharp
// Queries for read operations
GET /api/v1/tenants                    // GetTenantsQuery
GET /api/v1/tenants/{id}              // GetTenantByIdQuery

GET /api/v1/users                      // GetUsersQuery
GET /api/v1/users/{id}                // GetUserByIdQuery
GET /api/v1/users/profile              // GetUserProfileQuery

GET /api/v1/queues                     // GetQueuesQuery
GET /api/v1/queues/{id}               // GetQueueByIdQuery
GET /api/v1/queues/{id}/sessions      // GetQueueSessionsQuery

GET /api/v1/sessions                   // GetSessionsQuery
GET /api/v1/sessions/{id}             // GetSessionByIdQuery
GET /api/v1/sessions/{id}/position    // GetSessionPositionQuery

GET /api/v1/analytics/queues/{id}/stats // GetQueueStatsQuery
GET /api/v1/analytics/users/stats      // GetUserStatsQuery
```

## Authentication and Authorization

### **Authentication Strategy**

#### **JWT Token Implementation**
```csharp
public class JwtAuthenticationService
{
    public async Task<AuthenticationResult> AuthenticateAsync(LoginRequest request)
    {
        // Validate credentials
        var user = await _userService.ValidateCredentialsAsync(request.Email, request.Password);
        if (user == null)
            return AuthenticationResult.Failed("Invalid credentials");

        // Generate JWT token
        var token = _jwtTokenGenerator.GenerateToken(user);
        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();

        // Store refresh token
        await _refreshTokenService.StoreRefreshTokenAsync(user.Id, refreshToken);

        return AuthenticationResult.Success(token, refreshToken);
    }

    public async Task<AuthenticationResult> RefreshTokenAsync(RefreshTokenRequest request)
    {
        // Validate refresh token
        var refreshToken = await _refreshTokenService.ValidateRefreshTokenAsync(request.RefreshToken);
        if (refreshToken == null)
            return AuthenticationResult.Failed("Invalid refresh token");

        // Generate new tokens
        var user = await _userService.GetByIdAsync(refreshToken.UserId);
        var newToken = _jwtTokenGenerator.GenerateToken(user);
        var newRefreshToken = _jwtTokenGenerator.GenerateRefreshToken();

        // Update refresh token
        await _refreshTokenService.UpdateRefreshTokenAsync(refreshToken.Id, newRefreshToken);

        return AuthenticationResult.Success(newToken, newRefreshToken);
    }
}
```

#### **JWT Token Configuration**
```csharp
public class JwtTokenGenerator
{
    private readonly JwtSettings _jwtSettings;

    public string GenerateToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("tenant_id", user.TenantId.ToString()),
                new Claim("username", user.Username ?? string.Empty)
            }),
            Expires = DateTime.UtcNow.AddHours(_jwtSettings.ExpirationHours),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
```

### **Authorization Implementation**

#### **Role-Based Authorization**
```csharp
[Authorize(Roles = "Admin")]
[HttpPost("tenants")]
public async Task<ActionResult<TenantDto>> CreateTenant([FromBody] CreateTenantCommand command)
{
    var result = await _mediator.Send(command);
    return CreatedAtAction(nameof(GetTenant), new { id = result.Id }, result);
}

[Authorize(Roles = "Admin,Manager")]
[HttpPut("tenants/{id}")]
public async Task<ActionResult<TenantDto>> UpdateTenant(Guid id, [FromBody] UpdateTenantCommand command)
{
    command.Id = id;
    var result = await _mediator.Send(command);
    return Ok(result);
}

[Authorize(Roles = "Admin,Manager,Staff")]
[HttpGet("queues")]
public async Task<ActionResult<PagedResult<QueueDto>>> GetQueues([FromQuery] GetQueuesQuery query)
{
    var result = await _mediator.Send(query);
    return Ok(result);
}
```

#### **Policy-Based Authorization**
```csharp
public class AuthorizationPolicies
{
    public static void Configure(AuthorizationOptions options)
    {
        options.AddPolicy("TenantAdmin", policy =>
            policy.RequireAssertion(context =>
                context.User.IsInRole("Admin") ||
                (context.User.IsInRole("Manager") && 
                 context.User.HasClaim("tenant_admin", "true"))));

        options.AddPolicy("QueueManager", policy =>
            policy.RequireAssertion(context =>
                context.User.IsInRole("Admin") ||
                context.User.IsInRole("Manager") ||
                context.User.IsInRole("Staff")));

        options.AddPolicy("UserAccess", policy =>
            policy.RequireAssertion(context =>
                context.User.IsInRole("Admin") ||
                context.User.HasClaim("user_id", context.User.FindFirstValue(ClaimTypes.NameIdentifier))));
    }
}
```

## API Controllers Implementation

### **Base Controller**
```csharp
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Produces("application/json")]
public abstract class BaseController : ControllerBase
{
    protected readonly IMediator _mediator;
    protected readonly ILogger _logger;

    protected BaseController(IMediator mediator, ILogger logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    protected ActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
            return Ok(result.Value);

        return result.Error switch
        {
            ValidationError => BadRequest(result.Error),
            NotFoundError => NotFound(result.Error),
            UnauthorizedError => Unauthorized(result.Error),
            ForbiddenError => Forbid(),
            _ => StatusCode(500, result.Error)
        };
    }

    protected ActionResult HandlePagedResult<T>(PagedResult<T> result)
    {
        Response.Headers.Add("X-Total-Count", result.TotalCount.ToString());
        Response.Headers.Add("X-Page-Count", result.PageCount.ToString());
        Response.Headers.Add("X-Current-Page", result.CurrentPage.ToString());
        Response.Headers.Add("X-Page-Size", result.PageSize.ToString());

        return Ok(result.Items);
    }
}
```

### **Tenants Controller**
```csharp
[Authorize(Roles = "Admin")]
public class TenantsController : BaseController
{
    public TenantsController(IMediator mediator, ILogger<TenantsController> logger) 
        : base(mediator, logger) { }

    [HttpGet]
    public async Task<ActionResult<PagedResult<TenantDto>>> GetTenants([FromQuery] GetTenantsQuery query)
    {
        var result = await _mediator.Send(query);
        return HandlePagedResult(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TenantDto>> GetTenant(Guid id)
    {
        var query = new GetTenantByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<ActionResult<TenantDto>> CreateTenant([FromBody] CreateTenantCommand command)
    {
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TenantDto>> UpdateTenant(Guid id, [FromBody] UpdateTenantCommand command)
    {
        command.Id = id;
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTenant(Guid id)
    {
        var command = new DeleteTenantCommand { Id = id };
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }
}
```

### **Queues Controller**
```csharp
[Authorize]
public class QueuesController : BaseController
{
    public QueuesController(IMediator mediator, ILogger<QueuesController> logger) 
        : base(mediator, logger) { }

    [HttpGet]
    public async Task<ActionResult<PagedResult<QueueDto>>> GetQueues([FromQuery] GetQueuesQuery query)
    {
        var result = await _mediator.Send(query);
        return HandlePagedResult(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<QueueDto>> GetQueue(Guid id)
    {
        var query = new GetQueueByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<QueueDto>> CreateQueue([FromBody] CreateQueueCommand command)
    {
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<QueueDto>> UpdateQueue(Guid id, [FromBody] UpdateQueueCommand command)
    {
        command.Id = id;
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }

    [HttpPost("{id}/join")]
    public async Task<ActionResult<UserSessionDto>> JoinQueue(Guid id, [FromBody] JoinQueueCommand command)
    {
        command.QueueId = id;
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }

    [HttpPost("{id}/leave")]
    public async Task<ActionResult> LeaveQueue(Guid id, [FromBody] LeaveQueueCommand command)
    {
        command.QueueId = id;
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }

    [HttpGet("{id}/sessions")]
    public async Task<ActionResult<PagedResult<UserSessionDto>>> GetQueueSessions(Guid id, [FromQuery] GetQueueSessionsQuery query)
    {
        query.QueueId = id;
        var result = await _mediator.Send(query);
        return HandlePagedResult(result);
    }
}
```

## Request/Response Models

### **Request Models**
```csharp
public class CreateTenantRequest
{
    [Required]
    [StringLength(255)]
    public string Name { get; set; }

    [Required]
    [StringLength(100)]
    [RegularExpression(@"^[a-z0-9-]+$")]
    public string Slug { get; set; }

    [StringLength(1000)]
    public string Description { get; set; }

    public TenantSettingsDto Settings { get; set; }
}

public class CreateQueueRequest
{
    [Required]
    [StringLength(255)]
    public string Name { get; set; }

    [StringLength(1000)]
    public string Description { get; set; }

    [Required]
    [StringLength(100)]
    [RegularExpression(@"^[a-z0-9-]+$")]
    public string Slug { get; set; }

    [Range(1, 10000)]
    public int Capacity { get; set; } = 1000;

    [Range(0, 100)]
    public int Priority { get; set; } = 0;

    public QueueSettingsDto Settings { get; set; }
    public OperatingHoursDto OperatingHours { get; set; }
}

public class JoinQueueRequest
{
    [StringLength(500)]
    public string Notes { get; set; }

    public Dictionary<string, object> Metadata { get; set; }
}
```

### **Response Models**
```csharp
public class TenantDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    public string Description { get; set; }
    public TenantSettingsDto Settings { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class QueueDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Slug { get; set; }
    public int Capacity { get; set; }
    public int Priority { get; set; }
    public string Status { get; set; }
    public QueueSettingsDto Settings { get; set; }
    public OperatingHoursDto OperatingHours { get; set; }
    public QueueStatsDto Stats { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class UserSessionDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid QueueId { get; set; }
    public Guid? UserId { get; set; }
    public string SessionToken { get; set; }
    public int Position { get; set; }
    public string Status { get; set; }
    public int? EstimatedWaitTime { get; set; }
    public int? ActualWaitTime { get; set; }
    public DateTime? ServiceStartTime { get; set; }
    public DateTime? ServiceEndTime { get; set; }
    public Dictionary<string, object> Metadata { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

## Error Handling and Validation

### **Error Response Model**
```csharp
public class ErrorResponse
{
    public string Type { get; set; }
    public string Title { get; set; }
    public int Status { get; set; }
    public string Detail { get; set; }
    public string Instance { get; set; }
    public Dictionary<string, string[]> Errors { get; set; }
    public string TraceId { get; set; }
}

public class ValidationErrorResponse : ErrorResponse
{
    public ValidationErrorResponse()
    {
        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
        Title = "One or more validation errors occurred.";
        Status = 400;
    }
}
```

### **Global Exception Handling**
```csharp
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = exception switch
        {
            ValidationException ex => new ValidationErrorResponse
            {
                Errors = ex.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())
            },
            NotFoundException ex => new ErrorResponse
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                Title = "Not Found",
                Status = 404,
                Detail = ex.Message
            },
            UnauthorizedException ex => new ErrorResponse
            {
                Type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                Title = "Unauthorized",
                Status = 401,
                Detail = ex.Message
            },
            ForbiddenException ex => new ErrorResponse
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                Title = "Forbidden",
                Status = 403,
                Detail = ex.Message
            },
            _ => new ErrorResponse
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                Title = "Internal Server Error",
                Status = 500,
                Detail = "An error occurred while processing your request."
            }
        };

        response.TraceId = Activity.Current?.Id ?? context.TraceIdentifier;
        response.Instance = context.Request.Path;

        context.Response.StatusCode = response.Status;
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
```

## Rate Limiting and Throttling

### **Rate Limiting Implementation**
```csharp
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RateLimitingMiddleware> _logger;

    public RateLimitingMiddleware(RequestDelegate next, IConnectionMultiplexer redis, ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _redis = redis;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var rateLimitKey = GetRateLimitKey(context);
        var rateLimit = GetRateLimit(context.Request.Path);

        var database = _redis.GetDatabase();
        var current = await database.StringIncrementAsync(rateLimitKey);
        
        if (current == 1)
        {
            await database.KeyExpireAsync(rateLimitKey, TimeSpan.FromMinutes(1));
        }

        if (current > rateLimit.Limit)
        {
            context.Response.StatusCode = 429;
            context.Response.Headers.Add("Retry-After", rateLimit.WindowSeconds.ToString());
            await context.Response.WriteAsync("Rate limit exceeded");
            return;
        }

        context.Response.Headers.Add("X-RateLimit-Limit", rateLimit.Limit.ToString());
        context.Response.Headers.Add("X-RateLimit-Remaining", (rateLimit.Limit - current).ToString());
        context.Response.Headers.Add("X-RateLimit-Reset", DateTimeOffset.UtcNow.AddSeconds(rateLimit.WindowSeconds).ToUnixTimeSeconds().ToString());

        await _next(context);
    }

    private string GetRateLimitKey(HttpContext context)
    {
        var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
        var endpoint = context.Request.Path.Value?.ToLowerInvariant() ?? "";
        return $"rate_limit:{userId}:{endpoint}";
    }

    private RateLimit GetRateLimit(string path)
    {
        return path switch
        {
            var p when p.Contains("/auth/") => new RateLimit { Limit = 5, WindowSeconds = 60 },
            var p when p.Contains("/queues/") && p.Contains("/join") => new RateLimit { Limit = 10, WindowSeconds = 60 },
            _ => new RateLimit { Limit = 100, WindowSeconds = 60 }
        };
    }
}

public class RateLimit
{
    public int Limit { get; set; }
    public int WindowSeconds { get; set; }
}
```

## API Versioning

### **Versioning Strategy**
```csharp
public class ApiVersioningService
{
    public static void ConfigureApiVersioning(IServiceCollection services)
    {
        services.AddApiVersioning(opt =>
        {
            opt.DefaultApiVersion = new ApiVersion(1, 0);
            opt.AssumeDefaultVersionWhenUnspecified = true;
            opt.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),
                new HeaderApiVersionReader("X-Version"),
                new MediaTypeApiVersionReader("ver")
            );
        });

        services.AddVersionedApiExplorer(setup =>
        {
            setup.GroupNameFormat = "'v'VVV";
            setup.SubstituteApiVersionInUrl = true;
        });
    }
}
```

### **Versioned Controllers**
```csharp
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
public class QueuesController : BaseController
{
    [HttpGet]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<PagedResult<QueueDto>>> GetQueuesV1([FromQuery] GetQueuesQuery query)
    {
        // V1 implementation
    }

    [HttpGet]
    [MapToApiVersion("2.0")]
    public async Task<ActionResult<PagedResult<QueueV2Dto>>> GetQueuesV2([FromQuery] GetQueuesV2Query query)
    {
        // V2 implementation with enhanced features
    }
}
```

## API Documentation

### **Swagger Configuration**
```csharp
public class SwaggerConfiguration
{
    public static void ConfigureSwagger(IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Virtual Queue Management API",
                Version = "v1",
                Description = "API for managing virtual queues and user sessions",
                Contact = new OpenApiContact
                {
                    Name = "API Support",
                    Email = "support@virtualqueue.com"
                }
            });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);
        });
    }
}
```

## Performance Optimization

### **Response Caching**
```csharp
[HttpGet("{id}")]
[ResponseCache(Duration = 300, VaryByQueryKeys = new[] { "id" })]
public async Task<ActionResult<QueueDto>> GetQueue(Guid id)
{
    var query = new GetQueueByIdQuery { Id = id };
    var result = await _mediator.Send(query);
    return HandleResult(result);
}
```

### **Compression**
```csharp
public class CompressionMiddleware
{
    private readonly RequestDelegate _next;

    public CompressionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var acceptEncoding = context.Request.Headers["Accept-Encoding"].ToString();
        
        if (acceptEncoding.Contains("gzip"))
        {
            context.Response.Headers.Add("Content-Encoding", "gzip");
            using var gzipStream = new GZipStream(context.Response.Body, CompressionLevel.Fastest);
            context.Response.Body = gzipStream;
        }

        await _next(context);
    }
}
```

## Approval and Sign-off

### **API Architecture Approval**
- **API Architect**: [Name] - [Date]
- **Technical Lead**: [Name] - [Date]
- **Security Lead**: [Name] - [Date]
- **Performance Lead**: [Name] - [Date]

### **Document Control**
- **Version**: 1.0
- **Last Updated**: January 15, 2024
- **Next Review**: February 15, 2024
- **Distribution**: Development Team, API Team, Operations Team

---

**Document Status**: Draft  
**Next Phase**: Security Architecture Design  
**Dependencies**: Requirements approval, technology stack confirmation
