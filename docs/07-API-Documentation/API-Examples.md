# API Examples - Virtual Queue Management System

**Document Version:** 1.0  
**Date:** January 15, 2024  
**Author:** API Developer  
**Status:** Draft  
**Phase:** 07 - API Documentation  
**Priority:** 🔴 Critical  

---

## API Examples Overview

This document provides comprehensive code examples for integrating with the Virtual Queue Management System API. It includes examples in multiple programming languages and covers all major API endpoints and use cases.

## Authentication Examples

### **Getting an Access Token**

#### **cURL Example**
```bash
curl -X POST "https://api.virtualqueue.com/v1/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "your_password"
  }'
```

#### **JavaScript Example**
```javascript
const loginUser = async (email, password) => {
  try {
    const response = await fetch('https://api.virtualqueue.com/v1/auth/login', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        email: email,
        password: password
      })
    });

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    const data = await response.json();
    return data.accessToken;
  } catch (error) {
    console.error('Login failed:', error);
    throw error;
  }
};

// Usage
const token = await loginUser('user@example.com', 'your_password');
```

#### **Python Example**
```python
import requests
import json

def login_user(email, password):
    url = "https://api.virtualqueue.com/v1/auth/login"
    payload = {
        "email": email,
        "password": password
    }
    
    try:
        response = requests.post(url, json=payload)
        response.raise_for_status()
        return response.json()["accessToken"]
    except requests.exceptions.RequestException as e:
        print(f"Login failed: {e}")
        raise

# Usage
token = login_user("user@example.com", "your_password")
```

#### **C# Example**
```csharp
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class AuthService
{
    private readonly HttpClient _httpClient;

    public AuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> LoginAsync(string email, string password)
    {
        var loginRequest = new
        {
            email = email,
            password = password
        };

        var json = JsonSerializer.Serialize(loginRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync("/v1/auth/login", content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent);
            
            return loginResponse.AccessToken;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Login failed: {ex.Message}");
            throw;
        }
    }
}

public class LoginResponse
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public DateTime ExpiresAt { get; set; }
}
```

### **Using Access Tokens**

#### **JavaScript Example**
```javascript
const makeAuthenticatedRequest = async (url, options = {}) => {
  const token = localStorage.getItem('accessToken');
  
  const defaultOptions = {
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json',
      ...options.headers
    }
  };

  const response = await fetch(url, { ...defaultOptions, ...options });
  
  if (response.status === 401) {
    // Token expired, try to refresh
    await refreshToken();
    return makeAuthenticatedRequest(url, options);
  }
  
  return response;
};

const refreshToken = async () => {
  const refreshToken = localStorage.getItem('refreshToken');
  
  const response = await fetch('https://api.virtualqueue.com/v1/auth/refresh', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({ refreshToken })
  });

  if (response.ok) {
    const data = await response.json();
    localStorage.setItem('accessToken', data.accessToken);
    localStorage.setItem('refreshToken', data.refreshToken);
  } else {
    // Redirect to login
    window.location.href = '/login';
  }
};
```

## Queue Management Examples

### **Creating a Queue**

#### **cURL Example**
```bash
curl -X POST "https://api.virtualqueue.com/v1/queues" \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Customer Service",
    "description": "General customer service queue",
    "slug": "customer-service",
    "capacity": 100,
    "priority": 1,
    "settings": {
      "allowMultipleSessions": false,
      "maxWaitTime": 60,
      "notificationsEnabled": true
    },
    "operatingHours": {
      "monday": {"start": "09:00", "end": "17:00"},
      "tuesday": {"start": "09:00", "end": "17:00"},
      "wednesday": {"start": "09:00", "end": "17:00"},
      "thursday": {"start": "09:00", "end": "17:00"},
      "friday": {"start": "09:00", "end": "17:00"}
    }
  }'
```

#### **JavaScript Example**
```javascript
const createQueue = async (queueData) => {
  try {
    const response = await makeAuthenticatedRequest(
      'https://api.virtualqueue.com/v1/queues',
      {
        method: 'POST',
        body: JSON.stringify(queueData)
      }
    );

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message);
    }

    return await response.json();
  } catch (error) {
    console.error('Failed to create queue:', error);
    throw error;
  }
};

// Usage
const queueData = {
  name: "Customer Service",
  description: "General customer service queue",
  slug: "customer-service",
  capacity: 100,
  priority: 1,
  settings: {
    allowMultipleSessions: false,
    maxWaitTime: 60,
    notificationsEnabled: true
  },
  operatingHours: {
    monday: { start: "09:00", end: "17:00" },
    tuesday: { start: "09:00", end: "17:00" },
    wednesday: { start: "09:00", end: "17:00" },
    thursday: { start: "09:00", end: "17:00" },
    friday: { start: "09:00", end: "17:00" }
  }
};

const newQueue = await createQueue(queueData);
console.log('Queue created:', newQueue);
```

