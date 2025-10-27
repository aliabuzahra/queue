# Virtual Queue Management System - Docker Desktop Production Setup Guide

## 🐳 **Docker Desktop Production Deployment**

This guide will help you run the Virtual Queue Management System production version on Docker Desktop.

---

## 📋 **Prerequisites**

### **Required Software:**
- ✅ **Docker Desktop** (Latest version)
- ✅ **Docker Compose** (Included with Docker Desktop)
- ✅ **Git** (For cloning the repository)

### **System Requirements:**
- **RAM**: Minimum 8GB (Recommended: 16GB)
- **CPU**: Minimum 4 cores (Recommended: 8 cores)
- **Disk Space**: Minimum 10GB free space
- **Operating System**: Windows 10/11, macOS, or Linux

---

## 🚀 **Quick Start**

### **Step 1: Clone the Repository**
```bash
git clone <repository-url>
cd template_api_ddd
```

### **Step 2: Create Environment File**
```bash
# Copy the example environment file
cp env.desktop.example .env

# Edit the environment file with your settings
# Update passwords, API keys, and other configuration
```

### **Step 3: Start the Application**
```bash
# Start all services
docker-compose -f docker-compose.desktop.yml up -d

# Check service status
docker-compose -f docker-compose.desktop.yml ps
```

### **Step 4: Verify Deployment**
```bash
# Check API health
curl http://localhost:8080/healthz

# Check database connection
curl http://localhost:8080/healthz/db

# Check Redis connection
curl http://localhost:8080/healthz/redis
```

---

## 🌐 **Access Points**

### **Application Services:**
- **Virtual Queue API**: http://localhost:8080
- **API Documentation (Swagger)**: http://localhost:8080/swagger
- **Health Check**: http://localhost:8080/healthz

### **Monitoring Services:**
- **Prometheus**: http://localhost:9090
- **Grafana**: http://localhost:3000
  - Username: `admin`
  - Password: `admin123` (or your custom password)
- **Alertmanager**: http://localhost:9093

### **Database Services:**
- **PostgreSQL**: localhost:5432
  - Database: `virtualqueue_prod`
  - Username: `virtualqueue_user`
  - Password: `VirtualQueue123!` (or your custom password)
- **Redis**: localhost:6379
  - Password: `Redis123!` (or your custom password)

### **Monitoring Exporters:**
- **PostgreSQL Exporter**: http://localhost:9187
- **Redis Exporter**: http://localhost:9121
- **Node Exporter**: http://localhost:9100

---

## ⚙️ **Configuration**

### **Environment Variables**

Create a `.env` file in the project root with the following variables:

```bash
# Database Configuration
DB_PASSWORD=VirtualQueue123!
REDIS_PASSWORD=Redis123!

# JWT Configuration
JWT_SECRET_KEY=YourSuperSecretKeyThatIsAtLeast32CharactersLong!

# Email Configuration (Optional)
SMTP_SERVER=smtp.gmail.com
SMTP_USERNAME=noreply@virtualqueue.com
SMTP_PASSWORD=your-app-password

# SMS Configuration (Optional)
SMS_API_KEY=your-twilio-api-key
SMS_API_SECRET=your-twilio-api-secret

# Grafana Configuration
GRAFANA_PASSWORD=admin123

# Alert Configuration (Optional)
SLACK_WEBHOOK_URL=https://hooks.slack.com/services/YOUR/SLACK/WEBHOOK
```

### **Custom Configuration**

#### **1. Database Settings**
```bash
# In .env file
DB_PASSWORD=YourSecurePassword123!
REDIS_PASSWORD=YourRedisPassword123!
```

#### **2. JWT Settings**
```bash
# In .env file
JWT_SECRET_KEY=YourSuperSecretKeyThatIsAtLeast32CharactersLong!
JWT_EXPIRATION_MINUTES=60
```

#### **3. Email Settings**
```bash
# In .env file
SMTP_SERVER=smtp.gmail.com
SMTP_USERNAME=your-email@gmail.com
SMTP_PASSWORD=your-app-password
FROM_EMAIL=noreply@yourdomain.com
```

#### **4. Monitoring Settings**
```bash
# In .env file
GRAFANA_PASSWORD=YourSecurePassword123!
```

---

## 🔧 **Service Management**

### **Start Services**
```bash
# Start all services
docker-compose -f docker-compose.desktop.yml up -d

# Start specific service
docker-compose -f docker-compose.desktop.yml up -d virtualqueue-api
```

