# Authentication Guide - Virtual Queue Management System

**Document Version:** 1.0  
**Date:** January 15, 2024  
**Author:** Security Team  
**Status:** Draft  
**Phase:** 7 - API Documentation  
**Priority:** 🔴 Critical  

---

## Executive Summary

This document provides comprehensive authentication guidance for the Virtual Queue Management System API. It covers authentication methods, token management, security best practices, and integration examples to ensure secure API access.

## Authentication Overview

### **Authentication Methods**

#### **Supported Methods**
- **JWT Bearer Tokens**: Primary authentication method
- **API Keys**: For server-to-server communication
- **OAuth 2.0**: For third-party integrations
- **Multi-Factor Authentication**: Enhanced security
- **Session-based Authentication**: Web application support

#### **Authentication Flow**
```
1. Client Request → 2. Authentication Check → 3. Token Validation → 4. Authorization Check → 5. API Response
```

### **Security Features**

#### **Security Measures**
- **Token Expiration**: Short-lived access tokens
- **Refresh Tokens**: Long-lived refresh tokens
- **Token Rotation**: Automatic token rotation
- **Rate Limiting**: Per-token rate limiting
- **Audit Logging**: Comprehensive authentication logging

## JWT Bearer Token Authentication

### **Token Structure**

#### **JWT Token Components**
```json
{
  "header": {
    "alg": "HS256",
    "typ": "JWT"
  },
  "payload": {
    "sub": "user-uuid",
    "email": "user@example.com",
    "name": "John Doe",
    "roles": ["User", "Admin"],
    "tenantId": "tenant-uuid",
    "iat": 1642248600,
    "exp": 1642252200,
    "iss": "virtual-queue-api",
    "aud": "virtual-queue-client"
  }
}
```

#### **Token Claims**
- `sub`: Subject (user ID)
- `email`: User email address
- `name`: User display name
- `roles`: User roles and permissions
- `tenantId`: Tenant identifier
- `iat`: Issued at timestamp
- `exp`: Expiration timestamp
- `iss`: Issuer
- `aud`: Audience

### **Token Management**

#### **Access Token**
- **Lifetime**: 1 hour
- **Usage**: API request authentication
- **Storage**: Client-side storage
- **Rotation**: Automatic rotation on refresh

#### **Refresh Token**
- **Lifetime**: 30 days
- **Usage**: Access token renewal
- **Storage**: Secure server-side storage
- **Rotation**: Rotated on each use

### **Authentication Request**

#### **Login Request**
```http
POST /auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "password123",
  "tenantId": "tenant-uuid",
  "rememberMe": false
}
```

#### **Login Response**
```http
HTTP/1.1 200 OK
Content-Type: application/json
Set-Cookie: refreshToken=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...; HttpOnly; Secure; SameSite=Strict

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
      "tenantId": "tenant-uuid",
      "isEmailVerified": true,
      "lastLoginAt": "2024-01-15T10:30:00Z"
    }
  }
}
```

### **Token Refresh**

#### **Refresh Request**
```http
POST /auth/refresh
Content-Type: application/json

{
  "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

#### **Refresh Response**
```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresIn": 3600,
    "tokenType": "Bearer"
  }
}
```

### **API Request with Token**

#### **Authenticated Request**
```http
GET /queues
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json
```

#### **Token Validation Response**
```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "success": true,
  "data": {
    "items": [...],
    "pagination": {...}
  }
}
```

## API Key Authentication

### **API Key Generation**

#### **Create API Key Request**
```http
POST /api-keys
Authorization: Bearer {access_token}
Content-Type: application/json

{
  "name": "Production API Key",
  "description": "API key for production integration",
  "permissions": ["queues:read", "queues:write", "users:read"],
  "expiresAt": "2024-12-31T23:59:59Z"
}
```

#### **Create API Key Response**
```http
HTTP/1.1 201 Created
Content-Type: application/json

