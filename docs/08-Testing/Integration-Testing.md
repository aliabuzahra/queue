# Integration Testing - Virtual Queue Management System

**Document Version:** 1.0  
**Date:** January 15, 2024  
**Author:** QA Lead  
**Status:** Draft  
**Phase:** 8 - Testing  
**Priority:** 🔴 Critical  

---

## Executive Summary

This document provides comprehensive integration testing guidelines for the Virtual Queue Management System. It covers API testing, database integration testing, external service testing, and integration test automation to ensure seamless component interaction and system reliability.

## Integration Testing Overview

### **Integration Testing Objectives**

#### **Primary Objectives**
- **Component Interaction**: Verify components work together correctly
- **API Validation**: Ensure API endpoints function as expected
- **Data Flow**: Validate data flow between components
- **External Integration**: Test integration with external services
- **System Behavior**: Verify end-to-end system behavior

#### **Integration Testing Benefits**
- **Early Detection**: Find integration issues early
- **System Validation**: Validate system behavior
- **API Reliability**: Ensure API reliability and consistency
- **Data Integrity**: Verify data integrity across components
- **Performance Validation**: Validate performance under integration

### **Integration Testing Types**

#### **Testing Categories**
- **API Integration Testing**: REST API endpoint testing
- **Database Integration Testing**: Database operations and data persistence
- **External Service Integration**: Third-party service integration
- **Component Integration**: Internal component interaction
- **End-to-End Integration**: Complete system workflow testing

#### **Integration Levels**
```yaml
integration_levels:
  component_integration:
    scope: "Individual components and services"
    focus: "Component interaction and data flow"
    tools: ["Unit test frameworks", "Mocking frameworks"]
  
  api_integration:
    scope: "API endpoints and services"
    focus: "API functionality and data exchange"
    tools: ["Postman", "RestAssured", "Newman"]
  
  database_integration:
    scope: "Database operations and persistence"
    focus: "Data integrity and database functionality"
    tools: ["Entity Framework", "TestContainers", "In-memory databases"]
  
  external_service_integration:
    scope: "Third-party service integration"
    focus: "External service communication"
    tools: ["WireMock", "Mock servers", "Service virtualization"]
  
  system_integration:
    scope: "Complete system workflow"
    focus: "End-to-end system behavior"
    tools: ["Selenium", "Playwright", "Cypress"]
```

## API Integration Testing

### **API Testing Framework**

#### **Postman Collection Structure**
```json
{
  "info": {
    "name": "Virtual Queue API Tests",
    "description": "Integration tests for Virtual Queue Management System API",
    "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
  },
  "item": [
    {
      "name": "Authentication",
      "item": [
        {
          "name": "Login",
          "request": {
            "method": "POST",
            "header": [
              {
                "key": "Content-Type",
                "value": "application/json"
              }
            ],
            "body": {
              "mode": "raw",
              "raw": "{\n  \"email\": \"{{testUserEmail}}\",\n  \"password\": \"{{testUserPassword}}\",\n  \"tenantId\": \"{{testTenantId}}\"\n}"
            },
            "url": {
              "raw": "{{baseUrl}}/auth/login",
              "host": ["{{baseUrl}}"],
              "path": ["auth", "login"]
            }
          },
          "event": [
            {
              "listen": "test",
              "script": {
                "exec": [
                  "pm.test(\"Status code is 200\", function () {",
                  "    pm.response.to.have.status(200);",
                  "});",
                  "",
                  "pm.test(\"Response has access token\", function () {",
                  "    var jsonData = pm.response.json();",
                  "    pm.expect(jsonData.data.accessToken).to.exist;",
                  "    pm.environment.set(\"accessToken\", jsonData.data.accessToken);",
                  "});"
                ]
              }
            }
          ]
        }
      ]
    },
    {
      "name": "Queue Management",
      "item": [
        {
          "name": "Create Queue",
          "request": {
            "method": "POST",
            "header": [
              {
                "key": "Authorization",
                "value": "Bearer {{accessToken}}"
              },
              {
                "key": "Content-Type",
                "value": "application/json"
              }
            ],
            "body": {
              "mode": "raw",
              "raw": "{\n  \"tenantId\": \"{{testTenantId}}\",\n  \"name\": \"{{queueName}}\",\n  \"description\": \"Test queue description\",\n  \"maxConcurrentUsers\": 100,\n  \"releaseRatePerMinute\": 10\n}"
            },
            "url": {
              "raw": "{{baseUrl}}/queues",
              "host": ["{{baseUrl}}"],
              "path": ["queues"]
            }
          },
          "event": [
            {
              "listen": "test",
              "script": {
                "exec": [
                  "pm.test(\"Status code is 201\", function () {",
                  "    pm.response.to.have.status(201);",
                  "});",
                  "",
                  "pm.test(\"Queue created successfully\", function () {",
                  "    var jsonData = pm.response.json();",
                  "    pm.expect(jsonData.data.id).to.exist;",
                  "    pm.environment.set(\"queueId\", jsonData.data.id);",
                  "});"
                ]
              }
            }
          ]
        }
      ]
    }
  ]
}
```

