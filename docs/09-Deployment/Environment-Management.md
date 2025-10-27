# Environment Management - Virtual Queue Management System

**Document Version:** 1.0  
**Date:** January 15, 2024  
**Author:** DevOps Lead  
**Status:** Draft  
**Phase:** 9 - Deployment  
**Priority:** 🔴 Critical  

---

## Executive Summary

This document provides comprehensive environment management guidelines for the Virtual Queue Management System. It covers environment provisioning, configuration management, environment promotion, and environment monitoring to ensure consistent and reliable environments across the development lifecycle.

## Environment Management Overview

### **Environment Management Objectives**

#### **Primary Objectives**
- **Environment Consistency**: Maintain consistency across all environments
- **Configuration Management**: Centralize and manage environment configurations
- **Environment Provisioning**: Automate environment creation and setup
- **Environment Promotion**: Streamline promotion between environments
- **Environment Monitoring**: Monitor environment health and performance

#### **Environment Management Benefits**
- **Reduced Risk**: Minimize environment-related risks
- **Faster Delivery**: Accelerate environment setup and promotion
- **Quality Assurance**: Ensure environment quality and consistency
- **Operational Efficiency**: Simplify environment operations
- **Cost Optimization**: Optimize environment costs and resource usage

### **Environment Strategy**

#### **Environment Types**
- **Development**: Local development environments
- **Integration**: Continuous integration testing
- **Staging**: Pre-production testing and validation
- **Production**: Live production environment
- **Disaster Recovery**: Backup and recovery environment

#### **Environment Lifecycle**
```yaml
environment_lifecycle:
  provisioning:
    - "Infrastructure setup"
    - "Application deployment"
    - "Configuration setup"
    - "Data seeding"
    - "Health verification"
  
  configuration:
    - "Environment-specific settings"
    - "Secret management"
    - "Feature flags"
    - "Monitoring configuration"
    - "Security settings"
  
  promotion:
    - "Code promotion"
    - "Configuration promotion"
    - "Database migration"
    - "Health checks"
    - "Rollback procedures"
  
  monitoring:
    - "Health monitoring"
    - "Performance monitoring"
    - "Security monitoring"
    - "Cost monitoring"
    - "Alert management"
  
  decommissioning:
    - "Data backup"
    - "Resource cleanup"
    - "Cost optimization"
    - "Documentation update"
```

## Environment Provisioning

### **Infrastructure as Code**

