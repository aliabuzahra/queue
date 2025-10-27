#!/bin/bash

# Virtual Queue Management System - Monitoring Setup Script
# This script sets up comprehensive monitoring with Prometheus, Grafana, and Alertmanager

set -euo pipefail

# Configuration
MONITORING_DIR="/opt/virtualqueue/monitoring"
GRAFANA_ADMIN_PASSWORD="${GRAFANA_ADMIN_PASSWORD:-admin123}"
PROMETHEUS_RETENTION="${PROMETHEUS_RETENTION:-30d}"
ALERTMANAGER_RETENTION="${ALERTMANAGER_RETENTION:-120h}"

# Logging
LOG_FILE="/var/log/monitoring-setup.log"
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

# Create monitoring directories
log "Creating monitoring directories..."
mkdir -p "$MONITORING_DIR"/{prometheus,grafana,alertmanager}
mkdir -p "$MONITORING_DIR"/prometheus/{rules,consoles,console_libraries}
mkdir -p "$MONITORING_DIR"/grafana/{dashboards,datasources,provisioning}
mkdir -p "$MONITORING_DIR"/alertmanager/{templates,data}

# Set permissions
chown -R prometheus:prometheus "$MONITORING_DIR"/prometheus
chown -R grafana:grafana "$MONITORING_DIR"/grafana
chown -R alertmanager:alertmanager "$MONITORING_DIR"/alertmanager

# Create Prometheus configuration
log "Creating Prometheus configuration..."
cat > "$MONITORING_DIR/prometheus/prometheus.yml" << 'EOF'
global:
  scrape_interval: 15s
  evaluation_interval: 15s
  external_labels:
    cluster: 'virtualqueue-prod'
    environment: 'production'

rule_files:
  - "rules/*.yml"

alerting:
  alertmanagers:
    - static_configs:
        - targets:
          - alertmanager:9093

scrape_configs:
  # Prometheus itself
  - job_name: 'prometheus'
    static_configs:
      - targets: ['localhost:9090']
    scrape_interval: 15s
    metrics_path: '/metrics'

  # Virtual Queue API
  - job_name: 'virtualqueue-api'
    static_configs:
      - targets: ['virtualqueue-api:8080']
    metrics_path: '/metrics'
    scrape_interval: 10s
    scrape_timeout: 5s
    honor_labels: true

  # PostgreSQL Database
  - job_name: 'postgres'
    static_configs:
      - targets: ['postgres-exporter:9187']
    scrape_interval: 30s
    scrape_timeout: 10s

  # Redis Cache
  - job_name: 'redis'
    static_configs:
      - targets: ['redis-exporter:9121']
    scrape_interval: 30s
    scrape_timeout: 10s

  # Node Exporter (System Metrics)
  - job_name: 'node'
    static_configs:
      - targets: ['node-exporter:9100']
    scrape_interval: 30s
    scrape_timeout: 10s

  # Nginx (if using reverse proxy)
  - job_name: 'nginx'
    static_configs:
      - targets: ['nginx-exporter:9113']
    scrape_interval: 30s
    scrape_timeout: 10s

  # Grafana
  - job_name: 'grafana'
    static_configs:
      - targets: ['grafana:3000']
    scrape_interval: 30s
    scrape_timeout: 10s

  # Alertmanager
  - job_name: 'alertmanager'
    static_configs:
      - targets: ['alertmanager:9093']
    scrape_interval: 30s
    scrape_timeout: 10s
EOF

# Create Grafana datasource configuration
log "Creating Grafana datasource configuration..."
cat > "$MONITORING_DIR/grafana/provisioning/datasources/datasources.yml" << 'EOF'
apiVersion: 1

