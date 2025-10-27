# Integration Guide - Virtual Queue Management System

**Document Version:** 1.0  
**Date:** January 15, 2024  
**Author:** Integration Team  
**Status:** Draft  
**Phase:** 7 - API Documentation  
**Priority:** 🔴 Critical  

---

## Executive Summary

This document provides comprehensive integration guidance for the Virtual Queue Management System API. It covers integration patterns, SDK usage, webhook integration, real-time features, and best practices to enable seamless system integration.

## Integration Overview

### **Integration Types**

#### **Integration Patterns**
- **REST API Integration**: Standard REST API integration
- **Real-time Integration**: WebSocket/SignalR integration
- **Webhook Integration**: Event-driven integration
- **SDK Integration**: Client library integration
- **Batch Integration**: Bulk data processing

#### **Integration Scenarios**
- **Customer Service**: Queue management for customer service
- **Healthcare**: Patient queue management
- **Retail**: Customer queue management
- **Government**: Citizen service queues
- **Education**: Student service queues

### **Integration Architecture**

#### **Integration Components**
```
Client Application
├── SDK/Client Library
├── Authentication Module
├── API Client
├── WebSocket Client
└── Webhook Handler

Virtual Queue API
├── REST API Endpoints
├── WebSocket/SignalR Hub
├── Webhook Dispatcher
├── Authentication Service
└── Rate Limiting
```

## SDK Integration

### **JavaScript/Node.js SDK**

#### **Installation**
```bash
npm install @virtualqueue/sdk
```

#### **Basic Usage**
```javascript
import { VirtualQueueClient } from '@virtualqueue/sdk';

// Initialize client
const client = new VirtualQueueClient({
    baseUrl: 'https://api.virtualqueue.com/v1',
    apiKey: 'your-api-key',
    // or use JWT authentication
    // accessToken: 'your-access-token'
});

// Create a queue
const queue = await client.queues.create({
    tenantId: 'tenant-uuid',
    name: 'Customer Service Queue',
    description: 'Main customer service queue',
    maxConcurrentUsers: 100,
    releaseRatePerMinute: 10
});

// Join a queue
const session = await client.queues.join(queue.id, {
    userId: 'user-uuid',
    userName: 'John Doe',
    userEmail: 'john@example.com'
});

// Get queue position
const position = await client.queues.getPosition(queue.id, session.sessionId);
console.log(`Position: ${position.position}, Wait time: ${position.estimatedWaitTime}`);
```

#### **Advanced Usage**
```javascript
// Batch operations
const queues = await client.queues.createBatch([
    {
        tenantId: 'tenant-uuid',
        name: 'Queue 1',
        maxConcurrentUsers: 50
    },
    {
        tenantId: 'tenant-uuid',
        name: 'Queue 2',
        maxConcurrentUsers: 75
    }
]);

// Pagination
const { items, pagination } = await client.queues.list({
    tenantId: 'tenant-uuid',
    page: 1,
    limit: 10,
    status: 'active'
});

// Filtering and sorting
const filteredQueues = await client.queues.list({
    tenantId: 'tenant-uuid',
    search: 'customer',
    sort: 'name',
    order: 'asc'
});
```

### **Python SDK**

#### **Installation**
```bash
pip install virtualqueue-sdk
```

#### **Basic Usage**
```python
from virtualqueue import VirtualQueueClient

# Initialize client
client = VirtualQueueClient(
    base_url='https://api.virtualqueue.com/v1',
    api_key='your-api-key'
)

# Create a queue
queue = client.queues.create({
    'tenantId': 'tenant-uuid',
    'name': 'Customer Service Queue',
    'description': 'Main customer service queue',
    'maxConcurrentUsers': 100,
    'releaseRatePerMinute': 10
})

# Join a queue
session = client.queues.join(queue.id, {
    'userId': 'user-uuid',
    'userName': 'John Doe',
    'userEmail': 'john@example.com'
})

# Get queue position
position = client.queues.get_position(queue.id, session.session_id)
print(f"Position: {position.position}, Wait time: {position.estimated_wait_time}")
```

