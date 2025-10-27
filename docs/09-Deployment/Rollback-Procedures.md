# Rollback Procedures - Virtual Queue Management System

**Document Version:** 1.0  
**Date:** January 15, 2024  
**Author:** DevOps Engineer  
**Status:** Draft  
**Phase:** 09 - Deployment  
**Priority:** 🔴 Critical  

---

## Rollback Procedures Overview

This document outlines comprehensive rollback procedures for the Virtual Queue Management System. It covers different rollback scenarios, automated and manual rollback processes, data recovery procedures, and emergency response protocols to ensure system stability and data integrity during deployment issues.

## Rollback Strategy

### **Rollback Principles**
- **Minimize Downtime**: Achieve rollback within 5 minutes
- **Data Integrity**: Ensure no data loss during rollback
- **Service Continuity**: Maintain service availability during rollback
- **Automated Recovery**: Use automated rollback where possible
- **Documentation**: Document all rollback actions and decisions

### **Rollback Types**

| Rollback Type | Trigger | Timeframe | Impact | Method |
|---------------|---------|-----------|--------|--------|
| **Automated Rollback** | Health check failure | < 2 minutes | Minimal | Blue-green deployment |
| **Manual Rollback** | Manual decision | < 5 minutes | Low | Docker/Kubernetes rollback |
| **Emergency Rollback** | Critical failure | < 1 minute | High | Emergency procedures |
| **Data Rollback** | Data corruption | < 10 minutes | High | Database restoration |

## Automated Rollback

### **Health Check Integration**

#### **Health Check Configuration**
```yaml
# docker-compose.prod.yml
services:
  api-prod:
    image: virtualqueue-api:latest
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:80/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 60s
    deploy:
      replicas: 3
      update_config:
        parallelism: 1
        delay: 10s
        failure_action: rollback
        monitor: 60s
        max_failure_ratio: 0.3
```

#### **Automated Rollback Script**
```bash
#!/bin/bash
# auto-rollback.sh

set -e

echo "Starting automated rollback process..."

# Check health status
HEALTH_STATUS=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:80/health)

if [ "$HEALTH_STATUS" != "200" ]; then
    echo "Health check failed (HTTP $HEALTH_STATUS), initiating rollback..."
    
    # Get current deployment info
    CURRENT_VERSION=$(docker-compose -f docker-compose.prod.yml config --services | head -1)
    PREVIOUS_VERSION=$(docker-compose -f docker-compose.prod.yml config --services | tail -1)
    
    # Rollback to previous version
    docker-compose -f docker-compose.prod.yml up -d --no-deps $PREVIOUS_VERSION
    
    # Wait for rollback to complete
    sleep 30
    
    # Verify rollback success
    ROLLBACK_HEALTH=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:80/health)
    
    if [ "$ROLLBACK_HEALTH" = "200" ]; then
        echo "Rollback successful, service restored"
        
        # Send notification
        curl -X POST "https://hooks.slack.com/services/YOUR/SLACK/WEBHOOK" \
          -H "Content-Type: application/json" \
          -d '{"text":"Automated rollback completed successfully"}'
    else
        echo "Rollback failed, escalating to manual procedures"
        exit 1
    fi
else
    echo "Health check passed, no rollback needed"
fi
```

### **Blue-Green Deployment Rollback**

#### **Blue-Green Rollback Process**
```bash
#!/bin/bash
# blue-green-rollback.sh

set -e

echo "Starting blue-green rollback..."

# Determine current environment
if [ -f "/var/run/blue-green/current" ]; then
    CURRENT_ENV=$(cat /var/run/blue-green/current)
else
    CURRENT_ENV="blue"
fi

# Determine target environment
if [ "$CURRENT_ENV" = "blue" ]; then
    TARGET_ENV="green"
else
    TARGET_ENV="blue"
fi

echo "Current environment: $CURRENT_ENV"
echo "Rolling back to: $TARGET_ENV"

# Update load balancer to point to target environment
update_load_balancer() {
    local target_env=$1
    
    # Update Nginx configuration
    sed -i "s/proxy_pass http://.*;/proxy_pass http://$target_env;/g" /etc/nginx/sites-available/virtualqueue
    
    # Reload Nginx
    nginx -t && nginx -s reload
    
    echo "Load balancer updated to point to $target_env"
}

# Perform rollback
update_load_balancer $TARGET_ENV

# Wait for traffic to switch
sleep 30

# Verify rollback
HEALTH_STATUS=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:80/health)

if [ "$HEALTH_STATUS" = "200" ]; then
    echo "Blue-green rollback successful"
    
    # Update current environment marker
    echo $TARGET_ENV > /var/run/blue-green/current
    
    # Send success notification
    curl -X POST "https://hooks.slack.com/services/YOUR/SLACK/WEBHOOK" \
      -H "Content-Type: application/json" \
      -d "{\"text\":\"Blue-green rollback to $TARGET_ENV completed successfully\"}"
else
    echo "Blue-green rollback failed"
    exit 1
fi
```

