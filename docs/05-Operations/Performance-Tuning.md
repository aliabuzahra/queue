# Performance Tuning - Virtual Queue Management System

**Document Version:** 1.0  
**Date:** January 15, 2024  
**Author:** Performance Engineer  
**Status:** Draft  
**Phase:** 05 - Operations  
**Priority:** 🟡 Medium  

---

## Performance Tuning Overview

This document provides comprehensive performance tuning guidelines for the Virtual Queue Management System. It covers application performance optimization, database tuning, cache optimization, infrastructure scaling, and monitoring strategies to ensure optimal system performance.

## Performance Tuning Strategy

### **Performance Principles**
- **Measure First**: Always measure before optimizing
- **Identify Bottlenecks**: Focus on the biggest performance impact
- **Iterative Approach**: Make incremental improvements
- **Monitor Continuously**: Continuous performance monitoring
- **Document Changes**: Document all performance changes
- **Test Thoroughly**: Test performance changes thoroughly

### **Performance Metrics**

| Metric Category | Key Metrics | Target Values | Monitoring |
|-----------------|-------------|---------------|------------|
| **Response Time** | API response time, Page load time | < 2 seconds | Continuous |
| **Throughput** | Requests per second, Transactions per second | > 500 req/sec | Continuous |
| **Resource Usage** | CPU, Memory, Disk I/O | < 80% utilization | Continuous |
| **Database Performance** | Query execution time, Connection pool | < 100ms queries | Continuous |
| **Cache Performance** | Hit ratio, Response time | > 90% hit ratio | Continuous |

## Application Performance Tuning

### **API Performance Optimization**

#### **Response Time Optimization**
```csharp
public class PerformanceOptimizedController : ControllerBase
{
    private readonly ILogger<PerformanceOptimizedController> _logger;
    private readonly IMemoryCache _cache;
    private readonly IQueueService _queueService;

    [HttpGet("queues/{id}")]
    public async Task<ActionResult<QueueDto>> GetQueue(Guid id)
    {
        // 1. Check cache first
        var cacheKey = $"queue:{id}";
        if (_cache.TryGetValue(cacheKey, out QueueDto cachedQueue))
        {
            return Ok(cachedQueue);
        }

        // 2. Use async/await properly
        var queue = await _queueService.GetByIdAsync(id);
        if (queue == null)
        {
            return NotFound();
        }

        // 3. Cache the result
        _cache.Set(cacheKey, queue, TimeSpan.FromMinutes(5));

        return Ok(queue);
    }

    [HttpGet("queues")]
    public async Task<ActionResult<PagedResult<QueueDto>>> GetQueues(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 20)
    {
        // 1. Validate input parameters
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        // 2. Use pagination to limit data transfer
        var result = await _queueService.GetPagedAsync(page, pageSize);
        
        return Ok(result);
    }
}
```

#### **Memory Optimization**
```csharp
public class MemoryOptimizedService
{
    private readonly ILogger<MemoryOptimizedService> _logger;
    private readonly ObjectPool<StringBuilder> _stringBuilderPool;

    public MemoryOptimizedService(ILogger<MemoryOptimizedService> logger)
    {
        _logger = logger;
        
        // Use object pooling for frequently created objects
        var provider = new DefaultObjectPoolProvider();
        _stringBuilderPool = provider.CreateStringBuilderPool();
    }

    public async Task<string> ProcessLargeDataAsync(IEnumerable<QueueEvent> events)
    {
        // Use pooled StringBuilder to reduce allocations
        var sb = _stringBuilderPool.Get();
        try
        {
            foreach (var evt in events)
            {
                sb.AppendLine($"{evt.Id}: {evt.EventType}");
            }
            return sb.ToString();
        }
        finally
        {
            _stringBuilderPool.Return(sb);
        }
    }

    // Implement IDisposable for proper resource cleanup
    public void Dispose()
    {
        // Clean up resources
    }
}
```

### **Database Performance Tuning**