#### **Terraform Configuration**
```hcl
# environments/terraform/main.tf
terraform {
  required_version = ">= 1.0"
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.0"
    }
  }
}

provider "azurerm" {
  features {}
}

# Resource Group
resource "azurerm_resource_group" "main" {
  name     = "virtualqueue-${var.environment}-rg"
  location = var.location

  tags = {
    Environment = var.environment
    Project     = "VirtualQueue"
    ManagedBy   = "Terraform"
  }
}

# Virtual Network
resource "azurerm_virtual_network" "main" {
  name                = "virtualqueue-${var.environment}-vnet"
  address_space       = ["10.0.0.0/16"]
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name

  tags = {
    Environment = var.environment
  }
}

# Subnets
resource "azurerm_subnet" "web" {
  name                 = "web-subnet"
  resource_group_name  = azurerm_resource_group.main.name
  virtual_network_name = azurerm_virtual_network.main.name
  address_prefixes     = ["10.0.1.0/24"]
}

resource "azurerm_subnet" "app" {
  name                 = "app-subnet"
  resource_group_name  = azurerm_resource_group.main.name
  virtual_network_name = azurerm_virtual_network.main.name
  address_prefixes     = ["10.0.2.0/24"]
}

resource "azurerm_subnet" "data" {
  name                 = "data-subnet"
  resource_group_name  = azurerm_resource_group.main.name
  virtual_network_name = azurerm_virtual_network.main.name
  address_prefixes     = ["10.0.3.0/24"]
}

# App Service Plan
resource "azurerm_service_plan" "main" {
  name                = "virtualqueue-${var.environment}-plan"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  os_type             = "Linux"
  sku_name            = var.app_service_sku

  tags = {
    Environment = var.environment
  }
}

# App Service for API
resource "azurerm_linux_web_app" "api" {
  name                = "virtualqueue-${var.environment}-api"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_service_plan.main.location
  service_plan_id     = azurerm_service_plan.main.id

  site_config {
    application_stack {
      dotnet_version = "8.0"
    }
    always_on = var.environment == "production" ? true : false
  }

  app_settings = {
    "ASPNETCORE_ENVIRONMENT" = var.environment
    "ConnectionStrings__DefaultConnection" = azurerm_postgresql_flexible_server.main.connection_string
    "Redis__ConnectionString" = azurerm_redis_cache.main.primary_connection_string
  }

  tags = {
    Environment = var.environment
  }
}

# App Service for Worker
resource "azurerm_linux_web_app" "worker" {
  name                = "virtualqueue-${var.environment}-worker"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_service_plan.main.location
  service_plan_id     = azurerm_service_plan.main.id

  site_config {
    application_stack {
      dotnet_version = "8.0"
    }
    always_on = var.environment == "production" ? true : false
  }

  app_settings = {
    "ASPNETCORE_ENVIRONMENT" = var.environment
    "ConnectionStrings__DefaultConnection" = azurerm_postgresql_flexible_server.main.connection_string
    "Redis__ConnectionString" = azurerm_redis_cache.main.primary_connection_string
  }

  tags = {
    Environment = var.environment
  }
}

# PostgreSQL Database
resource "azurerm_postgresql_flexible_server" "main" {
  name                   = "virtualqueue-${var.environment}-db"
  resource_group_name    = azurerm_resource_group.main.name
  location               = azurerm_resource_group.main.location
  version                = "15"
  administrator_login    = "postgres"
  administrator_password  = var.db_password
  zone                   = "1"
  storage_mb             = var.db_storage_mb
  sku_name               = var.db_sku_name

  backup_retention_days  = var.environment == "production" ? 30 : 7
  geo_redundant_backup_enabled = var.environment == "production" ? true : false

  tags = {
    Environment = var.environment
  }
}

resource "azurerm_postgresql_flexible_server_database" "main" {
  name      = "virtualqueue"
  server_id = azurerm_postgresql_flexible_server.main.id
  collation = "en_US.utf8"
  charset   = "utf8"
}

# Redis Cache
resource "azurerm_redis_cache" "main" {
  name                = "virtualqueue-${var.environment}-redis"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  capacity            = var.redis_capacity
  family              = var.redis_family
  sku_name            = var.redis_sku_name
  enable_non_ssl_port = false
  minimum_tls_version = "1.2"

  redis_configuration {
    maxmemory_policy = "allkeys-lru"
  }

  tags = {
    Environment = var.environment
  }
}
```

#### **Environment Variables**
```hcl
# environments/terraform/variables.tf
variable "environment" {
  description = "Environment name (development, staging, production)"
  type        = string
  validation {
    condition     = contains(["development", "staging", "production"], var.environment)
    error_message = "Environment must be one of: development, staging, production."
  }
}

variable "location" {
  description = "Azure region"
  type        = string
  default     = "East US"
}

variable "app_service_sku" {
  description = "App Service SKU"
  type        = string
  default     = "P1V2"
}

variable "db_sku_name" {
  description = "Database SKU name"
  type        = string
  default     = "GP_Standard_D2s_v3"
}

variable "db_storage_mb" {
  description = "Database storage in MB"
  type        = number
  default     = 32768
}

variable "redis_sku_name" {
  description = "Redis SKU name"
  type        = string
  default     = "Premium"
}

variable "redis_capacity" {
  description = "Redis capacity"
  type        = number
  default     = 1
}

variable "redis_family" {
  description = "Redis family"
  type        = string
  default     = "P"
}

variable "db_password" {
  description = "Database password"
  type        = string
  sensitive   = true
}
```

### **Environment Provisioning Scripts**

