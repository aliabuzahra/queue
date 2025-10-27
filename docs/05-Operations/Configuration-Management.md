# Configuration Management - Virtual Queue Management System

**Document Version:** 1.0  
**Date:** January 15, 2024  
**Author:** DevOps Engineer  
**Status:** Draft  
**Phase:** 05 - Operations  
**Priority:** 🟡 Medium  

---

## Configuration Management Overview

This document outlines the comprehensive configuration management strategy for the Virtual Queue Management System. It covers configuration versioning, environment-specific configurations, configuration validation, and automated configuration deployment procedures.

## Configuration Management Strategy

### **Configuration Principles**
- **Version Control**: All configurations stored in version control
- **Environment Separation**: Separate configurations for each environment
- **Validation**: Configuration validation before deployment
- **Automation**: Automated configuration deployment
- **Documentation**: Comprehensive configuration documentation
- **Security**: Secure handling of sensitive configuration data

### **Configuration Types**

| Configuration Type | Description | Examples | Management |
|-------------------|-------------|----------|------------|
| **Application Config** | Application-specific settings | Database connections, API keys | JSON/YAML files |
| **Infrastructure Config** | Infrastructure settings | Server specs, network config | Terraform/CloudFormation |
| **Environment Config** | Environment-specific settings | URLs, feature flags | Environment variables |
| **Security Config** | Security-related settings | Certificates, secrets | Secret management |
| **Monitoring Config** | Monitoring and logging settings | Metrics, alerts | Configuration files |

## Configuration Structure

### **Configuration Hierarchy**

```
config/
├── base/                    # Base configurations
│   ├── appsettings.json
│   ├── logging.json
│   └── monitoring.json
├── environments/            # Environment-specific configs
│   ├── development/
│   ├── testing/
│   ├── staging/
│   └── production/
├── secrets/                # Secret configurations
│   ├── development/
│   ├── testing/
│   ├── staging/
│   └── production/
└── templates/              # Configuration templates
    ├── appsettings.template.json
    └── docker-compose.template.yml
```

### **Base Configuration**

#### **Application Settings Template**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "{{DB_CONNECTION_STRING}}"
  },
  "Redis": {
    "ConnectionString": "{{REDIS_CONNECTION_STRING}}",
    "Password": "{{REDIS_PASSWORD}}"
  },
  "Jwt": {
    "Secret": "{{JWT_SECRET}}",
    "Issuer": "{{JWT_ISSUER}}",
    "Audience": "{{JWT_AUDIENCE}}",
    "ExpiryMinutes": "{{JWT_EXPIRY_MINUTES}}"
  },
  "ExternalServices": {
    "EmailService": {
      "Url": "{{EMAIL_SERVICE_URL}}",
      "ApiKey": "{{EMAIL_API_KEY}}"
    },
    "SmsService": {
      "Url": "{{SMS_SERVICE_URL}}",
      "ApiKey": "{{SMS_API_KEY}}"
    }
  },
  "Monitoring": {
    "Prometheus": {
      "Endpoint": "{{PROMETHEUS_ENDPOINT}}"
    },
    "Grafana": {
      "Endpoint": "{{GRAFANA_ENDPOINT}}"
    }
  }
}
```

## Environment-Specific Configurations

### **Development Environment**

#### **Development Configuration**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft": "Information"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=VirtualQueueDev;Username=postgres;Password=devpassword"
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  },
  "Jwt": {
    "Secret": "dev-secret-key",
    "Issuer": "VirtualQueueDev",
    "Audience": "VirtualQueueDevUsers",
    "ExpiryMinutes": 60
  },
  "ExternalServices": {
    "EmailService": {
      "Url": "http://localhost:5001",
      "ApiKey": "dev-email-key"
    }
  }
}
```

### **Testing Environment**