#### **Query Optimization**
```sql
-- Optimized queue queries
-- 1. Use proper indexing
CREATE INDEX CONCURRENTLY idx_queues_tenant_status 
ON Queues (TenantId, Status) 
WHERE Status = 'Active';

-- 2. Optimize complex queries
EXPLAIN (ANALYZE, BUFFERS) 
SELECT q.Id, q.Name, q.Capacity, 
       COUNT(us.Id) as ActiveSessions
FROM Queues q
LEFT JOIN UserSessions us ON q.Id = us.QueueId 
    AND us.Status = 'Waiting'
WHERE q.TenantId = @tenantId 
    AND q.Status = 'Active'
GROUP BY q.Id, q.Name, q.Capacity
ORDER BY q.Priority DESC, q.Name;

-- 3. Use materialized views for complex aggregations
CREATE MATERIALIZED VIEW mv_queue_statistics AS
SELECT 
    q.Id as QueueId,
    q.TenantId,
    COUNT(us.Id) as TotalSessions,
    AVG(us.WaitTime) as AvgWaitTime,
    MAX(us.CreatedAt) as LastActivity
FROM Queues q
LEFT JOIN UserSessions us ON q.Id = us.QueueId
GROUP BY q.Id, q.TenantId;

-- Refresh materialized view periodically
REFRESH MATERIALIZED VIEW CONCURRENTLY mv_queue_statistics;
```

#### **Connection Pool Optimization**
```csharp
public class OptimizedDbContext : VirtualQueueDbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql(connectionString, options =>
            {
                // Connection pool settings
                options.CommandTimeout(30);
                options.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null);
            });

            // Enable query splitting for better performance
            optionsBuilder.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure indexes for performance
        modelBuilder.Entity<Queue>()
            .HasIndex(q => new { q.TenantId, q.Status })
            .HasDatabaseName("idx_queues_tenant_status");

        modelBuilder.Entity<UserSession>()
            .HasIndex(us => new { us.QueueId, us.Status })
            .HasDatabaseName("idx_sessions_queue_status");

        // Configure query filters for multi-tenancy
        modelBuilder.Entity<Queue>()
            .HasQueryFilter(q => EF.Property<Guid>(q, "TenantId") == CurrentTenantId);
    }
}
```

### **Cache Performance Optimization**

#### **Redis Cache Optimization**
```csharp
public class OptimizedCacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<OptimizedCacheService> _logger;

    public OptimizedCacheService(IConnectionMultiplexer redis, ILogger<OptimizedCacheService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null)
    {
        var database = _redis.GetDatabase();
        
        // Try to get from cache first
        var cachedValue = await database.StringGetAsync(key);
        if (cachedValue.HasValue)
        {
            return JsonSerializer.Deserialize<T>(cachedValue);
        }

        // Generate value if not in cache
        var value = await factory();
        
        // Set in cache with compression for large objects
        var serializedValue = JsonSerializer.Serialize(value);
        if (serializedValue.Length > 1024) // Compress large values
        {
            var compressed = CompressString(serializedValue);
            await database.StringSetAsync($"{key}:compressed", compressed, expiry);
        }
        else
        {
            await database.StringSetAsync(key, serializedValue, expiry);
        }

        return value;
    }

    public async Task InvalidatePatternAsync(string pattern)
    {
        var database = _redis.GetDatabase();
        var server = _redis.GetServer(_redis.GetEndPoints().First());
        
        var keys = server.Keys(pattern: pattern);
        await database.KeyDeleteAsync(keys.ToArray());
    }

    private byte[] CompressString(string text)
    {
        var bytes = Encoding.UTF8.GetBytes(text);
        using var output = new MemoryStream();
        using (var gzip = new GZipStream(output, CompressionLevel.Optimal))
        {
            gzip.Write(bytes, 0, bytes.Length);
        }
        return output.ToArray();
    }
}
```

## Infrastructure Performance Tuning

### **Server Performance Optimization**