#### **Provisioning Script**
```bash
#!/bin/bash
# provision-environment.sh

set -e

ENVIRONMENT=$1
LOCATION=${2:-"eastus"}

if [ -z "$ENVIRONMENT" ]; then
    echo "Usage: $0 <environment> [location]"
    echo "Environments: development, staging, production"
    exit 1
fi

echo "Provisioning environment: $ENVIRONMENT in $LOCATION"

# Initialize Terraform
cd environments/terraform
terraform init

# Plan infrastructure
terraform plan \
  -var="environment=$ENVIRONMENT" \
  -var="location=$LOCATION" \
  -var="db_password=$DB_PASSWORD" \
  -out="terraform-$ENVIRONMENT.tfplan"

# Apply infrastructure
terraform apply "terraform-$ENVIRONMENT.tfplan"

# Get outputs
API_URL=$(terraform output -raw api_url)
WORKER_URL=$(terraform output -raw worker_url)
DB_CONNECTION=$(terraform output -raw db_connection)
REDIS_CONNECTION=$(terraform output -raw redis_connection)

echo "Environment provisioned successfully!"
echo "API URL: $API_URL"
echo "Worker URL: $WORKER_URL"
echo "DB Connection: $DB_CONNECTION"
echo "Redis Connection: $REDIS_CONNECTION"

# Deploy application
echo "Deploying application..."
az webapp deployment source config-zip \
  --name "virtualqueue-$ENVIRONMENT-api" \
  --resource-group "virtualqueue-$ENVIRONMENT-rg" \
  --src "../artifacts/VirtualQueue.Api.zip"

az webapp deployment source config-zip \
  --name "virtualqueue-$ENVIRONMENT-worker" \
  --resource-group "virtualqueue-$ENVIRONMENT-rg" \
  --src "../artifacts/VirtualQueue.Worker.zip"

# Run database migrations
echo "Running database migrations..."
az postgres flexible-server execute \
  --name "virtualqueue-$ENVIRONMENT-db" \
  --admin-user postgres \
  --admin-password "$DB_PASSWORD" \
  --database virtualqueue \
  --queryfile "../migrations.sql"

# Health check
echo "Performing health check..."
sleep 30
curl -f "$API_URL/health" || exit 1

echo "Environment $ENVIRONMENT provisioned and deployed successfully!"
```

## Configuration Management

### **Configuration Strategy**

#### **Configuration Hierarchy**
```yaml
configuration_hierarchy:
  base_configuration:
    - "Common settings across all environments"
    - "Default values and fallbacks"
    - "Application structure"
  
  environment_configuration:
    - "Environment-specific settings"
    - "Connection strings"
    - "Feature flags"
    - "Logging levels"
  
  deployment_configuration:
    - "Deployment-specific settings"
    - "Resource limits"
    - "Scaling parameters"
    - "Monitoring configuration"
  
  runtime_configuration:
    - "Runtime environment variables"
    - "Secret values"
    - "Dynamic configuration"
    - "User-specific settings"
```

#### **Configuration Management Service**
```csharp
public class ConfigurationManagementService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ConfigurationManagementService> _logger;

    public ConfigurationManagementService(IConfiguration configuration, ILogger<ConfigurationManagementService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<EnvironmentConfiguration> GetEnvironmentConfigurationAsync(string environment)
    {
        var config = new EnvironmentConfiguration
        {
            Environment = environment,
            DatabaseConnection = _configuration.GetConnectionString("DefaultConnection"),
            RedisConnection = _configuration["Redis:ConnectionString"],
            JwtSettings = new JwtSettings
            {
                SecretKey = _configuration["JWT:SecretKey"],
                Issuer = _configuration["JWT:Issuer"],
                Audience = _configuration["JWT:Audience"],
                ExpiryMinutes = int.Parse(_configuration["JWT:ExpiryMinutes"] ?? "60")
            },
            FeatureFlags = await GetFeatureFlagsAsync(environment),
            MonitoringSettings = GetMonitoringSettings(environment)
        };

        return config;
    }

    private async Task<Dictionary<string, bool>> GetFeatureFlagsAsync(string environment)
    {
        var featureFlags = new Dictionary<string, bool>();
        
        // Load feature flags from configuration
        var flagsSection = _configuration.GetSection("FeatureFlags");
        foreach (var flag in flagsSection.GetChildren())
        {
            featureFlags[flag.Key] = flag.GetValue<bool>();
        }

        // Override with environment-specific flags
        var envFlagsSection = _configuration.GetSection($"FeatureFlags:{environment}");
        foreach (var flag in envFlagsSection.GetChildren())
        {
            featureFlags[flag.Key] = flag.GetValue<bool>();
        }

        return featureFlags;
    }

    private MonitoringSettings GetMonitoringSettings(string environment)
    {
        return new MonitoringSettings
        {
            EnableMetrics = _configuration.GetValue<bool>($"Monitoring:{environment}:EnableMetrics", true),
            EnableLogging = _configuration.GetValue<bool>($"Monitoring:{environment}:EnableLogging", true),
            LogLevel = _configuration.GetValue<string>($"Monitoring:{environment}:LogLevel", "Information"),
            MetricsEndpoint = _configuration.GetValue<string>($"Monitoring:{environment}:MetricsEndpoint", "/metrics"),
            HealthCheckEndpoint = _configuration.GetValue<string>($"Monitoring:{environment}:HealthCheckEndpoint", "/health")
        };
    }
}
```