#### **Testing Configuration**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=test-db;Database=VirtualQueueTest;Username=testuser;Password=testpassword"
  },
  "Redis": {
    "ConnectionString": "test-redis:6379"
  },
  "Jwt": {
    "Secret": "test-secret-key",
    "Issuer": "VirtualQueueTest",
    "Audience": "VirtualQueueTestUsers",
    "ExpiryMinutes": 30
  },
  "ExternalServices": {
    "EmailService": {
      "Url": "http://test-email-service:5001",
      "ApiKey": "test-email-key"
    }
  }
}
```

### **Staging Environment**

#### **Staging Configuration**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=staging-db;Database=VirtualQueueStaging;Username=staginguser;Password={{STAGING_DB_PASSWORD}}"
  },
  "Redis": {
    "ConnectionString": "staging-redis:6379",
    "Password": "{{STAGING_REDIS_PASSWORD}}"
  },
  "Jwt": {
    "Secret": "{{STAGING_JWT_SECRET}}",
    "Issuer": "VirtualQueueStaging",
    "Audience": "VirtualQueueStagingUsers",
    "ExpiryMinutes": 60
  },
  "ExternalServices": {
    "EmailService": {
      "Url": "{{STAGING_EMAIL_SERVICE_URL}}",
      "ApiKey": "{{STAGING_EMAIL_API_KEY}}"
    }
  }
}
```

### **Production Environment**

#### **Production Configuration**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft": "Error"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host={{PROD_DB_HOST}};Database={{PROD_DB_NAME}};Username={{PROD_DB_USER}};Password={{PROD_DB_PASSWORD}}"
  },
  "Redis": {
    "ConnectionString": "{{PROD_REDIS_HOST}}:{{PROD_REDIS_PORT}}",
    "Password": "{{PROD_REDIS_PASSWORD}}"
  },
  "Jwt": {
    "Secret": "{{PROD_JWT_SECRET}}",
    "Issuer": "{{PROD_JWT_ISSUER}}",
    "Audience": "{{PROD_JWT_AUDIENCE}}",
    "ExpiryMinutes": "{{PROD_JWT_EXPIRY_MINUTES}}"
  },
  "ExternalServices": {
    "EmailService": {
      "Url": "{{PROD_EMAIL_SERVICE_URL}}",
      "ApiKey": "{{PROD_EMAIL_API_KEY}}"
    },
    "SmsService": {
      "Url": "{{PROD_SMS_SERVICE_URL}}",
      "ApiKey": "{{PROD_SMS_API_KEY}}"
    }
  }
}
```

## Configuration Validation

### **Configuration Validation Script**

```bash
#!/bin/bash
# config-validation.sh

set -e

echo "Configuration Validation"
echo "========================"

CONFIG_FILE=$1
ENVIRONMENT=$2

if [ -z "$CONFIG_FILE" ] || [ -z "$ENVIRONMENT" ]; then
    echo "Usage: $0 <config_file> <environment>"
    exit 1
fi

echo "Validating configuration: $CONFIG_FILE for environment: $ENVIRONMENT"

# 1. JSON Syntax Validation
echo "1. Validating JSON syntax..."
if ! jq empty "$CONFIG_FILE" 2>/dev/null; then
    echo "❌ Invalid JSON syntax"
    exit 1
fi
echo "✅ JSON syntax valid"

# 2. Required Fields Validation
echo "2. Validating required fields..."
REQUIRED_FIELDS=("ConnectionStrings.DefaultConnection" "Redis.ConnectionString" "Jwt.Secret" "Jwt.Issuer" "Jwt.Audience")

for field in "${REQUIRED_FIELDS[@]}"; do
    if ! jq -e ".$field" "$CONFIG_FILE" > /dev/null 2>&1; then
        echo "❌ Required field missing: $field"
        exit 1
    fi
done
echo "✅ Required fields present"