#### **Advanced Usage**
```python
# Batch operations
queues = client.queues.create_batch([
    {
        'tenantId': 'tenant-uuid',
        'name': 'Queue 1',
        'maxConcurrentUsers': 50
    },
    {
        'tenantId': 'tenant-uuid',
        'name': 'Queue 2',
        'maxConcurrentUsers': 75
    }
])

# Pagination
result = client.queues.list(
    tenant_id='tenant-uuid',
    page=1,
    limit=10,
    status='active'
)
items = result.items
pagination = result.pagination

# Async operations
import asyncio

async def async_operations():
    async with VirtualQueueClient(
        base_url='https://api.virtualqueue.com/v1',
        api_key='your-api-key'
    ) as client:
        queue = await client.queues.create({
            'tenantId': 'tenant-uuid',
            'name': 'Async Queue',
            'maxConcurrentUsers': 50
        })
        return queue

# Run async operations
queue = asyncio.run(async_operations())
```

### **C#/.NET SDK**

#### **Installation**
```bash
dotnet add package VirtualQueue.SDK
```

#### **Basic Usage**
```csharp
using VirtualQueue.SDK;

// Initialize client
var client = new VirtualQueueClient(
    baseUrl: "https://api.virtualqueue.com/v1",
    apiKey: "your-api-key"
);

// Create a queue
var queue = await client.Queues.CreateAsync(new CreateQueueRequest
{
    TenantId = "tenant-uuid",
    Name = "Customer Service Queue",
    Description = "Main customer service queue",
    MaxConcurrentUsers = 100,
    ReleaseRatePerMinute = 10
});

// Join a queue
var session = await client.Queues.JoinAsync(queue.Id, new JoinQueueRequest
{
    UserId = "user-uuid",
    UserName = "John Doe",
    UserEmail = "john@example.com"
});

// Get queue position
var position = await client.Queues.GetPositionAsync(queue.Id, session.SessionId);
Console.WriteLine($"Position: {position.Position}, Wait time: {position.EstimatedWaitTime}");
```

#### **Advanced Usage**
```csharp
// Batch operations
var queues = await client.Queues.CreateBatchAsync(new[]
{
    new CreateQueueRequest
    {
        TenantId = "tenant-uuid",
        Name = "Queue 1",
        MaxConcurrentUsers = 50
    },
    new CreateQueueRequest
    {
        TenantId = "tenant-uuid",
        Name = "Queue 2",
        MaxConcurrentUsers = 75
    }
});

// Pagination
var result = await client.Queues.ListAsync(new QueueListRequest
{
    TenantId = "tenant-uuid",
    Page = 1,
    Limit = 10,
    Status = "active"
});

// Dependency injection
public class QueueService
{
    private readonly IVirtualQueueClient _client;

    public QueueService(IVirtualQueueClient client)
    {
        _client = client;
    }

    public async Task<Queue> CreateQueueAsync(string name, int maxUsers)
    {
        return await _client.Queues.CreateAsync(new CreateQueueRequest
        {
            TenantId = "tenant-uuid",
            Name = name,
            MaxConcurrentUsers = maxUsers
        });
    }
}
```

## Real-time Integration

### **WebSocket/SignalR Integration**