#### **API Test Automation**
```csharp
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;

namespace VirtualQueue.Tests.Integration
{
    public class QueueApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public QueueApiIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task CreateQueue_ValidRequest_ReturnsCreatedQueue()
        {
            // Arrange
            var createQueueRequest = new CreateQueueRequest
            {
                TenantId = Guid.NewGuid(),
                Name = "Integration Test Queue",
                Description = "Test Description",
                MaxConcurrentUsers = 100,
                ReleaseRatePerMinute = 10
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/queues", createQueueRequest);

            // Assert
            response.EnsureSuccessStatusCode();
            var queue = await response.Content.ReadFromJsonAsync<QueueDto>();
            
            queue.Should().NotBeNull();
            queue.Name.Should().Be("Integration Test Queue");
            queue.MaxConcurrentUsers.Should().Be(100);
        }

        [Fact]
        public async Task GetQueues_ValidRequest_ReturnsQueueList()
        {
            // Arrange
            var tenantId = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"/api/queues?tenantId={tenantId}");

            // Assert
            response.EnsureSuccessStatusCode();
            var queues = await response.Content.ReadFromJsonAsync<PagedResult<QueueDto>>();
            
            queues.Should().NotBeNull();
            queues.Items.Should().NotBeNull();
        }

        [Fact]
        public async Task JoinQueue_ValidRequest_ReturnsUserSession()
        {
            // Arrange
            var queueId = Guid.NewGuid();
            var joinRequest = new JoinQueueRequest
            {
                UserId = Guid.NewGuid(),
                UserName = "Test User",
                UserEmail = "test@example.com",
                Priority = "normal"
            };

            // Act
            var response = await _client.PostAsJsonAsync($"/api/queues/{queueId}/join", joinRequest);

            // Assert
            response.EnsureSuccessStatusCode();
            var session = await response.Content.ReadFromJsonAsync<UserSessionDto>();
            
            session.Should().NotBeNull();
            session.QueueId.Should().Be(queueId);
            session.UserId.Should().Be(joinRequest.UserId);
        }
    }
}
```

### **API Test Data Management**

