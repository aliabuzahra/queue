# Grafana Setup and Verification Guide

## Quick Access

**URL**: http://localhost:3000  
**Username**: `admin`  
**Password**: `admin123`

---

## Step 1: Login to Grafana

1. Open your browser: http://localhost:3000
2. Enter credentials:
   - Username: `admin`
   - Password: `admin123`
3. Click "Log in"

---

## Step 2: Check Data Sources

### Navigate to Data Sources
1. Click the ⚙️ (cog) icon in the left sidebar
2. Select **"Data Sources"**
3. You should see:
   - ✅ **Prometheus** (marked as default)
   - ✅ **PostgreSQL**
   - ✅ **Redis**

### Verify Prometheus Connection
1. Click on **"Prometheus"**
2. Click **"Save & Test"** button
3. You should see: ✅ **"Data source is working"**

### Test Query
1. Click **"Explore"** in left sidebar
2. Run query: `up`
3. You should see metrics from all targets

---

## Step 3: Check Dashboards

### Access Dashboards
1. Click **"Dashboards"** in left sidebar
2. Click **"Virtual Queue Dashboards"**
3. You should see pre-configured dashboards:
   - Virtual Queue API Dashboard
   - Virtual Queue Business Metrics
   - Virtual Queue Infrastructure

### View Specific Dashboard
1. Navigate to: **Virtual Queue API Dashboard**
2. You should see:
   - HTTP request metrics
   - API response times
   - Active user sessions
   - Database connection status
   - Cache hit/miss rates

---

## Step 4: Query Metrics Directly

### Using Grafana Explore
1. Click **"Explore"** (compass icon)
2. Select **"Prometheus"** from dropdown
3. Try these queries:

#### Check if targets are up:
```promql
up
```

#### View HTTP requests:
```promql
http_requests_total
```

#### Check PostgreSQL connections:
```promql
pg_stat_database_numbackends
```

#### Check Redis stats:
```promql
redis_connected_clients
```

---

## Step 5: Verify Metrics Collection

### Check Targets in Prometheus
1. In Grafana, click **"Explore"**
2. Select **"Prometheus"** data source
3. Run query to see all targets:
   ```promql
   up{job!="prometheus"}
   ```

**Expected Results:**
- `up{job="virtualqueue-api"}` = 1 (healthy)
- `up{job="postgres"}` = 1 (healthy)
- `up{job="redis"}` = 1 (healthy)
- `up{job="node"}` = 1 (healthy)

---

## Step 6: Create Custom Dashboard

### Add New Dashboard
1. Click **"+"** in left sidebar
2. Select **"Dashboard"**
3. Click **"Add visualization"**
4. Select **"Prometheus"** data source
5. Enter a metric query
6. Click **"Apply"**
7. Click **"Save dashboard"** (disk icon)

---

## Step 7: Monitor API in Real-Time

### Create API Metrics Panel
1. In Explore, query:
   ```promql
   rate(http_requests_total[5m])
   ```
2. Run query to see request rate
3. Add time range: **Last 15 minutes**

### Check API Response Times
1. Query:
   ```promql
   histogram_quantile(0.95, rate(http_request_duration_seconds_bucket[5m]))
   ```
2. Shows 95th percentile response time

---

## Step 8: Alerts Configuration

### View Alerts
1. Click ⚠️ **"Alerting"** in left sidebar
2. Click **"Alert rules"**
3. View existing alert rules (if configured)

### Create Alert
1. Click **"Create alert rule"**
2. Set conditions based on metrics
3. Set notification channels
4. Save alert

---

## Step 9: System Overview Dashboard

### Check System Health
1. Go to **"Dashboards"** → **"Virtual Queue Infrastructure"**
2. Panels to verify:
   - ✅ PostgreSQL connections
   - ✅ Redis memory usage
   - ✅ API uptime
   - ✅ System CPU/Memory
   - ✅ Network I/O

---

## Step 10: Verify Specific Metrics

### API Endpoints
```promql
sum by (endpoint) (rate(http_requests_total[5m]))
```

### Error Rate
```promql
sum by (status) (rate(http_requests_total{status=~"5.."}[5m]))
```

### Database Connections
```promql
pg_stat_database_numbackends
```

### Redis Operations
```promql
rate(redis_commands_processed_total[5m])
```

---

## Troubleshooting

### Issue: "No data"
**Solution**: 
- Check Prometheus is running: http://localhost:9090
- Verify data source URL is correct
- Check time range is correct

### Issue: "Connection refused"
**Solution**:
- Restart Grafana: `docker restart virtualqueue-grafana`
- Check container logs: `docker logs virtualqueue-grafana`

### Issue: "404 on dashboard"
**Solution**:
- Dashboard may have syntax errors
- Check logs: `docker logs virtualqueue-grafana | grep error`
- Use Explore to verify data is available

---

## Quick Verification Checklist

- [ ] Login successful
- [ ] Data sources configured
- [ ] Prometheus connection working
- [ ] Dashboards visible
- [ ] Metrics querying works
- [ ] Real-time updates visible
- [ ] API metrics available
- [ ] Database metrics visible
- [ ] Redis metrics visible
- [ ] No errors in logs

---

## Useful Queries

### All Targets Status
```promql
up
```

### HTTP Request Rate
```promql
rate(http_requests_total[5m])
```

### Average Response Time
```promql
rate(http_request_duration_seconds_sum[5m]) / rate(http_request_duration_seconds_count[5m])
```

### Database Size
```promql
pg_stat_database_datalength
```

### Redis Memory
```promql
redis_memory_used_bytes
```

### Active User Sessions
```promql
user_sessions_active
```

---

## Next Steps

1. Explore pre-configured dashboards
2. Create custom queries in Explore
3. Set up alert rules for critical metrics
4. Configure notification channels
5. Monitor system performance
6. Track business metrics
