# Rate Limiting - Virtual Queue Management System

**Document Version:** 1.0  
**Date:** January 15, 2024  
**Author:** API Team  
**Status:** Draft  
**Phase:** 7 - API Documentation  
**Priority:** 🔴 Critical  

---

## Executive Summary

This document provides comprehensive rate limiting guidance for the Virtual Queue Management System API. It covers rate limiting policies, limits, headers, handling procedures, best practices, and monitoring to ensure fair API usage and system stability.

## Rate Limiting Overview

### **Rate Limiting Purpose**

#### **Objectives**
- **Fair Usage**: Ensure fair access to API resources
- **System Protection**: Prevent system overload and abuse
- **Quality of Service**: Maintain consistent API performance
- **Cost Control**: Manage infrastructure costs
- **Compliance**: Meet service level agreements

#### **Rate Limiting Benefits**
- **Prevents Abuse**: Protects against malicious or excessive usage
- **Ensures Stability**: Maintains system performance and availability
- **Enables Scaling**: Allows predictable resource planning
- **Improves Experience**: Provides consistent response times
- **Reduces Costs**: Optimizes infrastructure utilization

### **Rate Limiting Architecture**

#### **Rate Limiting Components**
```
Client Request
├── Rate Limiter
├── Redis Cache (Rate Storage)
├── Policy Engine
├── Header Injection
└── Response/Error Handling
```

#### **Rate Limiting Flow**
```
1. Request Received → 2. Check Rate Limits → 3. Update Counters → 4. Allow/Deny Request → 5. Return Response
```

## Rate Limiting Policies

### **Rate Limiting Tiers**

#### **Free Tier**
```yaml
free_tier:
  requests_per_hour: 1000
  requests_per_day: 10000
  concurrent_requests: 10
  burst_limit: 100
  
  endpoints:
    authentication: 100_per_hour
    queues_read: 500_per_hour
    queues_write: 100_per_hour
    users_read: 200_per_hour
    users_write: 50_per_hour
```

#### **Standard Tier**
```yaml
standard_tier:
  requests_per_hour: 10000
  requests_per_day: 100000
  concurrent_requests: 50
  burst_limit: 500
  
  endpoints:
    authentication: 1000_per_hour
    queues_read: 5000_per_hour
    queues_write: 1000_per_hour
    users_read: 2000_per_hour
    users_write: 500_per_hour
```

#### **Premium Tier**
```yaml
premium_tier:
  requests_per_hour: 100000
  requests_per_day: 1000000
  concurrent_requests: 200
  burst_limit: 2000
  
  endpoints:
    authentication: 10000_per_hour
    queues_read: 50000_per_hour
    queues_write: 10000_per_hour
    users_read: 20000_per_hour
    users_write: 5000_per_hour
```

#### **Enterprise Tier**
```yaml
enterprise_tier:
  requests_per_hour: 1000000
  requests_per_day: 10000000
  concurrent_requests: 1000
  burst_limit: 10000
  
  endpoints:
    authentication: 100000_per_hour
    queues_read: 500000_per_hour
    queues_write: 100000_per_hour
    users_read: 200000_per_hour
    users_write: 50000_per_hour
```

### **Rate Limiting Rules**

#### **Global Rate Limits**
- **Per API Key**: Rate limits applied per API key
- **Per User**: Rate limits applied per authenticated user
- **Per IP Address**: Rate limits applied per IP address
- **Per Tenant**: Rate limits applied per tenant
- **Per Endpoint**: Rate limits applied per specific endpoint

#### **Rate Limiting Windows**
- **Sliding Window**: 1 hour, 1 day, 1 month
- **Fixed Window**: Calendar hour, day, month
- **Token Bucket**: Burst allowance with refill rate
- **Leaky Bucket**: Smooth rate limiting

## Rate Limiting Headers

### **Response Headers**

#### **Standard Rate Limit Headers**
```http
HTTP/1.1 200 OK
Content-Type: application/json
X-RateLimit-Limit: 1000
X-RateLimit-Remaining: 999
X-RateLimit-Reset: 1642252200
X-RateLimit-Window: 3600
X-RateLimit-Policy: standard
```

#### **Header Descriptions**
- `X-RateLimit-Limit`: Maximum requests allowed in the window
- `X-RateLimit-Remaining`: Number of requests remaining in the window
- `X-RateLimit-Reset`: Unix timestamp when the window resets
- `X-RateLimit-Window`: Window size in seconds
- `X-RateLimit-Policy`: Rate limiting policy/tier name

