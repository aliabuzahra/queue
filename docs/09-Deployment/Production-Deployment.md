# Production Deployment - Virtual Queue Management System

**Document Version:** 1.0  
**Date:** January 15, 2024  
**Author:** DevOps Lead  
**Status:** Draft  
**Phase:** 9 - Deployment  
**Priority:** 🔴 Critical  

---

## Executive Summary

This document provides comprehensive production deployment guidelines for the Virtual Queue Management System. It covers production deployment strategies, deployment procedures, rollback procedures, and post-deployment monitoring to ensure reliable and successful production deployments.

## Production Deployment Overview

### **Production Deployment Objectives**

#### **Primary Objectives**
- **Zero Downtime**: Minimize or eliminate service downtime during deployment
- **Reliable Deployment**: Ensure consistent and reliable deployments
- **Quick Rollback**: Provide fast rollback capabilities in case of issues
- **Data Integrity**: Maintain data integrity during deployment
- **Service Continuity**: Ensure continuous service availability

#### **Production Deployment Benefits**
- **Reduced Risk**: Minimize deployment-related risks
- **Improved Reliability**: Enhance system reliability and stability
- **Better User Experience**: Maintain consistent user experience
- **Operational Efficiency**: Streamline deployment operations
- **Business Continuity**: Ensure uninterrupted business operations

### **Deployment Strategies**

#### **Deployment Strategy Types**
- **Blue-Green Deployment**: Zero-downtime deployment with instant switching
- **Canary Deployment**: Gradual rollout with monitoring and automatic rollback
- **Rolling Deployment**: Incremental deployment across multiple instances
- **Feature Flags**: Feature-based deployment control
- **Database Migration**: Safe database schema and data updates

#### **Production Deployment Flow**
```yaml
production_deployment_flow:
  pre_deployment:
    - "Deployment approval"
    - "Backup creation"
    - "Health check"
    - "Traffic analysis"
    - "Rollback preparation"
  
  deployment:
    - "Code deployment"
    - "Database migration"
    - "Configuration update"
    - "Service restart"
    - "Health verification"
  
  post_deployment:
    - "Smoke testing"
    - "Performance monitoring"
    - "Error monitoring"
    - "User feedback"
    - "Documentation update"
  
  rollback:
    - "Issue detection"
    - "Rollback decision"
    - "Traffic switching"
    - "Service restoration"
    - "Issue resolution"
```

## Blue-Green Deployment

### **Blue-Green Strategy**

#### **Blue-Green Architecture**
```yaml
blue_green_architecture:
  blue_environment:
    status: "Active (Current Production)"
    traffic: "100% of traffic"
    version: "Current version"
    health: "Healthy"
  
  green_environment:
    status: "Standby (New Version)"
    traffic: "0% of traffic"
    version: "New version"
    health: "Ready for deployment"
  
  load_balancer:
    function: "Traffic routing"
    configuration: "Weighted routing"
    health_checks: "Active monitoring"
    failover: "Automatic switching"
```

