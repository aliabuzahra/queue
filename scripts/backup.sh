#!/bin/bash

# Virtual Queue Management System - Automated Backup Script
# This script performs automated backups of the database and application data

set -euo pipefail

# Configuration
BACKUP_DIR="/backups"
DB_HOST="${DB_HOST:-postgres}"
DB_PORT="${DB_PORT:-5432}"
DB_NAME="${DB_NAME:-virtualqueue_prod}"
DB_USER="${DB_USER:-virtualqueue_user}"
DB_PASSWORD="${DB_PASSWORD}"
REDIS_HOST="${REDIS_HOST:-redis}"
REDIS_PORT="${REDIS_PORT:-6379}"
REDIS_PASSWORD="${REDIS_PASSWORD}"
BACKUP_RETENTION_DAYS="${BACKUP_RETENTION_DAYS:-30}"
COMPRESSION_ENABLED="${COMPRESSION_ENABLED:-true}"
BACKUP_STORAGE_PROVIDER="${BACKUP_STORAGE_PROVIDER:-Local}"
BACKUP_STORAGE_PATH="${BACKUP_STORAGE_PATH:-/backups}"

# Logging
LOG_FILE="/var/log/backup.log"
exec 1> >(tee -a "$LOG_FILE")
exec 2> >(tee -a "$LOG_FILE" >&2)

# Functions
log() {
    echo "[$(date '+%Y-%m-%d %H:%M:%S')] $1"
}

error() {
    echo "[$(date '+%Y-%m-%d %H:%M:%S')] ERROR: $1" >&2
    exit 1
}

# Create backup directory
mkdir -p "$BACKUP_DIR"

# Generate backup filename with timestamp
TIMESTAMP=$(date '+%Y%m%d_%H%M%S')
BACKUP_FILENAME="virtualqueue_backup_${TIMESTAMP}"
BACKUP_PATH="$BACKUP_DIR/$BACKUP_FILENAME"

log "Starting backup process for Virtual Queue Management System"

# 1. Database Backup
log "Creating database backup..."
DB_BACKUP_FILE="${BACKUP_PATH}_database.sql"

if command -v pg_dump &> /dev/null; then
    PGPASSWORD="$DB_PASSWORD" pg_dump \
        -h "$DB_HOST" \
        -p "$DB_PORT" \
        -U "$DB_USER" \
        -d "$DB_NAME" \
        --verbose \
        --no-password \
        --format=plain \
        --file="$DB_BACKUP_FILE"
    
    if [ $? -eq 0 ]; then
        log "Database backup completed successfully: $DB_BACKUP_FILE"
    else
        error "Database backup failed"
    fi
else
    error "pg_dump not found. Please install PostgreSQL client tools."
fi

# 2. Redis Backup
log "Creating Redis backup..."
REDIS_BACKUP_FILE="${BACKUP_PATH}_redis.rdb"

if command -v redis-cli &> /dev/null; then
    # Trigger Redis BGSAVE
    redis-cli -h "$REDIS_HOST" -p "$REDIS_PORT" -a "$REDIS_PASSWORD" BGSAVE
    
    # Wait for BGSAVE to complete
    while [ "$(redis-cli -h "$REDIS_HOST" -p "$REDIS_PORT" -a "$REDIS_PASSWORD" LASTSAVE)" = "$(redis-cli -h "$REDIS_HOST" -p "$REDIS_PORT" -a "$REDIS_PASSWORD" LASTSAVE)" ]; do
        sleep 1
    done
    
    # Copy the RDB file
    cp "/var/lib/redis/dump.rdb" "$REDIS_BACKUP_FILE"
    
    if [ $? -eq 0 ]; then
        log "Redis backup completed successfully: $REDIS_BACKUP_FILE"
    else
        error "Redis backup failed"
    fi
else
    log "Warning: redis-cli not found. Skipping Redis backup."
fi

# 3. Application Configuration Backup
log "Creating application configuration backup..."
CONFIG_BACKUP_FILE="${BACKUP_PATH}_config.tar.gz"

tar -czf "$CONFIG_BACKUP_FILE" \
    -C /app \
    appsettings.Production.json \
    Production-Configuration.md \
    2>/dev/null || log "Warning: Some configuration files not found"

log "Configuration backup completed: $CONFIG_BACKUP_FILE"

# 4. Logs Backup (last 7 days)
log "Creating logs backup..."
LOGS_BACKUP_FILE="${BACKUP_PATH}_logs.tar.gz"

if [ -d "/app/logs" ]; then
    find /app/logs -name "*.log" -mtime -7 -exec tar -czf "$LOGS_BACKUP_FILE" {} + 2>/dev/null || log "Warning: No recent log files found"
    log "Logs backup completed: $LOGS_BACKUP_FILE"
else
    log "Warning: Logs directory not found. Skipping logs backup."
fi

# 5. Create backup manifest
log "Creating backup manifest..."
MANIFEST_FILE="${BACKUP_PATH}_manifest.json"