#### **System Resource Optimization**
```bash
#!/bin/bash
# system-performance-tuning.sh

echo "System Performance Tuning"
echo "========================="

# 1. CPU Performance Tuning
echo "1. CPU Performance Tuning..."

# Set CPU governor to performance
echo performance | sudo tee /sys/devices/system/cpu/cpu*/cpufreq/scaling_governor

# Disable CPU frequency scaling
sudo systemctl disable ondemand
sudo systemctl enable performance

# 2. Memory Performance Tuning
echo "2. Memory Performance Tuning..."

# Optimize memory settings
echo 'vm.swappiness=10' | sudo tee -a /etc/sysctl.conf
echo 'vm.vfs_cache_pressure=50' | sudo tee -a /etc/sysctl.conf
echo 'vm.dirty_ratio=15' | sudo tee -a /etc/sysctl.conf
echo 'vm.dirty_background_ratio=5' | sudo tee -a /etc/sysctl.conf

# Apply settings
sudo sysctl -p

# 3. Network Performance Tuning
echo "3. Network Performance Tuning..."

# Optimize network settings
echo 'net.core.rmem_max=16777216' | sudo tee -a /etc/sysctl.conf
echo 'net.core.wmem_max=16777216' | sudo tee -a /etc/sysctl.conf
echo 'net.ipv4.tcp_rmem=4096 65536 16777216' | sudo tee -a /etc/sysctl.conf
echo 'net.ipv4.tcp_wmem=4096 65536 16777216' | sudo tee -a /etc/sysctl.conf
echo 'net.core.netdev_max_backlog=5000' | sudo tee -a /etc/sysctl.conf

# Apply settings
sudo sysctl -p

# 4. Disk I/O Performance Tuning
echo "4. Disk I/O Performance Tuning..."

# Optimize disk settings
echo 'vm.dirty_writeback_centisecs=1500' | sudo tee -a /etc/sysctl.conf
echo 'vm.dirty_expire_centisecs=3000' | sudo tee -a /etc/sysctl.conf

# Apply settings
sudo sysctl -p

echo "System performance tuning completed"
```

#### **Docker Performance Optimization**
```yaml
# docker-compose.performance.yml
version: '3.8'

services:
  api-prod:
    image: virtualqueue-api:latest
    deploy:
      resources:
        limits:
          memory: 2G
          cpus: '2.0'
        reservations:
          memory: 1G
          cpus: '1.0'
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - DOTNET_GCHeapHardLimit=0x80000000  # 2GB heap limit
      - DOTNET_GCAllowVeryLargeObjects=1
      - DOTNET_GCLOHThreshold=100000
    ulimits:
      nproc: 65535
      nofile:
        soft: 65535
        hard: 65535
    sysctls:
      - net.core.somaxconn=65535
      - net.ipv4.tcp_max_syn_backlog=65535
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:80/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 60s

  db-prod:
    image: postgres:15
    deploy:
      resources:
        limits:
          memory: 4G
          cpus: '4.0'
        reservations:
          memory: 2G
          cpus: '2.0'
    environment:
      - POSTGRES_SHARED_BUFFERS=1GB
      - POSTGRES_EFFECTIVE_CACHE_SIZE=3GB
      - POSTGRES_WORK_MEM=64MB
      - POSTGRES_MAINTENANCE_WORK_MEM=256MB
      - POSTGRES_CHECKPOINT_COMPLETION_TARGET=0.9
      - POSTGRES_WAL_BUFFERS=16MB
      - POSTGRES_DEFAULT_STATISTICS_TARGET=100
    command: >
      postgres
      -c shared_buffers=1GB
      -c effective_cache_size=3GB
      -c work_mem=64MB
      -c maintenance_work_mem=256MB
      -c checkpoint_completion_target=0.9
      -c wal_buffers=16MB
      -c default_statistics_target=100
      -c random_page_cost=1.1
      -c effective_io_concurrency=200

  redis-prod:
    image: redis:7-alpine
    deploy:
      resources:
        limits:
          memory: 2G
          cpus: '2.0'
        reservations:
          memory: 1G
          cpus: '1.0'
    command: >
      redis-server
      --maxmemory 1gb
      --maxmemory-policy allkeys-lru
      --tcp-keepalive 60
      --timeout 300
      --tcp-backlog 511
      --databases 16
```

