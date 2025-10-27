#!/bin/bash

# Virtual Queue Management System - Backup Restoration Script
# This script restores the system from a backup

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

# Logging
LOG_FILE="/var/log/restore.log"
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

usage() {
    echo "Usage: $0 <backup_file> [--force]"
    echo "  backup_file: Path to the backup file to restore"
    echo "  --force: Force restoration without confirmation"
    exit 1
}

# Check arguments
if [ $# -lt 1 ]; then
    usage
fi

BACKUP_FILE="$1"
FORCE_RESTORE=false

if [ $# -eq 2 ] && [ "$2" = "--force" ]; then
    FORCE_RESTORE=true
fi

# Validate backup file
if [ ! -f "$BACKUP_FILE" ]; then
    error "Backup file not found: $BACKUP_FILE"
fi

log "Starting restore process from backup: $BACKUP_FILE"

# Confirmation prompt
if [ "$FORCE_RESTORE" = false ]; then
    echo "WARNING: This will restore the Virtual Queue Management System from backup."
    echo "This operation will overwrite existing data."
    echo "Backup file: $BACKUP_FILE"
    read -p "Are you sure you want to continue? (yes/no): " confirm
    
    if [ "$confirm" != "yes" ]; then
        log "Restore operation cancelled by user"
        exit 0
    fi
fi

# Extract backup if compressed
TEMP_DIR="/tmp/restore_$(date +%s)"
mkdir -p "$TEMP_DIR"

if [[ "$BACKUP_FILE" == *.tar.gz ]]; then
    log "Extracting compressed backup..."
    tar -xzf "$BACKUP_FILE" -C "$TEMP_DIR"
else
    log "Copying backup files..."
    cp "$BACKUP_FILE"* "$TEMP_DIR/"
fi

# Find backup files
DB_BACKUP_FILE=$(find "$TEMP_DIR" -name "*_database.sql" | head -1)
REDIS_BACKUP_FILE=$(find "$TEMP_DIR" -name "*_redis.rdb" | head -1)
CONFIG_BACKUP_FILE=$(find "$TEMP_DIR" -name "*_config.tar.gz" | head -1)
LOGS_BACKUP_FILE=$(find "$TEMP_DIR" -name "*_logs.tar.gz" | head -1)
MANIFEST_FILE=$(find "$TEMP_DIR" -name "*_manifest.json" | head -1)

# Validate backup components
if [ -z "$DB_BACKUP_FILE" ]; then
    error "Database backup file not found in backup"
fi

if [ -z "$MANIFEST_FILE" ]; then
    log "Warning: Backup manifest not found"
else
    log "Backup manifest found: $MANIFEST_FILE"
    cat "$MANIFEST_FILE"
fi

# 1. Database Restoration
log "Restoring database..."
if command -v psql &> /dev/null; then
    # Drop existing database and recreate
    PGPASSWORD="$DB_PASSWORD" psql \
        -h "$DB_HOST" \
        -p "$DB_PORT" \
        -U "$DB_USER" \
        -d "postgres" \
        -c "DROP DATABASE IF EXISTS $DB_NAME;"
    
    PGPASSWORD="$DB_PASSWORD" psql \
        -h "$DB_HOST" \
        -p "$DB_PORT" \
        -U "$DB_USER" \
        -d "postgres" \
        -c "CREATE DATABASE $DB_NAME;"
    
    # Restore database
    PGPASSWORD="$DB_PASSWORD" psql \
        -h "$DB_HOST" \
        -p "$DB_PORT" \
        -U "$DB_USER" \
        -d "$DB_NAME" \
        -f "$DB_BACKUP_FILE"
    
    if [ $? -eq 0 ]; then
        log "Database restored successfully"
    else
        error "Database restoration failed"
    fi
else
    error "psql not found. Please install PostgreSQL client tools."
fi

# 2. Redis Restoration
if [ -n "$REDIS_BACKUP_FILE" ]; then
    log "Restoring Redis data..."
    if command -v redis-cli &> /dev/null; then
        # Stop Redis gracefully
        redis-cli -h "$REDIS_HOST" -p "$REDIS_PORT" -a "$REDIS_PASSWORD" SHUTDOWN SAVE
        
        # Wait for Redis to stop
        sleep 5
        
        # Copy RDB file
        cp "$REDIS_BACKUP_FILE" "/var/lib/redis/dump.rdb"
        
        # Start Redis
        redis-server --daemonize yes --requirepass "$REDIS_PASSWORD"
        
        log "Redis restored successfully"
    else
        log "Warning: redis-cli not found. Skipping Redis restoration."
    fi
else
    log "Warning: Redis backup file not found. Skipping Redis restoration."
fi

# 3. Configuration Restoration
if [ -n "$CONFIG_BACKUP_FILE" ]; then
    log "Restoring application configuration..."
    tar -xzf "$CONFIG_BACKUP_FILE" -C /app/
    log "Configuration restored successfully"
else
    log "Warning: Configuration backup file not found. Skipping configuration restoration."
fi

# 4. Logs Restoration
if [ -n "$LOGS_BACKUP_FILE" ]; then
    log "Restoring application logs..."
    mkdir -p /app/logs
    tar -xzf "$LOGS_BACKUP_FILE" -C /app/logs/
    log "Logs restored successfully"
else
    log "Warning: Logs backup file not found. Skipping logs restoration."
fi

# 5. Cleanup
log "Cleaning up temporary files..."
rm -rf "$TEMP_DIR"

# 6. Verify restoration
log "Verifying restoration..."

# Check database connectivity
PGPASSWORD="$DB_PASSWORD" psql \
    -h "$DB_HOST" \
    -p "$DB_PORT" \
    -U "$DB_USER" \
    -d "$DB_NAME" \
    -c "SELECT COUNT(*) FROM tenants;" > /dev/null

if [ $? -eq 0 ]; then
    log "Database connectivity verified"
else
    error "Database connectivity verification failed"
fi

# Check Redis connectivity
redis-cli -h "$REDIS_HOST" -p "$REDIS_PORT" -a "$REDIS_PASSWORD" PING > /dev/null

if [ $? -eq 0 ]; then
    log "Redis connectivity verified"
else
    log "Warning: Redis connectivity verification failed"
fi

# 7. Restart services
log "Restarting Virtual Queue services..."
systemctl restart virtualqueue-api || log "Warning: Failed to restart API service"

log "Restore process completed successfully!"
log "Please verify that all services are running correctly."

# Send notification (if configured)
if [ -n "${NOTIFICATION_EMAIL:-}" ]; then
    log "Sending restore notification email..."
    echo "Virtual Queue Management System restore completed successfully.

Restore Details:
- Backup file: $BACKUP_FILE
- Timestamp: $(date)
- Database: Restored
- Redis: Restored
- Configuration: Restored
- Logs: Restored

This is an automated message from the Virtual Queue restore system." | \
    mail -s "Virtual Queue Restore Completed" "$NOTIFICATION_EMAIL"
fi