### **Secret Management**

#### **Azure Key Vault Integration**
```csharp
public class SecretManagementService
{
    private readonly SecretClient _secretClient;
    private readonly ILogger<SecretManagementService> _logger;

    public SecretManagementService(SecretClient secretClient, ILogger<SecretManagementService> logger)
    {
        _secretClient = secretClient;
        _logger = logger;
    }

    public async Task<string> GetSecretAsync(string secretName, string environment)
    {
        try
        {
            var secret = await _secretClient.GetSecretAsync($"{environment}-{secretName}");
            return secret.Value.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve secret {SecretName} for environment {Environment}", secretName, environment);
            throw;
        }
    }

    public async Task SetSecretAsync(string secretName, string secretValue, string environment)
    {
        try
        {
            await _secretClient.SetSecretAsync($"{environment}-{secretName}", secretValue);
            _logger.LogInformation("Secret {SecretName} set for environment {Environment}", secretName, environment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set secret {SecretName} for environment {Environment}", secretName, environment);
            throw;
        }
    }

    public async Task<Dictionary<string, string>> GetAllSecretsAsync(string environment)
    {
        var secrets = new Dictionary<string, string>();
        
        try
        {
            await foreach (var secret in _secretClient.GetPropertiesOfSecretsAsync())
            {
                if (secret.Name.StartsWith($"{environment}-"))
                {
                    var secretValue = await _secretClient.GetSecretAsync(secret.Name);
                    var secretName = secret.Name.Substring($"{environment}-".Length);
                    secrets[secretName] = secretValue.Value.Value;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve secrets for environment {Environment}", environment);
            throw;
        }

        return secrets;
    }
}
```

## Environment Promotion

### **Promotion Strategy**

#### **Promotion Pipeline**
```yaml
promotion_pipeline:
  development_to_integration:
    trigger: "Code push to develop branch"
    steps:
      - "Run integration tests"
      - "Deploy to integration environment"
      - "Run smoke tests"
      - "Notify team"
  
  integration_to_staging:
    trigger: "Integration tests pass"
    steps:
      - "Run full test suite"
      - "Deploy to staging environment"
      - "Run user acceptance tests"
      - "Performance testing"
      - "Security scanning"
  
  staging_to_production:
    trigger: "Manual approval"
    steps:
      - "Final validation"
      - "Deploy to production"
      - "Database migration"
      - "Health checks"
      - "Monitoring setup"
      - "Rollback preparation"
```

#### **Promotion Script**
```bash
#!/bin/bash
# promote-environment.sh

set -e

SOURCE_ENV=$1
TARGET_ENV=$2

if [ -z "$SOURCE_ENV" ] || [ -z "$TARGET_ENV" ]; then
    echo "Usage: $0 <source_environment> <target_environment>"
    echo "Example: $0 staging production"
    exit 1
fi

echo "Promoting from $SOURCE_ENV to $TARGET_ENV"

# Validate source environment
echo "Validating source environment: $SOURCE_ENV"
curl -f "https://virtualqueue-$SOURCE_ENV-api.azurewebsites.net/health" || exit 1

# Get source environment configuration
echo "Getting source environment configuration..."
SOURCE_CONFIG=$(az webapp config appsettings list \
  --name "virtualqueue-$SOURCE_ENV-api" \
  --resource-group "virtualqueue-$SOURCE_ENV-rg" \
  --query "[].{name:name, value:value}" \
  --output json)

# Deploy to target environment
echo "Deploying to target environment: $TARGET_ENV"
az webapp deployment source config-zip \
  --name "virtualqueue-$TARGET_ENV-api" \
  --resource-group "virtualqueue-$TARGET_ENV-rg" \
  --src "../artifacts/VirtualQueue.Api.zip"

az webapp deployment source config-zip \
  --name "virtualqueue-$TARGET_ENV-worker" \
  --resource-group "virtualqueue-$TARGET_ENV-rg" \
  --src "../artifacts/VirtualQueue.Worker.zip"

# Update target environment configuration
echo "Updating target environment configuration..."
az webapp config appsettings set \
  --name "virtualqueue-$TARGET_ENV-api" \
  --resource-group "virtualqueue-$TARGET_ENV-rg" \
  --settings @<(echo "$SOURCE_CONFIG" | jq -r '.[] | "\(.name)=\(.value)"')

# Run database migrations
echo "Running database migrations..."
az postgres flexible-server execute \
  --name "virtualqueue-$TARGET_ENV-db" \
  --admin-user postgres \
  --admin-password "$DB_PASSWORD" \
  --database virtualqueue \
  --queryfile "../migrations.sql"

# Health check
echo "Performing health check..."
sleep 30
curl -f "https://virtualqueue-$TARGET_ENV-api.azurewebsites.net/health" || exit 1

# Notify team
echo "Sending notification..."
curl -X POST "$SLACK_WEBHOOK_URL" \
  -H "Content-Type: application/json" \
  -d "{\"text\":\"Environment promoted from $SOURCE_ENV to $TARGET_ENV successfully!\"}"

echo "Promotion from $SOURCE_ENV to $TARGET_ENV completed successfully!"
```

