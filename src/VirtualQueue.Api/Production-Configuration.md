# Virtual Queue Management System - Production Environment Configuration

## Environment Variables

### Database Configuration
```bash
# PostgreSQL Database
export DB_HOST=your-db-host.com
export DB_PORT=5432
export DB_NAME=virtualqueue_prod
export DB_USER=virtualqueue_user
export DB_PASSWORD=your_secure_database_password

# Redis Cache
export REDIS_CONNECTION_STRING=your-redis-host.com:6379
```

### JWT Configuration
```bash
# JWT Security (Generate a strong secret key)
export JWT_SECRET_KEY=YourSuperSecretKeyThatIsAtLeast32CharactersLongForProduction!
export JWT_ISSUER=VirtualQueue-Production
export JWT_AUDIENCE=VirtualQueueUsers-Production
export JWT_EXPIRATION_MINUTES=60
```

### Email Configuration
```bash
# SMTP Settings
export SMTP_SERVER=smtp.gmail.com
export SMTP_PORT=587
export SMTP_USERNAME=your-email@gmail.com
export SMTP_PASSWORD=your-app-password
export FROM_EMAIL=noreply@virtualqueue.com
export FROM_NAME="Virtual Queue System"
```

### SMS Configuration
```bash
# SMS Provider (Twilio, AWS SNS, etc.)
export SMS_PROVIDER=Twilio
export SMS_API_KEY=your-sms-api-key
export SMS_API_SECRET=your-sms-api-secret
export SMS_FROM_NUMBER=+1234567890
```

### WhatsApp Configuration
```bash
# WhatsApp Provider (Twilio, etc.)
export WHATSAPP_PROVIDER=Twilio
export WHATSAPP_API_KEY=your-whatsapp-api-key
export WHATSAPP_API_SECRET=your-whatsapp-api-secret
export WHATSAPP_FROM_NUMBER=+1234567890
```

### Rate Limiting
```bash
# Rate Limiting Settings
export RATE_LIMIT_PER_MINUTE=100
export RATE_LIMIT_BURST=200
```

### Security Configuration
```bash
# CORS Origins
export ALLOWED_ORIGINS=https://yourdomain.com,https://app.yourdomain.com

# API Security
export API_KEY_HEADER_NAME=X-Tenant-Key
export JWT_HEADER_NAME=Authorization
```

### Backup Configuration
```bash
# Backup Settings
export BACKUP_STORAGE_PROVIDER=AWS_S3
export BACKUP_STORAGE_PATH=s3://your-backup-bucket/virtualqueue
```

## Docker Environment File (.env)

Create a `.env` file in your project root:

```bash
# Database
DB_HOST=postgres
DB_PORT=5432
DB_NAME=virtualqueue_prod
DB_USER=virtualqueue_user
DB_PASSWORD=secure_password_123

# Redis
REDIS_CONNECTION_STRING=redis:6379

# JWT
JWT_SECRET_KEY=YourSuperSecretKeyThatIsAtLeast32CharactersLongForProduction!
JWT_ISSUER=VirtualQueue-Production
JWT_AUDIENCE=VirtualQueueUsers-Production
JWT_EXPIRATION_MINUTES=60

# Email
SMTP_SERVER=smtp.gmail.com
SMTP_PORT=587
SMTP_USERNAME=your-email@gmail.com
SMTP_PASSWORD=your-app-password
FROM_EMAIL=noreply@virtualqueue.com
FROM_NAME=Virtual Queue System

# SMS
SMS_PROVIDER=Twilio
SMS_API_KEY=your-sms-api-key
SMS_API_SECRET=your-sms-api-secret
SMS_FROM_NUMBER=+1234567890

# WhatsApp
WHATSAPP_PROVIDER=Twilio
WHATSAPP_API_KEY=your-whatsapp-api-key
WHATSAPP_API_SECRET=your-whatsapp-api-secret
WHATSAPP_FROM_NUMBER=+1234567890

# Rate Limiting
RATE_LIMIT_PER_MINUTE=100
RATE_LIMIT_BURST=200

# Security
ALLOWED_ORIGINS=https://yourdomain.com,https://app.yourdomain.com

# Backup
BACKUP_STORAGE_PROVIDER=AWS_S3
BACKUP_STORAGE_PATH=s3://your-backup-bucket/virtualqueue
```

## Kubernetes ConfigMap

Create a Kubernetes ConfigMap for production:

```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: virtualqueue-config
  namespace: production
data:
  appsettings.Production.json: |
    {
      "Logging": {
        "LogLevel": {
          "Default": "Information",
          "Microsoft.AspNetCore": "Warning"
        }
      },
      "ConnectionStrings": {
        "DefaultConnection": "Host=postgres-service;Port=5432;Database=virtualqueue_prod;Username=virtualqueue_user;Password=secure_password_123;SSL Mode=Require"
      },
      "Jwt": {
        "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLongForProduction!",
        "Issuer": "VirtualQueue-Production",
        "Audience": "VirtualQueueUsers-Production",
        "ExpirationMinutes": "60"
      }
    }
```

## Kubernetes Secret