# 3. Environment-Specific Validation
echo "3. Validating environment-specific settings..."
case $ENVIRONMENT in
    "development")
        # Development-specific validations
        if jq -e '.Logging.LogLevel.Default' "$CONFIG_FILE" | grep -q "Debug"; then
            echo "✅ Development logging level correct"
        else
            echo "⚠️ Development logging level should be Debug"
        fi
        ;;
    "testing")
        # Testing-specific validations
        if jq -e '.Jwt.ExpiryMinutes' "$CONFIG_FILE" | grep -q "30"; then
            echo "✅ Testing JWT expiry correct"
        else
            echo "⚠️ Testing JWT expiry should be 30 minutes"
        fi
        ;;
    "staging")
        # Staging-specific validations
        if jq -e '.ConnectionStrings.DefaultConnection' "$CONFIG_FILE" | grep -q "staging"; then
            echo "✅ Staging database connection correct"
        else
            echo "❌ Staging database connection incorrect"
            exit 1
        fi
        ;;
    "production")
        # Production-specific validations
        if jq -e '.Logging.LogLevel.Default' "$CONFIG_FILE" | grep -q "Warning"; then
            echo "✅ Production logging level correct"
        else
            echo "❌ Production logging level should be Warning"
            exit 1
        fi
        ;;
esac

# 4. Security Validation
echo "4. Validating security settings..."
if jq -e '.Jwt.Secret' "$CONFIG_FILE" | grep -q "{{.*}}"; then
    echo "✅ JWT secret uses template variable"
else
    echo "⚠️ JWT secret should use template variable"
fi

# 5. Connection String Validation
echo "5. Validating connection strings..."
DB_CONNECTION=$(jq -r '.ConnectionStrings.DefaultConnection' "$CONFIG_FILE")
if [[ $DB_CONNECTION == *"{{"*"}}"* ]]; then
    echo "✅ Database connection uses template variables"
else
    echo "⚠️ Database connection should use template variables"
fi

echo "✅ Configuration validation completed successfully"
```

### **Configuration Schema Validation**

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "type": "object",
  "required": [
    "Logging",
    "ConnectionStrings",
    "Redis",
    "Jwt",
    "ExternalServices"
  ],
  "properties": {
    "Logging": {
      "type": "object",
      "required": ["LogLevel"],
      "properties": {
        "LogLevel": {
          "type": "object",
          "required": ["Default"],
          "properties": {
            "Default": {
              "type": "string",
              "enum": ["Debug", "Information", "Warning", "Error"]
            }
          }
        }
      }
    },
    "ConnectionStrings": {
      "type": "object",
      "required": ["DefaultConnection"],
      "properties": {
        "DefaultConnection": {
          "type": "string",
          "minLength": 10
        }
      }
    },
    "Redis": {
      "type": "object",
      "required": ["ConnectionString"],
      "properties": {
        "ConnectionString": {
          "type": "string",
          "minLength": 5
        },
        "Password": {
          "type": "string"
        }
      }
    },
    "Jwt": {
      "type": "object",
      "required": ["Secret", "Issuer", "Audience", "ExpiryMinutes"],
      "properties": {
        "Secret": {
          "type": "string",
          "minLength": 16
        },
        "Issuer": {
          "type": "string",
          "minLength": 1
        },
        "Audience": {
          "type": "string",
          "minLength": 1
        },
        "ExpiryMinutes": {
          "type": "integer",
          "minimum": 1,
          "maximum": 1440
        }
      }
    }
  }
}
```

## Configuration Deployment

### **Automated Configuration Deployment**