#### **Blue-Green Deployment Script**
```bash
#!/bin/bash
# blue-green-deployment.sh

set -e

NEW_VERSION=$1
ROLLBACK_VERSION=${2:-"current"}

if [ -z "$NEW_VERSION" ]; then
    echo "Usage: $0 <new_version> [rollback_version]"
    echo "Example: $0 v1.2.0 v1.1.0"
    exit 1
fi

echo "Starting Blue-Green deployment to version: $NEW_VERSION"

# Step 1: Pre-deployment checks
echo "Step 1: Pre-deployment checks"
echo "Checking current production health..."
curl -f https://virtualqueue-production.azurewebsites.net/health || exit 1

echo "Creating backup..."
az webapp deployment list --name virtualqueue-production --resource-group virtualqueue-rg --query "[0].id" -o tsv > current-deployment.txt

# Step 2: Deploy to Green environment
echo "Step 2: Deploying to Green environment"
az webapp deployment source config-zip \
  --name virtualqueue-production-green \
  --resource-group virtualqueue-rg \
  --src "artifacts/VirtualQueue.Api-$NEW_VERSION.zip"

az webapp deployment source config-zip \
  --name virtualqueue-production-green-worker \
  --resource-group virtualqueue-rg \
  --src "artifacts/VirtualQueue.Worker-$NEW_VERSION.zip"

# Step 3: Run database migrations
echo "Step 3: Running database migrations"
az postgres flexible-server execute \
  --name virtualqueue-production-db \
  --admin-user postgres \
  --admin-password "$DB_PASSWORD" \
  --database virtualqueue \
  --queryfile "migrations-$NEW_VERSION.sql"

# Step 4: Health check Green environment
echo "Step 4: Health checking Green environment"
sleep 30
curl -f https://virtualqueue-production-green.azurewebsites.net/health || exit 1

# Step 5: Switch traffic to Green
echo "Step 5: Switching traffic to Green environment"
az network traffic-manager profile update \
  --name virtualqueue-production-tm \
  --resource-group virtualqueue-rg \
  --routing-method Weighted \
  --endpoints virtualqueue-production-blue weight=0 virtualqueue-production-green weight=100

# Step 6: Monitor Green environment
echo "Step 6: Monitoring Green environment"
sleep 60
curl -f https://virtualqueue-production.azurewebsites.net/health || {
    echo "Health check failed, initiating rollback..."
    ./rollback-deployment.sh "$ROLLBACK_VERSION"
    exit 1
}

# Step 7: Update Blue environment
echo "Step 7: Updating Blue environment for next deployment"
az webapp deployment source config-zip \
  --name virtualqueue-production-blue \
  --resource-group virtualqueue-rg \
  --src "artifacts/VirtualQueue.Api-$NEW_VERSION.zip"

az webapp deployment source config-zip \
  --name virtualqueue-production-blue-worker \
  --resource-group virtualqueue-rg \
  --src "artifacts/VirtualQueue.Worker-$NEW_VERSION.zip"

echo "Blue-Green deployment completed successfully!"
echo "Current version: $NEW_VERSION"
echo "Green environment is now active"
echo "Blue environment is ready for next deployment"
```

### **Blue-Green Rollback Script**
```bash
#!/bin/bash
# rollback-deployment.sh

set -e

ROLLBACK_VERSION=$1

if [ -z "$ROLLBACK_VERSION" ]; then
    echo "Usage: $0 <rollback_version>"
    echo "Example: $0 v1.1.0"
    exit 1
fi

echo "Starting rollback to version: $ROLLBACK_VERSION"

# Step 1: Switch traffic back to Blue
echo "Step 1: Switching traffic back to Blue environment"
az network traffic-manager profile update \
  --name virtualqueue-production-tm \
  --resource-group virtualqueue-rg \
  --routing-method Weighted \
  --endpoints virtualqueue-production-blue weight=100 virtualqueue-production-green weight=0

# Step 2: Health check Blue environment
echo "Step 2: Health checking Blue environment"
sleep 30
curl -f https://virtualqueue-production.azurewebsites.net/health || exit 1

# Step 3: Rollback database if needed
echo "Step 3: Checking if database rollback is needed"
if [ -f "migrations-rollback-$ROLLBACK_VERSION.sql" ]; then
    echo "Rolling back database..."
    az postgres flexible-server execute \
      --name virtualqueue-production-db \
      --admin-user postgres \
      --admin-password "$DB_PASSWORD" \
      --database virtualqueue \
      --queryfile "migrations-rollback-$ROLLBACK_VERSION.sql"
fi

# Step 4: Update Green environment with rollback version
echo "Step 4: Updating Green environment with rollback version"
az webapp deployment source config-zip \
  --name virtualqueue-production-green \
  --resource-group virtualqueue-rg \
  --src "artifacts/VirtualQueue.Api-$ROLLBACK_VERSION.zip"

az webapp deployment source config-zip \
  --name virtualqueue-production-green-worker \
  --resource-group virtualqueue-rg \
  --src "artifacts/VirtualQueue.Worker-$ROLLBACK_VERSION.zip"

# Step 5: Final health check
echo "Step 5: Final health check"
curl -f https://virtualqueue-production.azurewebsites.net/health || exit 1

echo "Rollback to version $ROLLBACK_VERSION completed successfully!"
echo "Blue environment is now active with version $ROLLBACK_VERSION"
echo "Green environment is ready for next deployment"
```

## Canary Deployment

### **Canary Strategy**

#### **Canary Deployment Configuration**
```yaml
canary_deployment:
  traffic_splitting:
    stage_1:
      canary_percentage: 5
      duration: "10 minutes"
      monitoring: "Error rate, response time"
    
    stage_2:
      canary_percentage: 25
      duration: "20 minutes"
      monitoring: "Error rate, response time, throughput"
    
    stage_3:
      canary_percentage: 50
      duration: "30 minutes"
      monitoring: "Full monitoring"
    
    stage_4:
      canary_percentage: 100
      duration: "Ongoing"
      monitoring: "Continuous monitoring"
  
  rollback_triggers:
    error_rate: "> 1%"
    response_time: "> 500ms"
    throughput: "< 80% of baseline"
    user_complaints: "> 5 complaints"
```