Create a Kubernetes Secret for sensitive data:

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: virtualqueue-secrets
  namespace: production
type: Opaque
data:
  DB_PASSWORD: c2VjdXJlX3Bhc3N3b3JkXzEyMw==  # base64 encoded
  JWT_SECRET_KEY: WW91clN1cGVyU2VjcmV0S2V5VGhhdElzQXRMZWFzdDMyQ2hhcmFjdGVyc0xvbmdGb3JQcm9kdWN0aW9uIQ==  # base64 encoded
  SMTP_PASSWORD: eW91ci1hcHAtcGFzc3dvcmQ=  # base64 encoded
  SMS_API_SECRET: eW91ci1zbXMtYXBpLXNlY3JldA==  # base64 encoded
  WHATSAPP_API_SECRET: eW91ci13aGF0c2FwcC1hcGktc2VjcmV0  # base64 encoded
```

## Azure App Service Configuration

For Azure App Service, configure these application settings:

```bash
# Database
DB_HOST=your-azure-postgres-server.postgres.database.azure.com
DB_PORT=5432
DB_NAME=virtualqueue_prod
DB_USER=virtualqueue_user@your-azure-postgres-server
DB_PASSWORD=your_secure_password

# Redis
REDIS_CONNECTION_STRING=your-azure-redis.redis.cache.windows.net:6380,password=your-redis-key,ssl=True

# JWT
JWT_SECRET_KEY=YourSuperSecretKeyThatIsAtLeast32CharactersLongForProduction!
JWT_ISSUER=VirtualQueue-Production
JWT_AUDIENCE=VirtualQueueUsers-Production
JWT_EXPIRATION_MINUTES=60

# Email
SMTP_SERVER=smtp.gmail.com
SMTP_PORT=587
SMTP_USERNAME=your-email@gmail.com
SMTP_PASSWORD=your-app-password
FROM_EMAIL=noreply@virtualqueue.com
FROM_NAME=Virtual Queue System

# SMS
SMS_PROVIDER=Twilio
SMS_API_KEY=your-sms-api-key
SMS_API_SECRET=your-sms-api-secret
SMS_FROM_NUMBER=+1234567890

# Security
ALLOWED_ORIGINS=https://yourdomain.com,https://app.yourdomain.com
```

## AWS ECS Task Definition

For AWS ECS, configure environment variables in your task definition:

```json
{
  "environment": [
    {
      "name": "DB_HOST",
      "value": "your-rds-endpoint.region.rds.amazonaws.com"
    },
    {
      "name": "DB_PORT",
      "value": "5432"
    },
    {
      "name": "DB_NAME",
      "value": "virtualqueue_prod"
    },
    {
      "name": "DB_USER",
      "value": "virtualqueue_user"
    },
    {
      "name": "REDIS_CONNECTION_STRING",
      "value": "your-elasticache-endpoint.cache.amazonaws.com:6379"
    },
    {
      "name": "JWT_SECRET_KEY",
      "value": "YourSuperSecretKeyThatIsAtLeast32CharactersLongForProduction!"
    },
    {
      "name": "JWT_ISSUER",
      "value": "VirtualQueue-Production"
    },
    {
      "name": "JWT_AUDIENCE",
      "value": "VirtualQueueUsers-Production"
    },
    {
      "name": "JWT_EXPIRATION_MINUTES",
      "value": "60"
    }
  ],
  "secrets": [
    {
      "name": "DB_PASSWORD",
      "valueFrom": "arn:aws:secretsmanager:region:account:secret:virtualqueue/db-password"
    },
    {
      "name": "SMTP_PASSWORD",
      "valueFrom": "arn:aws:secretsmanager:region:account:secret:virtualqueue/smtp-password"
    },
    {
      "name": "SMS_API_SECRET",
      "valueFrom": "arn:aws:secretsmanager:region:account:secret:virtualqueue/sms-api-secret"
    }
  ]
}
```

## Security Best Practices

### 1. Secret Management
- Use Azure Key Vault, AWS Secrets Manager, or HashiCorp Vault
- Never commit secrets to version control
- Rotate secrets regularly
- Use least privilege access

### 2. Database Security
- Use SSL/TLS connections
- Enable connection pooling
- Use strong passwords
- Regular security updates

### 3. JWT Security
- Use strong secret keys (at least 32 characters)
- Set appropriate expiration times
- Implement token blacklisting
- Use HTTPS in production

### 4. Rate Limiting
- Implement per-tenant rate limiting
- Use Redis for distributed rate limiting
- Monitor and alert on rate limit violations
- Implement graceful degradation

### 5. Monitoring
- Enable comprehensive logging
- Set up health checks
- Monitor performance metrics
- Implement alerting

## Deployment Checklist

- [ ] Database migrations applied
- [ ] Environment variables configured
- [ ] Secrets properly managed
- [ ] SSL certificates installed
- [ ] Health checks configured
- [ ] Monitoring enabled
- [ ] Backup strategy implemented
- [ ] Security headers configured
- [ ] Rate limiting enabled
- [ ] Logging configured
- [ ] Performance monitoring active
- [ ] Alerting rules set up