datasources:
  - name: Prometheus
    type: prometheus
    access: proxy
    url: http://prometheus:9090
    isDefault: true
    editable: true
    jsonData:
      timeInterval: "5s"
      queryTimeout: "60s"
      httpMethod: "POST"
      manageAlerts: true
      alertmanagerUid: "alertmanager"

  - name: PostgreSQL
    type: postgres
    access: proxy
    url: postgres:5432
    database: virtualqueue_prod
    user: virtualqueue_user
    secureJsonData:
      password: ${DB_PASSWORD}
    jsonData:
      sslmode: require
      maxOpenConns: 100
      maxIdleConns: 100
      connMaxLifetime: 14400
      postgresVersion: 15
      timescaledb: false

  - name: Redis
    type: redis-datasource
    access: proxy
    url: redis:6379
    jsonData:
      client: standalone
      poolSize: 5
      timeout: 5
      pingInterval: 0
      pipelineWindow: 0
EOF

# Create Grafana dashboard configuration
log "Creating Grafana dashboard configuration..."
cat > "$MONITORING_DIR/grafana/provisioning/dashboards/dashboards.yml" << 'EOF'
apiVersion: 1

providers:
  - name: 'Virtual Queue Dashboards'
    orgId: 1
    folder: 'Virtual Queue Management System'
    type: file
    disableDeletion: false
    updateIntervalSeconds: 10
    allowUiUpdates: true
    options:
      path: /etc/grafana/provisioning/dashboards
EOF

# Create Alertmanager configuration
log "Creating Alertmanager configuration..."
cat > "$MONITORING_DIR/alertmanager/alertmanager.yml" << 'EOF'
global:
  smtp_smarthost: '${SMTP_SERVER:-localhost:587}'
  smtp_from: '${ALERT_FROM_EMAIL:-alerts@virtualqueue.com}'
  smtp_auth_username: '${SMTP_USERNAME:-alerts@virtualqueue.com}'
  smtp_auth_password: '${SMTP_PASSWORD}'
  smtp_require_tls: true
  slack_api_url: '${SLACK_WEBHOOK_URL}'

route:
  group_by: ['alertname', 'service', 'severity']
  group_wait: 10s
  group_interval: 10s
  repeat_interval: 1h
  receiver: 'default-receiver'
  routes:
    - match:
        severity: critical
      receiver: 'critical-alerts'
      group_wait: 5s
      repeat_interval: 30m
    - match:
        severity: warning
      receiver: 'warning-alerts'
      group_wait: 30s
      repeat_interval: 2h
    - match:
        severity: info
      receiver: 'info-alerts'
      group_wait: 5m
      repeat_interval: 6h

inhibit_rules:
  - source_match:
      severity: 'critical'
    target_match:
      severity: 'warning'
    equal: ['alertname', 'instance']

receivers:
  - name: 'default-receiver'
    email_configs:
      - to: '${DEFAULT_ALERT_EMAIL:-admin@virtualqueue.com}'
        subject: 'Virtual Queue Alert: {{ .GroupLabels.alertname }}'
        body: |
          {{ range .Alerts }}
          Alert: {{ .Annotations.summary }}
          Description: {{ .Annotations.description }}
          Severity: {{ .Labels.severity }}
          Service: {{ .Labels.service }}
          Instance: {{ .Labels.instance }}
          {{ end }}

  - name: 'critical-alerts'
    email_configs:
      - to: '${CRITICAL_ALERT_EMAIL:-critical@virtualqueue.com}'
        subject: '🚨 CRITICAL: {{ .GroupLabels.alertname }}'
        body: |
          {{ range .Alerts }}
          🚨 CRITICAL ALERT 🚨
          Alert: {{ .Annotations.summary }}
          Description: {{ .Annotations.description }}
          Severity: {{ .Labels.severity }}
          Service: {{ .Labels.service }}
          Instance: {{ .Labels.instance }}
          Time: {{ .StartsAt.Format "2006-01-02 15:04:05" }}
          Runbook: {{ .Annotations.runbook_url }}
          {{ end }}
    slack_configs:
      - api_url: '${SLACK_WEBHOOK_URL}'
        channel: '#alerts-critical'
        title: '🚨 CRITICAL ALERT'
        text: |
          {{ range .Alerts }}
          *{{ .Annotations.summary }}*
          {{ .Annotations.description }}
          Service: {{ .Labels.service }}
          {{ end }}
        send_resolved: true

  - name: 'warning-alerts'
    email_configs:
      - to: '${WARNING_ALERT_EMAIL:-ops@virtualqueue.com}'
        subject: '⚠️ WARNING: {{ .GroupLabels.alertname }}'
        body: |
          {{ range .Alerts }}
          ⚠️ WARNING ALERT ⚠️
          Alert: {{ .Annotations.summary }}
          Description: {{ .Annotations.description }}
          Severity: {{ .Labels.severity }}
          Service: {{ .Labels.service }}
          Instance: {{ .Labels.instance }}
          Time: {{ .StartsAt.Format "2006-01-02 15:04:05" }}
          Runbook: {{ .Annotations.runbook_url }}
          {{ end }}

  - name: 'info-alerts'
    email_configs:
      - to: '${INFO_ALERT_EMAIL:-info@virtualqueue.com}'
        subject: 'ℹ️ INFO: {{ .GroupLabels.alertname }}'
        body: |
          {{ range .Alerts }}
          ℹ️ INFO ALERT ℹ️
          Alert: {{ .Annotations.summary }}
          Description: {{ .Annotations.description }}
          Severity: {{ .Labels.severity }}
          Service: {{ .Labels.service }}
          Instance: {{ .Labels.instance }}
          Time: {{ .StartsAt.Format "2006-01-02 15:04:05" }}
          {{ end }}