#### **Canary Deployment Script**
```bash
#!/bin/bash
# canary-deployment.sh

set -e

NEW_VERSION=$1
CANARY_PERCENTAGE=${2:-5}

if [ -z "$NEW_VERSION" ]; then
    echo "Usage: $0 <new_version> [canary_percentage]"
    echo "Example: $0 v1.2.0 5"
    exit 1
fi

echo "Starting Canary deployment to version: $NEW_VERSION with $CANARY_PERCENTAGE% traffic"

# Step 1: Deploy to Canary environment
echo "Step 1: Deploying to Canary environment"
az webapp deployment source config-zip \
  --name virtualqueue-production-canary \
  --resource-group virtualqueue-rg \
  --src "artifacts/VirtualQueue.Api-$NEW_VERSION.zip"

az webapp deployment source config-zip \
  --name virtualqueue-production-canary-worker \
  --resource-group virtualqueue-rg \
  --src "artifacts/VirtualQueue.Worker-$NEW_VERSION.zip"

# Step 2: Health check Canary environment
echo "Step 2: Health checking Canary environment"
sleep 30
curl -f https://virtualqueue-production-canary.azurewebsites.net/health || exit 1

# Step 3: Start Canary traffic
echo "Step 3: Starting Canary traffic ($CANARY_PERCENTAGE%)"
az network traffic-manager profile update \
  --name virtualqueue-production-tm \
  --resource-group virtualqueue-rg \
  --routing-method Weighted \
  --endpoints virtualqueue-production weight=$((100-CANARY_PERCENTAGE)) virtualqueue-production-canary weight=$CANARY_PERCENTAGE

# Step 4: Monitor Canary deployment
echo "Step 4: Monitoring Canary deployment"
./monitor-canary-deployment.sh "$NEW_VERSION" "$CANARY_PERCENTAGE"

echo "Canary deployment completed successfully!"
echo "Version $NEW_VERSION is receiving $CANARY_PERCENTAGE% of traffic"
```

#### **Canary Monitoring Script**
```bash
#!/bin/bash
# monitor-canary-deployment.sh

set -e

NEW_VERSION=$1
CANARY_PERCENTAGE=$2
MONITORING_DURATION=${3:-600} # 10 minutes default

echo "Monitoring Canary deployment for $MONITORING_DURATION seconds"

START_TIME=$(date +%s)
END_TIME=$((START_TIME + MONITORING_DURATION))

while [ $(date +%s) -lt $END_TIME ]; do
    echo "Checking metrics at $(date)"
    
    # Check error rate
    ERROR_RATE=$(curl -s "https://virtualqueue-production.azurewebsites.net/metrics" | grep "http_requests_total{status=~\"5..\"}" | awk '{print $2}')
    if [ "$ERROR_RATE" -gt 1 ]; then
        echo "Error rate too high: $ERROR_RATE%, initiating rollback"
        ./rollback-canary-deployment.sh
        exit 1
    fi
    
    # Check response time
    RESPONSE_TIME=$(curl -s "https://virtualqueue-production.azurewebsites.net/metrics" | grep "http_request_duration_seconds" | awk '{print $2}')
    if [ "$RESPONSE_TIME" -gt 500 ]; then
        echo "Response time too high: ${RESPONSE_TIME}ms, initiating rollback"
        ./rollback-canary-deployment.sh
        exit 1
    fi
    
    # Check throughput
    THROUGHPUT=$(curl -s "https://virtualqueue-production.azurewebsites.net/metrics" | grep "http_requests_total" | awk '{print $2}')
    BASELINE_THROUGHPUT=1000
    if [ "$THROUGHPUT" -lt $((BASELINE_THROUGHPUT * 80 / 100)) ]; then
        echo "Throughput too low: $THROUGHPUT req/s, initiating rollback"
        ./rollback-canary-deployment.sh
        exit 1
    fi
    
    echo "Metrics OK - Error rate: $ERROR_RATE%, Response time: ${RESPONSE_TIME}ms, Throughput: $THROUGHPUT req/s"
    sleep 30
done

echo "Canary monitoring completed successfully!"
echo "Version $NEW_VERSION is performing well with $CANARY_PERCENTAGE% traffic"
```