#### **JavaScript WebSocket Client**
```javascript
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';

class VirtualQueueWebSocketClient {
    constructor(baseUrl, accessToken) {
        this.baseUrl = baseUrl;
        this.accessToken = accessToken;
        this.connection = null;
    }

    async connect() {
        this.connection = new HubConnectionBuilder()
            .withUrl(`${this.baseUrl}/hubs/queue`, {
                accessTokenFactory: () => this.accessToken
            })
            .withAutomaticReconnect()
            .configureLogging(LogLevel.Information)
            .build();

        // Register event handlers
        this.connection.on('QueueUpdated', (data) => {
            console.log('Queue updated:', data);
            this.onQueueUpdated(data);
        });

        this.connection.on('UserJoined', (data) => {
            console.log('User joined:', data);
            this.onUserJoined(data);
        });

        this.connection.on('UserLeft', (data) => {
            console.log('User left:', data);
            this.onUserLeft(data);
        });

        this.connection.on('PositionChanged', (data) => {
            console.log('Position changed:', data);
            this.onPositionChanged(data);
        });

        this.connection.on('QueueProcessed', (data) => {
            console.log('Queue processed:', data);
            this.onQueueProcessed(data);
        });

        // Start connection
        await this.connection.start();
        console.log('WebSocket connected');
    }

    async disconnect() {
        if (this.connection) {
            await this.connection.stop();
            console.log('WebSocket disconnected');
        }
    }

    // Event handlers
    onQueueUpdated(data) {
        // Handle queue update
    }

    onUserJoined(data) {
        // Handle user joined
    }

    onUserLeft(data) {
        // Handle user left
    }

    onPositionChanged(data) {
        // Handle position change
    }

    onQueueProcessed(data) {
        // Handle queue processing
    }

    // Join queue group
    async joinQueueGroup(queueId) {
        if (this.connection) {
            await this.connection.invoke('JoinQueueGroup', queueId);
        }
    }

    // Leave queue group
    async leaveQueueGroup(queueId) {
        if (this.connection) {
            await this.connection.invoke('LeaveQueueGroup', queueId);
        }
    }
}
```

#### **Python WebSocket Client**
```python
import asyncio
import websockets
import json

class VirtualQueueWebSocketClient:
    def __init__(self, base_url, access_token):
        self.base_url = base_url
        self.access_token = access_token
        self.websocket = None

    async def connect(self):
        uri = f"{self.base_url}/hubs/queue?access_token={self.access_token}"
        self.websocket = await websockets.connect(uri)
        
        # Start listening for messages
        asyncio.create_task(self.listen())

    async def listen(self):
        async for message in self.websocket:
            data = json.loads(message)
            await self.handle_message(data)

    async def handle_message(self, data):
        event_type = data.get('type')
        
        if event_type == 'QueueUpdated':
            await self.on_queue_updated(data['data'])
        elif event_type == 'UserJoined':
            await self.on_user_joined(data['data'])
        elif event_type == 'UserLeft':
            await self.on_user_left(data['data'])
        elif event_type == 'PositionChanged':
            await self.on_position_changed(data['data'])
        elif event_type == 'QueueProcessed':
            await self.on_queue_processed(data['data'])

    async def on_queue_updated(self, data):
        print(f"Queue updated: {data}")

    async def on_user_joined(self, data):
        print(f"User joined: {data}")

    async def on_user_left(self, data):
        print(f"User left: {data}")

    async def on_position_changed(self, data):
        print(f"Position changed: {data}")

    async def on_queue_processed(self, data):
        print(f"Queue processed: {data}")

    async def join_queue_group(self, queue_id):
        if self.websocket:
            message = {
                'type': 'JoinQueueGroup',
                'data': {'queueId': queue_id}
            }
            await self.websocket.send(json.dumps(message))

    async def disconnect(self):
        if self.websocket:
            await self.websocket.close()
```

## Webhook Integration

### **Webhook Setup**

#### **Create Webhook**
```http
POST /webhooks
Authorization: Bearer {access_token}
Content-Type: application/json

{
  "name": "Queue Events Webhook",
  "url": "https://your-app.com/webhooks/queue-events",
  "events": [
    "queue.created",
    "queue.updated",
    "queue.deleted",
    "user.joined",
    "user.left",
    "queue.processed"
  ],
  "secret": "your-webhook-secret",
  "isActive": true
}
```

#### **Webhook Response**
```http
HTTP/1.1 201 Created
Content-Type: application/json

{
  "success": true,
  "data": {
    "id": "webhook-uuid",
    "name": "Queue Events Webhook",
    "url": "https://your-app.com/webhooks/queue-events",
    "events": [
      "queue.created",
      "queue.updated",
      "queue.deleted",
      "user.joined",
      "user.left",
      "queue.processed"
    ],
    "secret": "your-webhook-secret",
    "isActive": true,
    "createdAt": "2024-01-15T10:30:00Z"
  }
}
```