```bash
#!/bin/bash
# deploy-config.sh

set -e

ENVIRONMENT=$1
CONFIG_VERSION=$2

if [ -z "$ENVIRONMENT" ] || [ -z "$CONFIG_VERSION" ]; then
    echo "Usage: $0 <environment> <config_version>"
    exit 1
fi

echo "Deploying configuration for environment: $ENVIRONMENT"
echo "Configuration version: $CONFIG_VERSION"

# 1. Validate configuration
echo "1. Validating configuration..."
CONFIG_FILE="config/environments/$ENVIRONMENT/appsettings.json"
./config-validation.sh "$CONFIG_FILE" "$ENVIRONMENT"

# 2. Backup current configuration
echo "2. Backing up current configuration..."
BACKUP_DIR="/var/backups/config/$ENVIRONMENT/$(date +%Y%m%d_%H%M%S)"
mkdir -p "$BACKUP_DIR"
cp "$CONFIG_FILE" "$BACKUP_DIR/appsettings.json.backup"

# 3. Deploy new configuration
echo "3. Deploying new configuration..."
if [ "$ENVIRONMENT" = "production" ]; then
    # Production deployment with zero downtime
    docker-compose -f docker-compose.prod.yml exec api-prod cp /app/config/appsettings.json /app/config/appsettings.json.backup
    docker-compose -f docker-compose.prod.yml exec api-prod cp /app/config/environments/production/appsettings.json /app/config/appsettings.json
    docker-compose -f docker-compose.prod.yml restart api-prod
else
    # Non-production deployment
    docker-compose -f "docker-compose.$ENVIRONMENT.yml" restart api
fi

# 4. Verify deployment
echo "4. Verifying deployment..."
sleep 30
if curl -f "http://localhost:80/health"; then
    echo "✅ Configuration deployment successful"
else
    echo "❌ Configuration deployment failed"
    # Rollback
    echo "Rolling back configuration..."
    docker-compose -f "docker-compose.$ENVIRONMENT.yml" exec api cp /app/config/appsettings.json.backup /app/config/appsettings.json
    docker-compose -f "docker-compose.$ENVIRONMENT.yml" restart api
    exit 1
fi

# 5. Update configuration version
echo "5. Updating configuration version..."
echo "$CONFIG_VERSION" > "/var/config/$ENVIRONMENT/version.txt"

echo "Configuration deployment completed successfully"
```

## Secret Management

### **Secret Management Strategy**

#### **Secret Types**
- **Database Passwords**: Database connection passwords
- **API Keys**: External service API keys
- **JWT Secrets**: JWT signing secrets
- **SSL Certificates**: SSL/TLS certificates
- **Encryption Keys**: Data encryption keys

#### **Secret Management Script**

```bash
#!/bin/bash
# secret-management.sh

SECRET_NAME=$1
SECRET_VALUE=$2
ENVIRONMENT=$3
ACTION=$4

if [ -z "$SECRET_NAME" ] || [ -z "$ENVIRONMENT" ] || [ -z "$ACTION" ]; then
    echo "Usage: $0 <secret_name> <secret_value> <environment> <action>"
    echo "Actions: set, get, delete, list"
    exit 1
fi

SECRET_FILE="/var/secrets/$ENVIRONMENT/$SECRET_NAME"

case $ACTION in
    "set")
        if [ -z "$SECRET_VALUE" ]; then
            echo "Secret value required for set action"
            exit 1
        fi
        echo "Setting secret: $SECRET_NAME for environment: $ENVIRONMENT"
        mkdir -p "/var/secrets/$ENVIRONMENT"
        echo "$SECRET_VALUE" | gpg --symmetric --cipher-algo AES256 --output "$SECRET_FILE.gpg"
        echo "✅ Secret set successfully"
        ;;
    "get")
        echo "Getting secret: $SECRET_NAME for environment: $ENVIRONMENT"
        if [ -f "$SECRET_FILE.gpg" ]; then
            gpg --decrypt "$SECRET_FILE.gpg" 2>/dev/null
        else
            echo "❌ Secret not found"
            exit 1
        fi
        ;;
    "delete")
        echo "Deleting secret: $SECRET_NAME for environment: $ENVIRONMENT"
        if [ -f "$SECRET_FILE.gpg" ]; then
            rm "$SECRET_FILE.gpg"
            echo "✅ Secret deleted successfully"
        else
            echo "❌ Secret not found"
            exit 1
        fi
        ;;
    "list")
        echo "Listing secrets for environment: $ENVIRONMENT"
        if [ -d "/var/secrets/$ENVIRONMENT" ]; then
            ls -la "/var/secrets/$ENVIRONMENT" | grep "\.gpg$" | awk '{print $9}' | sed 's/\.gpg$//'
        else
            echo "No secrets found"
        fi
        ;;
    *)
        echo "Invalid action: $ACTION"
        exit 1
        ;;
esac
```