### **Rate Limit Exceeded Headers**

#### **429 Response Headers**
```http
HTTP/1.1 429 Too Many Requests
Content-Type: application/json
Retry-After: 3600
X-RateLimit-Limit: 1000
X-RateLimit-Remaining: 0
X-RateLimit-Reset: 1642252200
X-RateLimit-Window: 3600
X-RateLimit-Policy: standard
```

#### **Additional Headers**
- `Retry-After`: Seconds to wait before retrying
- `X-RateLimit-Retry-After`: Alternative retry after header
- `X-RateLimit-Burst-Remaining`: Burst requests remaining
- `X-RateLimit-Concurrent-Remaining`: Concurrent requests remaining

## Rate Limiting Implementation

### **Rate Limiting Algorithm**

#### **Token Bucket Algorithm**
```javascript
class TokenBucket {
    constructor(capacity, refillRate, refillPeriod) {
        this.capacity = capacity;
        this.refillRate = refillRate;
        this.refillPeriod = refillPeriod;
        this.tokens = capacity;
        this.lastRefill = Date.now();
    }

    consume(tokens = 1) {
        this.refill();
        
        if (this.tokens >= tokens) {
            this.tokens -= tokens;
            return true;
        }
        
        return false;
    }

    refill() {
        const now = Date.now();
        const timePassed = now - this.lastRefill;
        const tokensToAdd = Math.floor(timePassed / this.refillPeriod) * this.refillRate;
        
        if (tokensToAdd > 0) {
            this.tokens = Math.min(this.capacity, this.tokens + tokensToAdd);
            this.lastRefill = now;
        }
    }

    getRemaining() {
        this.refill();
        return this.tokens;
    }

    getResetTime() {
        const tokensNeeded = this.capacity - this.tokens;
        const timeToRefill = tokensNeeded * this.refillPeriod;
        return Date.now() + timeToRefill;
    }
}
```

#### **Sliding Window Algorithm**
```javascript
class SlidingWindow {
    constructor(windowSize, maxRequests) {
        this.windowSize = windowSize; // in milliseconds
        this.maxRequests = maxRequests;
        this.requests = [];
    }

    isAllowed() {
        const now = Date.now();
        const windowStart = now - this.windowSize;
        
        // Remove old requests
        this.requests = this.requests.filter(time => time > windowStart);
        
        // Check if under limit
        if (this.requests.length < this.maxRequests) {
            this.requests.push(now);
            return true;
        }
        
        return false;
    }

    getRemaining() {
        const now = Date.now();
        const windowStart = now - this.windowSize;
        
        // Remove old requests
        this.requests = this.requests.filter(time => time > windowStart);
        
        return Math.max(0, this.maxRequests - this.requests.length);
    }

    getResetTime() {
        if (this.requests.length === 0) {
            return Date.now() + this.windowSize;
        }
        
        const oldestRequest = Math.min(...this.requests);
        return oldestRequest + this.windowSize;
    }
}
```

### **Rate Limiting Middleware**

#### **Express.js Middleware**
```javascript
const rateLimit = require('express-rate-limit');
const RedisStore = require('rate-limit-redis');
const Redis = require('redis');

const redisClient = Redis.createClient({
    host: 'redis-host',
    port: 6379
});

const rateLimiter = rateLimit({
    store: new RedisStore({
        client: redisClient,
        prefix: 'rl:',
    }),
    windowMs: 60 * 60 * 1000, // 1 hour
    max: 1000, // limit each IP to 1000 requests per windowMs
    message: {
        success: false,
        error: {
            code: 'RATE_LIMIT_EXCEEDED',
            message: 'Rate limit exceeded',
            details: {
                limit: 1000,
                remaining: 0,
                resetTime: new Date(Date.now() + 60 * 60 * 1000).toISOString()
            }
        }
    },
    standardHeaders: true,
    legacyHeaders: false,
    handler: (req, res) => {
        res.status(429).json({
            success: false,
            error: {
                code: 'RATE_LIMIT_EXCEEDED',
                message: 'Rate limit exceeded',
                details: {
                    limit: 1000,
                    remaining: 0,
                    resetTime: new Date(Date.now() + 60 * 60 * 1000).toISOString()
                }
            }
        });
    }
});

app.use('/api/', rateLimiter);
```