EOF

# Create systemd service files
log "Creating systemd service files..."

# Prometheus service
cat > /etc/systemd/system/prometheus.service << EOF
[Unit]
Description=Prometheus Server
Documentation=https://prometheus.io/docs/introduction/overview/
Wants=network-online.target
After=network-online.target

[Service]
Type=simple
User=prometheus
Group=prometheus
ExecReload=/bin/kill -HUP \$MAINPID
ExecStart=/usr/local/bin/prometheus \\
  --config.file=$MONITORING_DIR/prometheus/prometheus.yml \\
  --storage.tsdb.path=$MONITORING_DIR/prometheus/data \\
  --web.console.libraries=$MONITORING_DIR/prometheus/console_libraries \\
  --web.console.templates=$MONITORING_DIR/prometheus/consoles \\
  --storage.tsdb.retention.time=$PROMETHEUS_RETENTION \\
  --web.enable-lifecycle \\
  --web.enable-admin-api \\
  --web.listen-address=0.0.0.0:9090 \\
  --web.external-url=http://localhost:9090

SyslogIdentifier=prometheus
Restart=always

[Install]
WantedBy=multi-user.target
EOF

# Grafana service
cat > /etc/systemd/system/grafana-server.service << EOF
[Unit]
Description=Grafana Server
Documentation=https://grafana.com/docs/
Wants=network-online.target
After=network-online.target

[Service]
Type=notify
User=grafana
Group=grafana
ExecStart=/usr/local/bin/grafana-server \\
  --config=/etc/grafana/grafana.ini \\
  --pidfile=/var/run/grafana-server.pid \\
  --packaging=deb

Environment=GF_SECURITY_ADMIN_PASSWORD=$GRAFANA_ADMIN_PASSWORD
Environment=GF_USERS_ALLOW_SIGN_UP=false
Environment=GF_INSTALL_PLUGINS=redis-datasource

SyslogIdentifier=grafana-server
Restart=always

[Install]
WantedBy=multi-user.target
EOF

# Alertmanager service
cat > /etc/systemd/system/alertmanager.service << EOF
[Unit]
Description=Alertmanager
Documentation=https://prometheus.io/docs/alerting/alertmanager/
Wants=network-online.target
After=network-online.target

[Service]
Type=simple
User=alertmanager
Group=alertmanager
ExecStart=/usr/local/bin/alertmanager \\
  --config.file=$MONITORING_DIR/alertmanager/alertmanager.yml \\
  --storage.path=$MONITORING_DIR/alertmanager/data \\
  --web.external-url=http://localhost:9093 \\
  --web.listen-address=0.0.0.0:9093