#### **Test Data Setup**
```csharp
public class ApiTestDataManager
{
    private readonly HttpClient _client;
    private readonly Dictionary<string, object> _testData;

    public ApiTestDataManager(HttpClient client)
    {
        _client = client;
        _testData = new Dictionary<string, object>();
    }

    public async Task<string> GetAccessTokenAsync()
    {
        var loginRequest = new LoginRequest
        {
            Email = "test@example.com",
            Password = "TestPassword123",
            TenantId = Guid.NewGuid()
        };

        var response = await _client.PostAsJsonAsync("/auth/login", loginRequest);
        response.EnsureSuccessStatusCode();
        
        var loginResult = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return loginResult.AccessToken;
    }

    public async Task<Guid> CreateTestQueueAsync(string accessToken)
    {
        var createQueueRequest = new CreateQueueRequest
        {
            TenantId = Guid.NewGuid(),
            Name = "Test Queue",
            Description = "Test Description",
            MaxConcurrentUsers = 100,
            ReleaseRatePerMinute = 10
        };

        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _client.PostAsJsonAsync("/api/queues", createQueueRequest);
        response.EnsureSuccessStatusCode();
        
        var queue = await response.Content.ReadFromJsonAsync<QueueDto>();
        return queue.Id;
    }

    public async Task<Guid> CreateTestUserSessionAsync(string accessToken, Guid queueId)
    {
        var joinRequest = new JoinQueueRequest
        {
            UserId = Guid.NewGuid(),
            UserName = "Test User",
            UserEmail = "test@example.com",
            Priority = "normal"
        };

        var response = await _client.PostAsJsonAsync($"/api/queues/{queueId}/join", joinRequest);
        response.EnsureSuccessStatusCode();
        
        var session = await response.Content.ReadFromJsonAsync<UserSessionDto>();
        return session.Id;
    }
}
```

## Database Integration Testing

### **Database Test Setup**

#### **TestContainers Configuration**
```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Xunit;

namespace VirtualQueue.Tests.Integration
{
    public class DatabaseIntegrationTests : IAsyncLifetime
    {
        private readonly PostgreSqlContainer _postgreSqlContainer;
        private readonly VirtualQueueDbContext _context;

        public DatabaseIntegrationTests()
        {
            _postgreSqlContainer = new PostgreSqlBuilder()
                .WithImage("postgres:15")
                .WithDatabase("testdb")
                .WithUsername("testuser")
                .WithPassword("testpassword")
                .Build();

            var options = new DbContextOptionsBuilder<VirtualQueueDbContext>()
                .UseNpgsql(_postgreSqlContainer.GetConnectionString())
                .Options;

            _context = new VirtualQueueDbContext(options);
        }

        public async Task InitializeAsync()
        {
            await _postgreSqlContainer.StartAsync();
            await _context.Database.EnsureCreatedAsync();
        }

        public async Task DisposeAsync()
        {
            await _context.DisposeAsync();
            await _postgreSqlContainer.DisposeAsync();
        }

        [Fact]
        public async Task CreateQueue_SavesToDatabase()
        {
            // Arrange
            var queue = new Queue(
                Guid.NewGuid(),
                "Test Queue",
                "Test Description",
                100,
                10);

            // Act
            _context.Queues.Add(queue);
            await _context.SaveChangesAsync();

            // Assert
            var savedQueue = await _context.Queues
                .FirstOrDefaultAsync(q => q.Id == queue.Id);
            
            savedQueue.Should().NotBeNull();
            savedQueue.Name.Should().Be("Test Queue");
            savedQueue.MaxConcurrentUsers.Should().Be(100);
        }

        [Fact]
        public async Task CreateUserSession_SavesToDatabase()
        {
            // Arrange
            var queue = new Queue(
                Guid.NewGuid(),
                "Test Queue",
                "Test Description",
                100,
                10);

            var userSession = new UserSession(
                queue.Id,
                Guid.NewGuid(),
                "Test User",
                "test@example.com");

            // Act
            _context.Queues.Add(queue);
            _context.UserSessions.Add(userSession);
            await _context.SaveChangesAsync();

            // Assert
            var savedSession = await _context.UserSessions
                .FirstOrDefaultAsync(s => s.Id == userSession.Id);
            
            savedSession.Should().NotBeNull();
            savedSession.QueueId.Should().Be(queue.Id);
            savedSession.UserName.Should().Be("Test User");
        }
    }
}
```