### **Webhook Handler**

#### **Node.js Webhook Handler**
```javascript
const express = require('express');
const crypto = require('crypto');
const app = express();

app.use(express.json());

// Webhook endpoint
app.post('/webhooks/queue-events', (req, res) => {
    const signature = req.headers['x-webhook-signature'];
    const payload = JSON.stringify(req.body);
    const secret = 'your-webhook-secret';
    
    // Verify signature
    const expectedSignature = crypto
        .createHmac('sha256', secret)
        .update(payload)
        .digest('hex');
    
    if (signature !== expectedSignature) {
        return res.status(401).json({ error: 'Invalid signature' });
    }
    
    // Process webhook
    const event = req.body;
    console.log('Received webhook:', event);
    
    switch (event.type) {
        case 'queue.created':
            handleQueueCreated(event.data);
            break;
        case 'queue.updated':
            handleQueueUpdated(event.data);
            break;
        case 'user.joined':
            handleUserJoined(event.data);
            break;
        case 'user.left':
            handleUserLeft(event.data);
            break;
        case 'queue.processed':
            handleQueueProcessed(event.data);
            break;
    }
    
    res.status(200).json({ success: true });
});

function handleQueueCreated(data) {
    console.log('Queue created:', data);
    // Handle queue creation
}

function handleQueueUpdated(data) {
    console.log('Queue updated:', data);
    // Handle queue update
}

function handleUserJoined(data) {
    console.log('User joined:', data);
    // Handle user joined
}

function handleUserLeft(data) {
    console.log('User left:', data);
    // Handle user left
}

function handleQueueProcessed(data) {
    console.log('Queue processed:', data);
    // Handle queue processing
}

app.listen(3000, () => {
    console.log('Webhook server running on port 3000');
});
```

#### **Python Webhook Handler**
```python
from flask import Flask, request, jsonify
import hmac
import hashlib
import json

app = Flask(__name__)

@app.route('/webhooks/queue-events', methods=['POST'])
def handle_webhook():
    signature = request.headers.get('X-Webhook-Signature')
    payload = request.get_data()
    secret = 'your-webhook-secret'
    
    # Verify signature
    expected_signature = hmac.new(
        secret.encode(),
        payload,
        hashlib.sha256
    ).hexdigest()
    
    if signature != expected_signature:
        return jsonify({'error': 'Invalid signature'}), 401
    
    # Process webhook
    event = request.get_json()
    print(f'Received webhook: {event}')
    
    event_type = event.get('type')
    event_data = event.get('data')
    
    if event_type == 'queue.created':
        handle_queue_created(event_data)
    elif event_type == 'queue.updated':
        handle_queue_updated(event_data)
    elif event_type == 'user.joined':
        handle_user_joined(event_data)
    elif event_type == 'user.left':
        handle_user_left(event_data)
    elif event_type == 'queue.processed':
        handle_queue_processed(event_data)
    
    return jsonify({'success': True})

def handle_queue_created(data):
    print(f'Queue created: {data}')
    # Handle queue creation

def handle_queue_updated(data):
    print(f'Queue updated: {data}')
    # Handle queue update

def handle_user_joined(data):
    print(f'User joined: {data}')
    # Handle user joined

def handle_user_left(data):
    print(f'User left: {data}')
    # Handle user left

def handle_queue_processed(data):
    print(f'Queue processed: {data}')
    # Handle queue processing

if __name__ == '__main__':
    app.run(port=3000)
```

## Integration Patterns

### **Queue Management Pattern**

