# SDK Documentation - Virtual Queue Management System

**Document Version:** 1.0  
**Date:** January 15, 2024  
**Author:** SDK Developer  
**Status:** Draft  
**Phase:** 07 - API Documentation  
**Priority:** 🔴 Critical  

---

## SDK Overview

The Virtual Queue Management System Software Development Kit (SDK) provides developers with easy-to-use libraries for integrating queue management functionality into their applications. The SDK is available for multiple programming languages and platforms, offering a consistent API across all implementations.

## Supported Platforms

### **Programming Languages**
- **JavaScript/TypeScript**: Node.js and browser environments
- **Python**: Python 3.7+ with async/await support
- **C#**: .NET 6+ with async/await support
- **Java**: Java 8+ with modern features
- **Go**: Go 1.18+ with generics support
- **PHP**: PHP 8.0+ with modern features

### **Platforms**
- **Web Applications**: Browser-based applications
- **Mobile Applications**: iOS and Android native apps
- **Desktop Applications**: Windows, macOS, and Linux
- **Server Applications**: Backend services and microservices
- **Cloud Functions**: AWS Lambda, Azure Functions, Google Cloud Functions

## Installation

### **JavaScript/TypeScript**

#### **NPM Installation**
```bash
npm install @virtualqueue/sdk
```

#### **Yarn Installation**
```bash
yarn add @virtualqueue/sdk
```

#### **CDN Usage**
```html
<script src="https://cdn.virtualqueue.com/sdk/v1.0.0/virtualqueue-sdk.min.js"></script>
```

### **Python**

#### **Pip Installation**
```bash
pip install virtualqueue-sdk
```

#### **Conda Installation**
```bash
conda install -c virtualqueue virtualqueue-sdk
```

### **C#**

#### **NuGet Installation**
```bash
dotnet add package VirtualQueue.SDK
```

#### **Package Manager Console**
```powershell
Install-Package VirtualQueue.SDK
```

### **Java**

#### **Maven Installation**
```xml
<dependency>
    <groupId>com.virtualqueue</groupId>
    <artifactId>sdk</artifactId>
    <version>1.0.0</version>
</dependency>
```

#### **Gradle Installation**
```gradle
implementation 'com.virtualqueue:sdk:1.0.0'
```

### **Go**

#### **Go Modules**
```bash
go get github.com/virtualqueue/sdk-go
```

### **PHP**

#### **Composer Installation**
```bash
composer require virtualqueue/sdk
```

## Quick Start

### **JavaScript/TypeScript Example**

```typescript
import { VirtualQueueClient } from '@virtualqueue/sdk';

// Initialize the client
const client = new VirtualQueueClient({
  apiKey: 'your-api-key',
  baseUrl: 'https://api.virtualqueue.com',
  timeout: 30000
});

// Create a queue
const queue = await client.queues.create({
  name: 'Customer Service',
  description: 'General customer service queue',
  capacity: 100,
  settings: {
    allowMultipleSessions: false,
    maxWaitTime: 60,
    notificationsEnabled: true
  }
});

// Join the queue
const session = await client.queues.join(queue.id, {
  notes: 'Need help with billing issue'
});

// Get real-time updates
client.on('positionUpdate', (data) => {
  console.log(`Position: ${data.position}, Wait time: ${data.estimatedWaitTime} minutes`);
});

// Connect to real-time updates
await client.connect();
```

### **Python Example**

```python
from virtualqueue import VirtualQueueClient

# Initialize the client
client = VirtualQueueClient(
    api_key='your-api-key',
    base_url='https://api.virtualqueue.com',
    timeout=30
)

# Create a queue
queue = await client.queues.create(
    name='Customer Service',
    description='General customer service queue',
    capacity=100,
    settings={
        'allowMultipleSessions': False,
        'maxWaitTime': 60,
        'notificationsEnabled': True
    }
)

# Join the queue
session = await client.queues.join(queue.id, notes='Need help with billing issue')

# Get real-time updates
@client.on('positionUpdate')
def handle_position_update(data):
    print(f"Position: {data['position']}, Wait time: {data['estimatedWaitTime']} minutes")

# Connect to real-time updates
await client.connect()
```

### **C# Example**

