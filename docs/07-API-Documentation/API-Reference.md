# API Reference - Virtual Queue Management System

**Document Version:** 1.0  
**Date:** January 15, 2024  
**Author:** API Team  
**Status:** Draft  
**Phase:** 7 - API Documentation  
**Priority:** 🔴 Critical  

---

## Executive Summary

This document provides comprehensive API reference documentation for the Virtual Queue Management System. It includes all available endpoints, request/response schemas, authentication methods, error handling, and usage examples to enable seamless integration.

## API Overview

### **Base Information**

#### **API Details**
- **Base URL**: `https://api.virtualqueue.com/v1`
- **Protocol**: HTTPS
- **Content Type**: `application/json`
- **API Version**: v1
- **Rate Limit**: 1000 requests per hour per API key
- **Authentication**: Bearer Token (JWT) or API Key

#### **API Features**
- **RESTful Design**: Standard REST API endpoints
- **JSON Format**: All requests and responses in JSON
- **Pagination**: Cursor-based pagination for large datasets
- **Filtering**: Query parameter filtering
- **Sorting**: Query parameter sorting
- **Real-time Updates**: WebSocket/SignalR support

### **API Endpoints Overview**

#### **Core Endpoints**
```
Authentication
├── POST /auth/login
├── POST /auth/refresh
├── POST /auth/logout
└── GET /auth/me

Tenants
├── GET /tenants
├── POST /tenants
├── GET /tenants/{id}
├── PUT /tenants/{id}
└── DELETE /tenants/{id}

Queues
├── GET /queues
├── POST /queues
├── GET /queues/{id}
├── PUT /queues/{id}
├── DELETE /queues/{id}
├── POST /queues/{id}/join
├── POST /queues/{id}/leave
└── GET /queues/{id}/position

User Sessions
├── GET /user-sessions
├── POST /user-sessions
├── GET /user-sessions/{id}
├── PUT /user-sessions/{id}
└── DELETE /user-sessions/{id}
```

## Authentication Endpoints

### **POST /auth/login**

#### **Description**
Authenticate user and receive access token.

#### **Request**
```http
POST /auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "password123",
  "tenantId": "tenant-uuid"
}
```

#### **Response**
```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresIn": 3600,
    "tokenType": "Bearer",
    "user": {
      "id": "user-uuid",
      "email": "user@example.com",
      "name": "John Doe",
      "roles": ["User", "Admin"],
      "tenantId": "tenant-uuid"
    }
  }
}
```

#### **Error Responses**
```http
HTTP/1.1 401 Unauthorized
Content-Type: application/json

{
  "success": false,
  "error": {
    "code": "INVALID_CREDENTIALS",
    "message": "Invalid email or password",
    "details": null
  }
}
```

### **POST /auth/refresh**

#### **Description**
Refresh access token using refresh token.

#### **Request**
```http
POST /auth/refresh
Content-Type: application/json

{
  "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

#### **Response**
```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresIn": 3600,
    "tokenType": "Bearer"
  }
}
```

## Tenant Management Endpoints

### **GET /tenants**

#### **Description**
Retrieve list of tenants with pagination and filtering.

#### **Request**
```http
GET /tenants?page=1&limit=10&search=company&sort=name&order=asc
Authorization: Bearer {access_token}
```

#### **Query Parameters**
- `page` (integer, optional): Page number (default: 1)
- `limit` (integer, optional): Items per page (default: 10, max: 100)
- `search` (string, optional): Search term for name or description
- `sort` (string, optional): Sort field (name, createdAt, updatedAt)
- `order` (string, optional): Sort order (asc, desc)

#### **Response**
```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "success": true,
  "data": {
    "items": [
      {
        "id": "tenant-uuid-1",
        "name": "Company A",
        "description": "Company A description",
        "isActive": true,
        "createdAt": "2024-01-15T10:30:00Z",
        "updatedAt": "2024-01-15T10:30:00Z"
      }
    ],
    "pagination": {
      "page": 1,
      "limit": 10,
      "total": 25,
      "pages": 3,
      "hasNext": true,
      "hasPrev": false
    }
  }
}
```

### **POST /tenants**

#### **Description**
Create a new tenant.

#### **Request**
```http
POST /tenants
Content-Type: application/json
Authorization: Bearer {access_token}

{
  "name": "New Company",
  "description": "New company description",
  "isActive": true
}
```

#### **Response**
```http
HTTP/1.1 201 Created
Content-Type: application/json