{
  "success": true,
  "data": {
    "id": "api-key-uuid",
    "name": "Production API Key",
    "description": "API key for production integration",
    "key": "vq_live_1234567890abcdef",
    "permissions": ["queues:read", "queues:write", "users:read"],
    "expiresAt": "2024-12-31T23:59:59Z",
    "createdAt": "2024-01-15T10:30:00Z",
    "lastUsedAt": null
  }
}
```

### **API Key Usage**

#### **Request with API Key**
```http
GET /queues
X-API-Key: vq_live_1234567890abcdef
Content-Type: application/json
```

#### **API Key Validation**
- **Key Format**: `vq_{environment}_{key}`
- **Environment**: `live`, `sandbox`, `test`
- **Permissions**: Scoped permissions
- **Rate Limiting**: Per-key rate limiting
- **Audit Logging**: All API key usage logged

## OAuth 2.0 Integration

### **OAuth 2.0 Flow**

#### **Authorization Code Flow**
```
1. Client redirects to authorization server
2. User authorizes application
3. Authorization server returns authorization code
4. Client exchanges code for access token
5. Client uses access token for API requests
```

#### **Authorization Request**
```http
GET /oauth/authorize?
  client_id=your-client-id&
  redirect_uri=https://your-app.com/callback&
  response_type=code&
  scope=queues:read queues:write&
  state=random-state-string
```

#### **Token Exchange Request**
```http
POST /oauth/token
Content-Type: application/x-www-form-urlencoded

grant_type=authorization_code&
code=authorization-code&
redirect_uri=https://your-app.com/callback&
client_id=your-client-id&
client_secret=your-client-secret
```

#### **Token Exchange Response**
```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "access_token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refresh_token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "token_type": "Bearer",
  "expires_in": 3600,
  "scope": "queues:read queues:write"
}
```

### **OAuth Scopes**

#### **Available Scopes**
- `queues:read`: Read queue information
- `queues:write`: Create and update queues
- `queues:delete`: Delete queues
- `users:read`: Read user information
- `users:write`: Create and update users
- `sessions:read`: Read user sessions
- `sessions:write`: Create and update sessions
- `admin`: Administrative access

## Multi-Factor Authentication

### **MFA Setup**

#### **Enable MFA Request**
```http
POST /auth/mfa/enable
Authorization: Bearer {access_token}
Content-Type: application/json

{
  "method": "totp",
  "phoneNumber": "+1234567890"
}
```

#### **MFA Setup Response**
```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "success": true,
  "data": {
    "qrCode": "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAA...",
    "secret": "JBSWY3DPEHPK3PXP",
    "backupCodes": [
      "12345678",
      "87654321",
      "11223344"
    ]
  }
}
```

### **MFA Authentication**

#### **Login with MFA**
```http
POST /auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "password123",
  "tenantId": "tenant-uuid",
  "mfaCode": "123456"
}
```

#### **MFA Verification**
```http
POST /auth/mfa/verify
Authorization: Bearer {access_token}
Content-Type: application/json

{
  "code": "123456"
}
```

## Security Best Practices

### **Token Security**

#### **Token Storage**
- **Client-side**: Secure storage (HttpOnly cookies)
- **Server-side**: Encrypted storage
- **Mobile**: Keychain/Keystore
- **Web**: Secure cookie storage

#### **Token Transmission**
- **HTTPS Only**: Always use HTTPS
- **Secure Headers**: Use secure headers
- **Token Rotation**: Regular token rotation
- **Audit Logging**: Log all token usage

### **API Security**

#### **Request Security**
```http
GET /queues
Authorization: Bearer {access_token}
X-API-Key: {api_key}
X-Request-ID: {request_id}
X-Timestamp: {timestamp}
X-Signature: {signature}
Content-Type: application/json
```

#### **Response Security**
```http
HTTP/1.1 200 OK
Content-Type: application/json
X-Content-Type-Options: nosniff
X-Frame-Options: DENY
X-XSS-Protection: 1; mode=block
Strict-Transport-Security: max-age=31536000; includeSubDomains
```

### **Error Handling**

#### **Authentication Errors**
```http
HTTP/1.1 401 Unauthorized
Content-Type: application/json
WWW-Authenticate: Bearer realm="api"

{
  "success": false,
  "error": {
    "code": "AUTHENTICATION_REQUIRED",
    "message": "Authentication required",
    "details": {
      "authUrl": "https://api.virtualqueue.com/auth/login",
      "tokenExpired": true
    }
  }
}
```

#### **Authorization Errors**
```http
HTTP/1.1 403 Forbidden
Content-Type: application/json