```csharp
using VirtualQueue.SDK;

// Initialize the client
var client = new VirtualQueueClient(new ClientOptions
{
    ApiKey = "your-api-key",
    BaseUrl = "https://api.virtualqueue.com",
    Timeout = TimeSpan.FromSeconds(30)
});

// Create a queue
var queue = await client.Queues.CreateAsync(new CreateQueueRequest
{
    Name = "Customer Service",
    Description = "General customer service queue",
    Capacity = 100,
    Settings = new QueueSettings
    {
        AllowMultipleSessions = false,
        MaxWaitTime = 60,
        NotificationsEnabled = true
    }
});

// Join the queue
var session = await client.Queues.JoinAsync(queue.Id, new JoinQueueRequest
{
    Notes = "Need help with billing issue"
});

// Get real-time updates
client.OnPositionUpdate += (sender, data) =>
{
    Console.WriteLine($"Position: {data.Position}, Wait time: {data.EstimatedWaitTime} minutes");
};

// Connect to real-time updates
await client.ConnectAsync();
```

## Core Features

### **Queue Management**

#### **Creating Queues**
```typescript
// JavaScript/TypeScript
const queue = await client.queues.create({
  name: 'Customer Service',
  description: 'General customer service queue',
  slug: 'customer-service',
  capacity: 100,
  priority: 1,
  settings: {
    allowMultipleSessions: false,
    maxWaitTime: 60,
    notificationsEnabled: true,
    autoClose: true,
    closeAfterHours: 2
  },
  operatingHours: {
    monday: { start: '09:00', end: '17:00' },
    tuesday: { start: '09:00', end: '17:00' },
    wednesday: { start: '09:00', end: '17:00' },
    thursday: { start: '09:00', end: '17:00' },
    friday: { start: '09:00', end: '17:00' }
  }
});
```

#### **Managing Queues**
```typescript
// Get all queues
const queues = await client.queues.list({
  status: 'active',
  limit: 20,
  offset: 0
});

// Get specific queue
const queue = await client.queues.get(queueId);

// Update queue
const updatedQueue = await client.queues.update(queueId, {
  capacity: 150,
  settings: {
    maxWaitTime: 90
  }
});

// Delete queue
await client.queues.delete(queueId);
```

### **Session Management**

#### **Joining Queues**
```typescript
// Join a queue
const session = await client.queues.join(queueId, {
  notes: 'Need help with billing issue',
  metadata: {
    priority: 'high',
    department: 'billing'
  }
});

// Leave a queue
await client.queues.leave(queueId);

// Get session status
const sessionStatus = await client.sessions.get(sessionId);
```

#### **Session Operations**
```typescript
// Update session
const updatedSession = await client.sessions.update(sessionId, {
  status: 'serving',
  serviceStartTime: new Date().toISOString()
});

// Get user sessions
const userSessions = await client.sessions.list({
  status: 'waiting',
  limit: 10
});

// Complete session
await client.sessions.complete(sessionId, {
  serviceEndTime: new Date().toISOString(),
  notes: 'Issue resolved successfully'
});
```

### **Real-Time Updates**

#### **WebSocket Connection**
```typescript
// Connect to real-time updates
await client.connect();

// Listen for events
client.on('positionUpdate', (data) => {
  console.log(`Position: ${data.position}, Wait time: ${data.estimatedWaitTime} minutes`);
});

client.on('queueStatusChange', (data) => {
  console.log(`Queue status changed: ${data.status}`);
});

client.on('notification', (data) => {
  console.log(`Notification: ${data.message}`);
});

// Disconnect
await client.disconnect();
```

#### **Event Types**
```typescript
interface PositionUpdateEvent {
  sessionId: string;
  queueId: string;
  position: number;
  estimatedWaitTime: number;
  timestamp: string;
}

interface QueueStatusChangeEvent {
  queueId: string;
  status: 'active' | 'paused' | 'closed';
  timestamp: string;
}

interface NotificationEvent {
  type: 'email' | 'sms' | 'push';
  message: string;
  timestamp: string;
}
```

### **Analytics and Reporting**

#### **Queue Analytics**
```typescript
// Get queue statistics
const stats = await client.analytics.getQueueStats(queueId, {
  startDate: '2024-01-01',
  endDate: '2024-01-31',
  granularity: 'daily'
});

// Get performance metrics
const metrics = await client.analytics.getPerformanceMetrics(queueId, {
  startDate: '2024-01-01',
  endDate: '2024-01-31'
});

// Get user analytics
const userAnalytics = await client.analytics.getUserAnalytics({
  startDate: '2024-01-01',
  endDate: '2024-01-31',
  groupBy: 'day'
});
```