#### **Python Example**
```python
import requests
import json

def create_queue(token, queue_data):
    url = "https://api.virtualqueue.com/v1/queues"
    headers = {
        "Authorization": f"Bearer {token}",
        "Content-Type": "application/json"
    }
    
    try:
        response = requests.post(url, json=queue_data, headers=headers)
        response.raise_for_status()
        return response.json()
    except requests.exceptions.RequestException as e:
        print(f"Failed to create queue: {e}")
        raise

# Usage
queue_data = {
    "name": "Customer Service",
    "description": "General customer service queue",
    "slug": "customer-service",
    "capacity": 100,
    "priority": 1,
    "settings": {
        "allowMultipleSessions": False,
        "maxWaitTime": 60,
        "notificationsEnabled": True
    },
    "operatingHours": {
        "monday": {"start": "09:00", "end": "17:00"},
        "tuesday": {"start": "09:00", "end": "17:00"},
        "wednesday": {"start": "09:00", "end": "17:00"},
        "thursday": {"start": "09:00", "end": "17:00"},
        "friday": {"start": "09:00", "end": "17:00"}
    }
}

new_queue = create_queue(token, queue_data)
print(f"Queue created: {new_queue}")
```

### **Joining a Queue**

#### **cURL Example**
```bash
curl -X POST "https://api.virtualqueue.com/v1/queues/{queue_id}/join" \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "notes": "Need help with billing issue",
    "metadata": {
      "priority": "high",
      "department": "billing"
    }
  }'
```

#### **JavaScript Example**
```javascript
const joinQueue = async (queueId, joinData = {}) => {
  try {
    const response = await makeAuthenticatedRequest(
      `https://api.virtualqueue.com/v1/queues/${queueId}/join`,
      {
        method: 'POST',
        body: JSON.stringify(joinData)
      }
    );

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message);
    }

    return await response.json();
  } catch (error) {
    console.error('Failed to join queue:', error);
    throw error;
  }
};

// Usage
const joinData = {
  notes: "Need help with billing issue",
  metadata: {
    priority: "high",
    department: "billing"
  }
};

const session = await joinQueue("queue-id-here", joinData);
console.log('Joined queue:', session);
```

### **Getting Queue Status**

#### **cURL Example**
```bash
curl -X GET "https://api.virtualqueue.com/v1/queues/{queue_id}" \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"
```

#### **JavaScript Example**
```javascript
const getQueueStatus = async (queueId) => {
  try {
    const response = await makeAuthenticatedRequest(
      `https://api.virtualqueue.com/v1/queues/${queueId}`
    );

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message);
    }

    return await response.json();
  } catch (error) {
    console.error('Failed to get queue status:', error);
    throw error;
  }
};

// Usage
const queueStatus = await getQueueStatus("queue-id-here");
console.log('Queue status:', queueStatus);
```

## User Session Examples

### **Getting User Sessions**

#### **cURL Example**
```bash
curl -X GET "https://api.virtualqueue.com/v1/sessions" \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN" \
  -G -d "status=waiting&limit=10&offset=0"
```

#### **JavaScript Example**
```javascript
const getUserSessions = async (filters = {}) => {
  try {
    const queryParams = new URLSearchParams(filters);
    const response = await makeAuthenticatedRequest(
      `https://api.virtualqueue.com/v1/sessions?${queryParams}`
    );

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message);
    }

    return await response.json();
  } catch (error) {
    console.error('Failed to get user sessions:', error);
    throw error;
  }
};

// Usage
const sessions = await getUserSessions({
  status: 'waiting',
  limit: 10,
  offset: 0
});
console.log('User sessions:', sessions);
```

### **Updating Session Status**

#### **cURL Example**
```bash
curl -X PUT "https://api.virtualqueue.com/v1/sessions/{session_id}" \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "status": "serving",
    "serviceStartTime": "2024-01-15T10:30:00Z"
  }'
```

#### **JavaScript Example**
```javascript
const updateSessionStatus = async (sessionId, updateData) => {
  try {
    const response = await makeAuthenticatedRequest(
      `https://api.virtualqueue.com/v1/sessions/${sessionId}`,
      {
        method: 'PUT',
        body: JSON.stringify(updateData)
      }
    );

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message);
    }

    return await response.json();
  } catch (error) {
    console.error('Failed to update session status:', error);
    throw error;
  }
};

