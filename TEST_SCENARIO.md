# Virtual Queue Management System - Complete Test Scenario

## Test Environment
- **API**: http://localhost:8080
- **Grafana**: http://localhost:3000
- **Prometheus**: http://localhost:9090
- **PostgreSQL**: localhost:5432
- **Redis**: localhost:6379

## Prerequisites
- All Docker containers running
- Database initialized
- JWT authentication working

---

## Test Scenario 1: System Health Check

### 1.1 Check API Health
```bash
curl http://localhost:8080/healthz
```
**Expected**: `200 OK` response

### 1.2 Check API Metrics
```bash
curl http://localhost:8080/metrics
```
**Expected**: Prometheus metrics output

### 1.3 Check Prometheus Targets
```bash
open http://localhost:9090/targets
```
**Expected**: All targets showing as UP

---

## Test Scenario 2: Authentication & Authorization

### 2.1 Create Test User
```bash
curl -X POST http://localhost:8080/api/v1/users \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "email": "test@example.com",
    "password": "Test123!",
    "firstName": "Test",
    "lastName": "User",
    "role": "Admin"
  }'
```
**Expected**: `201 Created` with user details

### 2.2 Get Access Token
```bash
curl -X POST http://localhost:8080/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "password": "Test123!"
  }'
```
**Expected**: JWT token with expiration

### 2.3 Test Token Validation
```bash
curl -X GET http://localhost:8080/api/v1/auth/validate \
  -H "Authorization: Bearer {TOKEN_FROM_2.2}"
```
**Expected**: `200 OK` with token information

---

## Test Scenario 3: Tenant Management

### 3.1 Create Tenant
```bash
curl -X POST http://localhost:8080/api/v1/tenants \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {TOKEN}" \
  -d '{
    "name": "Acme Corporation",
    "domain": "acme.com"
  }'
```
**Expected**: `201 Created` with tenant details including:
- Tenant ID (Guid)
- API Key
- Created timestamp

### 3.2 Get All Tenants
```bash
curl -X GET http://localhost:8080/api/v1/tenants \
  -H "Authorization: Bearer {TOKEN}"
```
**Expected**: `200 OK` with list of tenants

### 3.3 Get Specific Tenant
```bash
curl -X GET http://localhost:8080/api/v1/tenants/{TENANT_ID} \
  -H "Authorization: Bearer {TOKEN}"
```
**Expected**: `200 OK` with tenant details

---

## Test Scenario 4: Queue Management

### 4.1 Create Queue
```bash
curl -X POST http://localhost:8080/api/v1/tenants/{TENANT_ID}/queues \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {TOKEN}" \
  -d '{
    "name": "Customer Support",
    "description": "Main customer support queue",
    "maxConcurrentUsers": 10,
    "releaseRatePerMinute": 5
  }'
```
**Expected**: `201 Created` with queue details

### 4.2 Get All Queues for Tenant
```bash
curl -X GET http://localhost:8080/api/v1/tenants/{TENANT_ID}/queues \
  -H "Authorization: Bearer {TOKEN}"
```
**Expected**: `200 OK` with list of queues

### 4.3 Get Specific Queue
```bash
curl -X GET http://localhost:8080/api/v1/tenants/{TENANT_ID}/queues/{QUEUE_ID} \
  -H "Authorization: Bearer {TOKEN}"
```
**Expected**: `200 OK` with queue details

### 4.4 Update Queue
```bash
curl -X PUT http://localhost:8080/api/v1/tenants/{TENANT_ID}/queues/{QUEUE_ID} \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {TOKEN}" \
  -d '{
    "name": "Customer Support - Updated",
    "description": "Updated description",
    "maxConcurrentUsers": 15,
    "releaseRatePerMinute": 8
  }'
```
**Expected**: `200 OK` with updated queue details

### 4.5 Set Queue Schedule
```bash
curl -X POST http://localhost:8080/api/v1/tenants/{TENANT_ID}/queues/{QUEUE_ID}/schedule \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {TOKEN}" \
  -d '{
    "schedule": {
      "businessHours": {
        "startTime": "09:00:00",
        "endTime": "17:00:00",
        "workingDays": [1, 2, 3, 4, 5],
        "timeZone": "America/New_York"
      },
      "isRecurring": true
    }
  }'
```
**Expected**: `200 OK` with updated queue