#### **Custom Reports**
```typescript
// Generate custom report
const report = await client.analytics.generateReport({
  type: 'queue_performance',
  queueIds: [queueId1, queueId2],
  startDate: '2024-01-01',
  endDate: '2024-01-31',
  metrics: ['waitTime', 'throughput', 'satisfaction']
});

// Export report
const exportUrl = await client.analytics.exportReport(report.id, 'csv');
```

## Advanced Features

### **Authentication**

#### **API Key Authentication**
```typescript
const client = new VirtualQueueClient({
  apiKey: 'your-api-key',
  baseUrl: 'https://api.virtualqueue.com'
});
```

#### **OAuth Authentication**
```typescript
const client = new VirtualQueueClient({
  oauth: {
    clientId: 'your-client-id',
    clientSecret: 'your-client-secret',
    redirectUri: 'https://your-app.com/callback'
  },
  baseUrl: 'https://api.virtualqueue.com'
});

// Get authorization URL
const authUrl = client.auth.getAuthorizationUrl();

// Handle callback
const token = await client.auth.handleCallback(code);
```

#### **JWT Authentication**
```typescript
const client = new VirtualQueueClient({
  jwt: {
    token: 'your-jwt-token',
    refreshToken: 'your-refresh-token'
  },
  baseUrl: 'https://api.virtualqueue.com'
});
```

### **Error Handling**

#### **Custom Error Handling**
```typescript
try {
  const queue = await client.queues.create(queueData);
} catch (error) {
  if (error instanceof ValidationError) {
    console.error('Validation error:', error.details);
  } else if (error instanceof RateLimitError) {
    console.error('Rate limited:', error.retryAfter);
    // Implement retry logic
  } else if (error instanceof AuthenticationError) {
    console.error('Authentication failed:', error.message);
    // Redirect to login
  } else {
    console.error('Unexpected error:', error.message);
  }
}
```

#### **Retry Logic**
```typescript
const client = new VirtualQueueClient({
  apiKey: 'your-api-key',
  retry: {
    maxRetries: 3,
    retryDelay: 1000,
    retryDelayMultiplier: 2
  }
});
```

### **Caching**

#### **Response Caching**
```typescript
const client = new VirtualQueueClient({
  apiKey: 'your-api-key',
  cache: {
    enabled: true,
    ttl: 300, // 5 minutes
    maxSize: 1000
  }
});
```

#### **Custom Cache Implementation**
```typescript
class CustomCache implements CacheInterface {
  async get(key: string): Promise<any> {
    // Custom cache implementation
  }

  async set(key: string, value: any, ttl: number): Promise<void> {
    // Custom cache implementation
  }

  async delete(key: string): Promise<void> {
    // Custom cache implementation
  }
}

const client = new VirtualQueueClient({
  apiKey: 'your-api-key',
  cache: new CustomCache()
});
```

### **Webhooks**

#### **Webhook Configuration**
```typescript
// Register webhook
const webhook = await client.webhooks.create({
  url: 'https://your-app.com/webhook',
  events: ['queue.created', 'session.joined', 'position.updated'],
  secret: 'your-webhook-secret'
});

// Verify webhook signature
const isValid = client.webhooks.verifySignature(
  payload,
  signature,
  secret
);
```

#### **Webhook Event Handling**
```typescript
// Express.js example
app.post('/webhook', (req, res) => {
  const signature = req.headers['x-virtualqueue-signature'];
  const payload = req.body;

  if (client.webhooks.verifySignature(payload, signature, secret)) {
    // Process webhook event
    const event = JSON.parse(payload);
    handleWebhookEvent(event);
    res.status(200).send('OK');
  } else {
    res.status(401).send('Unauthorized');
  }
});
```

## Configuration Options

### **Client Configuration**

#### **JavaScript/TypeScript**
```typescript
interface ClientOptions {
  apiKey: string;
  baseUrl?: string;
  timeout?: number;
  retry?: RetryOptions;
  cache?: CacheOptions;
  logger?: Logger;
  userAgent?: string;
}

interface RetryOptions {
  maxRetries: number;
  retryDelay: number;
  retryDelayMultiplier: number;
  retryCondition?: (error: Error) => boolean;
}

interface CacheOptions {
  enabled: boolean;
  ttl: number;
  maxSize: number;
  implementation?: CacheInterface;
}
```