// Usage
const updateData = {
  status: 'serving',
  serviceStartTime: new Date().toISOString()
};

const updatedSession = await updateSessionStatus("session-id-here", updateData);
console.log('Session updated:', updatedSession);
```

## Notification Examples

### **Sending Notifications**

#### **cURL Example**
```bash
curl -X POST "https://api.virtualqueue.com/v1/notifications" \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "user-id-here",
    "type": "queue_position_update",
    "channel": "email",
    "subject": "Your queue position has been updated",
    "content": "You are now position 3 in the Customer Service queue. Estimated wait time: 15 minutes."
  }'
```

#### **JavaScript Example**
```javascript
const sendNotification = async (notificationData) => {
  try {
    const response = await makeAuthenticatedRequest(
      'https://api.virtualqueue.com/v1/notifications',
      {
        method: 'POST',
        body: JSON.stringify(notificationData)
      }
    );

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message);
    }

    return await response.json();
  } catch (error) {
    console.error('Failed to send notification:', error);
    throw error;
  }
};

// Usage
const notificationData = {
  userId: "user-id-here",
  type: "queue_position_update",
  channel: "email",
  subject: "Your queue position has been updated",
  content: "You are now position 3 in the Customer Service queue. Estimated wait time: 15 minutes."
};

const notification = await sendNotification(notificationData);
console.log('Notification sent:', notification);
```

## Analytics Examples

### **Getting Queue Analytics**

#### **cURL Example**
```bash
curl -X GET "https://api.virtualqueue.com/v1/analytics/queues/{queue_id}/stats" \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN" \
  -G -d "startDate=2024-01-01&endDate=2024-01-31&granularity=daily"
```

#### **JavaScript Example**
```javascript
const getQueueAnalytics = async (queueId, dateRange, granularity = 'daily') => {
  try {
    const queryParams = new URLSearchParams({
      startDate: dateRange.startDate,
      endDate: dateRange.endDate,
      granularity: granularity
    });

    const response = await makeAuthenticatedRequest(
      `https://api.virtualqueue.com/v1/analytics/queues/${queueId}/stats?${queryParams}`
    );

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message);
    }

    return await response.json();
  } catch (error) {
    console.error('Failed to get queue analytics:', error);
    throw error;
  }
};

// Usage
const analytics = await getQueueAnalytics(
  "queue-id-here",
  {
    startDate: "2024-01-01",
    endDate: "2024-01-31"
  },
  "daily"
);
console.log('Queue analytics:', analytics);
```

## Error Handling Examples

### **JavaScript Error Handling**

```javascript
const handleApiError = (error, response) => {
  if (response) {
    switch (response.status) {
      case 400:
        console.error('Bad Request:', error.message);
        break;
      case 401:
        console.error('Unauthorized:', error.message);
        // Redirect to login
        window.location.href = '/login';
        break;
      case 403:
        console.error('Forbidden:', error.message);
        break;
      case 404:
        console.error('Not Found:', error.message);
        break;
      case 429:
        console.error('Rate Limited:', error.message);
        // Implement retry logic
        break;
      case 500:
        console.error('Server Error:', error.message);
        break;
      default:
        console.error('Unknown Error:', error.message);
    }
  } else {
    console.error('Network Error:', error.message);
  }
};

const makeApiRequest = async (url, options = {}) => {
  try {
    const response = await fetch(url, options);
    
    if (!response.ok) {
      const errorData = await response.json();
      throw new Error(errorData.message || 'Request failed');
    }
    
    return await response.json();
  } catch (error) {
    handleApiError(error, error.response);
    throw error;
  }
};
```

### **Python Error Handling**

```python
import requests
from requests.exceptions import RequestException, HTTPError

def handle_api_error(error):
    if isinstance(error, HTTPError):
        status_code = error.response.status_code
        
        if status_code == 400:
            print(f"Bad Request: {error}")
        elif status_code == 401:
            print(f"Unauthorized: {error}")
            # Redirect to login or refresh token
        elif status_code == 403:
            print(f"Forbidden: {error}")
        elif status_code == 404:
            print(f"Not Found: {error}")
        elif status_code == 429:
            print(f"Rate Limited: {error}")
            # Implement retry logic
        elif status_code == 500:
            print(f"Server Error: {error}")
        else:
            print(f"Unknown Error: {error}")
    elif isinstance(error, RequestException):
        print(f"Network Error: {error}")
    else:
        print(f"Unexpected Error: {error}")