### **Database Migration Management**

#### **Migration Service**
```csharp
public class DatabaseMigrationService
{
    private readonly VirtualQueueDbContext _context;
    private readonly ILogger<DatabaseMigrationService> _logger;

    public DatabaseMigrationService(VirtualQueueDbContext context, ILogger<DatabaseMigrationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<MigrationResult> MigrateDatabaseAsync(string environment)
    {
        var result = new MigrationResult
        {
            Environment = environment,
            StartTime = DateTime.UtcNow
        };

        try
        {
            _logger.LogInformation("Starting database migration for environment: {Environment}", environment);

            // Check if database exists
            if (!await _context.Database.CanConnectAsync())
            {
                _logger.LogInformation("Database does not exist, creating...");
                await _context.Database.EnsureCreatedAsync();
                result.Created = true;
            }
            else
            {
                // Apply migrations
                _logger.LogInformation("Applying database migrations...");
                await _context.Database.MigrateAsync();
                result.Migrated = true;
            }

            // Seed data for non-production environments
            if (environment != "production")
            {
                await SeedDataAsync(environment);
                result.Seeded = true;
            }

            result.Success = true;
            result.EndTime = DateTime.UtcNow;
            result.Duration = result.EndTime - result.StartTime;

            _logger.LogInformation("Database migration completed successfully for environment: {Environment} in {Duration}ms", 
                environment, result.Duration.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Error = ex.Message;
            result.EndTime = DateTime.UtcNow;
            result.Duration = result.EndTime - result.StartTime;

            _logger.LogError(ex, "Database migration failed for environment: {Environment}", environment);
        }

        return result;
    }

    private async Task SeedDataAsync(string environment)
    {
        _logger.LogInformation("Seeding data for environment: {Environment}", environment);

        // Seed tenants
        if (!await _context.Tenants.AnyAsync())
        {
            var tenants = new List<Tenant>
            {
                new Tenant($"Test Tenant 1 - {environment}", $"test1-{environment}.com"),
                new Tenant($"Test Tenant 2 - {environment}", $"test2-{environment}.com")
            };

            _context.Tenants.AddRange(tenants);
            await _context.SaveChangesAsync();
        }

        // Seed queues
        if (!await _context.Queues.AnyAsync())
        {
            var tenant = await _context.Tenants.FirstAsync();
            var queues = new List<Queue>
            {
                new Queue(tenant.Id, $"Test Queue 1 - {environment}", $"Test queue for {environment}", 100, 10),
                new Queue(tenant.Id, $"Test Queue 2 - {environment}", $"Another test queue for {environment}", 150, 15)
            };

            _context.Queues.AddRange(queues);
            await _context.SaveChangesAsync();
        }

        _logger.LogInformation("Data seeding completed for environment: {Environment}", environment);
    }
}
```

## Environment Monitoring

### **Environment Health Monitoring**

