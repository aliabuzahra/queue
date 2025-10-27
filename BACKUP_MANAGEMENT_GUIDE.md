# Virtual Queue Management System - Backup Management Guide

## Overview

The Virtual Queue Management System includes a comprehensive automated backup system that provides:

- **Database Backup**: PostgreSQL database with full schema and data
- **Redis Backup**: Cache data and session information
- **Configuration Backup**: Application settings and configuration files
- **Logs Backup**: Application logs for troubleshooting
- **Automated Scheduling**: Daily backups with configurable retention
- **Multiple Storage Options**: Local, AWS S3, Azure Blob, Google Cloud Storage
- **Backup Verification**: Checksums and integrity verification
- **Restoration Capabilities**: Complete system restoration from backups

## Backup Components

### 1. Database Backup
- **Format**: SQL dump (plain text)
- **Content**: Complete database schema and data
- **Frequency**: Daily at 2:00 AM
- **Retention**: 30 days (configurable)

### 2. Redis Backup
- **Format**: RDB file
- **Content**: Cache data, sessions, rate limiting data
- **Frequency**: Daily at 2:00 AM
- **Retention**: 30 days (configurable)

### 3. Configuration Backup
- **Format**: Compressed tar archive
- **Content**: Application configuration files
- **Frequency**: Daily at 2:00 AM
- **Retention**: 30 days (configurable)

### 4. Logs Backup
- **Format**: Compressed tar archive
- **Content**: Application logs (last 7 days)
- **Frequency**: Daily at 2:00 AM
- **Retention**: 7 days (configurable)

## Storage Options

### Local Storage
```bash
BACKUP_STORAGE_PROVIDER=Local
BACKUP_STORAGE_PATH=/backups
```

### AWS S3
```bash
BACKUP_STORAGE_PROVIDER=AWS_S3
BACKUP_STORAGE_PATH=s3://your-bucket-name/backups
AWS_ACCESS_KEY_ID=your-access-key
AWS_SECRET_ACCESS_KEY=your-secret-key
AWS_REGION=us-east-1
```

### Azure Blob Storage
```bash
BACKUP_STORAGE_PROVIDER=Azure_Blob
BACKUP_STORAGE_PATH=https://yourstorageaccount.blob.core.windows.net/backups
AZURE_STORAGE_ACCOUNT=your-storage-account
AZURE_STORAGE_KEY=your-storage-key
```

### Google Cloud Storage
```bash
BACKUP_STORAGE_PROVIDER=Google_Cloud
BACKUP_STORAGE_PATH=gs://your-bucket-name/backups
GOOGLE_APPLICATION_CREDENTIALS=/path/to/service-account.json
GOOGLE_CLOUD_PROJECT=your-project-id
```

## Configuration

### Environment Variables
```bash
# Backup Schedule (Cron format)
BACKUP_SCHEDULE="0 2 * * *"  # Daily at 2 AM

# Retention Settings
BACKUP_RETENTION_DAYS=30
BACKUP_LOGS_RETENTION_DAYS=7

# Storage Configuration
BACKUP_STORAGE_PROVIDER=Local
BACKUP_STORAGE_PATH=/backups
COMPRESSION_ENABLED=true

# Notification Settings
NOTIFICATION_EMAIL=admin@virtualqueue.com
BACKUP_ALERT_ON_FAILURE=true
BACKUP_ALERT_EMAIL=alerts@virtualqueue.com

# Database Configuration
DB_HOST=postgres
DB_PORT=5432
DB_NAME=virtualqueue_prod
DB_USER=virtualqueue_user
DB_PASSWORD=your-password

# Redis Configuration
REDIS_HOST=redis
REDIS_PORT=6379
REDIS_PASSWORD=your-password
```

## Manual Backup Operations

### Create Manual Backup
```bash
# Run backup script manually
docker exec backup-service /scripts/backup.sh

# Or run locally
./scripts/backup.sh
```

### List Available Backups
```bash
# List backup files
ls -la /backups/

# List S3 backups
aws s3 ls s3://your-bucket-name/backups/

# List Azure backups
az storage blob list --container-name backups --output table
```

### Restore from Backup
```bash
# Restore from local backup
docker exec backup-service /scripts/restore.sh /backups/virtualqueue_backup_20240115_020000.tar.gz

# Restore with force (no confirmation)
docker exec backup-service /scripts/restore.sh /backups/virtualqueue_backup_20240115_020000.tar.gz --force
```