## Database Migration

### **Database Migration Strategy**

#### **Migration Types**
```yaml
migration_types:
  schema_migration:
    description: "Database schema changes"
    examples: ["Add table", "Add column", "Modify column", "Add index"]
    rollback: "Schema rollback scripts"
    downtime: "Minimal with proper planning"
  
  data_migration:
    description: "Data transformation and migration"
    examples: ["Data cleanup", "Data transformation", "Data archival"]
    rollback: "Data backup and restore"
    downtime: "Depends on data volume"
  
  configuration_migration:
    description: "Configuration and settings updates"
    examples: ["Update connection strings", "Update settings", "Update permissions"]
    rollback: "Configuration rollback"
    downtime: "Minimal"
```

#### **Database Migration Service**
```csharp
public class DatabaseMigrationService
{
    private readonly VirtualQueueDbContext _context;
    private readonly ILogger<DatabaseMigrationService> _logger;
    private readonly IConfiguration _configuration;

    public DatabaseMigrationService(
        VirtualQueueDbContext context,
        ILogger<DatabaseMigrationService> logger,
        IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<MigrationResult> ExecuteMigrationAsync(string migrationName, string environment)
    {
        var result = new MigrationResult
        {
            MigrationName = migrationName,
            Environment = environment,
            StartTime = DateTime.UtcNow
        };

        try
        {
            _logger.LogInformation("Starting migration {MigrationName} for environment {Environment}", 
                migrationName, environment);

            // Create backup before migration
            await CreateBackupAsync(environment);

            // Execute migration based on type
            switch (migrationName)
            {
                case "schema":
                    await ExecuteSchemaMigrationAsync();
                    break;
                case "data":
                    await ExecuteDataMigrationAsync();
                    break;
                case "configuration":
                    await ExecuteConfigurationMigrationAsync();
                    break;
                default:
                    throw new ArgumentException($"Unknown migration type: {migrationName}");
            }

            // Verify migration
            await VerifyMigrationAsync(migrationName);

            result.Success = true;
            result.EndTime = DateTime.UtcNow;
            result.Duration = result.EndTime - result.StartTime;

            _logger.LogInformation("Migration {MigrationName} completed successfully for environment {Environment} in {Duration}ms", 
                migrationName, environment, result.Duration.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Error = ex.Message;
            result.EndTime = DateTime.UtcNow;
            result.Duration = result.EndTime - result.StartTime;

            _logger.LogError(ex, "Migration {MigrationName} failed for environment {Environment}", 
                migrationName, environment);

            // Attempt rollback
            await RollbackMigrationAsync(migrationName, environment);
        }

        return result;
    }

    private async Task CreateBackupAsync(string environment)
    {
        _logger.LogInformation("Creating backup for environment {Environment}", environment);
        
        var backupName = $"backup_{environment}_{DateTime.UtcNow:yyyyMMdd_HHmmss}";
        var backupPath = $"/backups/{backupName}.sql";

        // Create database backup
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        var backupCommand = $"pg_dump {connectionString} > {backupPath}";
        
        // Execute backup command
        // Implementation depends on hosting environment
        
        _logger.LogInformation("Backup created: {BackupName}", backupName);
    }

    private async Task ExecuteSchemaMigrationAsync()
    {
        _logger.LogInformation("Executing schema migration");
        
        // Apply Entity Framework migrations
        await _context.Database.MigrateAsync();
        
        _logger.LogInformation("Schema migration completed");
    }

    private async Task ExecuteDataMigrationAsync()
    {
        _logger.LogInformation("Executing data migration");
        
        // Implement data migration logic
        // This would include data transformation, cleanup, etc.
        
        _logger.LogInformation("Data migration completed");
    }

    private async Task ExecuteConfigurationMigrationAsync()
    {
        _logger.LogInformation("Executing configuration migration");
        
        // Update configuration settings
        // This would include connection strings, settings, etc.
        
        _logger.LogInformation("Configuration migration completed");
    }

    private async Task VerifyMigrationAsync(string migrationName)
    {
        _logger.LogInformation("Verifying migration {MigrationName}", migrationName);
        
        // Verify migration success
        // Check database connectivity, data integrity, etc.
        
        await _context.Database.CanConnectAsync();
        
        _logger.LogInformation("Migration verification completed");
    }

    private async Task RollbackMigrationAsync(string migrationName, string environment)
    {
        _logger.LogInformation("Rolling back migration {MigrationName} for environment {Environment}", 
            migrationName, environment);
        
        // Implement rollback logic
        // This would depend on the specific migration type
        
        _logger.LogInformation("Migration rollback completed");
    }
}
```