{
  "success": false,
  "error": {
    "code": "AUTHORIZATION_FAILED",
    "message": "Insufficient permissions",
    "details": {
      "requiredPermissions": ["queues:write"],
      "userPermissions": ["queues:read"]
    }
  }
}
```

## Integration Examples

### **JavaScript/Node.js**

#### **Authentication Client**
```javascript
class VirtualQueueAuthClient {
    constructor(baseUrl, clientId, clientSecret) {
        this.baseUrl = baseUrl;
        this.clientId = clientId;
        this.clientSecret = clientSecret;
        this.accessToken = null;
        this.refreshToken = null;
    }

    async login(email, password, tenantId) {
        const response = await fetch(`${this.baseUrl}/auth/login`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                email,
                password,
                tenantId
            })
        });

        const data = await response.json();
        
        if (data.success) {
            this.accessToken = data.data.accessToken;
            this.refreshToken = data.data.refreshToken;
            return data.data;
        }
        
        throw new Error(data.error.message);
    }

    async refreshAccessToken() {
        const response = await fetch(`${this.baseUrl}/auth/refresh`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                refreshToken: this.refreshToken
            })
        });

        const data = await response.json();
        
        if (data.success) {
            this.accessToken = data.data.accessToken;
            this.refreshToken = data.data.refreshToken;
            return data.data;
        }
        
        throw new Error(data.error.message);
    }

    async makeAuthenticatedRequest(url, options = {}) {
        if (!this.accessToken) {
            throw new Error('No access token available');
        }

        const response = await fetch(url, {
            ...options,
            headers: {
                'Authorization': `Bearer ${this.accessToken}`,
                'Content-Type': 'application/json',
                ...options.headers
            }
        });

        if (response.status === 401) {
            // Token expired, try to refresh
            await this.refreshAccessToken();
            
            // Retry request with new token
            return fetch(url, {
                ...options,
                headers: {
                    'Authorization': `Bearer ${this.accessToken}`,
                    'Content-Type': 'application/json',
                    ...options.headers
                }
            });
        }

        return response;
    }
}
```

### **Python**

#### **Authentication Client**
```python
import requests
import json
from datetime import datetime, timedelta

class VirtualQueueAuthClient:
    def __init__(self, base_url, client_id, client_secret):
        self.base_url = base_url
        self.client_id = client_id
        self.client_secret = client_secret
        self.access_token = None
        self.refresh_token = None
        self.token_expires_at = None

    def login(self, email, password, tenant_id):
        response = requests.post(
            f"{self.base_url}/auth/login",
            json={
                "email": email,
                "password": password,
                "tenantId": tenant_id
            }
        )
        
        data = response.json()
        
        if data["success"]:
            self.access_token = data["data"]["accessToken"]
            self.refresh_token = data["data"]["refreshToken"]
            self.token_expires_at = datetime.now() + timedelta(seconds=data["data"]["expiresIn"])
            return data["data"]
        
        raise Exception(data["error"]["message"])

    def refresh_access_token(self):
        response = requests.post(
            f"{self.base_url}/auth/refresh",
            json={"refreshToken": self.refresh_token}
        )
        
        data = response.json()
        
        if data["success"]:
            self.access_token = data["data"]["accessToken"]
            self.refresh_token = data["data"]["refreshToken"]
            self.token_expires_at = datetime.now() + timedelta(seconds=data["data"]["expiresIn"])
            return data["data"]
        
        raise Exception(data["error"]["message"])

    def make_authenticated_request(self, method, url, **kwargs):
        if not self.access_token:
            raise Exception("No access token available")
        
        if self.token_expires_at and datetime.now() >= self.token_expires_at:
            self.refresh_access_token()
        
        headers = kwargs.get("headers", {})
        headers["Authorization"] = f"Bearer {self.access_token}"
        headers["Content-Type"] = "application/json"
        kwargs["headers"] = headers
        
        response = requests.request(method, url, **kwargs)
        
        if response.status_code == 401:
            # Token expired, try to refresh
            self.refresh_access_token()
            
            # Retry request with new token
            headers["Authorization"] = f"Bearer {self.access_token}"
            response = requests.request(method, url, **kwargs)
        
        return response