### 4.6 Activate Queue
```bash
curl -X PATCH http://localhost:8080/api/v1/tenants/{TENANT_ID}/queues/{QUEUE_ID}/activate \
  -H "Authorization: Bearer {TOKEN}"
```
**Expected**: `200 OK`

### 4.7 Get Queue Availability
```bash
curl -X GET "http://localhost:8080/api/v1/tenants/{TENANT_ID}/queues/{QUEUE_ID}/availability?checkTime=2024-01-01T10:00:00Z" \
  -H "Authorization: Bearer {TOKEN}"
```
**Expected**: `200 OK` with availability status

---

## Test Scenario 5: User Session Management

### 5.1 Enqueue User
```bash
curl -X POST http://localhost:8080/api/v1/tenants/{TENANT_ID}/queues/{QUEUE_ID}/usersessions/enqueue \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {TOKEN}" \
  -d '{
    "userIdentifier": "user-001",
    "metadata": "VIP customer",
    "priority": 2
  }'
```
**Expected**: `201 Created` with session details:
- Session ID
- Position in queue
- Estimated wait time
- Status: "Waiting"

### 5.2 Get User Session Status
```bash
curl -X GET http://localhost:8080/api/v1/tenants/{TENANT_ID}/queues/{QUEUE_ID}/usersessions/{SESSION_ID} \
  -H "Authorization: Bearer {TOKEN}"
```
**Expected**: `200 OK` with session status:
- Current position
- Total ahead
- Status
- Enqueued timestamp

### 5.3 Get All User Sessions
```bash
curl -X GET "http://localhost:8080/api/v1/tenants/{TENANT_ID}/queues/{QUEUE_ID}/usersessions?status=Waiting" \
  -H "Authorization: Bearer {TOKEN}"
```
**Expected**: `200 OK` with list of sessions

### 5.4 Release Users from Queue
```bash
curl -X POST http://localhost:8080/api/v1/tenants/{TENANT_ID}/queues/{QUEUE_ID}/usersessions/release \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {TOKEN}" \
  -d '{
    "count": 3
  }'
```
**Expected**: `200 OK` with released user count

### 5.5 Check User Session Status After Release
```bash
# Check position updated
curl -X GET http://localhost:8080/api/v1/tenants/{TENANT_ID}/queues/{QUEUE_ID}/usersessions/{SESSION_ID} \
  -H "Authorization: Bearer {TOKEN}"
```
**Expected**: Position decreased or status changed to "Released"

### 5.6 Leave Queue
```bash
curl -X DELETE http://localhost:8080/api/v1/tenants/{TENANT_ID}/queues/{QUEUE_ID}/usersessions/{SESSION_ID} \
  -H "Authorization: Bearer {TOKEN}"
```
**Expected**: `204 No Content`

---

## Test Scenario 6: Real-Time Updates (SignalR)

### 6.1 Connect to SignalR Hub
```javascript
// Using browser console or SignalR client
const connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:8080/queuehub")
    .build();

await connection.start();
console.log("Connected to SignalR hub");

// Subscribe to queue updates
connection.on("QueueUpdated", (data) => {
    console.log("Queue updated:", data);
});

connection.on("UserUpdated", (data) => {
    console.log("User updated:", data);
});

connection.on("PositionUpdated", (data) => {
    console.log("Position updated:", data);
});
```

### 6.2 Join Queue Group
```javascript
await connection.invoke("JoinQueueGroup", "{QUEUE_ID}");
```

### 6.3 Trigger Position Update
```bash
# Enqueue user (creates real-time event)
curl -X POST http://localhost:8080/api/v1/tenants/{TENANT_ID}/queues/{QUEUE_ID}/usersessions/enqueue \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {TOKEN}" \
  -d '{"userIdentifier": "realtime-user", "priority": 1}'
```
**Expected**: SignalR event fired to connected clients

---

## Test Scenario 7: Analytics & Reporting