### **Stop Services**
```bash
# Stop all services
docker-compose -f docker-compose.desktop.yml down

# Stop specific service
docker-compose -f docker-compose.desktop.yml stop virtualqueue-api
```

### **Restart Services**
```bash
# Restart all services
docker-compose -f docker-compose.desktop.yml restart

# Restart specific service
docker-compose -f docker-compose.desktop.yml restart virtualqueue-api
```

### **View Logs**
```bash
# View all logs
docker-compose -f docker-compose.desktop.yml logs -f

# View specific service logs
docker-compose -f docker-compose.desktop.yml logs -f virtualqueue-api

# View last 100 lines
docker-compose -f docker-compose.desktop.yml logs --tail=100 virtualqueue-api
```

### **Check Service Status**
```bash
# Check all services
docker-compose -f docker-compose.desktop.yml ps

# Check specific service
docker-compose -f docker-compose.desktop.yml ps virtualqueue-api
```

---

## 🗄️ **Data Management**

### **Database Backup**
```bash
# Create backup
docker exec virtualqueue-postgres pg_dump -U virtualqueue_user -d virtualqueue_prod > backup.sql

# Restore backup
docker exec -i virtualqueue-postgres psql -U virtualqueue_user -d virtualqueue_prod < backup.sql
```

### **Redis Backup**
```bash
# Create Redis backup
docker exec virtualqueue-redis redis-cli --rdb /data/dump.rdb

# Copy backup file
docker cp virtualqueue-redis:/data/dump.rdb ./redis-backup.rdb
```

### **Volume Management**
```bash
# List volumes
docker volume ls

# Inspect volume
docker volume inspect template_api_ddd_postgres_data

# Remove volume (WARNING: This will delete all data)
docker volume rm template_api_ddd_postgres_data
```

---

## 🔍 **Troubleshooting**

### **Common Issues**

#### **1. Port Conflicts**
```bash
# Check if ports are in use
netstat -tulpn | grep :8080
netstat -tulpn | grep :5432
netstat -tulpn | grep :6379

# Change ports in docker-compose.desktop.yml if needed
```

#### **2. Service Not Starting**
```bash
# Check service logs
docker-compose -f docker-compose.desktop.yml logs virtualqueue-api

# Check service status
docker-compose -f docker-compose.desktop.yml ps

# Restart service
docker-compose -f docker-compose.desktop.yml restart virtualqueue-api
```

#### **3. Database Connection Issues**
```bash
# Check database logs
docker-compose -f docker-compose.desktop.yml logs postgres

# Test database connection
docker exec virtualqueue-postgres pg_isready -U virtualqueue_user -d virtualqueue_prod

# Check database health
curl http://localhost:8080/healthz/db
```

#### **4. Redis Connection Issues**
```bash
# Check Redis logs
docker-compose -f docker-compose.desktop.yml logs redis

# Test Redis connection
docker exec virtualqueue-redis redis-cli ping

# Check Redis health
curl http://localhost:8080/healthz/redis
```

#### **5. Monitoring Issues**
```bash
# Check Prometheus logs
docker-compose -f docker-compose.desktop.yml logs prometheus

# Check Grafana logs
docker-compose -f docker-compose.desktop.yml logs grafana

# Check Alertmanager logs
docker-compose -f docker-compose.desktop.yml logs alertmanager
```

### **Performance Issues**

#### **1. High Memory Usage**
```bash
# Check container memory usage
docker stats

# Increase Docker Desktop memory limit
# Docker Desktop -> Settings -> Resources -> Memory
```

#### **2. Slow Performance**
```bash
# Check container CPU usage
docker stats

# Increase Docker Desktop CPU limit
# Docker Desktop -> Settings -> Resources -> CPUs
```

---

## 📊 **Monitoring & Observability**

### **Grafana Dashboards**

Access Grafana at http://localhost:3000 with credentials:
- Username: `admin`
- Password: `admin123` (or your custom password)

#### **Available Dashboards:**
1. **Virtual Queue API - Performance Dashboard**
   - HTTP request rates and response times
   - Error rates and status codes
   - Endpoint performance metrics

2. **Virtual Queue - Business Metrics Dashboard**
   - Active users and queue lengths
   - Queue processing rates
   - Wait times and capacity utilization