#### **Health Check Service**
```csharp
public class EnvironmentHealthService
{
    private readonly VirtualQueueDbContext _context;
    private readonly IConnectionMultiplexer _redis;
    private readonly HttpClient _httpClient;
    private readonly ILogger<EnvironmentHealthService> _logger;

    public EnvironmentHealthService(
        VirtualQueueDbContext context,
        IConnectionMultiplexer redis,
        HttpClient httpClient,
        ILogger<EnvironmentHealthService> logger)
    {
        _context = context;
        _redis = redis;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<EnvironmentHealth> CheckEnvironmentHealthAsync(string environment)
    {
        var health = new EnvironmentHealth
        {
            Environment = environment,
            CheckTime = DateTime.UtcNow
        };

        // Check database connectivity
        health.DatabaseHealth = await CheckDatabaseHealthAsync();
        
        // Check Redis connectivity
        health.RedisHealth = await CheckRedisHealthAsync();
        
        // Check application health
        health.ApplicationHealth = await CheckApplicationHealthAsync();
        
        // Check external dependencies
        health.ExternalDependenciesHealth = await CheckExternalDependenciesHealthAsync();

        // Calculate overall health
        health.OverallHealth = CalculateOverallHealth(health);

        return health;
    }

    private async Task<HealthStatus> CheckDatabaseHealthAsync()
    {
        try
        {
            await _context.Database.CanConnectAsync();
            return new HealthStatus { IsHealthy = true, Message = "Database connection successful" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            return new HealthStatus { IsHealthy = false, Message = ex.Message };
        }
    }

    private async Task<HealthStatus> CheckRedisHealthAsync()
    {
        try
        {
            var db = _redis.GetDatabase();
            await db.PingAsync();
            return new HealthStatus { IsHealthy = true, Message = "Redis connection successful" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis health check failed");
            return new HealthStatus { IsHealthy = false, Message = ex.Message };
        }
    }

    private async Task<HealthStatus> CheckApplicationHealthAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/health");
            response.EnsureSuccessStatusCode();
            return new HealthStatus { IsHealthy = true, Message = "Application health check successful" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Application health check failed");
            return new HealthStatus { IsHealthy = false, Message = ex.Message };
        }
    }

    private async Task<HealthStatus> CheckExternalDependenciesHealthAsync()
    {
        try
        {
            // Check external services
            var tasks = new List<Task<HttpResponseMessage>>();
            
            // Add external service health checks here
            // tasks.Add(_httpClient.GetAsync("https://external-service.com/health"));
            
            if (tasks.Any())
            {
                await Task.WhenAll(tasks);
            }
            
            return new HealthStatus { IsHealthy = true, Message = "External dependencies healthy" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "External dependencies health check failed");
            return new HealthStatus { IsHealthy = false, Message = ex.Message };
        }
    }

    private bool CalculateOverallHealth(EnvironmentHealth health)
    {
        return health.DatabaseHealth.IsHealthy &&
               health.RedisHealth.IsHealthy &&
               health.ApplicationHealth.IsHealthy &&
               health.ExternalDependenciesHealth.IsHealthy;
    }
}
```

### **Environment Metrics**

#### **Environment Metrics Service**
```csharp
public class EnvironmentMetricsService
{
    private readonly Counter _environmentHealthCounter;
    private readonly Gauge _environmentUptime;
    private readonly Histogram _environmentResponseTime;

    public EnvironmentMetricsService()
    {
        _environmentHealthCounter = Metrics.CreateCounter("environment_health_checks_total", "Total environment health checks", new[] { "environment", "status" });
        _environmentUptime = Metrics.CreateGauge("environment_uptime_seconds", "Environment uptime in seconds", new[] { "environment" });
        _environmentResponseTime = Metrics.CreateHistogram("environment_response_time_seconds", "Environment response time", new[] { "environment" });
    }

    public void RecordEnvironmentHealth(string environment, bool isHealthy, double responseTime)
    {
        _environmentHealthCounter.WithLabels(environment, isHealthy ? "healthy" : "unhealthy").Inc();
        _environmentResponseTime.WithLabels(environment).Observe(responseTime);
        
        if (isHealthy)
        {
            _environmentUptime.WithLabels(environment).SetToCurrentTime();
        }
    }

    public EnvironmentMetrics GetEnvironmentMetrics(string environment)
    {
        return new EnvironmentMetrics
        {
            Environment = environment,
            HealthChecksTotal = _environmentHealthCounter.WithLabels(environment, "healthy").Value + 
                               _environmentHealthCounter.WithLabels(environment, "unhealthy").Value,
            HealthChecksPassed = _environmentHealthCounter.WithLabels(environment, "healthy").Value,
            HealthChecksFailed = _environmentHealthCounter.WithLabels(environment, "unhealthy").Value,
            AverageResponseTime = _environmentResponseTime.WithLabels(environment).Value,
            Uptime = _environmentUptime.WithLabels(environment).Value
        };
    }
}
```

## Approval and Sign-off

### **Environment Management Approval**
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
**Next Phase**: Production Deployment  
**Dependencies**: Environment provisioning, configuration management, monitoring setup