{
  "success": true,
  "data": {
    "id": "tenant-uuid",
    "name": "New Company",
    "description": "New company description",
    "isActive": true,
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedAt": "2024-01-15T10:30:00Z"
  }
}
```

## Queue Management Endpoints

### **GET /queues**

#### **Description**
Retrieve list of queues with pagination and filtering.

#### **Request**
```http
GET /queues?tenantId=tenant-uuid&page=1&limit=10&status=active
Authorization: Bearer {access_token}
```

#### **Query Parameters**
- `tenantId` (string, required): Tenant ID filter
- `page` (integer, optional): Page number (default: 1)
- `limit` (integer, optional): Items per page (default: 10, max: 100)
- `status` (string, optional): Queue status filter (active, inactive, paused)
- `search` (string, optional): Search term for name or description

#### **Response**
```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "success": true,
  "data": {
    "items": [
      {
        "id": "queue-uuid-1",
        "tenantId": "tenant-uuid",
        "name": "Customer Service Queue",
        "description": "Main customer service queue",
        "maxConcurrentUsers": 100,
        "releaseRatePerMinute": 10,
        "isActive": true,
        "currentUsers": 45,
        "averageWaitTime": 15,
        "createdAt": "2024-01-15T10:30:00Z",
        "updatedAt": "2024-01-15T10:30:00Z"
      }
    ],
    "pagination": {
      "page": 1,
      "limit": 10,
      "total": 15,
      "pages": 2,
      "hasNext": true,
      "hasPrev": false
    }
  }
}
```

### **POST /queues**

#### **Description**
Create a new queue.

#### **Request**
```http
POST /queues
Content-Type: application/json
Authorization: Bearer {access_token}

{
  "tenantId": "tenant-uuid",
  "name": "New Queue",
  "description": "New queue description",
  "maxConcurrentUsers": 50,
  "releaseRatePerMinute": 5,
  "isActive": true
}
```

#### **Response**
```http
HTTP/1.1 201 Created
Content-Type: application/json

{
  "success": true,
  "data": {
    "id": "queue-uuid",
    "tenantId": "tenant-uuid",
    "name": "New Queue",
    "description": "New queue description",
    "maxConcurrentUsers": 50,
    "releaseRatePerMinute": 5,
    "isActive": true,
    "currentUsers": 0,
    "averageWaitTime": 0,
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedAt": "2024-01-15T10:30:00Z"
  }
}
```

### **POST /queues/{id}/join**

#### **Description**
Join a queue as a user.

#### **Request**
```http
POST /queues/queue-uuid/join
Content-Type: application/json
Authorization: Bearer {access_token}

{
  "userId": "user-uuid",
  "userName": "John Doe",
  "userEmail": "john@example.com",
  "priority": "normal"
}
```

#### **Response**
```http
HTTP/1.1 201 Created
Content-Type: application/json

{
  "success": true,
  "data": {
    "sessionId": "session-uuid",
    "queueId": "queue-uuid",
    "userId": "user-uuid",
    "position": 12,
    "estimatedWaitTime": 8,
    "status": "waiting",
    "joinedAt": "2024-01-15T10:30:00Z"
  }
}
```

### **GET /queues/{id}/position**

#### **Description**
Get current position in queue.

#### **Request**
```http
GET /queues/queue-uuid/position?sessionId=session-uuid
Authorization: Bearer {access_token}
```

#### **Response**
```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "success": true,
  "data": {
    "sessionId": "session-uuid",
    "queueId": "queue-uuid",
    "position": 8,
    "estimatedWaitTime": 5,
    "status": "waiting",
    "totalUsers": 45,
    "lastUpdated": "2024-01-15T10:35:00Z"
  }
}
```

## User Session Endpoints

### **GET /user-sessions**

#### **Description**
Retrieve user sessions with pagination and filtering.

#### **Request**
```http
GET /user-sessions?userId=user-uuid&status=active&page=1&limit=10
Authorization: Bearer {access_token}
```

#### **Query Parameters**
- `userId` (string, optional): User ID filter
- `queueId` (string, optional): Queue ID filter
- `status` (string, optional): Session status filter (waiting, active, completed, cancelled)
- `page` (integer, optional): Page number (default: 1)
- `limit` (integer, optional): Items per page (default: 10, max: 100)

#### **Response**
```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "success": true,
  "data": {
    "items": [
      {
        "id": "session-uuid-1",
        "queueId": "queue-uuid",
        "userId": "user-uuid",
        "userName": "John Doe",
        "userEmail": "john@example.com",
        "status": "waiting",
        "position": 8,
        "priority": "normal",
        "joinedAt": "2024-01-15T10:30:00Z",
        "estimatedWaitTime": 5
      }
    ],
    "pagination": {
      "page": 1,
      "limit": 10,
      "total": 5,
      "pages": 1,
      "hasNext": false,
      "hasPrev": false
    }
  }
}
```

## Error Handling

### **Error Response Format**

#### **Standard Error Response**
```http
HTTP/1.1 400 Bad Request
Content-Type: application/json