#### **Complete Queue Management Flow**
```javascript
class QueueManager {
    constructor(client) {
        this.client = client;
        this.queues = new Map();
        this.sessions = new Map();
    }

    async createQueue(queueData) {
        try {
            const queue = await this.client.queues.create(queueData);
            this.queues.set(queue.id, queue);
            
            // Set up real-time monitoring
            await this.setupQueueMonitoring(queue.id);
            
            return queue;
        } catch (error) {
            console.error('Failed to create queue:', error);
            throw error;
        }
    }

    async joinQueue(queueId, userData) {
        try {
            const session = await this.client.queues.join(queueId, userData);
            this.sessions.set(session.sessionId, session);
            
            // Set up position monitoring
            await this.setupPositionMonitoring(queueId, session.sessionId);
            
            return session;
        } catch (error) {
            console.error('Failed to join queue:', error);
            throw error;
        }
    }

    async leaveQueue(queueId, sessionId) {
        try {
            await this.client.queues.leave(queueId, sessionId);
            this.sessions.delete(sessionId);
            
            // Clean up monitoring
            await this.cleanupPositionMonitoring(sessionId);
            
        } catch (error) {
            console.error('Failed to leave queue:', error);
            throw error;
        }
    }

    async setupQueueMonitoring(queueId) {
        // Set up WebSocket monitoring for queue updates
        await this.client.websocket.joinQueueGroup(queueId);
    }

    async setupPositionMonitoring(queueId, sessionId) {
        // Set up position monitoring
        const interval = setInterval(async () => {
            try {
                const position = await this.client.queues.getPosition(queueId, sessionId);
                this.sessions.set(sessionId, { ...this.sessions.get(sessionId), ...position });
                
                // Notify position change
                this.onPositionChanged(position);
            } catch (error) {
                console.error('Failed to get position:', error);
                clearInterval(interval);
            }
        }, 5000); // Check every 5 seconds
    }

    onPositionChanged(position) {
        console.log(`Position updated: ${position.position}, Wait time: ${position.estimatedWaitTime}`);
    }
}
```

### **Batch Processing Pattern**

#### **Bulk Operations**
```javascript
class BatchProcessor {
    constructor(client) {
        this.client = client;
        this.batchSize = 100;
        this.concurrency = 5;
    }

    async processQueues(queues) {
        const batches = this.createBatches(queues, this.batchSize);
        const results = [];

        for (const batch of batches) {
            const batchResults = await this.processBatch(batch);
            results.push(...batchResults);
        }

        return results;
    }

    createBatches(items, batchSize) {
        const batches = [];
        for (let i = 0; i < items.length; i += batchSize) {
            batches.push(items.slice(i, i + batchSize));
        }
        return batches;
    }

    async processBatch(batch) {
        const promises = batch.map(item => this.processItem(item));
        return Promise.all(promises);
    }

    async processItem(item) {
        try {
            return await this.client.queues.create(item);
        } catch (error) {
            console.error('Failed to process item:', error);
            return { error: error.message, item };
        }
    }
}
```

## Error Handling and Retry Logic

### **Retry Strategy**

#### **Exponential Backoff**
```javascript
class RetryClient {
    constructor(client, options = {}) {
        this.client = client;
        this.maxRetries = options.maxRetries || 3;
        this.baseDelay = options.baseDelay || 1000;
        this.maxDelay = options.maxDelay || 10000;
    }

    async executeWithRetry(operation, ...args) {
        let lastError;
        
        for (let attempt = 0; attempt <= this.maxRetries; attempt++) {
            try {
                return await operation.apply(this.client, args);
            } catch (error) {
                lastError = error;
                
                if (attempt === this.maxRetries) {
                    throw error;
                }
                
                if (this.shouldRetry(error)) {
                    const delay = this.calculateDelay(attempt);
                    await this.sleep(delay);
                } else {
                    throw error;
                }
            }
        }
    }

    shouldRetry(error) {
        // Retry on network errors, rate limits, and server errors
        return error.status >= 500 || 
               error.status === 429 || 
               error.code === 'NETWORK_ERROR';
    }

    calculateDelay(attempt) {
        const delay = this.baseDelay * Math.pow(2, attempt);
        return Math.min(delay, this.maxDelay);
    }

    sleep(ms) {
        return new Promise(resolve => setTimeout(resolve, ms));
    }
}
```

### **Circuit Breaker Pattern**