```

### **C#/.NET**

#### **Authentication Client**
```csharp
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class VirtualQueueAuthClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _clientId;
    private readonly string _clientSecret;
    
    private string _accessToken;
    private string _refreshToken;
    private DateTime _tokenExpiresAt;

    public VirtualQueueAuthClient(string baseUrl, string clientId, string clientSecret)
    {
        _baseUrl = baseUrl;
        _clientId = clientId;
        _clientSecret = clientSecret;
        _httpClient = new HttpClient();
    }

    public async Task<LoginResponse> LoginAsync(string email, string password, string tenantId)
    {
        var request = new LoginRequest
        {
            Email = email,
            Password = password,
            TenantId = tenantId
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{_baseUrl}/auth/login", content);
        var responseContent = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<ApiResponse<LoginResponse>>(responseContent);

        if (data.Success)
        {
            _accessToken = data.Data.AccessToken;
            _refreshToken = data.Data.RefreshToken;
            _tokenExpiresAt = DateTime.UtcNow.AddSeconds(data.Data.ExpiresIn);
            return data.Data;
        }

        throw new Exception(data.Error.Message);
    }

    public async Task<RefreshResponse> RefreshAccessTokenAsync()
    {
        var request = new RefreshRequest
        {
            RefreshToken = _refreshToken
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{_baseUrl}/auth/refresh", content);
        var responseContent = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<ApiResponse<RefreshResponse>>(responseContent);

        if (data.Success)
        {
            _accessToken = data.Data.AccessToken;
            _refreshToken = data.Data.RefreshToken;
            _tokenExpiresAt = DateTime.UtcNow.AddSeconds(data.Data.ExpiresIn);
            return data.Data;
        }

        throw new Exception(data.Error.Message);
    }

    public async Task<HttpResponseMessage> MakeAuthenticatedRequestAsync(HttpMethod method, string url, object content = null)
    {
        if (string.IsNullOrEmpty(_accessToken))
        {
            throw new Exception("No access token available");
        }

        if (DateTime.UtcNow >= _tokenExpiresAt)
        {
            await RefreshAccessTokenAsync();
        }

        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);

        if (content != null)
        {
            var json = JsonSerializer.Serialize(content);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        var response = await _httpClient.SendAsync(request);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            // Token expired, try to refresh
            await RefreshAccessTokenAsync();
            
            // Retry request with new token
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);
            response = await _httpClient.SendAsync(request);
        }

        return response;
    }
}
```

## Troubleshooting

### **Common Issues**

#### **Authentication Issues**
- **Invalid Credentials**: Check email and password
- **Token Expired**: Refresh access token
- **Invalid Token**: Re-authenticate
- **Permission Denied**: Check user permissions
- **Rate Limited**: Wait and retry

#### **Integration Issues**
- **CORS Errors**: Configure CORS properly
- **SSL Errors**: Use HTTPS
- **Network Errors**: Check network connectivity
- **Timeout Errors**: Increase timeout values
- **Parsing Errors**: Check JSON format

### **Debugging Tips**

#### **Token Debugging**
```javascript
// Decode JWT token (client-side)
function decodeJWT(token) {
    const base64Url = token.split('.')[1];
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    const jsonPayload = decodeURIComponent(atob(base64).split('').map(function(c) {
        return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
    }).join(''));
    
    return JSON.parse(jsonPayload);
}

// Check token expiration
const token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...";
const payload = decodeJWT(token);
const isExpired = Date.now() >= payload.exp * 1000;
console.log('Token expired:', isExpired);
```

## Approval and Sign-off

### **Authentication Guide Approval**
- **Security Team**: [Name] - [Date]
- **API Team**: [Name] - [Date]
- **Development Team**: [Name] - [Date]
- **Management**: [Name] - [Date]

### **Document Control**
- **Version**: 1.0
- **Last Updated**: January 15, 2024
- **Next Review**: February 15, 2024
- **Distribution**: Security Team, API Team, Development Team

---

**Document Status**: Draft  
**Next Phase**: Integration Guide  
**Dependencies**: Authentication implementation, security testing, documentation validation