### **Load Balancing Optimization**

#### **Nginx Load Balancer Configuration**
```nginx
# nginx.conf
worker_processes auto;
worker_rlimit_nofile 65535;

events {
    worker_connections 4096;
    use epoll;
    multi_accept on;
}

http {
    # Performance optimizations
    sendfile on;
    tcp_nopush on;
    tcp_nodelay on;
    keepalive_timeout 65;
    keepalive_requests 1000;
    
    # Gzip compression
    gzip on;
    gzip_vary on;
    gzip_min_length 1024;
    gzip_proxied any;
    gzip_comp_level 6;
    gzip_types
        text/plain
        text/css
        text/xml
        text/javascript
        application/json
        application/javascript
        application/xml+rss
        application/atom+xml
        image/svg+xml;

    # Rate limiting
    limit_req_zone $binary_remote_addr zone=api:10m rate=10r/s;
    limit_req_zone $binary_remote_addr zone=login:10m rate=1r/s;

    # Upstream configuration
    upstream api_backend {
        least_conn;
        server api1:80 max_fails=3 fail_timeout=30s;
        server api2:80 max_fails=3 fail_timeout=30s;
        server api3:80 max_fails=3 fail_timeout=30s;
        keepalive 32;
    }

    server {
        listen 80;
        server_name api.virtualqueue.com;

        # Rate limiting
        limit_req zone=api burst=20 nodelay;

        location / {
            proxy_pass http://api_backend;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
            
            # Connection optimization
            proxy_http_version 1.1;
            proxy_set_header Connection "";
            proxy_buffering on;
            proxy_buffer_size 4k;
            proxy_buffers 8 4k;
            
            # Timeouts
            proxy_connect_timeout 5s;
            proxy_send_timeout 60s;
            proxy_read_timeout 60s;
        }

        location /health {
            access_log off;
            proxy_pass http://api_backend;
        }
    }
}
```

## Performance Monitoring

### **Performance Monitoring Script**

```bash
#!/bin/bash
# performance-monitoring.sh

echo "Performance Monitoring Report"
echo "============================="

# 1. System Performance Metrics
echo "1. System Performance Metrics:"
echo "CPU Usage:"
top -bn1 | grep "Cpu(s)" | awk '{print $2}' | cut -d'%' -f1

echo "Memory Usage:"
free | awk 'NR==2{printf "%.0f%%", $3*100/$2}'

echo "Disk I/O:"
iostat -x 1 1 | tail -n +4

# 2. Application Performance Metrics
echo "2. Application Performance Metrics:"
API_RESPONSE_TIME=$(curl -s -o /dev/null -w "%{time_total}" http://localhost:80/health)
echo "API Response Time: ${API_RESPONSE_TIME}s"

# 3. Database Performance Metrics
echo "3. Database Performance Metrics:"
docker-compose exec db-prod psql -U postgres -d VirtualQueue -c "
SELECT 
    query,
    mean_time,
    calls,
    total_time
FROM pg_stat_statements
ORDER BY mean_time DESC
LIMIT 5;"

# 4. Cache Performance Metrics
echo "4. Cache Performance Metrics:"
docker-compose exec redis-prod redis-cli info stats | grep -E "(keyspace_hits|keyspace_misses)"

# 5. Network Performance Metrics
echo "5. Network Performance Metrics:"
netstat -i

# 6. Performance Alerts
echo "6. Performance Alerts:"
CPU_USAGE=$(top -bn1 | grep "Cpu(s)" | awk '{print $2}' | cut -d'%' -f1)
if (( $(echo "$CPU_USAGE > 80" | bc -l) )); then
    echo "⚠️ High CPU usage: ${CPU_USAGE}%"
fi

MEMORY_USAGE=$(free | awk 'NR==2{printf "%.0f", $3*100/$2}')
if [ "$MEMORY_USAGE" -gt 80 ]; then
    echo "⚠️ High memory usage: ${MEMORY_USAGE}%"
fi

if (( $(echo "$API_RESPONSE_TIME > 2.0" | bc -l) )); then
    echo "⚠️ Slow API response time: ${API_RESPONSE_TIME}s"
fi

echo "Performance monitoring completed"
```