#### **ASP.NET Core Middleware**
```csharp
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;
    private readonly ILogger<RateLimitingMiddleware> _logger;

    public RateLimitingMiddleware(RequestDelegate next, IMemoryCache cache, ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _cache = cache;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var key = GetRateLimitKey(context);
        var rateLimit = GetRateLimit(context);
        
        var currentCount = await GetCurrentCount(key);
        
        if (currentCount >= rateLimit.Limit)
        {
            context.Response.StatusCode = 429;
            context.Response.Headers.Add("Retry-After", rateLimit.ResetTime.ToString());
            context.Response.Headers.Add("X-RateLimit-Limit", rateLimit.Limit.ToString());
            context.Response.Headers.Add("X-RateLimit-Remaining", "0");
            context.Response.Headers.Add("X-RateLimit-Reset", rateLimit.ResetTime.ToString());
            
            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                success = false,
                error = new
                {
                    code = "RATE_LIMIT_EXCEEDED",
                    message = "Rate limit exceeded",
                    details = new
                    {
                        limit = rateLimit.Limit,
                        remaining = 0,
                        resetTime = rateLimit.ResetTime
                    }
                }
            }));
            
            return;
        }
        
        await IncrementCount(key, rateLimit.Window);
        
        context.Response.Headers.Add("X-RateLimit-Limit", rateLimit.Limit.ToString());
        context.Response.Headers.Add("X-RateLimit-Remaining", (rateLimit.Limit - currentCount - 1).ToString());
        context.Response.Headers.Add("X-RateLimit-Reset", rateLimit.ResetTime.ToString());
        
        await _next(context);
    }

    private string GetRateLimitKey(HttpContext context)
    {
        var apiKey = context.Request.Headers["X-API-Key"].FirstOrDefault();
        var userId = context.User?.FindFirst("sub")?.Value;
        var ipAddress = context.Connection.RemoteIpAddress?.ToString();
        
        return $"rate_limit:{apiKey ?? userId ?? ipAddress}";
    }

    private RateLimit GetRateLimit(HttpContext context)
    {
        var apiKey = context.Request.Headers["X-API-Key"].FirstOrDefault();
        
        if (!string.IsNullOrEmpty(apiKey))
        {
            return GetApiKeyRateLimit(apiKey);
        }
        
        return GetDefaultRateLimit();
    }

    private async Task<int> GetCurrentCount(string key)
    {
        if (_cache.TryGetValue(key, out int count))
        {
            return count;
        }
        
        return 0;
    }

    private async Task IncrementCount(string key, TimeSpan window)
    {
        if (_cache.TryGetValue(key, out int count))
        {
            _cache.Set(key, count + 1, window);
        }
        else
        {
            _cache.Set(key, 1, window);
        }
    }
}
```

## Rate Limit Handling

### **Client-Side Handling**

#### **JavaScript Rate Limit Handler**
```javascript
class RateLimitHandler {
    constructor() {
        this.retryAfter = 0;
        this.rateLimitInfo = null;
    }

    async handleRateLimit(response) {
        if (response.status === 429) {
            const rateLimitInfo = this.extractRateLimitInfo(response);
            this.rateLimitInfo = rateLimitInfo;
            this.retryAfter = rateLimitInfo.retryAfter || 60;
            
            console.warn(`Rate limit exceeded. Retry after ${this.retryAfter} seconds`);
            
            // Wait for retry after period
            await this.sleep(this.retryAfter * 1000);
            
            return true; // Indicates retry should be attempted
        }
        
        return false;
    }

    extractRateLimitInfo(response) {
        const headers = response.headers;
        
        return {
            limit: parseInt(headers.get('X-RateLimit-Limit') || '0'),
            remaining: parseInt(headers.get('X-RateLimit-Remaining') || '0'),
            reset: parseInt(headers.get('X-RateLimit-Reset') || '0'),
            retryAfter: parseInt(headers.get('Retry-After') || '60'),
            policy: headers.get('X-RateLimit-Policy') || 'default'
        };
    }

    async sleep(ms) {
        return new Promise(resolve => setTimeout(resolve, ms));
    }

    getRateLimitInfo() {
        return this.rateLimitInfo;
    }

    isRateLimited() {
        return this.retryAfter > 0;
    }

    getRetryAfter() {
        return this.retryAfter;
    }
}
```