#### **In-Memory Database Testing**
```csharp
public class InMemoryDatabaseTests
{
    private readonly VirtualQueueDbContext _context;

    public InMemoryDatabaseTests()
    {
        var options = new DbContextOptionsBuilder<VirtualQueueDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new VirtualQueueDbContext(options);
    }

    [Fact]
    public async Task GetQueuesByTenant_ReturnsCorrectQueues()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var queue1 = new Queue(tenantId, "Queue 1", "Description 1", 100, 10);
        var queue2 = new Queue(tenantId, "Queue 2", "Description 2", 150, 15);
        var queue3 = new Queue(Guid.NewGuid(), "Queue 3", "Description 3", 200, 20);

        _context.Queues.AddRange(queue1, queue2, queue3);
        await _context.SaveChangesAsync();

        // Act
        var queues = await _context.Queues
            .Where(q => q.TenantId == tenantId)
            .ToListAsync();

        // Assert
        queues.Should().HaveCount(2);
        queues.Should().Contain(q => q.Name == "Queue 1");
        queues.Should().Contain(q => q.Name == "Queue 2");
        queues.Should().NotContain(q => q.Name == "Queue 3");
    }

    [Fact]
    public async Task UpdateQueue_UpdatesCorrectly()
    {
        // Arrange
        var queue = new Queue(
            Guid.NewGuid(),
            "Original Name",
            "Original Description",
            100,
            10);

        _context.Queues.Add(queue);
        await _context.SaveChangesAsync();

        // Act
        queue.UpdateName("Updated Name");
        queue.UpdateDescription("Updated Description");
        await _context.SaveChangesAsync();

        // Assert
        var updatedQueue = await _context.Queues
            .FirstOrDefaultAsync(q => q.Id == queue.Id);
        
        updatedQueue.Name.Should().Be("Updated Name");
        updatedQueue.Description.Should().Be("Updated Description");
    }
}
```

## External Service Integration Testing

### **Service Mocking**

#### **WireMock Configuration**
```csharp
using WireMock.Server;
using WireMock.Settings;
using Xunit;

namespace VirtualQueue.Tests.Integration
{
    public class ExternalServiceIntegrationTests : IAsyncLifetime
    {
        private WireMockServer _mockServer;
        private readonly HttpClient _client;

        public ExternalServiceIntegrationTests()
        {
            _mockServer = WireMockServer.Start(new WireMockServerSettings
            {
                Port = 5000,
                StartAdminInterface = true
            });

            _client = new HttpClient();
        }

        public async Task InitializeAsync()
        {
            // Setup mock responses
            _mockServer
                .Given(WireMock.RequestBuilders.Request.Create()
                    .WithPath("/api/notifications")
                    .UsingPost())
                .RespondWith(WireMock.ResponseBuilders.Response.Create()
                    .WithStatusCode(200)
                    .WithBody("{\"success\": true}"));

            _mockServer
                .Given(WireMock.RequestBuilders.Request.Create()
                    .WithPath("/api/email")
                    .UsingPost())
                .RespondWith(WireMock.ResponseBuilders.Response.Create()
                    .WithStatusCode(200)
                    .WithBody("{\"messageId\": \"12345\"}"));
        }

        public async Task DisposeAsync()
        {
            _mockServer?.Dispose();
            _client?.Dispose();
        }

        [Fact]
        public async Task SendNotification_CallsExternalService()
        {
            // Arrange
            var notificationRequest = new NotificationRequest
            {
                UserId = Guid.NewGuid(),
                Message = "Test notification",
                Type = "info"
            };

            // Act
            var response = await _client.PostAsJsonAsync(
                "http://localhost:5000/api/notifications", 
                notificationRequest);

            // Assert
            response.EnsureSuccessStatusCode();
            
            var requests = _mockServer.LogEntries;
            requests.Should().HaveCount(1);
            
            var request = requests.First();
            request.RequestMessage.Path.Should().Be("/api/notifications");
        }
    }
}
```