#### **Python**
```python
class ClientOptions:
    def __init__(self, api_key: str, base_url: str = None, timeout: int = 30):
        self.api_key = api_key
        self.base_url = base_url or 'https://api.virtualqueue.com'
        self.timeout = timeout
        self.retry = RetryOptions()
        self.cache = CacheOptions()
        self.logger = None
        self.user_agent = 'VirtualQueue-SDK-Python/1.0.0'
```

#### **C#**
```csharp
public class ClientOptions
{
    public string ApiKey { get; set; }
    public string BaseUrl { get; set; } = "https://api.virtualqueue.com";
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
    public RetryOptions Retry { get; set; } = new RetryOptions();
    public CacheOptions Cache { get; set; } = new CacheOptions();
    public ILogger Logger { get; set; }
    public string UserAgent { get; set; } = "VirtualQueue-SDK-CSharp/1.0.0";
}
```

### **Environment Configuration**

#### **Environment Variables**
```bash
# API Configuration
VIRTUALQUEUE_API_KEY=your-api-key
VIRTUALQUEUE_BASE_URL=https://api.virtualqueue.com
VIRTUALQUEUE_TIMEOUT=30

# Logging Configuration
VIRTUALQUEUE_LOG_LEVEL=info
VIRTUALQUEUE_LOG_FILE=/var/log/virtualqueue.log

# Cache Configuration
VIRTUALQUEUE_CACHE_ENABLED=true
VIRTUALQUEUE_CACHE_TTL=300
VIRTUALQUEUE_CACHE_MAX_SIZE=1000
```

#### **Configuration Files**
```yaml
# config.yaml
virtualqueue:
  api:
    key: ${VIRTUALQUEUE_API_KEY}
    base_url: ${VIRTUALQUEUE_BASE_URL}
    timeout: ${VIRTUALQUEUE_TIMEOUT}
  cache:
    enabled: ${VIRTUALQUEUE_CACHE_ENABLED}
    ttl: ${VIRTUALQUEUE_CACHE_TTL}
    max_size: ${VIRTUALQUEUE_CACHE_MAX_SIZE}
  logging:
    level: ${VIRTUALQUEUE_LOG_LEVEL}
    file: ${VIRTUALQUEUE_LOG_FILE}
```

## Testing

### **Unit Testing**

#### **JavaScript/TypeScript**
```typescript
import { VirtualQueueClient } from '@virtualqueue/sdk';
import { mockClient } from '@virtualqueue/sdk/testing';

describe('VirtualQueueClient', () => {
  let client: VirtualQueueClient;

  beforeEach(() => {
    client = mockClient({
      apiKey: 'test-api-key'
    });
  });

  it('should create a queue', async () => {
    const queueData = {
      name: 'Test Queue',
      capacity: 100
    };

    const queue = await client.queues.create(queueData);
    
    expect(queue).toBeDefined();
    expect(queue.name).toBe('Test Queue');
    expect(queue.capacity).toBe(100);
  });

  it('should handle errors gracefully', async () => {
    client.mockError('queues.create', new Error('API Error'));

    await expect(client.queues.create({})).rejects.toThrow('API Error');
  });
});
```

#### **Python**
```python
import pytest
from unittest.mock import Mock, patch
from virtualqueue import VirtualQueueClient

class TestVirtualQueueClient:
    def setup_method(self):
        self.client = VirtualQueueClient(api_key='test-api-key')

    @pytest.mark.asyncio
    async def test_create_queue(self):
        queue_data = {
            'name': 'Test Queue',
            'capacity': 100
        }

        with patch.object(self.client.queues, 'create') as mock_create:
            mock_create.return_value = {'id': 'test-id', 'name': 'Test Queue'}
            
            queue = await self.client.queues.create(queue_data)
            
            assert queue['name'] == 'Test Queue'
            mock_create.assert_called_once_with(queue_data)

    @pytest.mark.asyncio
    async def test_handle_errors(self):
        with patch.object(self.client.queues, 'create') as mock_create:
            mock_create.side_effect = Exception('API Error')
            
            with pytest.raises(Exception, match='API Error'):
                await self.client.queues.create({})
```

### **Integration Testing**