## Configuration Monitoring

### **Configuration Monitoring Script**

```bash
#!/bin/bash
# config-monitoring.sh

echo "Configuration Monitoring Report"
echo "==============================="

# 1. Configuration File Status
echo "1. Configuration File Status:"
for env in development testing staging production; do
    CONFIG_FILE="config/environments/$env/appsettings.json"
    if [ -f "$CONFIG_FILE" ]; then
        echo "✅ $env: Configuration file exists"
        # Check file age
        FILE_AGE=$(stat -c %Y "$CONFIG_FILE")
        CURRENT_TIME=$(date +%s)
        AGE_DAYS=$(( (CURRENT_TIME - FILE_AGE) / 86400 ))
        echo "   Last modified: $AGE_DAYS days ago"
    else
        echo "❌ $env: Configuration file missing"
    fi
done

# 2. Secret Status
echo "2. Secret Status:"
for env in development testing staging production; do
    SECRET_DIR="/var/secrets/$env"
    if [ -d "$SECRET_DIR" ]; then
        SECRET_COUNT=$(ls -1 "$SECRET_DIR"/*.gpg 2>/dev/null | wc -l)
        echo "✅ $env: $SECRET_COUNT secrets configured"
    else
        echo "❌ $env: No secrets directory"
    fi
done

# 3. Configuration Validation
echo "3. Configuration Validation:"
for env in development testing staging production; do
    CONFIG_FILE="config/environments/$env/appsettings.json"
    if [ -f "$CONFIG_FILE" ]; then
        if ./config-validation.sh "$CONFIG_FILE" "$env" > /dev/null 2>&1; then
            echo "✅ $env: Configuration valid"
        else
            echo "❌ $env: Configuration invalid"
        fi
    fi
done

# 4. Configuration Drift Detection
echo "4. Configuration Drift Detection:"
BASE_CONFIG="config/base/appsettings.json"
for env in development testing staging production; do
    ENV_CONFIG="config/environments/$env/appsettings.json"
    if [ -f "$BASE_CONFIG" ] && [ -f "$ENV_CONFIG" ]; then
        # Compare configurations (simplified)
        DIFF_COUNT=$(diff "$BASE_CONFIG" "$ENV_CONFIG" | wc -l)
        echo "✅ $env: $DIFF_COUNT differences from base configuration"
    fi
done

echo "Configuration monitoring completed"
```

## Configuration Backup and Recovery

### **Configuration Backup Script**

```bash
#!/bin/bash
# config-backup.sh

BACKUP_DIR="/var/backups/config/$(date +%Y%m%d_%H%M%S)"
mkdir -p "$BACKUP_DIR"

echo "Backing up configurations to: $BACKUP_DIR"

# 1. Backup configuration files
echo "1. Backing up configuration files..."
cp -r config/ "$BACKUP_DIR/"

# 2. Backup secrets
echo "2. Backing up secrets..."
for env in development testing staging production; do
    if [ -d "/var/secrets/$env" ]; then
        mkdir -p "$BACKUP_DIR/secrets/$env"
        cp -r "/var/secrets/$env"/* "$BACKUP_DIR/secrets/$env/"
    fi
done

# 3. Backup environment variables
echo "3. Backing up environment variables..."
env > "$BACKUP_DIR/environment_variables.txt"

# 4. Create backup manifest
echo "4. Creating backup manifest..."
cat > "$BACKUP_DIR/manifest.txt" << EOF
Configuration Backup
===================
Date: $(date)
Backup Directory: $BACKUP_DIR
Files Backed Up:
- Configuration files: $(find config/ -type f | wc -l)
- Secret files: $(find "$BACKUP_DIR/secrets" -name "*.gpg" 2>/dev/null | wc -l)
- Environment variables: $(wc -l < "$BACKUP_DIR/environment_variables.txt")
EOF

# 5. Compress backup
echo "5. Compressing backup..."
tar -czf "$BACKUP_DIR.tar.gz" -C "$(dirname "$BACKUP_DIR")" "$(basename "$BACKUP_DIR")"
rm -rf "$BACKUP_DIR"

echo "✅ Configuration backup completed: $BACKUP_DIR.tar.gz"
```