#### **Service Virtualization**
```csharp
public class ServiceVirtualizationTests
{
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly UserSessionService _userSessionService;

    public ServiceVirtualizationTests()
    {
        _mockNotificationService = new Mock<INotificationService>();
        _mockEmailService = new Mock<IEmailService>();
        _userSessionService = new UserSessionService(
            _mockNotificationService.Object,
            _mockEmailService.Object);
    }

    [Fact]
    public async Task ProcessUserSession_CallsExternalServices()
    {
        // Arrange
        var session = new UserSession(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Test User",
            "test@example.com");

        _mockNotificationService.Setup(s => s.SendNotificationAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        
        _mockEmailService.Setup(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        await _userSessionService.ProcessUserSession(session);

        // Assert
        _mockNotificationService.Verify(s => s.SendNotificationAsync(
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Once);
        
        _mockEmailService.Verify(s => s.SendEmailAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Once);
    }
}
```

## End-to-End Integration Testing

### **E2E Test Framework**

#### **Playwright Configuration**
```csharp
using Microsoft.Playwright;
using Xunit;

namespace VirtualQueue.Tests.Integration
{
    public class E2EIntegrationTests : IAsyncLifetime
    {
        private IPlaywright _playwright;
        private IBrowser _browser;
        private IPage _page;

        public async Task InitializeAsync()
        {
            _playwright = await Playwright.CreateAsync();
            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true
            });
            _page = await _browser.NewPageAsync();
        }

        public async Task DisposeAsync()
        {
            await _page.CloseAsync();
            await _browser.CloseAsync();
            _playwright.Dispose();
        }

        [Fact]
        public async Task CompleteQueueWorkflow_UserJourney_Success()
        {
            // Arrange
            await _page.GotoAsync("https://localhost:7001");

            // Act & Assert - Login
            await _page.FillAsync("[data-testid='email-input']", "test@example.com");
            await _page.FillAsync("[data-testid='password-input']", "TestPassword123");
            await _page.ClickAsync("[data-testid='login-button']");
            
            await _page.WaitForSelectorAsync("[data-testid='dashboard']");
            var dashboardVisible = await _page.IsVisibleAsync("[data-testid='dashboard']");
            Assert.True(dashboardVisible);

            // Act & Assert - Create Queue
            await _page.ClickAsync("[data-testid='create-queue-button']");
            await _page.FillAsync("[data-testid='queue-name-input']", "E2E Test Queue");
            await _page.FillAsync("[data-testid='queue-description-input']", "E2E Test Description");
            await _page.FillAsync("[data-testid='max-users-input']", "100");
            await _page.ClickAsync("[data-testid='create-button']");
            
            await _page.WaitForSelectorAsync("[data-testid='queue-created']");
            var queueCreated = await _page.IsVisibleAsync("[data-testid='queue-created']");
            Assert.True(queueCreated);

            // Act & Assert - Join Queue
            await _page.ClickAsync("[data-testid='join-queue-button']");
            await _page.FillAsync("[data-testid='user-name-input']", "E2E Test User");
            await _page.FillAsync("[data-testid='user-email-input']", "e2e@example.com");
            await _page.ClickAsync("[data-testid='join-button']");
            
            await _page.WaitForSelectorAsync("[data-testid='queue-position']");
            var positionVisible = await _page.IsVisibleAsync("[data-testid='queue-position']");
            Assert.True(positionVisible);
        }
    }
}
```