def make_api_request(url, headers=None, json_data=None, method='GET'):
    try:
        if method.upper() == 'GET':
            response = requests.get(url, headers=headers)
        elif method.upper() == 'POST':
            response = requests.post(url, headers=headers, json=json_data)
        elif method.upper() == 'PUT':
            response = requests.put(url, headers=headers, json=json_data)
        elif method.upper() == 'DELETE':
            response = requests.delete(url, headers=headers)
        
        response.raise_for_status()
        return response.json()
    except (HTTPError, RequestException) as e:
        handle_api_error(e)
        raise
```

## Rate Limiting Examples

### **Handling Rate Limits**

#### **JavaScript Example**
```javascript
const makeRequestWithRetry = async (url, options = {}, maxRetries = 3) => {
  for (let attempt = 1; attempt <= maxRetries; attempt++) {
    try {
      const response = await fetch(url, options);
      
      if (response.status === 429) {
        const retryAfter = response.headers.get('Retry-After');
        const delay = retryAfter ? parseInt(retryAfter) * 1000 : Math.pow(2, attempt) * 1000;
        
        console.log(`Rate limited. Retrying after ${delay}ms (attempt ${attempt}/${maxRetries})`);
        await new Promise(resolve => setTimeout(resolve, delay));
        continue;
      }
      
      return response;
    } catch (error) {
      if (attempt === maxRetries) {
        throw error;
      }
      
      const delay = Math.pow(2, attempt) * 1000;
      console.log(`Request failed. Retrying after ${delay}ms (attempt ${attempt}/${maxRetries})`);
      await new Promise(resolve => setTimeout(resolve, delay));
    }
  }
};
```

#### **Python Example**
```python
import time
import requests
from requests.exceptions import HTTPError

def make_request_with_retry(url, headers=None, json_data=None, method='GET', max_retries=3):
    for attempt in range(1, max_retries + 1):
        try:
            if method.upper() == 'GET':
                response = requests.get(url, headers=headers)
            elif method.upper() == 'POST':
                response = requests.post(url, headers=headers, json=json_data)
            
            response.raise_for_status()
            return response.json()
            
        except HTTPError as e:
            if e.response.status_code == 429:
                retry_after = e.response.headers.get('Retry-After')
                delay = int(retry_after) if retry_after else 2 ** attempt
                
                print(f"Rate limited. Retrying after {delay}s (attempt {attempt}/{max_retries})")
                time.sleep(delay)
                continue
            else:
                raise
        except Exception as e:
            if attempt == max_retries:
                raise
            
            delay = 2 ** attempt
            print(f"Request failed. Retrying after {delay}s (attempt {attempt}/{max_retries})")
            time.sleep(delay)
```

## WebSocket Examples

### **Real-Time Updates**

#### **JavaScript Example**
```javascript
class QueueWebSocket {
  constructor(token) {
    this.token = token;
    this.ws = null;
    this.reconnectAttempts = 0;
    this.maxReconnectAttempts = 5;
  }

  connect() {
    const wsUrl = `wss://api.virtualqueue.com/v1/ws?token=${this.token}`;
    
    this.ws = new WebSocket(wsUrl);
    
    this.ws.onopen = () => {
      console.log('WebSocket connected');
      this.reconnectAttempts = 0;
    };
    
    this.ws.onmessage = (event) => {
      const data = JSON.parse(event.data);
      this.handleMessage(data);
    };
    
    this.ws.onclose = () => {
      console.log('WebSocket disconnected');
      this.attemptReconnect();
    };
    
    this.ws.onerror = (error) => {
      console.error('WebSocket error:', error);
    };
  }

  handleMessage(data) {
    switch (data.type) {
      case 'queue_position_update':
        this.handlePositionUpdate(data.payload);
        break;
      case 'queue_status_change':
        this.handleStatusChange(data.payload);
        break;
      case 'notification':
        this.handleNotification(data.payload);
        break;
      default:
        console.log('Unknown message type:', data.type);
    }
  }

  handlePositionUpdate(payload) {
    console.log(`Position updated: ${payload.position}, Wait time: ${payload.estimatedWaitTime} minutes`);
    // Update UI with new position
  }

  handleStatusChange(payload) {
    console.log(`Queue status changed: ${payload.status}`);
    // Update UI with new status
  }

  handleNotification(payload) {
    console.log(`Notification: ${payload.message}`);
    // Show notification to user
  }

  attemptReconnect() {
    if (this.reconnectAttempts < this.maxReconnectAttempts) {
      this.reconnectAttempts++;
      const delay = Math.pow(2, this.reconnectAttempts) * 1000;
      
      console.log(`Attempting to reconnect in ${delay}ms (attempt ${this.reconnectAttempts}/${this.maxReconnectAttempts})`);
      
      setTimeout(() => {
        this.connect();
      }, delay);
    } else {
      console.error('Max reconnection attempts reached');
    }
  }