### **Performance Alerting**

#### **Performance Alert Script**
```bash
#!/bin/bash
# performance-alerts.sh

# Performance thresholds
CPU_THRESHOLD=80
MEMORY_THRESHOLD=80
RESPONSE_TIME_THRESHOLD=2.0
DISK_USAGE_THRESHOLD=85

# Get current metrics
CPU_USAGE=$(top -bn1 | grep "Cpu(s)" | awk '{print $2}' | cut -d'%' -f1)
MEMORY_USAGE=$(free | awk 'NR==2{printf "%.0f", $3*100/$2}')
API_RESPONSE_TIME=$(curl -s -o /dev/null -w "%{time_total}" http://localhost:80/health)
DISK_USAGE=$(df -h / | awk 'NR==2 {print $5}' | sed 's/%//')

# Check thresholds and send alerts
if (( $(echo "$CPU_USAGE > $CPU_THRESHOLD" | bc -l) )); then
    curl -X POST "https://hooks.slack.com/services/YOUR/SLACK/WEBHOOK" \
      -H "Content-Type: application/json" \
      -d "{\"text\":\"🚨 High CPU usage: ${CPU_USAGE}%\"}"
fi

if [ "$MEMORY_USAGE" -gt "$MEMORY_THRESHOLD" ]; then
    curl -X POST "https://hooks.slack.com/services/YOUR/SLACK/WEBHOOK" \
      -H "Content-Type: application/json" \
      -d "{\"text\":\"🚨 High memory usage: ${MEMORY_USAGE}%\"}"
fi

if (( $(echo "$API_RESPONSE_TIME > $RESPONSE_TIME_THRESHOLD" | bc -l) )); then
    curl -X POST "https://hooks.slack.com/services/YOUR/SLACK/WEBHOOK" \
      -H "Content-Type: application/json" \
      -d "{\"text\":\"🚨 Slow API response time: ${API_RESPONSE_TIME}s\"}"
fi

if [ "$DISK_USAGE" -gt "$DISK_USAGE_THRESHOLD" ]; then
    curl -X POST "https://hooks.slack.com/services/YOUR/SLACK/WEBHOOK" \
      -H "Content-Type: application/json" \
      -d "{\"text\":\"🚨 High disk usage: ${DISK_USAGE}%\"}"
fi
```

## Performance Testing

### **Load Testing Script**

```bash
#!/bin/bash
# load-testing.sh

echo "Load Testing"
echo "============"

# 1. Baseline Performance Test
echo "1. Baseline Performance Test..."
ab -n 1000 -c 10 http://localhost:80/health

# 2. API Load Test
echo "2. API Load Test..."
ab -n 5000 -c 50 http://localhost:80/api/queues

# 3. Database Load Test
echo "3. Database Load Test..."
for i in {1..100}; do
    curl -s http://localhost:80/api/queues > /dev/null &
done
wait

# 4. Memory Load Test
echo "4. Memory Load Test..."
stress --cpu 4 --timeout 60s

# 5. Network Load Test
echo "5. Network Load Test..."
iperf3 -c localhost -t 30

echo "Load testing completed"
```

### **Performance Benchmarking**