cat > "$MANIFEST_FILE" << EOF
{
  "backup_id": "$BACKUP_FILENAME",
  "timestamp": "$(date -u +%Y-%m-%dT%H:%M:%SZ)",
  "version": "1.0",
  "components": {
    "database": {
      "file": "$(basename "$DB_BACKUP_FILE")",
      "size_bytes": $(stat -c%s "$DB_BACKUP_FILE" 2>/dev/null || echo 0),
      "checksum": "$(md5sum "$DB_BACKUP_FILE" | cut -d' ' -f1 2>/dev/null || echo 'N/A')"
    },
    "redis": {
      "file": "$(basename "$REDIS_BACKUP_FILE")",
      "size_bytes": $(stat -c%s "$REDIS_BACKUP_FILE" 2>/dev/null || echo 0),
      "checksum": "$(md5sum "$REDIS_BACKUP_FILE" | cut -d' ' -f1 2>/dev/null || echo 'N/A')"
    },
    "config": {
      "file": "$(basename "$CONFIG_BACKUP_FILE")",
      "size_bytes": $(stat -c%s "$CONFIG_BACKUP_FILE" 2>/dev/null || echo 0),
      "checksum": "$(md5sum "$CONFIG_BACKUP_FILE" | cut -d' ' -f1 2>/dev/null || echo 'N/A')"
    },
    "logs": {
      "file": "$(basename "$LOGS_BACKUP_FILE")",
      "size_bytes": $(stat -c%s "$LOGS_BACKUP_FILE" 2>/dev/null || echo 0),
      "checksum": "$(md5sum "$LOGS_BACKUP_FILE" | cut -d' ' -f1 2>/dev/null || echo 'N/A')"
    }
  },
  "retention_days": $BACKUP_RETENTION_DAYS,
  "compression_enabled": $COMPRESSION_ENABLED
}
EOF

log "Backup manifest created: $MANIFEST_FILE"

# 6. Compress all backup files
if [ "$COMPRESSION_ENABLED" = "true" ]; then
    log "Compressing backup files..."
    FINAL_BACKUP_FILE="${BACKUP_PATH}.tar.gz"
    
    tar -czf "$FINAL_BACKUP_FILE" \
        -C "$BACKUP_DIR" \
        "$(basename "$DB_BACKUP_FILE")" \
        "$(basename "$REDIS_BACKUP_FILE")" \
        "$(basename "$CONFIG_BACKUP_FILE")" \
        "$(basename "$LOGS_BACKUP_FILE")" \
        "$(basename "$MANIFEST_FILE")"
    
    # Remove individual files
    rm -f "$DB_BACKUP_FILE" "$REDIS_BACKUP_FILE" "$CONFIG_BACKUP_FILE" "$LOGS_BACKUP_FILE" "$MANIFEST_FILE"
    
    log "Backup compressed: $FINAL_BACKUP_FILE"
    BACKUP_FILE="$FINAL_BACKUP_FILE"
else
    log "Compression disabled. Backup files remain uncompressed."
    BACKUP_FILE="$BACKUP_PATH"
fi

# 7. Upload to remote storage (if configured)
if [ "$BACKUP_STORAGE_PROVIDER" != "Local" ]; then
    log "Uploading backup to remote storage: $BACKUP_STORAGE_PROVIDER"
    
    case "$BACKUP_STORAGE_PROVIDER" in
        "AWS_S3")
            if command -v aws &> /dev/null; then
                aws s3 cp "$BACKUP_FILE" "$BACKUP_STORAGE_PATH/"
                log "Backup uploaded to S3 successfully"
            else
                error "AWS CLI not found. Please install AWS CLI."
            fi
            ;;
        "Azure_Blob")
            if command -v az &> /dev/null; then
                az storage blob upload \
                    --file "$BACKUP_FILE" \
                    --container-name "backups" \
                    --name "$(basename "$BACKUP_FILE")"
                log "Backup uploaded to Azure Blob Storage successfully"
            else
                error "Azure CLI not found. Please install Azure CLI."
            fi
            ;;
        "Google_Cloud")
            if command -v gsutil &> /dev/null; then
                gsutil cp "$BACKUP_FILE" "$BACKUP_STORAGE_PATH/"
                log "Backup uploaded to Google Cloud Storage successfully"
            else
                error "Google Cloud SDK not found. Please install Google Cloud SDK."
            fi
            ;;
        *)
            log "Warning: Unknown storage provider: $BACKUP_STORAGE_PROVIDER"
            ;;
    esac
fi

# 8. Cleanup old backups
log "Cleaning up old backups (retention: $BACKUP_RETENTION_DAYS days)..."
find "$BACKUP_DIR" -name "virtualqueue_backup_*" -type f -mtime +$BACKUP_RETENTION_DAYS -delete
log "Old backups cleaned up"

# 9. Generate backup summary
BACKUP_SIZE=$(du -h "$BACKUP_FILE" | cut -f1)
log "Backup completed successfully!"
log "Backup file: $BACKUP_FILE"
log "Backup size: $BACKUP_SIZE"
log "Backup location: $BACKUP_STORAGE_PATH"

# 10. Send notification (if configured)
if [ -n "${NOTIFICATION_EMAIL:-}" ]; then
    log "Sending backup notification email..."
    echo "Virtual Queue Management System backup completed successfully.

Backup Details:
- Backup ID: $BACKUP_FILENAME
- Timestamp: $(date)
- Size: $BACKUP_SIZE
- Location: $BACKUP_STORAGE_PATH
- Retention: $BACKUP_RETENTION_DAYS days

This is an automated message from the Virtual Queue backup system." | \
    mail -s "Virtual Queue Backup Completed - $BACKUP_FILENAME" "$NOTIFICATION_EMAIL"
fi

log "Backup process completed successfully!"