### 7.1 Get Queue Analytics
```bash
curl -X GET "http://localhost:8080/api/v1/tenants/{TENANT_ID}/queues/{QUEUE_ID}/analytics?startDate=2024-01-01&endDate=2024-01-31" \
  -H "Authorization: Bearer {TOKEN}"
```
**Expected**: `200 OK` with analytics data:
- Total users served
- Average wait time
- Peak times
- Success rate

### 7.2 Get Admin Dashboard Data
```bash
curl -X GET http://localhost:8080/api/v1/admin/dashboard \
  -H "Authorization: Bearer {TOKEN}"
```
**Expected**: `200 OK` with:
- Total tenants
- Total queues
- Active users
- System health metrics

### 7.3 Get Tenant Statistics
```bash
curl -X GET http://localhost:8080/api/v1/tenants/{TENANT_ID}/statistics \
  -H "Authorization: Bearer {TOKEN}"
```
**Expected**: `200 OK` with tenant-specific statistics

---

## Test Scenario 8: Webhook Management

### 8.1 Create Webhook Subscription
```bash
curl -X POST http://localhost:8080/api/v1/tenants/{TENANT_ID}/webhooks \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {TOKEN}" \
  -d '{
    "name": "Queue Updates",
    "url": "https://webhook.site/your-webhook-endpoint",
    "eventType": "QueueUpdated",
    "isActive": true,
    "secret": "webhook-secret-key"
  }'
```
**Expected**: `201 Created` with webhook subscription details

### 8.2 Get All Webhooks
```bash
curl -X GET http://localhost:8080/api/v1/tenants/{TENANT_ID}/webhooks \
  -H "Authorization: Bearer {TOKEN}"
```
**Expected**: `200 OK` with list of webhook subscriptions

### 8.3 Test Webhook Event
```bash
# Trigger an event that should fire the webhook
curl -X POST http://localhost:8080/api/v1/tenants/{TENANT_ID}/queues/{QUEUE_ID}/usersessions/enqueue \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {TOKEN}" \
  -d '{"userIdentifier": "webhook-test-user", "priority": 1}'
```
**Expected**: HTTP POST to webhook URL with event data

---

## Test Scenario 9: Alert Management

### 9.1 Create Alert Rule
```bash
curl -X POST http://localhost:8080/api/v1/tenants/{TENANT_ID}/alerts/rules \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {TOKEN}" \
  -d '{
    "name": "High Queue Wait Time",
    "description": "Alert when queue wait time exceeds threshold",
    "metric": "wait_time_seconds",
    "condition": "greater_than",
    "threshold": 300,
    "severity": "warning",
    "notificationChannels": ["email", "sms"]
  }'
```
**Expected**: `201 Created` with alert rule details

### 9.2 Get All Alert Rules
```bash
curl -X GET http://localhost:8080/api/v1/tenants/{TENANT_ID}/alerts/rules \
  -H "Authorization: Bearer {TOKEN}"
```
**Expected**: `200 OK` with list of alert rules

### 9.3 Get Active Alerts
```bash
curl -X GET http://localhost:8080/api/v1/tenants/{TENANT_ID}/alerts/active \
  -H "Authorization: Bearer {TOKEN}"
```
**Expected**: `200 OK` with list of active alerts

### 9.4 Trigger Test Alert
```bash
curl -X POST http://localhost:8080/api/v1/tenants/{TENANT_ID}/alerts/trigger \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {TOKEN}" \
  -d '{
    "ruleId": "{ALERT_RULE_ID}",
    "message": "Test alert trigger"
  }'
```
**Expected**: `201 Created` with alert details

---

## Test Scenario 10: Rate Limiting

### 10.1 Test Rate Limit
```bash
# Send 110 requests rapidly (rate limit is 100 per minute)
for i in {1..110}; do
  curl -X GET http://localhost:8080/api/v1/tenants \
    -H "Authorization: Bearer {TOKEN}"
  sleep 0.1
done
```
**Expected**: 
- First 100 requests: `200 OK`
- Remaining requests: `429 Too Many Requests`

### 10.2 Check Rate Limit Headers
```bash
curl -I http://localhost:8080/api/v1/tenants \
  -H "Authorization: Bearer {TOKEN}"
```
**Expected**: Rate limit headers present:
- `X-RateLimit-Limit: 100`
- `X-RateLimit-Remaining: 99`
- `X-RateLimit-Reset: {timestamp}`