#### **Benchmark Script**
```bash
#!/bin/bash
# performance-benchmark.sh

echo "Performance Benchmark"
echo "====================="

# 1. API Response Time Benchmark
echo "1. API Response Time Benchmark:"
for endpoint in "/health" "/api/queues" "/api/users" "/api/sessions"; do
    echo "Testing $endpoint..."
    ab -n 100 -c 1 "http://localhost:80$endpoint" | grep "Time per request"
done

# 2. Database Query Benchmark
echo "2. Database Query Benchmark:"
docker-compose exec db-prod psql -U postgres -d VirtualQueue -c "
\timing on
SELECT COUNT(*) FROM Queues;
SELECT COUNT(*) FROM UserSessions;
SELECT COUNT(*) FROM QueueEvents;
"

# 3. Cache Performance Benchmark
echo "3. Cache Performance Benchmark:"
docker-compose exec redis-prod redis-cli --latency-history -i 1

# 4. Memory Usage Benchmark
echo "4. Memory Usage Benchmark:"
docker stats --no-stream

echo "Performance benchmark completed"
```

## Performance Optimization Checklist

### **Performance Optimization Checklist**

- [ ] **Application Optimization**
  - [ ] Implement proper caching strategies
  - [ ] Optimize database queries
  - [ ] Use async/await properly
  - [ ] Implement connection pooling
  - [ ] Optimize memory usage
  - [ ] Use object pooling where appropriate

- [ ] **Database Optimization**
  - [ ] Create proper indexes
  - [ ] Optimize query execution plans
  - [ ] Configure connection pooling
  - [ ] Implement query caching
  - [ ] Use materialized views for complex queries
  - [ ] Regular database maintenance

- [ ] **Cache Optimization**
  - [ ] Implement Redis caching
  - [ ] Optimize cache hit ratios
  - [ ] Use appropriate cache expiration
  - [ ] Implement cache invalidation strategies
  - [ ] Monitor cache performance

- [ ] **Infrastructure Optimization**
  - [ ] Optimize server resources
  - [ ] Configure load balancing
  - [ ] Implement CDN for static assets
  - [ ] Optimize network settings
  - [ ] Configure proper monitoring

- [ ] **Monitoring and Alerting**
  - [ ] Implement performance monitoring
  - [ ] Set up performance alerts
  - [ ] Regular performance testing
  - [ ] Performance benchmarking
  - [ ] Continuous performance optimization

## Performance Best Practices

### **Performance Best Practices**

1. **Measure Before Optimizing**: Always measure performance before making changes
2. **Focus on Bottlenecks**: Identify and address the biggest performance bottlenecks
3. **Use Profiling Tools**: Use profiling tools to identify performance issues
4. **Implement Caching**: Implement appropriate caching strategies
5. **Optimize Database Queries**: Optimize database queries and indexes
6. **Use Connection Pooling**: Implement connection pooling for databases
7. **Monitor Continuously**: Implement continuous performance monitoring
8. **Test Performance Changes**: Test all performance changes thoroughly
9. **Document Performance**: Document all performance optimizations
10. **Regular Performance Reviews**: Conduct regular performance reviews

### **Performance Anti-Patterns**

1. **Premature Optimization**: Optimizing before measuring
2. **Ignoring Bottlenecks**: Focusing on minor optimizations while ignoring major bottlenecks
3. **No Monitoring**: Not implementing performance monitoring
4. **Poor Caching**: Implementing ineffective caching strategies
5. **Inefficient Queries**: Using inefficient database queries
6. **Resource Leaks**: Not properly managing resources
7. **No Testing**: Not testing performance changes
8. **Ignoring Scalability**: Not considering scalability requirements

## Approval and Sign-off

### **Performance Tuning Approval**
- **Performance Engineer**: [Name] - [Date]
- **Technical Lead**: [Name] - [Date]
- **DevOps Lead**: [Name] - [Date]
- **Operations Manager**: [Name] - [Date]

### **Document Control**
- **Version**: 1.0
- **Last Updated**: January 15, 2024
- **Next Review**: February 15, 2024
- **Distribution**: Development Team, DevOps Team, Operations Team

---

**Document Status**: Draft  
**Next Phase**: Disaster Recovery  
**Dependencies**: Performance monitoring setup, load testing validation