## Manual Rollback

### **Docker Compose Rollback**

#### **Docker Rollback Script**
```bash
#!/bin/bash
# docker-rollback.sh

set -e

echo "Starting Docker rollback process..."

# Get current image version
CURRENT_IMAGE=$(docker-compose -f docker-compose.prod.yml config | grep "image:" | head -1 | awk '{print $2}')
echo "Current image: $CURRENT_IMAGE"

# Get previous image version from backup
PREVIOUS_IMAGE=$(cat /var/backups/docker/previous_image.txt)
echo "Previous image: $PREVIOUS_IMAGE"

# Update docker-compose.yml with previous image
sed -i "s|image: $CURRENT_IMAGE|image: $PREVIOUS_IMAGE|g" docker-compose.prod.yml

# Stop current services
echo "Stopping current services..."
docker-compose -f docker-compose.prod.yml down

# Start with previous image
echo "Starting with previous image..."
docker-compose -f docker-compose.prod.yml up -d

# Wait for services to start
echo "Waiting for services to start..."
sleep 60

# Verify rollback
HEALTH_STATUS=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:80/health)

if [ "$HEALTH_STATUS" = "200" ]; then
    echo "Docker rollback successful"
    
    # Update backup with current image
    echo $CURRENT_IMAGE > /var/backups/docker/previous_image.txt
    
    # Send success notification
    curl -X POST "https://hooks.slack.com/services/YOUR/SLACK/WEBHOOK" \
      -H "Content-Type: application/json" \
      -d '{"text":"Docker rollback completed successfully"}'
else
    echo "Docker rollback failed"
    exit 1
fi
```

### **Kubernetes Rollback**

#### **Kubernetes Rollback Script**
```bash
#!/bin/bash
# k8s-rollback.sh

set -e

echo "Starting Kubernetes rollback process..."

# Get current deployment
CURRENT_DEPLOYMENT=$(kubectl get deployments -o jsonpath='{.items[0].metadata.name}')
echo "Current deployment: $CURRENT_DEPLOYMENT"

# Get current revision
CURRENT_REVISION=$(kubectl rollout history deployment/$CURRENT_DEPLOYMENT --revision=0 | tail -1 | awk '{print $1}')
echo "Current revision: $CURRENT_REVISION"

# Get previous revision
PREVIOUS_REVISION=$(kubectl rollout history deployment/$CURRENT_DEPLOYMENT | tail -2 | head -1 | awk '{print $1}')
echo "Previous revision: $PREVIOUS_REVISION"

# Perform rollback
echo "Rolling back to revision $PREVIOUS_REVISION..."
kubectl rollout undo deployment/$CURRENT_DEPLOYMENT --to-revision=$PREVIOUS_REVISION

# Wait for rollback to complete
echo "Waiting for rollback to complete..."
kubectl rollout status deployment/$CURRENT_DEPLOYMENT --timeout=300s

# Verify rollback
echo "Verifying rollback..."
kubectl get pods -l app=virtualqueue-api

# Check service health
HEALTH_STATUS=$(kubectl get service virtualqueue-api -o jsonpath='{.status.loadBalancer.ingress[0].ip}')
if [ -n "$HEALTH_STATUS" ]; then
    echo "Kubernetes rollback successful"
    
    # Send success notification
    curl -X POST "https://hooks.slack.com/services/YOUR/SLACK/WEBHOOK" \
      -H "Content-Type: application/json" \
      -d '{"text":"Kubernetes rollback completed successfully"}'
else
    echo "Kubernetes rollback failed"
    exit 1
fi
```

## Emergency Rollback

### **Emergency Response Protocol**