3. **Virtual Queue - Notifications & Webhooks Dashboard**
   - Notification delivery rates
   - Webhook success/failure rates
   - Queue lengths for different notification types

4. **Virtual Queue - Database & Cache Dashboard**
   - PostgreSQL connection and query metrics
   - Redis memory usage and hit rates
   - Database performance indicators

### **Prometheus Metrics**

Access Prometheus at http://localhost:9090 to:
- View raw metrics
- Create custom queries
- Set up alerting rules
- Monitor system performance

### **Alertmanager**

Access Alertmanager at http://localhost:9093 to:
- View active alerts
- Manage alert routing
- Configure notification channels
- Silence alerts during maintenance

---

## 🔒 **Security Considerations**

### **Production Security**

#### **1. Change Default Passwords**
```bash
# Update .env file with strong passwords
DB_PASSWORD=YourVerySecurePassword123!
REDIS_PASSWORD=YourVerySecureRedisPassword123!
GRAFANA_PASSWORD=YourVerySecureGrafanaPassword123!
JWT_SECRET_KEY=YourVerySecureJWTSecretKeyThatIsAtLeast32CharactersLong!
```

#### **2. Enable SSL/TLS**
```bash
# Add SSL certificates to nginx/ssl directory
# Update nginx configuration for HTTPS
```

#### **3. Network Security**
```bash
# Use Docker networks for service isolation
# Configure firewall rules
# Limit exposed ports
```

#### **4. Data Encryption**
```bash
# Enable PostgreSQL SSL
# Use Redis AUTH
# Encrypt sensitive data at rest
```

---

## 🚀 **Production Deployment**

### **Scaling Considerations**

#### **1. Horizontal Scaling**
```bash
# Scale API service
docker-compose -f docker-compose.desktop.yml up -d --scale virtualqueue-api=3

# Use load balancer (nginx) for multiple API instances
```

#### **2. Database Scaling**
```bash
# Use PostgreSQL read replicas
# Implement connection pooling
# Optimize database queries
```

#### **3. Cache Scaling**
```bash
# Use Redis Cluster
# Implement cache partitioning
# Monitor cache performance
```

### **High Availability**

#### **1. Service Redundancy**
```bash
# Run multiple instances of critical services
# Implement health checks
# Use Docker restart policies
```

#### **2. Data Backup**
```bash
# Automated database backups
# Redis persistence configuration
# Volume snapshots
```

#### **3. Monitoring**
```bash
# Comprehensive alerting
# Performance monitoring
# Log aggregation
```

---

## 📚 **API Documentation**

### **Swagger Documentation**
Access the interactive API documentation at:
http://localhost:8080/swagger

### **API Endpoints**

#### **Health Checks**
- `GET /healthz` - Overall system health
- `GET /healthz/db` - Database health
- `GET /healthz/redis` - Redis health

#### **Queue Management**
- `GET /api/queues` - List all queues
- `POST /api/queues` - Create new queue
- `GET /api/queues/{id}` - Get queue details
- `PUT /api/queues/{id}` - Update queue
- `DELETE /api/queues/{id}` - Delete queue

#### **User Sessions**
- `GET /api/usersessions` - List user sessions
- `POST /api/usersessions` - Create user session
- `GET /api/usersessions/{id}` - Get session details
- `PUT /api/usersessions/{id}` - Update session
- `DELETE /api/usersessions/{id}` - Delete session

#### **Authentication**
- `POST /api/auth/login` - User login
- `POST /api/auth/logout` - User logout
- `POST /api/auth/refresh` - Refresh token
- `GET /api/auth/profile` - Get user profile

---

## 🎉 **Success!**

Your Virtual Queue Management System is now running in production mode on Docker Desktop!

### **Next Steps:**
1. **Configure Email/SMS**: Update SMTP and SMS settings in `.env`
2. **Set Up Monitoring**: Configure Grafana dashboards and alerts
3. **Test API**: Use Swagger UI to test all endpoints
4. **Monitor Performance**: Check Grafana dashboards for system metrics
5. **Set Up Alerts**: Configure Alertmanager for critical events

### **Support:**
- **API Documentation**: http://localhost:8080/swagger
- **Monitoring**: http://localhost:3000 (Grafana)
- **Metrics**: http://localhost:9090 (Prometheus)
- **Alerts**: http://localhost:9093 (Alertmanager)

**Happy Queue Management! 🚀**
