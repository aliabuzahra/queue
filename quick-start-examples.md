# 🚀 Quick Start Examples

## 1. Create a Tenant

```bash
curl -X POST "http://localhost:5000/api/v1/tenants" \
     -H "Content-Type: application/json" \
     -d '{
       "name": "My Website",
       "domain": "mywebsite.com"
     }'
```

## 2. Create a Queue

```bash
curl -X POST "http://localhost:5000/api/v1/tenants/{TENANT_ID}/queues" \
     -H "Content-Type: application/json" \
     -d '{
       "name": "Main Queue",
       "description": "Primary waiting queue",
       "maxConcurrentUsers": 100,
       "releaseRatePerMinute": 10
     }'
```

## 3. Enqueue a User

```bash
curl -X POST "http://localhost:5000/api/v1/tenants/{TENANT_ID}/queues/{QUEUE_ID}/enqueue" \
     -H "Content-Type: application/json" \
     -d '{
       "userIdentifier": "user123",
       "metadata": "VIP user"
     }'
```

## 4. Check User Status

```bash
curl "http://localhost:5000/api/v1/tenants/{TENANT_ID}/queues/{QUEUE_ID}/status/user123"
```

## 5. Release Users Manually

```bash
curl -X POST "http://localhost:5000/api/v1/tenants/{TENANT_ID}/queues/{QUEUE_ID}/release" \
     -H "Content-Type: application/json" \
     -d '{
       "count": 5
     }'
```

## 6. Run Tests

```bash
# Run all tests
dotnet test

# Run specific test projects
dotnet test tests/VirtualQueue.UnitTests/
dotnet test tests/VirtualQueue.IntegrationTests/
```

## 7. View Monitoring

- **Grafana**: http://localhost:3000 (admin/admin)
- **Prometheus**: http://localhost:9090
- **API Metrics**: http://localhost:5000/metrics