#### **Selenium WebDriver**
```csharp
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Xunit;

namespace VirtualQueue.Tests.Integration
{
    public class SeleniumIntegrationTests : IAsyncLifetime
    {
        private IWebDriver _driver;
        private WebDriverWait _wait;

        public async Task InitializeAsync()
        {
            var options = new ChromeOptions();
            options.AddArgument("--headless");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            
            _driver = new ChromeDriver(options);
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
        }

        public async Task DisposeAsync()
        {
            _driver?.Quit();
        }

        [Fact]
        public async Task Login_ValidCredentials_Success()
        {
            // Arrange
            _driver.Navigate().GoToUrl("https://localhost:7001");

            // Act
            var emailInput = _wait.Until(d => d.FindElement(By.Id("email-input")));
            emailInput.SendKeys("test@example.com");

            var passwordInput = _driver.FindElement(By.Id("password-input"));
            passwordInput.SendKeys("TestPassword123");

            var loginButton = _driver.FindElement(By.Id("login-button"));
            loginButton.Click();

            // Assert
            var dashboard = _wait.Until(d => d.FindElement(By.Id("dashboard")));
            Assert.True(dashboard.Displayed);
        }

        [Fact]
        public async Task CreateQueue_ValidData_Success()
        {
            // Arrange
            await Login_ValidCredentials_Success();

            // Act
            var createQueueButton = _wait.Until(d => d.FindElement(By.Id("create-queue-button")));
            createQueueButton.Click();

            var queueNameInput = _wait.Until(d => d.FindElement(By.Id("queue-name-input")));
            queueNameInput.SendKeys("Selenium Test Queue");

            var queueDescriptionInput = _driver.FindElement(By.Id("queue-description-input"));
            queueDescriptionInput.SendKeys("Selenium Test Description");

            var maxUsersInput = _driver.FindElement(By.Id("max-users-input"));
            maxUsersInput.SendKeys("100");

            var createButton = _driver.FindElement(By.Id("create-button"));
            createButton.Click();

            // Assert
            var successMessage = _wait.Until(d => d.FindElement(By.Id("queue-created")));
            Assert.True(successMessage.Displayed);
        }
    }
}
```

## Integration Test Automation

### **CI/CD Integration**

#### **Azure DevOps Pipeline**
```yaml
# azure-pipelines-integration.yml
trigger:
- main
- develop

pool:
  vmImage: 'ubuntu-latest'

variables:
  buildConfiguration: 'Release'

steps:
- task: DotNetCoreCLI@2
  displayName: 'Restore packages'
  inputs:
    command: 'restore'
    projects: '**/*.csproj'

- task: DotNetCoreCLI@2
  displayName: 'Build solution'
  inputs:
    command: 'build'
    projects: '**/*.csproj'
    arguments: '--configuration $(buildConfiguration)'

- task: DotNetCoreCLI@2
  displayName: 'Run integration tests'
  inputs:
    command: 'test'
    projects: '**/*IntegrationTests.csproj'
    arguments: '--configuration $(buildConfiguration) --logger trx --results-directory $(Agent.TempDirectory)'

- task: PublishTestResults@2
  displayName: 'Publish integration test results'
  inputs:
    testResultsFormat: 'VSTest'
    testResultsFiles: '**/*.trx'
    searchFolder: '$(Agent.TempDirectory)'

- task: PublishCodeCoverageResults@1
  displayName: 'Publish integration test coverage'
  inputs:
    codeCoverageTool: 'Cobertura'
    summaryFileLocation: '$(Agent.TempDirectory)/**/coverage.cobertura.xml'
```

#### **GitHub Actions**
```yaml
# .github/workflows/integration-tests.yml
name: Integration Tests

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  integration-tests:
    runs-on: ubuntu-latest
    
    services:
      postgres:
        image: postgres:15
        env:
          POSTGRES_PASSWORD: postgres
          POSTGRES_DB: testdb
        options: >-
          --health-cmd pg_isready
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5
      
      redis:
        image: redis:7
        options: >-
          --health-cmd "redis-cli ping"
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 8.0.x
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --no-restore
    
    - name: Run integration tests
      run: dotnet test --no-build --verbosity normal --collect:"XPlat Code Coverage"
      env:
        ConnectionStrings__DefaultConnection: "Host=localhost;Database=testdb;Username=postgres;Password=postgres"
        Redis__ConnectionString: "localhost:6379"
```