#### **Test Environment Setup**
```typescript
import { VirtualQueueClient } from '@virtualqueue/sdk';

describe('Integration Tests', () => {
  let client: VirtualQueueClient;

  beforeAll(async () => {
    client = new VirtualQueueClient({
      apiKey: process.env.TEST_API_KEY,
      baseUrl: process.env.TEST_BASE_URL || 'https://test-api.virtualqueue.com'
    });
  });

  it('should create and manage a queue', async () => {
    // Create queue
    const queue = await client.queues.create({
      name: 'Integration Test Queue',
      capacity: 50
    });

    expect(queue.id).toBeDefined();

    // Join queue
    const session = await client.queues.join(queue.id, {
      notes: 'Integration test session'
    });

    expect(session.id).toBeDefined();
    expect(session.queueId).toBe(queue.id);

    // Get queue status
    const status = await client.queues.get(queue.id);
    expect(status.activeSessions).toBe(1);

    // Leave queue
    await client.queues.leave(queue.id);

    // Cleanup
    await client.queues.delete(queue.id);
  });
});
```

## Best Practices

### **Performance Optimization**

#### **Connection Pooling**
```typescript
const client = new VirtualQueueClient({
  apiKey: 'your-api-key',
  connectionPool: {
    maxConnections: 10,
    keepAlive: true,
    timeout: 30000
  }
});
```

#### **Batch Operations**
```typescript
// Batch create multiple queues
const queues = await client.queues.batchCreate([
  { name: 'Queue 1', capacity: 100 },
  { name: 'Queue 2', capacity: 150 },
  { name: 'Queue 3', capacity: 200 }
]);

// Batch update sessions
await client.sessions.batchUpdate([
  { id: 'session1', status: 'serving' },
  { id: 'session2', status: 'serving' },
  { id: 'session3', status: 'serving' }
]);
```

### **Security Best Practices**

#### **Secure API Key Storage**
```typescript
// Use environment variables
const client = new VirtualQueueClient({
  apiKey: process.env.VIRTUALQUEUE_API_KEY
});

// Use secure key management
const client = new VirtualQueueClient({
  apiKey: await keyManager.getSecret('virtualqueue-api-key')
});
```

#### **Request Signing**
```typescript
const client = new VirtualQueueClient({
  apiKey: 'your-api-key',
  requestSigning: {
    enabled: true,
    secret: 'your-signing-secret'
  }
});
```

### **Error Handling**

#### **Comprehensive Error Handling**
```typescript
try {
  const queue = await client.queues.create(queueData);
} catch (error) {
  switch (error.constructor) {
    case ValidationError:
      // Handle validation errors
      console.error('Validation failed:', error.details);
      break;
    case RateLimitError:
      // Handle rate limiting
      console.error('Rate limited:', error.retryAfter);
      break;
    case AuthenticationError:
      // Handle authentication errors
      console.error('Authentication failed:', error.message);
      break;
    case NetworkError:
      // Handle network errors
      console.error('Network error:', error.message);
      break;
    default:
      // Handle unexpected errors
      console.error('Unexpected error:', error.message);
  }
}
```

## Migration Guide

### **Version Migration**

#### **From v0.x to v1.0**
```typescript
// Old API (v0.x)
const client = new VirtualQueueClient('your-api-key');
const queue = await client.createQueue(queueData);

// New API (v1.0)
const client = new VirtualQueueClient({
  apiKey: 'your-api-key'
});
const queue = await client.queues.create(queueData);
```

#### **Breaking Changes**
- **Authentication**: Changed from string to object configuration
- **Method Names**: Changed from camelCase to dot notation
- **Error Handling**: Improved error types and messages
- **Event Handling**: Changed from callbacks to event emitters

## Support and Resources

### **Documentation**
- **API Reference**: Complete API documentation
- **Code Examples**: Comprehensive code samples
- **Tutorials**: Step-by-step guides
- **Best Practices**: Recommended patterns and practices

### **Community**
- **GitHub Repository**: Source code and issues
- **Discord Community**: Developer discussions
- **Stack Overflow**: Q&A and support
- **Blog**: Updates and announcements

### **Support**
- **Email Support**: sdk-support@virtualqueue.com
- **Documentation**: https://docs.virtualqueue.com
- **Status Page**: https://status.virtualqueue.com
- **Support Hours**: Monday-Friday, 9 AM - 6 PM EST

## Approval and Sign-off

### **SDK Documentation Approval**
- **SDK Developer**: [Name] - [Date]
- **Technical Lead**: [Name] - [Date]
- **Documentation Lead**: [Name] - [Date]
- **QA Lead**: [Name] - [Date]

### **Document Control**
- **Version**: 1.0
- **Last Updated**: January 15, 2024
- **Next Review**: February 15, 2024
- **Distribution**: Development Team, SDK Team, Integration Team

---

**Document Status**: Draft  
**Next Phase**: Test Results  
**Dependencies**: SDK testing, example validation, community feedback
