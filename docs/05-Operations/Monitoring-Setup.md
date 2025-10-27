# Monitoring Setup - Virtual Queue Management System

**Document Version:** 1.0  
**Date:** January 15, 2024  
**Author:** DevOps Lead  
**Status:** Draft  
**Phase:** 5 - Operations  
**Priority:** 🔴 Critical  

---

## Executive Summary

This document provides comprehensive guidance for setting up monitoring infrastructure for the Virtual Queue Management System. It covers metrics collection, dashboard configuration, alerting rules, and performance monitoring to ensure optimal system visibility and proactive issue detection.

## Monitoring Architecture

### **Monitoring Stack**

#### **Core Components**
- **Prometheus**: Metrics collection and storage
- **Grafana**: Visualization and dashboards
- **AlertManager**: Alert routing and notification
- **Jaeger**: Distributed tracing
- **ELK Stack**: Log aggregation and analysis
- **Application Insights**: Application performance monitoring

#### **Monitoring Layers**
```
Application Layer
├── Business Metrics (Queue operations, user sessions)
├── Application Metrics (API performance, errors)
└── Custom Metrics (Business KPIs)

Infrastructure Layer
├── System Metrics (CPU, memory, disk)
├── Network Metrics (bandwidth, latency)
└── Database Metrics (connections, queries)

Service Layer
├── API Metrics (endpoints, response times)
├── Database Metrics (performance, connections)
└── External Service Metrics (Redis, PostgreSQL)
```

### **Metrics Collection**

#### **Application Metrics**
```csharp
// Prometheus metrics configuration
public class QueueMetrics
{
    private readonly Counter _queueCreatedTotal;
    private readonly Counter _queueDeletedTotal;
    private readonly Histogram _queueOperationDuration;
    private readonly Gauge _activeQueues;
    private readonly Gauge _totalUsersInQueues;

    public QueueMetrics()
    {
        _queueCreatedTotal = Metrics.CreateCounter("queue_created_total", "Total number of queues created");
        _queueDeletedTotal = Metrics.CreateCounter("queue_deleted_total", "Total number of queues deleted");
        _queueOperationDuration = Metrics.CreateHistogram("queue_operation_duration_seconds", "Duration of queue operations");
        _activeQueues = Metrics.CreateGauge("active_queues", "Number of active queues");
        _totalUsersInQueues = Metrics.CreateGauge("total_users_in_queues", "Total number of users in all queues");
    }

    public void RecordQueueCreated(string tenantId)
    {
        _queueCreatedTotal.WithLabels(tenantId).Inc();
    }

    public void RecordQueueOperationDuration(double duration)
    {
        _queueOperationDuration.Observe(duration);
    }
}
```

#### **Infrastructure Metrics**
```yaml
# Prometheus configuration
global:
  scrape_interval: 15s
  evaluation_interval: 15s

scrape_configs:
  - job_name: 'virtual-queue-api'
    static_configs:
      - targets: ['api:80']
    metrics_path: '/metrics'
    scrape_interval: 5s

  - job_name: 'postgres'
    static_configs:
      - targets: ['postgres-exporter:9187']

  - job_name: 'redis'
    static_configs:
      - targets: ['redis-exporter:9121']

  - job_name: 'node-exporter'
    static_configs:
      - targets: ['node-exporter:9100']
```

## Dashboard Configuration

### **Grafana Dashboards**

#### **System Overview Dashboard**
```json
{
  "dashboard": {
    "title": "Virtual Queue System Overview",
    "panels": [
      {
        "title": "System Health",
        "type": "stat",
        "targets": [
          {
            "expr": "up{job=\"virtual-queue-api\"}",
            "legendFormat": "API Status"
          }
        ]
      },
      {
        "title": "Active Queues",
        "type": "stat",
        "targets": [
          {
            "expr": "active_queues",
            "legendFormat": "Active Queues"
          }
        ]
      },
      {
        "title": "Total Users in Queues",
        "type": "stat",
        "targets": [
          {
            "expr": "total_users_in_queues",
            "legendFormat": "Total Users"
          }
        ]
      }
    ]
  }
}
```

#### **Performance Dashboard**
```json
{
  "dashboard": {
    "title": "Performance Metrics",
    "panels": [
      {
        "title": "API Response Time",
        "type": "graph",
        "targets": [
          {
            "expr": "histogram_quantile(0.95, rate(http_request_duration_seconds_bucket[5m]))",
            "legendFormat": "95th Percentile"
          },
          {
            "expr": "histogram_quantile(0.50, rate(http_request_duration_seconds_bucket[5m]))",
            "legendFormat": "50th Percentile"
          }
        ]
      },
      {
        "title": "Request Rate",
        "type": "graph",
        "targets": [
          {
            "expr": "rate(http_requests_total[5m])",
            "legendFormat": "Requests/sec"
          }
        ]
      }
    ]
  }
}
```

### **Custom Dashboards**