#### **Emergency Rollback Checklist**
```bash
#!/bin/bash
# emergency-rollback.sh

set -e

echo "EMERGENCY ROLLBACK INITIATED"
echo "Timestamp: $(date)"
echo "================================"

# 1. Immediate Service Stop
echo "Step 1: Stopping all services..."
docker-compose -f docker-compose.prod.yml down

# 2. Restore Previous Version
echo "Step 2: Restoring previous version..."
docker-compose -f docker-compose.prod.yml up -d --no-deps api-prod

# 3. Database Rollback (if needed)
echo "Step 3: Checking database status..."
DB_STATUS=$(docker-compose exec db-prod pg_isready -U postgres)
if [ "$DB_STATUS" != "accepting connections" ]; then
    echo "Database rollback required..."
    # Restore from latest backup
    LATEST_BACKUP=$(ls -t /var/backups/db/*.sql.gz | head -1)
    gunzip -c $LATEST_BACKUP | docker-compose exec -T db-prod psql -U postgres -d VirtualQueue
fi

# 4. Cache Rollback (if needed)
echo "Step 4: Checking cache status..."
REDIS_STATUS=$(docker-compose exec redis-prod redis-cli ping)
if [ "$REDIS_STATUS" != "PONG" ]; then
    echo "Cache rollback required..."
    docker-compose restart redis-prod
fi

# 5. Service Verification
echo "Step 5: Verifying service restoration..."
sleep 30
HEALTH_STATUS=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:80/health)

if [ "$HEALTH_STATUS" = "200" ]; then
    echo "Emergency rollback successful"
    
    # Send emergency notification
    curl -X POST "https://hooks.slack.com/services/YOUR/SLACK/WEBHOOK" \
      -H "Content-Type: application/json" \
      -d '{"text":"🚨 EMERGENCY ROLLBACK COMPLETED SUCCESSFULLY"}'
else
    echo "Emergency rollback failed - ESCALATING"
    
    # Send failure notification
    curl -X POST "https://hooks.slack.com/services/YOUR/SLACK/WEBHOOK" \
      -H "Content-Type: application/json" \
      -d '{"text":"🚨 EMERGENCY ROLLBACK FAILED - IMMEDIATE ATTENTION REQUIRED"}'
    
    exit 1
fi
```

### **Emergency Contacts**

#### **Emergency Escalation Matrix**
| Level | Role | Contact | Response Time |
|-------|------|---------|---------------|
| **L1** | DevOps Engineer | devops@company.com | 15 minutes |
| **L2** | Technical Lead | tech-lead@company.com | 30 minutes |
| **L3** | Engineering Manager | eng-manager@company.com | 1 hour |
| **L4** | CTO | cto@company.com | 2 hours |

#### **Emergency Notification Script**
```bash
#!/bin/bash
# emergency-notify.sh

INCIDENT_ID=$(date +%Y%m%d_%H%M%S)
SEVERITY=$1
MESSAGE=$2

# Send Slack notification
curl -X POST "https://hooks.slack.com/services/YOUR/SLACK/WEBHOOK" \
  -H "Content-Type: application/json" \
  -d "{
    \"text\": \"🚨 EMERGENCY INCIDENT: $INCIDENT_ID\",
    \"attachments\": [
      {
        \"color\": \"danger\",
        \"fields\": [
          {
            \"title\": \"Severity\",
            \"value\": \"$SEVERITY\",
            \"short\": true
          },
          {
            \"title\": \"Message\",
            \"value\": \"$MESSAGE\",
            \"short\": false
          },
          {
            \"title\": \"Timestamp\",
            \"value\": \"$(date)\",
            \"short\": true
          }
        ]
      }
    ]
  }"

# Send email notification
echo "Emergency Incident: $INCIDENT_ID" | mail -s "EMERGENCY: Virtual Queue System" \
  -a "From: alerts@virtualqueue.com" \
  devops@company.com,tech-lead@company.com,eng-manager@company.com
```

## Data Rollback

### **Database Rollback**

#### **Database Rollback Script**
```bash
#!/bin/bash
# db-rollback.sh

set -e

echo "Starting database rollback process..."

# Get latest backup
LATEST_BACKUP=$(ls -t /var/backups/db/*.sql.gz | head -1)
echo "Latest backup: $LATEST_BACKUP"

# Create current state backup before rollback
echo "Creating current state backup..."
CURRENT_BACKUP="/var/backups/db/current_state_$(date +%Y%m%d_%H%M%S).sql.gz"
docker-compose exec db-prod pg_dump -U postgres -d VirtualQueue | gzip > $CURRENT_BACKUP

# Stop application services
echo "Stopping application services..."
docker-compose stop api-prod

# Restore database
echo "Restoring database from backup..."
gunzip -c $LATEST_BACKUP | docker-compose exec -T db-prod psql -U postgres -d VirtualQueue

# Verify database restoration
echo "Verifying database restoration..."
DB_TABLES=$(docker-compose exec db-prod psql -U postgres -d VirtualQueue -c "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public';" -t)
echo "Database tables count: $DB_TABLES"

# Start application services
echo "Starting application services..."
docker-compose start api-prod

# Wait for services to start
sleep 30

# Verify application health
HEALTH_STATUS=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:80/health)

if [ "$HEALTH_STATUS" = "200" ]; then
    echo "Database rollback successful"
    
    # Send success notification
    curl -X POST "https://hooks.slack.com/services/YOUR/SLACK/WEBHOOK" \
      -H "Content-Type: application/json" \
      -d '{"text":"Database rollback completed successfully"}'
else
    echo "Database rollback failed"
    exit 1
fi
```