### **Configuration Recovery Script**

```bash
#!/bin/bash
# config-recovery.sh

BACKUP_FILE=$1

if [ -z "$BACKUP_FILE" ]; then
    echo "Usage: $0 <backup_file>"
    exit 1
fi

if [ ! -f "$BACKUP_FILE" ]; then
    echo "Backup file not found: $BACKUP_FILE"
    exit 1
fi

echo "Recovering configurations from: $BACKUP_FILE"

# 1. Extract backup
echo "1. Extracting backup..."
BACKUP_DIR="/tmp/config_recovery_$(date +%Y%m%d_%H%M%S)"
mkdir -p "$BACKUP_DIR"
tar -xzf "$BACKUP_FILE" -C "$BACKUP_DIR"

# 2. Restore configuration files
echo "2. Restoring configuration files..."
cp -r "$BACKUP_DIR"/*/config/ ./

# 3. Restore secrets
echo "3. Restoring secrets..."
if [ -d "$BACKUP_DIR"/*/secrets ]; then
    cp -r "$BACKUP_DIR"/*/secrets/* /var/secrets/
fi

# 4. Verify restoration
echo "4. Verifying restoration..."
for env in development testing staging production; do
    CONFIG_FILE="config/environments/$env/appsettings.json"
    if [ -f "$CONFIG_FILE" ]; then
        echo "✅ $env: Configuration restored"
    else
        echo "❌ $env: Configuration not restored"
    fi
done

# 5. Cleanup
echo "5. Cleaning up..."
rm -rf "$BACKUP_DIR"

echo "✅ Configuration recovery completed"
```

## Configuration Best Practices

### **Configuration Management Best Practices**

1. **Version Control**: Store all configurations in version control
2. **Environment Separation**: Use separate configurations for each environment
3. **Template Variables**: Use template variables for sensitive data
4. **Validation**: Validate configurations before deployment
5. **Documentation**: Document all configuration options
6. **Security**: Secure handling of sensitive configuration data
7. **Monitoring**: Monitor configuration changes and drift
8. **Backup**: Regular backup of configurations and secrets
9. **Automation**: Automate configuration deployment
10. **Testing**: Test configuration changes in non-production environments

### **Configuration Security Guidelines**

1. **Secret Management**: Use dedicated secret management tools
2. **Access Control**: Implement proper access controls for configurations
3. **Encryption**: Encrypt sensitive configuration data
4. **Audit Logging**: Log all configuration changes
5. **Rotation**: Regularly rotate secrets and certificates
6. **Least Privilege**: Apply least privilege principle to configuration access
7. **Network Security**: Secure configuration transmission
8. **Compliance**: Ensure compliance with security standards

## Approval and Sign-off

### **Configuration Management Approval**
- **DevOps Engineer**: [Name] - [Date]
- **Security Lead**: [Name] - [Date]
- **Technical Lead**: [Name] - [Date]
- **Operations Manager**: [Name] - [Date]

### **Document Control**
- **Version**: 1.0
- **Last Updated**: January 15, 2024
- **Next Review**: February 15, 2024
- **Distribution**: DevOps Team, Operations Team, Development Team

---

**Document Status**: Draft  
**Next Phase**: Performance Tuning  
**Dependencies**: Configuration validation, secret management setup