#### **Business Metrics Dashboard**
- **Queue Operations**: Queue creation, deletion, updates
- **User Sessions**: Active sessions, session duration
- **Queue Performance**: Wait times, throughput
- **Tenant Metrics**: Per-tenant performance
- **Business KPIs**: Key business indicators

#### **Infrastructure Dashboard**
- **System Resources**: CPU, memory, disk usage
- **Network Performance**: Bandwidth, latency
- **Database Performance**: Query times, connections
- **Cache Performance**: Redis metrics
- **Service Health**: Service availability

## Alerting Configuration

### **Alert Rules**

#### **Critical Alerts**
```yaml
# Critical alert rules
groups:
  - name: critical
    rules:
      - alert: APIDown
        expr: up{job="virtual-queue-api"} == 0
        for: 1m
        labels:
          severity: critical
        annotations:
          summary: "Virtual Queue API is down"
          description: "The Virtual Queue API has been down for more than 1 minute"

      - alert: HighErrorRate
        expr: rate(http_requests_total{status=~"5.."}[5m]) > 0.1
        for: 2m
        labels:
          severity: critical
        annotations:
          summary: "High error rate detected"
          description: "Error rate is above 10% for more than 2 minutes"

      - alert: DatabaseDown
        expr: up{job="postgres"} == 0
        for: 1m
        labels:
          severity: critical
        annotations:
          summary: "PostgreSQL database is down"
          description: "The PostgreSQL database has been down for more than 1 minute"
```

#### **Warning Alerts**
```yaml
# Warning alert rules
groups:
  - name: warning
    rules:
      - alert: HighResponseTime
        expr: histogram_quantile(0.95, rate(http_request_duration_seconds_bucket[5m])) > 2
        for: 5m
        labels:
          severity: warning
        annotations:
          summary: "High response time detected"
          description: "95th percentile response time is above 2 seconds"

      - alert: HighMemoryUsage
        expr: (node_memory_MemTotal_bytes - node_memory_MemAvailable_bytes) / node_memory_MemTotal_bytes > 0.8
        for: 10m
        labels:
          severity: warning
        annotations:
          summary: "High memory usage"
          description: "Memory usage is above 80% for more than 10 minutes"

      - alert: LowDiskSpace
        expr: (node_filesystem_avail_bytes / node_filesystem_size_bytes) < 0.1
        for: 5m
        labels:
          severity: warning
        annotations:
          summary: "Low disk space"
          description: "Disk space is below 10%"
```

### **Alert Routing**

#### **AlertManager Configuration**
```yaml
# AlertManager configuration
global:
  smtp_smarthost: 'smtp.company.com:587'
  smtp_from: 'alerts@company.com'

route:
  group_by: ['alertname']
  group_wait: 10s
  group_interval: 10s
  repeat_interval: 1h
  receiver: 'default'
  routes:
    - match:
        severity: critical
      receiver: 'critical-alerts'
    - match:
        severity: warning
      receiver: 'warning-alerts'

receivers:
  - name: 'default'
    email_configs:
      - to: 'ops-team@company.com'
        subject: 'Virtual Queue Alert: {{ .GroupLabels.alertname }}'
        body: |
          {{ range .Alerts }}
          Alert: {{ .Annotations.summary }}
          Description: {{ .Annotations.description }}
          {{ end }}

  - name: 'critical-alerts'
    email_configs:
      - to: 'critical-alerts@company.com'
        subject: 'CRITICAL: Virtual Queue Alert'
    slack_configs:
      - api_url: 'https://hooks.slack.com/services/...'
        channel: '#critical-alerts'
        title: 'Critical Alert'
        text: '{{ range .Alerts }}{{ .Annotations.summary }}{{ end }}'

  - name: 'warning-alerts'
    email_configs:
      - to: 'warning-alerts@company.com'
        subject: 'WARNING: Virtual Queue Alert'
    slack_configs:
      - api_url: 'https://hooks.slack.com/services/...'
        channel: '#warning-alerts'
        title: 'Warning Alert'
        text: '{{ range .Alerts }}{{ .Annotations.summary }}{{ end }}'
```

## Log Management

### **Log Aggregation**

#### **ELK Stack Configuration**
```yaml
# Elasticsearch configuration
elasticsearch:
  hosts: ["elasticsearch:9200"]
  index: "virtual-queue-logs-%{+YYYY.MM.dd}"

# Logstash configuration
input {
  beats {
    port => 5044
  }
}

filter {
  if [fields][service] == "virtual-queue-api" {
    grok {
      match => { "message" => "%{TIMESTAMP_ISO8601:timestamp} %{LOGLEVEL:level} %{DATA:logger} %{GREEDYDATA:message}" }
    }
  }
}

output {
  elasticsearch {
    hosts => ["elasticsearch:9200"]
    index => "virtual-queue-logs-%{+YYYY.MM.dd}"
  }
}
```