#### **Circuit Breaker Implementation**
```javascript
class CircuitBreaker {
    constructor(options = {}) {
        this.failureThreshold = options.failureThreshold || 5;
        this.timeout = options.timeout || 60000;
        this.resetTimeout = options.resetTimeout || 30000;
        
        this.state = 'CLOSED';
        this.failureCount = 0;
        this.lastFailureTime = null;
        this.nextAttempt = null;
    }

    async execute(operation) {
        if (this.state === 'OPEN') {
            if (Date.now() < this.nextAttempt) {
                throw new Error('Circuit breaker is OPEN');
            }
            this.state = 'HALF_OPEN';
        }

        try {
            const result = await operation();
            this.onSuccess();
            return result;
        } catch (error) {
            this.onFailure();
            throw error;
        }
    }

    onSuccess() {
        this.failureCount = 0;
        this.state = 'CLOSED';
    }

    onFailure() {
        this.failureCount++;
        this.lastFailureTime = Date.now();
        
        if (this.failureCount >= this.failureThreshold) {
            this.state = 'OPEN';
            this.nextAttempt = Date.now() + this.resetTimeout;
        }
    }
}
```

## Testing Integration

### **Integration Testing**

#### **Test Setup**
```javascript
import { VirtualQueueClient } from '@virtualqueue/sdk';

describe('Virtual Queue Integration', () => {
    let client;
    let testQueue;
    let testSession;

    beforeAll(async () => {
        client = new VirtualQueueClient({
            baseUrl: 'https://sandbox-api.virtualqueue.com/v1',
            apiKey: 'test-api-key'
        });
    });

    beforeEach(async () => {
        // Create test queue
        testQueue = await client.queues.create({
            tenantId: 'test-tenant',
            name: 'Test Queue',
            maxConcurrentUsers: 10
        });
    });

    afterEach(async () => {
        // Clean up test data
        if (testQueue) {
            await client.queues.delete(testQueue.id);
        }
    });

    test('should create queue', async () => {
        expect(testQueue).toBeDefined();
        expect(testQueue.name).toBe('Test Queue');
        expect(testQueue.maxConcurrentUsers).toBe(10);
    });

    test('should join queue', async () => {
        testSession = await client.queues.join(testQueue.id, {
            userId: 'test-user',
            userName: 'Test User',
            userEmail: 'test@example.com'
        });

        expect(testSession).toBeDefined();
        expect(testSession.queueId).toBe(testQueue.id);
        expect(testSession.userId).toBe('test-user');
    });

    test('should get queue position', async () => {
        const position = await client.queues.getPosition(
            testQueue.id, 
            testSession.sessionId
        );

        expect(position).toBeDefined();
        expect(position.position).toBeGreaterThan(0);
    });
});
```

## Best Practices

### **Integration Best Practices**

#### **Performance Optimization**
- **Connection Pooling**: Use connection pooling for HTTP clients
- **Caching**: Implement appropriate caching strategies
- **Batch Operations**: Use batch operations for bulk data
- **Pagination**: Use pagination for large datasets
- **Rate Limiting**: Respect rate limits and implement backoff

#### **Error Handling**
- **Retry Logic**: Implement retry logic for transient errors
- **Circuit Breaker**: Use circuit breaker pattern for resilience
- **Graceful Degradation**: Handle errors gracefully
- **Logging**: Implement comprehensive logging
- **Monitoring**: Monitor integration health

#### **Security**
- **Authentication**: Use secure authentication methods
- **HTTPS**: Always use HTTPS for API calls
- **Token Management**: Implement secure token management
- **Input Validation**: Validate all inputs
- **Audit Logging**: Log all integration activities

## Approval and Sign-off

### **Integration Guide Approval**
- **Integration Team**: [Name] - [Date]
- **API Team**: [Name] - [Date]
- **Development Team**: [Name] - [Date]
- **Management**: [Name] - [Date]

### **Document Control**
- **Version**: 1.0
- **Last Updated**: January 15, 2024
- **Next Review**: February 15, 2024
- **Distribution**: Integration Team, API Team, Development Team

---

**Document Status**: Draft  
**Next Phase**: Error Handling  
**Dependencies**: SDK development, integration testing, documentation validation