### **Redis Rollback**

#### **Redis Rollback Script**
```bash
#!/bin/bash
# redis-rollback.sh

set -e

echo "Starting Redis rollback process..."

# Get latest Redis backup
LATEST_BACKUP=$(ls -t /var/backups/redis/*.rdb.gz | head -1)
echo "Latest Redis backup: $LATEST_BACKUP"

# Stop Redis service
echo "Stopping Redis service..."
docker-compose stop redis-prod

# Restore Redis data
echo "Restoring Redis data..."
gunzip -c $LATEST_BACKUP > /var/lib/redis/dump.rdb

# Start Redis service
echo "Starting Redis service..."
docker-compose start redis-prod

# Wait for Redis to start
sleep 10

# Verify Redis restoration
echo "Verifying Redis restoration..."
REDIS_STATUS=$(docker-compose exec redis-prod redis-cli ping)
if [ "$REDIS_STATUS" = "PONG" ]; then
    echo "Redis rollback successful"
    
    # Send success notification
    curl -X POST "https://hooks.slack.com/services/YOUR/SLACK/WEBHOOK" \
      -H "Content-Type: application/json" \
      -d '{"text":"Redis rollback completed successfully"}'
else
    echo "Redis rollback failed"
    exit 1
fi
```

## Rollback Validation

### **Post-Rollback Verification**

#### **Rollback Validation Script**
```bash
#!/bin/bash
# rollback-validation.sh

set -e

echo "Starting rollback validation..."

# 1. Service Health Check
echo "1. Checking service health..."
HEALTH_STATUS=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:80/health)
if [ "$HEALTH_STATUS" != "200" ]; then
    echo "❌ Service health check failed"
    exit 1
fi
echo "✅ Service health check passed"

# 2. Database Connectivity
echo "2. Checking database connectivity..."
DB_STATUS=$(docker-compose exec db-prod pg_isready -U postgres)
if [ "$DB_STATUS" != "accepting connections" ]; then
    echo "❌ Database connectivity check failed"
    exit 1
fi
echo "✅ Database connectivity check passed"

# 3. Redis Connectivity
echo "3. Checking Redis connectivity..."
REDIS_STATUS=$(docker-compose exec redis-prod redis-cli ping)
if [ "$REDIS_STATUS" != "PONG" ]; then
    echo "❌ Redis connectivity check failed"
    exit 1
fi
echo "✅ Redis connectivity check passed"

# 4. API Endpoints
echo "4. Checking API endpoints..."
API_ENDPOINTS=("/health" "/api/queues" "/api/users" "/api/sessions")
for endpoint in "${API_ENDPOINTS[@]}"; do
    STATUS=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:80$endpoint)
    if [ "$STATUS" != "200" ]; then
        echo "❌ API endpoint $endpoint check failed"
        exit 1
    fi
done
echo "✅ API endpoints check passed"

# 5. Performance Check
echo "5. Checking performance..."
RESPONSE_TIME=$(curl -s -o /dev/null -w "%{time_total}" http://localhost:80/health)
if (( $(echo "$RESPONSE_TIME > 2.0" | bc -l) )); then
    echo "❌ Performance check failed (response time: ${RESPONSE_TIME}s)"
    exit 1
fi
echo "✅ Performance check passed (response time: ${RESPONSE_TIME}s)"

# 6. Data Integrity
echo "6. Checking data integrity..."
DB_TABLES=$(docker-compose exec db-prod psql -U postgres -d VirtualQueue -c "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public';" -t)
if [ "$DB_TABLES" -lt 5 ]; then
    echo "❌ Data integrity check failed (tables: $DB_TABLES)"
    exit 1
fi
echo "✅ Data integrity check passed (tables: $DB_TABLES)"

echo "🎉 All rollback validation checks passed!"
```

### **Rollback Metrics**