#### **Python Rate Limit Handler**
```python
import time
import logging
from typing import Dict, Any, Optional

class RateLimitHandler:
    def __init__(self):
        self.retry_after = 0
        self.rate_limit_info = None
        self.logger = logging.getLogger(__name__)

    def handle_rate_limit(self, response) -> bool:
        if response.status_code == 429:
            rate_limit_info = self.extract_rate_limit_info(response)
            self.rate_limit_info = rate_limit_info
            self.retry_after = rate_limit_info.get('retry_after', 60)
            
            self.logger.warning(f"Rate limit exceeded. Retry after {self.retry_after} seconds")
            
            # Wait for retry after period
            time.sleep(self.retry_after)
            
            return True  # Indicates retry should be attempted
        
        return False

    def extract_rate_limit_info(self, response) -> Dict[str, Any]:
        headers = response.headers
        
        return {
            'limit': int(headers.get('X-RateLimit-Limit', 0)),
            'remaining': int(headers.get('X-RateLimit-Remaining', 0)),
            'reset': int(headers.get('X-RateLimit-Reset', 0)),
            'retry_after': int(headers.get('Retry-After', 60)),
            'policy': headers.get('X-RateLimit-Policy', 'default')
        }

    def get_rate_limit_info(self) -> Optional[Dict[str, Any]]:
        return self.rate_limit_info

    def is_rate_limited(self) -> bool:
        return self.retry_after > 0

    def get_retry_after(self) -> int:
        return self.retry_after
```

### **Exponential Backoff**

#### **Exponential Backoff Implementation**
```javascript
class ExponentialBackoff {
    constructor(options = {}) {
        this.baseDelay = options.baseDelay || 1000;
        this.maxDelay = options.maxDelay || 60000;
        this.maxRetries = options.maxRetries || 5;
        this.multiplier = options.multiplier || 2;
        this.jitter = options.jitter || 0.1;
    }

    async executeWithBackoff(operation, ...args) {
        let lastError;
        
        for (let attempt = 0; attempt <= this.maxRetries; attempt++) {
            try {
                return await operation.apply(this, args);
            } catch (error) {
                lastError = error;
                
                if (attempt === this.maxRetries) {
                    throw error;
                }
                
                if (this.shouldRetry(error)) {
                    const delay = this.calculateDelay(attempt);
                    console.log(`Retrying in ${delay}ms (attempt ${attempt + 1})`);
                    await this.sleep(delay);
                } else {
                    throw error;
                }
            }
        }
    }

    shouldRetry(error) {
        return error.status === 429 || error.status >= 500;
    }

    calculateDelay(attempt) {
        let delay = this.baseDelay * Math.pow(this.multiplier, attempt);
        
        // Add jitter to prevent thundering herd
        const jitterAmount = delay * this.jitter;
        delay += (Math.random() - 0.5) * 2 * jitterAmount;
        
        return Math.min(delay, this.maxDelay);
    }

    sleep(ms) {
        return new Promise(resolve => setTimeout(resolve, ms));
    }
}
```

## Rate Limiting Best Practices

### **Client Best Practices**

#### **Rate Limit Optimization**
- **Batch Requests**: Combine multiple requests into batches
- **Caching**: Cache responses to reduce API calls
- **Pagination**: Use pagination for large datasets
- **Polling**: Use appropriate polling intervals
- **Webhooks**: Use webhooks instead of polling when possible

#### **Error Handling**
- **Respect Headers**: Always check rate limit headers
- **Implement Backoff**: Use exponential backoff for retries
- **Monitor Usage**: Track API usage and limits
- **Handle Gracefully**: Handle rate limits gracefully
- **User Feedback**: Inform users about rate limits

### **Server Best Practices**

#### **Rate Limit Configuration**
- **Appropriate Limits**: Set reasonable rate limits
- **Tiered Access**: Provide different limits for different tiers
- **Endpoint-Specific**: Set limits per endpoint
- **User-Specific**: Set limits per user/tenant
- **Dynamic Limits**: Adjust limits based on usage patterns

#### **Monitoring and Alerting**
- **Usage Tracking**: Track rate limit usage
- **Alerting**: Alert on high usage or abuse
- **Analytics**: Analyze usage patterns
- **Optimization**: Optimize based on usage data
- **Reporting**: Provide usage reports to users