  disconnect() {
    if (this.ws) {
      this.ws.close();
    }
  }
}

// Usage
const ws = new QueueWebSocket(token);
ws.connect();
```

## Complete Integration Example

### **JavaScript Complete Example**

```javascript
class VirtualQueueClient {
  constructor(baseUrl, token) {
    this.baseUrl = baseUrl;
    this.token = token;
    this.ws = null;
  }

  async makeRequest(endpoint, options = {}) {
    const url = `${this.baseUrl}${endpoint}`;
    const defaultOptions = {
      headers: {
        'Authorization': `Bearer ${this.token}`,
        'Content-Type': 'application/json',
        ...options.headers
      }
    };

    try {
      const response = await fetch(url, { ...defaultOptions, ...options });
      
      if (!response.ok) {
        const error = await response.json();
        throw new Error(error.message || 'Request failed');
      }
      
      return await response.json();
    } catch (error) {
      console.error(`API request failed for ${endpoint}:`, error);
      throw error;
    }
  }

  // Queue Management
  async createQueue(queueData) {
    return this.makeRequest('/v1/queues', {
      method: 'POST',
      body: JSON.stringify(queueData)
    });
  }

  async getQueues(filters = {}) {
    const queryParams = new URLSearchParams(filters);
    return this.makeRequest(`/v1/queues?${queryParams}`);
  }

  async getQueue(queueId) {
    return this.makeRequest(`/v1/queues/${queueId}`);
  }

  async joinQueue(queueId, joinData = {}) {
    return this.makeRequest(`/v1/queues/${queueId}/join`, {
      method: 'POST',
      body: JSON.stringify(joinData)
    });
  }

  async leaveQueue(queueId) {
    return this.makeRequest(`/v1/queues/${queueId}/leave`, {
      method: 'POST'
    });
  }

  // Session Management
  async getSessions(filters = {}) {
    const queryParams = new URLSearchParams(filters);
    return this.makeRequest(`/v1/sessions?${queryParams}`);
  }

  async updateSession(sessionId, updateData) {
    return this.makeRequest(`/v1/sessions/${sessionId}`, {
      method: 'PUT',
      body: JSON.stringify(updateData)
    });
  }

  // Analytics
  async getQueueAnalytics(queueId, dateRange, granularity = 'daily') {
    const queryParams = new URLSearchParams({
      startDate: dateRange.startDate,
      endDate: dateRange.endDate,
      granularity: granularity
    });
    
    return this.makeRequest(`/v1/analytics/queues/${queueId}/stats?${queryParams}`);
  }

  // WebSocket Connection
  connectWebSocket() {
    const wsUrl = `${this.baseUrl.replace('http', 'ws')}/v1/ws?token=${this.token}`;
    
    this.ws = new WebSocket(wsUrl);
    
    this.ws.onopen = () => {
      console.log('WebSocket connected');
    };
    
    this.ws.onmessage = (event) => {
      const data = JSON.parse(event.data);
      this.handleWebSocketMessage(data);
    };
    
    this.ws.onclose = () => {
      console.log('WebSocket disconnected');
    };
    
    this.ws.onerror = (error) => {
      console.error('WebSocket error:', error);
    };
  }

  handleWebSocketMessage(data) {
    // Handle different message types
    console.log('WebSocket message:', data);
  }

  disconnectWebSocket() {
    if (this.ws) {
      this.ws.close();
    }
  }
}

// Usage Example
const client = new VirtualQueueClient('https://api.virtualqueue.com', 'your-access-token');

// Create a queue
const queue = await client.createQueue({
  name: 'Customer Service',
  description: 'General customer service queue',
  capacity: 100
});

// Join the queue
const session = await client.joinQueue(queue.id, {
  notes: 'Need help with billing'
});

// Get queue analytics
const analytics = await client.getQueueAnalytics(queue.id, {
  startDate: '2024-01-01',
  endDate: '2024-01-31'
});

// Connect to WebSocket for real-time updates
client.connectWebSocket();
```

## Approval and Sign-off

### **API Examples Approval**
- **API Developer**: [Name] - [Date]
- **Technical Lead**: [Name] - [Date]
- **Documentation Lead**: [Name] - [Date]
- **QA Lead**: [Name] - [Date]

### **Document Control**
- **Version**: 1.0
- **Last Updated**: January 15, 2024
- **Next Review**: February 15, 2024
- **Distribution**: Development Team, API Team, Integration Team

---

**Document Status**: Draft  
**Next Phase**: SDK Documentation  
**Dependencies**: API testing, example validation