---

## Test Scenario 11: CORS & Security

### 11.1 Test CORS Preflight
```bash
curl -X OPTIONS http://localhost:8080/api/v1/tenants \
  -H "Origin: http://localhost:3000" \
  -H "Access-Control-Request-Method: GET"
```
**Expected**: 
- `200 OK`
- CORS headers present:
  - `Access-Control-Allow-Origin`
  - `Access-Control-Allow-Methods`
  - `Access-Control-Allow-Headers`

### 11.2 Test API Key Authentication
```bash
curl -X GET http://localhost:8080/api/v1/tenants/{TENANT_ID} \
  -H "X-Tenant-Key: {TENANT_API_KEY}"
```
**Expected**: `200 OK` with tenant details

---

## Test Scenario 12: Data Validation

### 12.1 Test Invalid Queue Creation
```bash
curl -X POST http://localhost:8080/api/v1/tenants/{TENANT_ID}/queues \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {TOKEN}" \
  -d '{
    "name": "",
    "maxConcurrentUsers": -1,
    "releaseRatePerMinute": 0
  }'
```
**Expected**: `400 Bad Request` with validation errors

### 12.2 Test Missing Required Fields
```bash
curl -X POST http://localhost:8080/api/v1/tenants/{TENANT_ID}/queues \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {TOKEN}" \
  -d '{}'
```
**Expected**: `400 Bad Request` with field validation errors

---

## Test Scenario 13: Error Handling

### 13.1 Test Non-Existent Resource
```bash
curl -X GET http://localhost:8080/api/v1/tenants/00000000-0000-0000-0000-000000000000/queues \
  -H "Authorization: Bearer {TOKEN}"
```
**Expected**: `404 Not Found`

### 13.2 Test Invalid GUID
```bash
curl -X GET http://localhost:8080/api/v1/tenants/invalid-guid/queues \
  -H "Authorization: Bearer {TOKEN}"
```
**Expected**: `400 Bad Request`

### 13.3 Test Unauthorized Access
```bash
curl -X GET http://localhost:8080/api/v1/tenants/{TENANT_ID}/queues
```
**Expected**: `401 Unauthorized`

---

## Test Scenario 14: Monitoring Integration

### 14.1 Check Prometheus Metrics
```bash
curl http://localhost:9090/api/v1/query?query=http_requests_total
```
**Expected**: Metric values returned

### 14.2 Check Grafana Dashboard
- Open: http://localhost:3000
- Login: admin / admin123
- Navigate to Virtual Queue dashboards
- Verify metrics are displaying

### 14.3 Check Database Health
```bash
docker exec virtualqueue-postgres psql -U virtualqueue_user -d virtualqueue_prod -c "SELECT COUNT(*) FROM \"Tenants\";"
```
**Expected**: Returns tenant count

### 14.4 Check Redis Cache
```bash
docker exec virtualqueue-redis redis-cli -a Redis123! ping
```
**Expected**: `PONG`

---

## Test Scenario 15: Load Testing

### 15.1 Concurrent User Enqueue
```bash
# Create 50 concurrent requests
for i in {1..50}; do
  curl -X POST http://localhost:8080/api/v1/tenants/{TENANT_ID}/queues/{QUEUE_ID}/usersessions/enqueue \
    -H "Content-Type: application/json" \
    -H "Authorization: Bearer {TOKEN}" \
    -d "{\"userIdentifier\": \"load-test-user-$i\", \"priority\": 1}" &
done
wait
```
**Expected**: All requests complete successfully

### 15.2 Check Database Performance
```bash
docker exec virtualqueue-postgres psql -U virtualqueue_user -d virtualqueue_prod -c "SELECT COUNT(*) FROM \"UserSessions\" WHERE \"Status\" = 'Waiting';"
```
**Expected**: Shows 50 or more waiting users

---

## Success Criteria