#### **Rollback Performance Metrics**
```bash
#!/bin/bash
# rollback-metrics.sh

echo "Rollback Performance Metrics"
echo "============================"

# Rollback duration
ROLLBACK_START=$(cat /var/log/rollback/start_time.txt)
ROLLBACK_END=$(date +%s)
ROLLBACK_DURATION=$((ROLLBACK_END - ROLLBACK_START))
echo "Rollback Duration: ${ROLLBACK_DURATION} seconds"

# Service downtime
SERVICE_DOWN_START=$(cat /var/log/rollback/service_down_start.txt)
SERVICE_DOWN_END=$(cat /var/log/rollback/service_down_end.txt)
SERVICE_DOWNTIME=$((SERVICE_DOWN_END - SERVICE_DOWN_START))
echo "Service Downtime: ${SERVICE_DOWNTIME} seconds"

# Data loss
DATA_LOSS=$(cat /var/log/rollback/data_loss.txt)
echo "Data Loss: $DATA_LOSS"

# User impact
USER_IMPACT=$(cat /var/log/rollback/user_impact.txt)
echo "User Impact: $USER_IMPACT"

# Send metrics to monitoring system
curl -X POST "https://monitoring.company.com/api/metrics" \
  -H "Content-Type: application/json" \
  -d "{
    \"metric\": \"rollback.duration\",
    \"value\": $ROLLBACK_DURATION,
    \"timestamp\": $(date +%s)
  }"
```

## Rollback Documentation

### **Rollback Log Template**

#### **Rollback Log Entry**
```bash
#!/bin/bash
# log-rollback.sh

ROLLBACK_ID=$(date +%Y%m%d_%H%M%S)
ROLLBACK_TYPE=$1
ROLLBACK_REASON=$2
ROLLBACK_DURATION=$3

# Create rollback log entry
cat >> /var/log/rollback/rollback.log << EOF
========================================
Rollback ID: $ROLLBACK_ID
Timestamp: $(date)
Type: $ROLLBACK_TYPE
Reason: $ROLLBACK_REASON
Duration: $ROLLBACK_DURATION seconds
Performed by: $(whoami)
System: $(hostname)
========================================
EOF

echo "Rollback logged with ID: $ROLLBACK_ID"
```

### **Post-Rollback Analysis**

#### **Rollback Analysis Script**
```bash
#!/bin/bash
# rollback-analysis.sh

echo "Rollback Analysis Report"
echo "======================="

# Analyze rollback frequency
echo "Rollback Frequency Analysis:"
grep -c "Type:" /var/log/rollback/rollback.log

# Analyze rollback reasons
echo "Rollback Reasons Analysis:"
grep "Reason:" /var/log/rollback/rollback.log | sort | uniq -c

# Analyze rollback duration
echo "Rollback Duration Analysis:"
grep "Duration:" /var/log/rollback/rollback.log | awk '{print $2}' | sort -n

# Generate recommendations
echo "Recommendations:"
echo "1. Review deployment process to reduce rollback frequency"
echo "2. Improve testing to catch issues before deployment"
echo "3. Implement better monitoring to detect issues early"
echo "4. Consider implementing canary deployments"
```

## Rollback Training

### **Rollback Procedure Training**

#### **Training Checklist**
- [ ] **Understanding Rollback Types**: Automated, Manual, Emergency, Data
- [ ] **Rollback Triggers**: Health check failures, manual decisions, critical failures
- [ ] **Rollback Procedures**: Step-by-step rollback processes
- [ ] **Validation Steps**: Post-rollback verification procedures
- [ ] **Emergency Response**: Emergency contact and escalation procedures
- [ ] **Documentation**: Rollback logging and analysis procedures

#### **Rollback Drill Script**
```bash
#!/bin/bash
# rollback-drill.sh

echo "Starting rollback drill..."

# Simulate deployment failure
echo "Simulating deployment failure..."
docker-compose -f docker-compose.prod.yml stop api-prod

# Practice rollback procedure
echo "Practicing rollback procedure..."
./docker-rollback.sh

# Validate rollback
echo "Validating rollback..."
./rollback-validation.sh

# Log drill results
echo "Rollback drill completed successfully" >> /var/log/rollback/drill.log

echo "Rollback drill completed!"
```

## Approval and Sign-off

### **Rollback Procedures Approval**
- **DevOps Engineer**: [Name] - [Date]
- **Technical Lead**: [Name] - [Date]
- **Security Lead**: [Name] - [Date]
- **Operations Manager**: [Name] - [Date]

### **Document Control**
- **Version**: 1.0
- **Last Updated**: January 15, 2024
- **Next Review**: February 15, 2024
- **Distribution**: Development Team, DevOps Team, Operations Team

---

**Document Status**: Draft  
**Next Phase**: Deployment Checklist  
**Dependencies**: Rollback testing, emergency procedure validation