#### **Application Logging**
```csharp
// Serilog configuration
public class Program
{
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "VirtualQueue.Api")
            .WriteTo.Console()
            .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://elasticsearch:9200"))
            {
                IndexFormat = "virtual-queue-logs-{0:yyyy.MM.dd}",
                AutoRegisterTemplate = true
            })
            .CreateLogger();

        CreateHostBuilder(args).Build().Run();
    }
}
```

### **Log Analysis**

#### **Log Queries**
```json
// Elasticsearch queries for log analysis
{
  "query": {
    "bool": {
      "must": [
        {
          "range": {
            "@timestamp": {
              "gte": "now-1h"
            }
          }
        },
        {
          "term": {
            "level": "ERROR"
          }
        }
      ]
    }
  }
}
```

## Performance Monitoring

### **Performance Metrics**

#### **Key Performance Indicators**
- **Response Time**: API endpoint response times
- **Throughput**: Requests per second
- **Error Rate**: Percentage of failed requests
- **Queue Performance**: Queue processing times
- **Database Performance**: Query execution times
- **Cache Performance**: Cache hit/miss ratios

#### **Performance Thresholds**
```yaml
# Performance thresholds
performance_thresholds:
  api_response_time:
    warning: 1.0s
    critical: 2.0s
  
  error_rate:
    warning: 5%
    critical: 10%
  
  queue_processing_time:
    warning: 30s
    critical: 60s
  
  database_query_time:
    warning: 500ms
    critical: 1000ms
```

### **Performance Analysis**

#### **Performance Dashboards**
- **Real-time Performance**: Current performance metrics
- **Historical Performance**: Performance trends over time
- **Performance Comparison**: Compare performance across environments
- **Performance Anomalies**: Detect performance anomalies
- **Performance Forecasting**: Predict future performance needs

## Security Monitoring

### **Security Metrics**

#### **Security KPIs**
- **Authentication Failures**: Failed login attempts
- **Authorization Violations**: Unauthorized access attempts
- **Suspicious Activity**: Unusual access patterns
- **Security Events**: Security-related events
- **Compliance Metrics**: Compliance-related metrics

#### **Security Alerts**
```yaml
# Security alert rules
groups:
  - name: security
    rules:
      - alert: HighAuthenticationFailures
        expr: rate(authentication_failures_total[5m]) > 10
        for: 2m
        labels:
          severity: warning
        annotations:
          summary: "High authentication failure rate"
          description: "Authentication failure rate is above 10 per minute"

      - alert: SuspiciousActivity
        expr: rate(suspicious_activity_total[5m]) > 5
        for: 1m
        labels:
          severity: critical
        annotations:
          summary: "Suspicious activity detected"
          description: "Suspicious activity rate is above 5 per minute"
```

## Monitoring Best Practices

### **Monitoring Guidelines**

#### **Metric Design**
- **Meaningful Metrics**: Use meaningful metric names
- **Consistent Labels**: Use consistent label naming
- **Appropriate Granularity**: Choose appropriate metric granularity
- **Avoid High Cardinality**: Avoid high cardinality metrics
- **Document Metrics**: Document all metrics and their purpose

#### **Alert Design**
- **Clear Alerts**: Use clear, actionable alert messages
- **Appropriate Thresholds**: Set appropriate alert thresholds
- **Avoid Alert Fatigue**: Avoid too many alerts
- **Test Alerts**: Test alerts regularly
- **Review Alerts**: Review and update alerts regularly

### **Monitoring Maintenance**

#### **Regular Maintenance**
- **Metric Review**: Review metrics regularly
- **Alert Tuning**: Tune alerts based on experience
- **Dashboard Updates**: Update dashboards as needed
- **Performance Optimization**: Optimize monitoring performance
- **Documentation Updates**: Keep documentation current

## Monitoring Troubleshooting

### **Common Issues**

#### **Monitoring Issues**
- **Missing Metrics**: Check metric collection configuration
- **Alert Noise**: Tune alert thresholds
- **Performance Impact**: Optimize monitoring overhead
- **Data Retention**: Configure appropriate data retention
- **Dashboard Performance**: Optimize dashboard queries

#### **Troubleshooting Steps**
1. **Check Collection**: Verify metric collection is working
2. **Check Storage**: Verify metrics are being stored
3. **Check Visualization**: Verify dashboards are working
4. **Check Alerting**: Verify alerts are being sent
5. **Check Performance**: Verify monitoring performance

## Approval and Sign-off

### **Monitoring Setup Approval**
- **DevOps Lead**: [Name] - [Date]
- **Operations Team**: [Name] - [Date]
- **Security Team**: [Name] - [Date]
- **Management**: [Name] - [Date]

### **Document Control**
- **Version**: 1.0
- **Last Updated**: January 15, 2024
- **Next Review**: February 15, 2024
- **Distribution**: Operations Team, DevOps Team, Security Team

---

**Document Status**: Draft  
**Next Phase**: Incident Management  
**Dependencies**: Infrastructure setup, monitoring tool selection, alerting configuration