### All scenarios should pass:
- ✅ API responds to all endpoints
- ✅ Authentication works correctly
- ✅ Real-time updates via SignalR
- ✅ Database transactions committed
- ✅ Metrics collected in Prometheus
- ✅ Dashboards display in Grafana
- ✅ Error handling works correctly
- ✅ Rate limiting enforced
- ✅ Data validation working
- ✅ CORS configured correctly

### Performance Requirements:
- Response time < 200ms for most endpoints
- SignalR updates < 100ms latency
- Database queries < 50ms
- No memory leaks in long-running tests
- Graceful degradation under load

---

## Cleanup

### Remove Test Data
```bash
# Delete test tenant
curl -X DELETE http://localhost:8080/api/v1/tenants/{TENANT_ID} \
  -H "Authorization: Bearer {TOKEN}"

# Stop containers
docker-compose -f docker-compose.desktop.yml down
```

---

## Automated Test Runner

Save as `run-tests.sh`:

```bash
#!/bin/bash

# Test scenario runner for Virtual Queue Management System

API_URL="http://localhost:8080"
TOKEN=""

# Helper functions
log_success() { echo "✅ $1"; }
log_error() { echo "❌ $1"; }
log_info() { echo "ℹ️  $1"; }

# Test 1: Health Check
test_health() {
  log_info "Testing API health..."
  response=$(curl -s -o /dev/null -w "%{http_code}" $API_URL/healthz)
  if [ "$response" = "200" ]; then
    log_success "Health check passed"
  else
    log_error "Health check failed: $response"
  fi
}

# Test 2: Authentication
test_auth() {
  log_info "Testing authentication..."
  response=$(curl -s -X POST $API_URL/api/v1/auth/login \
    -H "Content-Type: application/json" \
    -d '{"username":"testuser","password":"Test123!"}')
  
  TOKEN=$(echo $response | jq -r '.token')
  if [ -n "$TOKEN" ]; then
    log_success "Authentication successful"
  else
    log_error "Authentication failed"
  fi
}

# Run tests
test_health
test_auth

echo "Test run complete!"
```

---

## Postman Collection

Import this collection for easier testing:

```json
{
  "info": {
    "name": "Virtual Queue API",
    "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
  },
  "item": [
    {
      "name": "Health Check",
      "request": {
        "method": "GET",
        "url": {
          "raw": "http://localhost:8080/healthz",
          "host": ["localhost"],
          "port": "8080",
          "path": ["healthz"]
        }
      }
    },
    {
      "name": "Login",
      "request": {
        "method": "POST",
        "header": [{"key": "Content-Type", "value": "application/json"}],
        "body": {
          "mode": "raw",
          "raw": "{\n  \"username\": \"testuser\",\n  \"password\": \"Test123!\"\n}"
        },
        "url": "http://localhost:8080/api/v1/auth/login"
      }
    }
  ]
}
```

---

## Expected Test Results

| Scenario | Expected Result | Status |
|----------|----------------|--------|
| Health Check | 200 OK | ⏳ |
| Authentication | JWT Token | ⏳ |
| Tenant Creation | 201 Created | ⏳ |
| Queue Management | Success | ⏳ |
| User Sessions | Success | ⏳ |
| Real-time Updates | Events fired | ⏳ |
| Analytics | Data returned | ⏳ |
| Webhooks | Event fired | ⏳ |
| Alerts | Alert created | ⏳ |
| Rate Limiting | 429 after limit | ⏳ |
| Error Handling | Proper errors | ⏳ |
| Monitoring | Metrics visible | ⏳ |
| Load Testing | Handles load | ⏳ |

---

## Troubleshooting

### Common Issues:

1. **Token Expired**
   - Re-run authentication to get new token

2. **Database Connection Error**
   - Check PostgreSQL is running: `docker ps | grep postgres`
   - Restart if needed: `docker restart virtualqueue-postgres`

3. **Redis Connection Error**
   - Check Redis: `docker ps | grep redis`
   - Restart: `docker restart virtualqueue-redis`

4. **Prometheus Scraping Errors**
   - Check targets: http://localhost:9090/targets
   - Restart Prometheus if needed

5. **Grafana Not Loading**
   - Check logs: `docker logs virtualqueue-grafana`
   - Clear browser cache
   - Restart Grafana: `docker restart virtualqueue-grafana`