## Backup Verification

### Automatic Verification
- **Checksums**: MD5 checksums for all backup files
- **Size Validation**: File size verification
- **Integrity Check**: Database connection test after restore

### Manual Verification
```bash
# Verify backup file integrity
tar -tzf /backups/virtualqueue_backup_20240115_020000.tar.gz

# Check backup manifest
cat /backups/virtualqueue_backup_20240115_020000_manifest.json

# Verify database backup
head -20 /backups/virtualqueue_backup_20240115_020000_database.sql
```

## Monitoring and Alerting

### Backup Monitoring
- **Success/Failure Tracking**: Backup status monitoring
- **Size Monitoring**: Backup size trends
- **Duration Monitoring**: Backup completion time
- **Storage Usage**: Disk space monitoring

### Alerting
- **Backup Failures**: Email alerts on backup failures
- **Storage Issues**: Alerts when storage is full
- **Retention Issues**: Alerts when backups are not cleaned up

### Grafana Dashboards
- **Backup Status**: Current backup status
- **Backup History**: Historical backup information
- **Storage Usage**: Storage utilization metrics
- **Backup Performance**: Backup duration and size trends

## Disaster Recovery

### Recovery Procedures

#### 1. Complete System Recovery
```bash
# Stop all services
docker-compose down

# Restore from backup
docker exec backup-service /scripts/restore.sh /backups/latest_backup.tar.gz --force

# Start services
docker-compose up -d
```

#### 2. Database-Only Recovery
```bash
# Restore only database
PGPASSWORD=password psql -h postgres -U virtualqueue_user -d virtualqueue_prod -f /backups/database.sql
```

#### 3. Redis-Only Recovery
```bash
# Restore only Redis
cp /backups/redis.rdb /var/lib/redis/dump.rdb
docker restart redis
```

### Recovery Testing
```bash
# Test backup restoration
docker exec backup-service /scripts/restore.sh /backups/test_backup.tar.gz --force

# Verify system functionality
curl http://localhost:8080/healthz
```

## Best Practices

### 1. Backup Strategy
- **Daily Backups**: Automated daily backups
- **Weekly Full Backups**: Complete system backups
- **Monthly Archive**: Long-term backup storage
- **Offsite Storage**: Cloud storage for disaster recovery

### 2. Security
- **Encryption**: Encrypt backups before storage
- **Access Control**: Limit backup access to authorized personnel
- **Audit Logging**: Log all backup operations
- **Secure Storage**: Use secure storage providers

### 3. Testing
- **Regular Testing**: Test backup restoration monthly
- **Recovery Drills**: Practice disaster recovery procedures
- **Documentation**: Maintain recovery procedures documentation
- **Training**: Train staff on backup and recovery procedures

### 4. Monitoring
- **Backup Status**: Monitor backup success/failure
- **Storage Usage**: Monitor storage utilization
- **Performance**: Monitor backup performance
- **Alerts**: Set up appropriate alerting

## Troubleshooting

### Common Issues

#### Backup Failures
```bash
# Check backup logs
docker logs backup-service

# Check disk space
df -h /backups

# Check database connectivity
docker exec postgres pg_isready -U virtualqueue_user -d virtualqueue_prod
```

#### Storage Issues
```bash
# Check S3 connectivity
aws s3 ls s3://your-bucket-name/

# Check Azure connectivity
az storage account show --name your-storage-account

# Check Google Cloud connectivity
gsutil ls gs://your-bucket-name/
```

#### Restoration Issues
```bash
# Check backup file integrity
tar -tzf /backups/backup_file.tar.gz

# Check database permissions
docker exec postgres psql -U virtualqueue_user -d virtualqueue_prod -c "\dt"

# Check Redis permissions
docker exec redis redis-cli -a password PING
```

## Maintenance

### Regular Maintenance Tasks
- **Cleanup Old Backups**: Remove backups older than retention period
- **Verify Backup Integrity**: Check backup file integrity
- **Update Backup Scripts**: Keep backup scripts updated
- **Monitor Storage Usage**: Monitor storage utilization
- **Test Restoration**: Test backup restoration procedures

### Monthly Tasks
- **Review Backup Logs**: Review backup operation logs
- **Test Disaster Recovery**: Test complete system recovery
- **Update Documentation**: Update backup procedures documentation
- **Review Storage Costs**: Review backup storage costs
- **Security Audit**: Audit backup security procedures