### **Test Execution**

#### **Test Execution Scripts**
```bash
#!/bin/bash
# run-integration-tests.sh

echo "Starting integration tests..."

# Start test environment
docker-compose -f docker-compose.test.yml up -d

# Wait for services to be ready
echo "Waiting for services to be ready..."
sleep 30

# Run integration tests
echo "Running integration tests..."
dotnet test --filter "Category=Integration" --logger "console;verbosity=normal"

# Check exit code
if [ $? -eq 0 ]; then
    echo "All integration tests passed!"
else
    echo "Some integration tests failed!"
    exit 1
fi

# Cleanup
echo "Cleaning up test environment..."
docker-compose -f docker-compose.test.yml down

echo "Integration tests completed!"
```

#### **Test Environment Management**
```yaml
# docker-compose.test.yml
version: '3.8'

services:
  postgres-test:
    image: postgres:15
    environment:
      - POSTGRES_DB=testdb
      - POSTGRES_USER=testuser
      - POSTGRES_PASSWORD=testpassword
    ports:
      - "5433:5432"
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U testuser -d testdb"]
      interval: 10s
      timeout: 5s
      retries: 5

  redis-test:
    image: redis:7
    ports:
      - "6380:6379"
    healthcheck:
      test: ["CMD", "redis-cli", "ping"]
      interval: 10s
      timeout: 5s
      retries: 5

  api-test:
    build:
      context: .
      dockerfile: Dockerfile.test
    environment:
      - ASPNETCORE_ENVIRONMENT=Testing
      - ConnectionStrings__DefaultConnection=Host=postgres-test;Database=testdb;Username=testuser;Password=testpassword
      - Redis__ConnectionString=redis-test:6379
    depends_on:
      postgres-test:
        condition: service_healthy
      redis-test:
        condition: service_healthy
    ports:
      - "5001:80"
```

## Integration Test Best Practices

### **Test Design Principles**

#### **Test Organization**
- **Test Isolation**: Each test should be independent
- **Test Data**: Use consistent test data across tests
- **Test Cleanup**: Clean up test data after execution
- **Test Parallelization**: Run tests in parallel where possible
- **Test Reliability**: Ensure tests are reliable and repeatable

#### **Test Maintenance**
- **Test Updates**: Update tests when APIs change
- **Test Refactoring**: Refactor tests for maintainability
- **Test Documentation**: Document test purposes and scenarios
- **Test Monitoring**: Monitor test execution and results
- **Test Optimization**: Optimize tests for performance

### **Integration Test Metrics**

#### **Key Metrics**
- **Test Coverage**: Percentage of integration points tested
- **Test Pass Rate**: Percentage of tests passing
- **Test Execution Time**: Time to execute test suite
- **Test Reliability**: Consistency of test results
- **Test Maintenance**: Effort required to maintain tests

#### **Metrics Tracking**
```csharp
public class IntegrationTestMetrics
{
    public int TotalTests { get; set; }
    public int PassedTests { get; set; }
    public int FailedTests { get; set; }
    public TimeSpan ExecutionTime { get; set; }
    public double PassRate => (double)PassedTests / TotalTests * 100;
    public double FailureRate => (double)FailedTests / TotalTests * 100;
}
```

## Approval and Sign-off

### **Integration Testing Approval**
- **QA Lead**: [Name] - [Date]
- **Development Lead**: [Name] - [Date]
- **Technical Lead**: [Name] - [Date]
- **Management**: [Name] - [Date]

### **Document Control**
- **Version**: 1.0
- **Last Updated**: January 15, 2024
- **Next Review**: February 15, 2024
- **Distribution**: QA Team, Development Team, Technical Team

---

**Document Status**: Draft  
**Next Phase**: Performance Testing  
**Dependencies**: Integration testing implementation, test environment setup, CI/CD integration