SyslogIdentifier=alertmanager
Restart=always

[Install]
WantedBy=multi-user.target
EOF

# Reload systemd and enable services
log "Reloading systemd and enabling services..."
systemctl daemon-reload
systemctl enable prometheus grafana-server alertmanager

# Create monitoring users
log "Creating monitoring users..."
useradd --system --no-create-home --shell /bin/false prometheus || true
useradd --system --no-create-home --shell /bin/false grafana || true
useradd --system --no-create-home --shell /bin/false alertmanager || true

# Download and install Prometheus
log "Downloading and installing Prometheus..."
PROMETHEUS_VERSION="2.45.0"
cd /tmp
wget -q "https://github.com/prometheus/prometheus/releases/download/v$PROMETHEUS_VERSION/prometheus-$PROMETHEUS_VERSION.linux-amd64.tar.gz"
tar xzf "prometheus-$PROMETHEUS_VERSION.linux-amd64.tar.gz"
cp "prometheus-$PROMETHEUS_VERSION.linux-amd64/prometheus" /usr/local/bin/
cp "prometheus-$PROMETHEUS_VERSION.linux-amd64/promtool" /usr/local/bin/
chmod +x /usr/local/bin/prometheus /usr/local/bin/promtool

# Download and install Grafana
log "Downloading and installing Grafana..."
GRAFANA_VERSION="10.0.0"
wget -q "https://dl.grafana.com/oss/release/grafana-$GRAFANA_VERSION.linux-amd64.tar.gz"
tar xzf "grafana-$GRAFANA_VERSION.linux-amd64.tar.gz"
cp -r "grafana-$GRAFANA_VERSION"/* /usr/local/
chmod +x /usr/local/bin/grafana-server

# Download and install Alertmanager
log "Downloading and installing Alertmanager..."
ALERTMANAGER_VERSION="0.25.0"
wget -q "https://github.com/prometheus/alertmanager/releases/download/v$ALERTMANAGER_VERSION/alertmanager-$ALERTMANAGER_VERSION.linux-amd64.tar.gz"
tar xzf "alertmanager-$ALERTMANAGER_VERSION.linux-amd64.tar.gz"
cp "alertmanager-$ALERTMANAGER_VERSION/alertmanager" /usr/local/bin/
cp "alertmanager-$ALERTMANAGER_VERSION/amtool" /usr/local/bin/
chmod +x /usr/local/bin/alertmanager /usr/local/bin/amtool

# Create Grafana configuration
log "Creating Grafana configuration..."
mkdir -p /etc/grafana
cat > /etc/grafana/grafana.ini << 'EOF'
[server]
http_port = 3000
domain = localhost
root_url = http://localhost:3000/

[security]
admin_user = admin
admin_password = ${GRAFANA_ADMIN_PASSWORD}

[users]
allow_sign_up = false
auto_assign_org = true
auto_assign_org_role = Viewer

[dashboards]
default_home_dashboard_path = /etc/grafana/provisioning/dashboards

[log]
mode = console
level = info

[metrics]
enabled = true
EOF

# Copy dashboard files
log "Copying dashboard files..."
cp -r monitoring/grafana/dashboards/* "$MONITORING_DIR/grafana/provisioning/dashboards/"
cp -r monitoring/rules/* "$MONITORING_DIR/prometheus/rules/"

# Set proper permissions
chown -R prometheus:prometheus "$MONITORING_DIR/prometheus"
chown -R grafana:grafana "$MONITORING_DIR/grafana" /etc/grafana
chown -R alertmanager:alertmanager "$MONITORING_DIR/alertmanager"

# Start services
log "Starting monitoring services..."
systemctl start prometheus
systemctl start grafana-server
systemctl start alertmanager

# Wait for services to start
sleep 10

# Check service status
log "Checking service status..."
systemctl status prometheus --no-pager
systemctl status grafana-server --no-pager
systemctl status alertmanager --no-pager

# Test Prometheus
log "Testing Prometheus..."
curl -f http://localhost:9090/-/healthy || error "Prometheus health check failed"

# Test Grafana
log "Testing Grafana..."
curl -f http://localhost:3000/api/health || error "Grafana health check failed"

# Test Alertmanager
log "Testing Alertmanager..."
curl -f http://localhost:9093/-/healthy || error "Alertmanager health check failed"

# Create monitoring summary
log "Creating monitoring summary..."
cat > "$MONITORING_DIR/MONITORING_SUMMARY.md" << 'EOF'
# Virtual Queue Management System - Monitoring Setup Complete

## Services Installed

### Prometheus
- **URL**: http://localhost:9090
- **Status**: Running
- **Configuration**: /opt/virtualqueue/monitoring/prometheus/prometheus.yml
- **Data Directory**: /opt/virtualqueue/monitoring/prometheus/data
- **Retention**: 30 days

### Grafana
- **URL**: http://localhost:3000
- **Username**: admin
- **Password**: [Set via GRAFANA_ADMIN_PASSWORD environment variable]
- **Status**: Running
- **Configuration**: /etc/grafana/grafana.ini

### Alertmanager
- **URL**: http://localhost:9093
- **Status**: Running
- **Configuration**: /opt/virtualqueue/monitoring/alertmanager/alertmanager.yml
- **Data Directory**: /opt/virtualqueue/monitoring/alertmanager/data

## Dashboards Available

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

## Alert Rules Configured

### Critical Alerts
- Service down alerts
- High error rates
- Database/Redis failures
- Security incidents
- SLA breaches

### Warning Alerts
- Performance degradation
- Resource usage warnings
- Capacity warnings
- Business metric alerts

### Info Alerts
- Business insights
- Usage patterns
- System status updates

## Next Steps

1. **Configure Email Notifications**
   - Set SMTP server details in environment variables
   - Configure alert email addresses

2. **Configure Slack Notifications**
   - Set SLACK_WEBHOOK_URL environment variable
   - Configure Slack channels for different alert types

3. **Customize Dashboards**
   - Modify dashboard configurations as needed
   - Add custom metrics and visualizations

4. **Set Up Alert Rules**
   - Review and customize alert thresholds
   - Add business-specific alert rules

5. **Monitor and Maintain**
   - Regular monitoring of monitoring system health
   - Update configurations as system evolves

## Service Management

### Start Services
```bash
systemctl start prometheus grafana-server alertmanager
```

### Stop Services
```bash
systemctl stop prometheus grafana-server alertmanager
```

### Restart Services
```bash
systemctl restart prometheus grafana-server alertmanager
```

### Check Status
```bash
systemctl status prometheus grafana-server alertmanager
```

### View Logs
```bash
journalctl -u prometheus -f
journalctl -u grafana-server -f
journalctl -u alertmanager -f
```

## Configuration Files

- **Prometheus**: /opt/virtualqueue/monitoring/prometheus/prometheus.yml
- **Grafana**: /etc/grafana/grafana.ini
- **Alertmanager**: /opt/virtualqueue/monitoring/alertmanager/alertmanager.yml
- **Dashboard Configs**: /opt/virtualqueue/monitoring/grafana/provisioning/
- **Alert Rules**: /opt/virtualqueue/monitoring/prometheus/rules/

## Monitoring Complete! 🎉
EOF

log "Monitoring setup completed successfully!"
log "Prometheus: http://localhost:9090"
log "Grafana: http://localhost:3000 (admin / $GRAFANA_ADMIN_PASSWORD)"
log "Alertmanager: http://localhost:9093"
log "Configuration files: $MONITORING_DIR"
log "Service management: systemctl start/stop/restart prometheus grafana-server alertmanager"

# Cleanup
rm -rf /tmp/prometheus-* /tmp/grafana-* /tmp/alertmanager-*

log "Virtual Queue Management System monitoring is now fully operational! 🚀"