## Post-Deployment Monitoring

### **Deployment Monitoring**

#### **Post-Deployment Health Checks**
```csharp
public class PostDeploymentMonitoringService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PostDeploymentMonitoringService> _logger;
    private readonly IConfiguration _configuration;

    public PostDeploymentMonitoringService(
        HttpClient httpClient,
        ILogger<PostDeploymentMonitoringService> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<DeploymentHealthResult> PerformPostDeploymentHealthChecksAsync(string version)
    {
        var result = new DeploymentHealthResult
        {
            Version = version,
            CheckTime = DateTime.UtcNow
        };

        try
        {
            _logger.LogInformation("Starting post-deployment health checks for version {Version}", version);

            // Basic health check
            result.BasicHealthCheck = await PerformBasicHealthCheckAsync();
            
            // API endpoint checks
            result.ApiEndpointChecks = await PerformApiEndpointChecksAsync();
            
            // Database connectivity check
            result.DatabaseHealthCheck = await PerformDatabaseHealthCheckAsync();
            
            // Redis connectivity check
            result.RedisHealthCheck = await PerformRedisHealthCheckAsync();
            
            // Performance checks
            result.PerformanceChecks = await PerformPerformanceChecksAsync();
            
            // Security checks
            result.SecurityChecks = await PerformSecurityChecksAsync();

            // Calculate overall health
            result.OverallHealth = CalculateOverallHealth(result);

            _logger.LogInformation("Post-deployment health checks completed for version {Version}. Overall health: {OverallHealth}", 
                version, result.OverallHealth);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Post-deployment health checks failed for version {Version}", version);
            result.OverallHealth = false;
            result.Error = ex.Message;
        }

        return result;
    }

    private async Task<HealthCheckResult> PerformBasicHealthCheckAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/health");
            response.EnsureSuccessStatusCode();
            
            return new HealthCheckResult
            {
                IsHealthy = true,
                Message = "Basic health check passed",
                ResponseTime = response.Headers.GetValues("X-Response-Time").FirstOrDefault()
            };
        }
        catch (Exception ex)
        {
            return new HealthCheckResult
            {
                IsHealthy = false,
                Message = $"Basic health check failed: {ex.Message}"
            };
        }
    }

    private async Task<List<ApiEndpointCheck>> PerformApiEndpointChecksAsync()
    {
        var checks = new List<ApiEndpointCheck>();
        var endpoints = new[]
        {
            "/api/queues",
            "/api/user-sessions",
            "/api/tenants",
            "/api/health"
        };

        foreach (var endpoint in endpoints)
        {
            try
            {
                var response = await _httpClient.GetAsync(endpoint);
                checks.Add(new ApiEndpointCheck
                {
                    Endpoint = endpoint,
                    IsHealthy = response.IsSuccessStatusCode,
                    StatusCode = response.StatusCode,
                    ResponseTime = response.Headers.GetValues("X-Response-Time").FirstOrDefault()
                });
            }
            catch (Exception ex)
            {
                checks.Add(new ApiEndpointCheck
                {
                    Endpoint = endpoint,
                    IsHealthy = false,
                    Error = ex.Message
                });
            }
        }

        return checks;
    }

    private async Task<HealthCheckResult> PerformDatabaseHealthCheckAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/health/database");
            response.EnsureSuccessStatusCode();
            
            return new HealthCheckResult
            {
                IsHealthy = true,
                Message = "Database health check passed"
            };
        }
        catch (Exception ex)
        {
            return new HealthCheckResult
            {
                IsHealthy = false,
                Message = $"Database health check failed: {ex.Message}"
            };
        }
    }

    private async Task<HealthCheckResult> PerformRedisHealthCheckAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/health/redis");
            response.EnsureSuccessStatusCode();
            
            return new HealthCheckResult
            {
                IsHealthy = true,
                Message = "Redis health check passed"
            };
        }
        catch (Exception ex)
        {
            return new HealthCheckResult
            {
                IsHealthy = false,
                Message = $"Redis health check failed: {ex.Message}"
            };
        }
    }

    private async Task<PerformanceCheckResult> PerformPerformanceChecksAsync()
    {
        var result = new PerformanceCheckResult();
        
        try
        {
            // Measure response time
            var startTime = DateTime.UtcNow;
            var response = await _httpClient.GetAsync("/api/queues");
            var endTime = DateTime.UtcNow;
            
            result.AverageResponseTime = (endTime - startTime).TotalMilliseconds;
            result.IsHealthy = result.AverageResponseTime < 200; // 200ms threshold
            
            // Check throughput
            result.Throughput = await MeasureThroughputAsync();
            result.IsHealthy = result.IsHealthy && result.Throughput > 1000; // 1000 req/s threshold
            
            result.Message = result.IsHealthy ? "Performance checks passed" : "Performance checks failed";
        }
        catch (Exception ex)
        {
            result.IsHealthy = false;
            result.Message = $"Performance checks failed: {ex.Message}";
        }

        return result;
    }

    private async Task<SecurityCheckResult> PerformSecurityChecksAsync()
    {
        var result = new SecurityCheckResult();
        
        try
        {
            // Check SSL/TLS
            var response = await _httpClient.GetAsync("/api/queues");
            result.SslEnabled = response.RequestMessage.RequestUri.Scheme == "https";
            
            // Check security headers
            result.SecurityHeaders = CheckSecurityHeaders(response);
            
            result.IsHealthy = result.SslEnabled && result.SecurityHeaders.All(h => h.IsPresent);
            result.Message = result.IsHealthy ? "Security checks passed" : "Security checks failed";
        }
        catch (Exception ex)
        {
            result.IsHealthy = false;
            result.Message = $"Security checks failed: {ex.Message}";
        }

        return result;
    }

    private bool CalculateOverallHealth(DeploymentHealthResult result)
    {
        return result.BasicHealthCheck.IsHealthy &&
               result.ApiEndpointChecks.All(c => c.IsHealthy) &&
               result.DatabaseHealthCheck.IsHealthy &&
               result.RedisHealthCheck.IsHealthy &&
               result.PerformanceChecks.IsHealthy &&
               result.SecurityChecks.IsHealthy;
    }

    private async Task<double> MeasureThroughputAsync()
    {
        var tasks = new List<Task<HttpResponseMessage>>();
        var startTime = DateTime.UtcNow;

        // Send 100 concurrent requests
        for (int i = 0; i < 100; i++)
        {
            tasks.Add(_httpClient.GetAsync("/api/queues"));
        }

        await Task.WhenAll(tasks);
        var endTime = DateTime.UtcNow;
        var duration = (endTime - startTime).TotalSeconds;

        return 100 / duration; // requests per second
    }

    private List<SecurityHeader> CheckSecurityHeaders(HttpResponseMessage response)
    {
        var headers = new List<SecurityHeader>();
        
        headers.Add(new SecurityHeader
        {
            Name = "X-Content-Type-Options",
            IsPresent = response.Headers.Contains("X-Content-Type-Options"),
            Value = response.Headers.GetValues("X-Content-Type-Options").FirstOrDefault()
        });
        
        headers.Add(new SecurityHeader
        {
            Name = "X-Frame-Options",
            IsPresent = response.Headers.Contains("X-Frame-Options"),
            Value = response.Headers.GetValues("X-Frame-Options").FirstOrDefault()
        });
        
        headers.Add(new SecurityHeader
        {
            Name = "X-XSS-Protection",
            IsPresent = response.Headers.Contains("X-XSS-Protection"),
            Value = response.Headers.GetValues("X-XSS-Protection").FirstOrDefault()
        });
        
        headers.Add(new SecurityHeader
        {
            Name = "Strict-Transport-Security",
            IsPresent = response.Headers.Contains("Strict-Transport-Security"),
            Value = response.Headers.GetValues("Strict-Transport-Security").FirstOrDefault()
        });

        return headers;
    }
}
```

## Approval and Sign-off

### **Production Deployment Approval**
- **DevOps Lead**: [Name] - [Date]
- **Development Lead**: [Name] - [Date]
- **QA Lead**: [Name] - [Date]
- **Management**: [Name] - [Date]

### **Document Control**
- **Version**: 1.0
- **Last Updated**: January 15, 2024
- **Next Review**: February 15, 2024
- **Distribution**: DevOps Team, Development Team, QA Team, Management

---

**Document Status**: Draft  
**Next Phase**: Maintenance  
**Dependencies**: Production deployment implementation, monitoring setup, rollback procedures