## Rate Limiting Monitoring

### **Rate Limit Metrics**

#### **Key Metrics**
```yaml
rate_limit_metrics:
  requests_per_second:
    description: "Current requests per second"
    threshold: "Based on tier limits"
    alert: "High request rate"
  
  rate_limit_hits:
    description: "Number of rate limit hits"
    threshold: "> 10% of requests"
    alert: "High rate limit hit rate"
  
  average_response_time:
    description: "Average response time"
    threshold: "< 2 seconds"
    alert: "Slow response times"
  
  error_rate:
    description: "Rate of 429 responses"
    threshold: "> 5%"
    alert: "High rate limit error rate"
```

#### **Rate Limit Dashboard**
```yaml
dashboard_metrics:
  current_usage:
    - requests_per_hour
    - requests_per_day
    - concurrent_requests
    - burst_usage
  
  rate_limit_status:
    - limit_utilization
    - remaining_requests
    - reset_times
    - policy_distribution
  
  error_analysis:
    - rate_limit_errors
    - error_patterns
    - user_behavior
    - abuse_detection
```

### **Rate Limit Alerting**

#### **Alert Configuration**
```yaml
rate_limit_alerts:
  high_usage:
    conditions: ["Usage > 80% of limit", "Sustained high usage"]
    actions: ["Send email alert", "Create ticket", "Notify user"]
  
  rate_limit_abuse:
    conditions: ["Consistent rate limit hits", "Suspicious patterns"]
    actions: ["Block IP", "Suspend API key", "Investigate user"]
  
  system_overload:
    conditions: ["High error rate", "Slow response times"]
    actions: ["Scale infrastructure", "Adjust limits", "Emergency response"]
```

## Rate Limiting Testing

### **Rate Limit Testing**

#### **Load Testing**
```javascript
const loadtest = require('loadtest');

const options = {
    url: 'https://api.virtualqueue.com/v1/queues',
    maxRequests: 1000,
    concurrency: 10,
    requestsPerSecond: 100,
    headers: {
        'Authorization': 'Bearer your-token',
        'Content-Type': 'application/json'
    }
};

loadtest.loadTest(options, (error, result) => {
    if (error) {
        console.error('Load test error:', error);
        return;
    }
    
    console.log('Load test results:', {
        totalRequests: result.totalRequests,
        totalErrors: result.totalErrors,
        totalTimeSeconds: result.totalTimeSeconds,
        requestsPerSecond: result.rps,
        meanLatencyMs: result.meanLatencyMs,
        maxLatencyMs: result.maxLatencyMs,
        minLatencyMs: result.minLatencyMs,
        percentiles: result.percentiles
    });
});
```

#### **Rate Limit Testing**
```javascript
class RateLimitTester {
    constructor(apiClient) {
        this.apiClient = apiClient;
        this.results = [];
    }

    async testRateLimit(endpoint, limit) {
        console.log(`Testing rate limit for ${endpoint} (limit: ${limit})`);
        
        const requests = [];
        for (let i = 0; i < limit + 10; i++) {
            requests.push(this.makeRequest(endpoint, i));
        }
        
        const responses = await Promise.all(requests);
        
        const successful = responses.filter(r => r.status === 200).length;
        const rateLimited = responses.filter(r => r.status === 429).length;
        
        console.log(`Results: ${successful} successful, ${rateLimited} rate limited`);
        
        return {
            successful,
            rateLimited,
            total: responses.length,
            limit
        };
    }

    async makeRequest(endpoint, index) {
        try {
            const response = await this.apiClient.get(endpoint);
            return {
                status: response.status,
                index,
                headers: response.headers
            };
        } catch (error) {
            return {
                status: error.response?.status || 500,
                index,
                error: error.message
            };
        }
    }
}
```

## Approval and Sign-off

### **Rate Limiting Approval**
- **API Team**: [Name] - [Date]
- **Infrastructure Team**: [Name] - [Date]
- **Security Team**: [Name] - [Date]
- **Management**: [Name] - [Date]

### **Document Control**
- **Version**: 1.0
- **Last Updated**: January 15, 2024
- **Next Review**: February 15, 2024
- **Distribution**: API Team, Infrastructure Team, Security Team

---

**Document Status**: Draft  
**Next Phase**: Webhook Documentation  
**Dependencies**: Rate limiting implementation, testing validation, monitoring setup