{
  "success": false,
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Request validation failed",
    "details": [
      {
        "field": "name",
        "message": "Name is required"
      },
      {
        "field": "maxConcurrentUsers",
        "message": "Max concurrent users must be greater than 0"
      }
    ],
    "timestamp": "2024-01-15T10:30:00Z",
    "requestId": "req-123456"
  }
}
```

### **Error Codes**

#### **Common Error Codes**
- `VALIDATION_ERROR`: Request validation failed
- `AUTHENTICATION_REQUIRED`: Authentication required
- `AUTHORIZATION_FAILED`: Insufficient permissions
- `RESOURCE_NOT_FOUND`: Resource not found
- `RESOURCE_CONFLICT`: Resource conflict
- `RATE_LIMIT_EXCEEDED`: Rate limit exceeded
- `INTERNAL_SERVER_ERROR`: Internal server error
- `SERVICE_UNAVAILABLE`: Service temporarily unavailable

## Rate Limiting

### **Rate Limit Headers**

#### **Response Headers**
```http
HTTP/1.1 200 OK
X-RateLimit-Limit: 1000
X-RateLimit-Remaining: 999
X-RateLimit-Reset: 1642248600
X-RateLimit-Window: 3600
```

#### **Rate Limit Exceeded Response**
```http
HTTP/1.1 429 Too Many Requests
Content-Type: application/json
Retry-After: 3600

{
  "success": false,
  "error": {
    "code": "RATE_LIMIT_EXCEEDED",
    "message": "Rate limit exceeded",
    "details": {
      "limit": 1000,
      "remaining": 0,
      "resetTime": "2024-01-15T11:30:00Z"
    }
  }
}
```

## Pagination

### **Pagination Format**

#### **Pagination Parameters**
- `page`: Page number (1-based)
- `limit`: Items per page (max: 100)
- `cursor`: Cursor for cursor-based pagination
- `sort`: Sort field
- `order`: Sort order (asc, desc)

#### **Pagination Response**
```json
{
  "pagination": {
    "page": 1,
    "limit": 10,
    "total": 25,
    "pages": 3,
    "hasNext": true,
    "hasPrev": false,
    "nextCursor": "cursor-string",
    "prevCursor": null
  }
}
```

## WebSocket/SignalR Integration

### **Connection**

#### **Connection URL**
```
wss://api.virtualqueue.com/hubs/queue
```

#### **Authentication**
```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/queue", {
        accessTokenFactory: () => getAccessToken()
    })
    .build();
```

### **Events**

#### **Queue Events**
- `QueueUpdated`: Queue information updated
- `UserJoined`: User joined queue
- `UserLeft`: User left queue
- `PositionChanged`: User position changed
- `QueueProcessed`: Queue processing completed

#### **Event Example**
```javascript
connection.on("PositionChanged", (data) => {
    console.log(`Position updated: ${data.position}`);
    console.log(`Estimated wait time: ${data.estimatedWaitTime}`);
});
```

## SDK and Client Libraries

### **Available SDKs**

#### **JavaScript/Node.js**
```bash
npm install @virtualqueue/sdk
```

```javascript
import { VirtualQueueClient } from '@virtualqueue/sdk';

const client = new VirtualQueueClient({
    baseUrl: 'https://api.virtualqueue.com/v1',
    apiKey: 'your-api-key'
});

const queue = await client.queues.create({
    name: 'New Queue',
    maxConcurrentUsers: 50
});
```

#### **Python**
```bash
pip install virtualqueue-sdk
```

```python
from virtualqueue import VirtualQueueClient

client = VirtualQueueClient(
    base_url='https://api.virtualqueue.com/v1',
    api_key='your-api-key'
)

queue = client.queues.create({
    'name': 'New Queue',
    'max_concurrent_users': 50
})
```

#### **C#/.NET**
```bash
dotnet add package VirtualQueue.SDK
```

```csharp
using VirtualQueue.SDK;

var client = new VirtualQueueClient(
    baseUrl: "https://api.virtualqueue.com/v1",
    apiKey: "your-api-key"
);

var queue = await client.Queues.CreateAsync(new CreateQueueRequest
{
    Name = "New Queue",
    MaxConcurrentUsers = 50
});
```

## Testing and Sandbox

### **Sandbox Environment**

#### **Sandbox URL**
```
https://sandbox-api.virtualqueue.com/v1
```

#### **Sandbox Features**
- Full API functionality
- Test data and scenarios
- No production impact
- Rate limit testing
- Webhook testing

### **API Testing Tools**

#### **Postman Collection**
- Import Postman collection
- Pre-configured requests
- Environment variables
- Test scripts and assertions

#### **OpenAPI Specification**
- Download OpenAPI spec
- Generate client SDKs
- Interactive documentation
- API testing tools

## Approval and Sign-off

### **API Reference Approval**
- **API Team**: [Name] - [Date]
- **Development Team**: [Name] - [Date]
- **Integration Team**: [Name] - [Date]
- **Management**: [Name] - [Date]

### **Document Control**
- **Version**: 1.0
- **Last Updated**: January 15, 2024
- **Next Review**: February 15, 2024
- **Distribution**: API Team, Development Team, Integration Team

---

**Document Status**: Draft  
**Next Phase**: Authentication Guide  
**Dependencies**: API implementation, endpoint testing, documentation validation
