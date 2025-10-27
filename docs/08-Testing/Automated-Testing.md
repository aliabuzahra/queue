# Automated Testing - Virtual Queue Management System

**Document Version:** 1.0  
**Date:** January 15, 2024  
**Author:** QA Engineer  
**Status:** Draft  
**Phase:** 08 - Testing  
**Priority:** 🟡 Medium  

---

## Automated Testing Overview

This document outlines the comprehensive automated testing strategy for the Virtual Queue Management System. It covers test automation frameworks, continuous integration testing, automated test execution, test reporting, and maintenance procedures to ensure consistent and reliable testing.

## Automated Testing Strategy

### **Automation Principles**
- **Test Early**: Automate tests as early as possible in the development cycle
- **Test Often**: Run automated tests frequently and continuously
- **Test Reliable**: Ensure automated tests are stable and reliable
- **Test Maintainable**: Keep automated tests maintainable and readable
- **Test Fast**: Optimize test execution speed
- **Test Comprehensive**: Cover all critical functionality

### **Automation Levels**

| Level | Description | Tools | Frequency | Coverage |
|-------|-------------|-------|-----------|----------|
| **Unit Tests** | Individual component testing | xUnit, NUnit | Every commit | 90%+ |
| **Integration Tests** | Component interaction testing | TestHost, HttpClient | Every build | 80%+ |
| **API Tests** | API endpoint testing | RestSharp, Postman | Every deployment | 95%+ |
| **UI Tests** | User interface testing | Selenium, Playwright | Daily | 70%+ |
| **Performance Tests** | Performance and load testing | JMeter, K6 | Weekly | 60%+ |
| **Security Tests** | Security vulnerability testing | OWASP ZAP, Snyk | Daily | 85%+ |

## Test Automation Framework

### **Unit Test Automation**

#### **xUnit Test Framework Setup**
```csharp
// TestBase.cs
public abstract class TestBase : IDisposable
{
    protected readonly IServiceProvider ServiceProvider;
    protected readonly VirtualQueueDbContext Context;
    protected readonly HttpClient HttpClient;

    protected TestBase()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        ServiceProvider = services.BuildServiceProvider();
        
        Context = ServiceProvider.GetRequiredService<VirtualQueueDbContext>();
        HttpClient = ServiceProvider.GetRequiredService<HttpClient>();
        
        SetupTestData();
    }

    protected virtual void ConfigureServices(IServiceCollection services)
    {
        // Configure test services
        services.AddDbContext<VirtualQueueDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        
        services.AddScoped<IQueueService, QueueService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ISessionService, SessionService>();
        
        // Add test HTTP client
        services.AddHttpClient();
    }

    protected virtual void SetupTestData()
    {
        // Setup test data
        var testTenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = "Test Tenant",
            Slug = "test-tenant"
        };
        
        Context.Tenants.Add(testTenant);
        Context.SaveChanges();
    }

    public void Dispose()
    {
        Context?.Dispose();
        ServiceProvider?.Dispose();
    }
}

// QueueServiceTests.cs
public class QueueServiceTests : TestBase
{
    private readonly IQueueService _queueService;

    public QueueServiceTests()
    {
        _queueService = ServiceProvider.GetRequiredService<IQueueService>();
    }

    [Fact]
    public async Task CreateQueue_ValidData_ReturnsQueue()
    {
        // Arrange
        var createQueueRequest = new CreateQueueRequest
        {
            Name = "Test Queue",
            Description = "Test Description",
            Capacity = 100
        };

        // Act
        var result = await _queueService.CreateAsync(createQueueRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createQueueRequest.Name, result.Name);
        Assert.Equal(createQueueRequest.Capacity, result.Capacity);
    }

    [Theory]
    [InlineData("", 100)]
    [InlineData("Test Queue", 0)]
    [InlineData("Test Queue", -1)]
    public async Task CreateQueue_InvalidData_ThrowsValidationException(string name, int capacity)
    {
        // Arrange
        var createQueueRequest = new CreateQueueRequest
        {
            Name = name,
            Capacity = capacity
        };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => 
            _queueService.CreateAsync(createQueueRequest));
    }
}
```

### **Integration Test Automation**

#### **API Integration Tests**
```csharp
// ApiIntegrationTests.cs
public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetQueues_ReturnsQueues()
    {
        // Act
        var response = await _client.GetAsync("/api/queues");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var queues = JsonSerializer.Deserialize<List<QueueDto>>(content);
        Assert.NotNull(queues);
    }

    [Fact]
    public async Task CreateQueue_ValidData_ReturnsCreatedQueue()
    {
        // Arrange
        var createQueueRequest = new CreateQueueRequest
        {
            Name = "Integration Test Queue",
            Description = "Integration Test Description",
            Capacity = 100
        };

        var json = JsonSerializer.Serialize(createQueueRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/queues", content);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var queue = JsonSerializer.Deserialize<QueueDto>(responseContent);
        Assert.NotNull(queue);
        Assert.Equal(createQueueRequest.Name, queue.Name);
    }

    [Fact]
    public async Task JoinQueue_ValidData_ReturnsSession()
    {
        // Arrange
        var queue = await CreateTestQueue();
        var joinRequest = new JoinQueueRequest
        {
            Notes = "Integration test session"
        };

        var json = JsonSerializer.Serialize(joinRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync($"/api/queues/{queue.Id}/join", content);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var session = JsonSerializer.Deserialize<UserSessionDto>(responseContent);
        Assert.NotNull(session);
        Assert.Equal(queue.Id, session.QueueId);
    }

    private async Task<QueueDto> CreateTestQueue()
    {
        var createQueueRequest = new CreateQueueRequest
        {
            Name = "Test Queue for Integration",
            Description = "Test Description",
            Capacity = 100
        };

        var json = JsonSerializer.Serialize(createQueueRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/queues", content);
        response.EnsureSuccessStatusCode();
        
        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<QueueDto>(responseContent);
    }
}
```

### **UI Test Automation**

#### **Selenium WebDriver Tests**
```csharp
// UiTests.cs
public class UiTests : IDisposable
{
    private readonly IWebDriver _driver;
    private readonly WebDriverWait _wait;

    public UiTests()
    {
        var options = new ChromeOptions();
        options.AddArgument("--headless");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");
        
        _driver = new ChromeDriver(options);
        _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
    }

    [Fact]
    public void Login_ValidCredentials_RedirectsToDashboard()
    {
        // Arrange
        _driver.Navigate().GoToUrl("http://localhost:3000/login");

        // Act
        var emailInput = _wait.Until(ExpectedConditions.ElementIsVisible(By.Id("email")));
        emailInput.SendKeys("test@example.com");

        var passwordInput = _driver.FindElement(By.Id("password"));
        passwordInput.SendKeys("password123");

        var loginButton = _driver.FindElement(By.Id("login-button"));
        loginButton.Click();

        // Assert
        _wait.Until(ExpectedConditions.UrlContains("/dashboard"));
        Assert.Contains("/dashboard", _driver.Url);
    }

    [Fact]
    public void CreateQueue_ValidData_CreatesQueue()
    {
        // Arrange
        Login();
        _driver.Navigate().GoToUrl("http://localhost:3000/queues/create");

        // Act
        var nameInput = _wait.Until(ExpectedConditions.ElementIsVisible(By.Id("queue-name")));
        nameInput.SendKeys("UI Test Queue");

        var descriptionInput = _driver.FindElement(By.Id("queue-description"));
        descriptionInput.SendKeys("UI Test Description");

        var capacityInput = _driver.FindElement(By.Id("queue-capacity"));
        capacityInput.SendKeys("100");

        var createButton = _driver.FindElement(By.Id("create-queue-button"));
        createButton.Click();

        // Assert
        _wait.Until(ExpectedConditions.ElementIsVisible(By.ClassName("success-message")));
        var successMessage = _driver.FindElement(By.ClassName("success-message"));
        Assert.Contains("Queue created successfully", successMessage.Text);
    }

    private void Login()
    {
        _driver.Navigate().GoToUrl("http://localhost:3000/login");
        
        var emailInput = _wait.Until(ExpectedConditions.ElementIsVisible(By.Id("email")));
        emailInput.SendKeys("test@example.com");

        var passwordInput = _driver.FindElement(By.Id("password"));
        passwordInput.SendKeys("password123");

        var loginButton = _driver.FindElement(By.Id("login-button"));
        loginButton.Click();

        _wait.Until(ExpectedConditions.UrlContains("/dashboard"));
    }

    public void Dispose()
    {
        _driver?.Quit();
        _driver?.Dispose();
    }
}
```

## Continuous Integration Testing

### **CI/CD Pipeline Integration**

#### **GitHub Actions Workflow**
```yaml
# .github/workflows/automated-tests.yml
name: Automated Tests

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  unit-tests:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --no-restore
    
    - name: Run unit tests
      run: dotnet test --no-build --verbosity normal --collect:"XPlat Code Coverage"
    
    - name: Upload coverage reports
      uses: codecov/codecov-action@v3
      with:
        file: ./coverage.xml

  integration-tests:
    runs-on: ubuntu-latest
    services:
      postgres:
        image: postgres:15
        env:
          POSTGRES_PASSWORD: postgres
          POSTGRES_DB: VirtualQueueTest
        options: >-
          --health-cmd pg_isready
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5
      
      redis:
        image: redis:7-alpine
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
        dotnet-version: '8.0.x'
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --no-restore
    
    - name: Run integration tests
      run: dotnet test --no-build --verbosity normal --filter "Category=Integration"
      env:
        ConnectionStrings__DefaultConnection: "Host=localhost;Database=VirtualQueueTest;Username=postgres;Password=postgres"
        Redis__ConnectionString: "localhost:6379"

  api-tests:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --no-restore
    
    - name: Start application
      run: dotnet run --project src/VirtualQueue.Api --no-build &
    
    - name: Wait for application
      run: sleep 30
    
    - name: Run API tests
      run: dotnet test --no-build --verbosity normal --filter "Category=API"

  ui-tests:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    
    - name: Install Chrome
      run: |
        wget -q -O - https://dl.google.com/linux/linux_signing_key.pub | sudo apt-key add -
        sudo sh -c 'echo "deb [arch=amd64] http://dl.google.com/linux/chrome/deb/ stable main" >> /etc/apt/sources.list.d/google-chrome.list'
        sudo apt-get update
        sudo apt-get install -y google-chrome-stable
    
    - name: Install ChromeDriver
      run: |
        CHROME_VERSION=$(google-chrome --version | cut -d' ' -f3 | cut -d'.' -f1)
        CHROMEDRIVER_VERSION=$(curl -s "https://chromedriver.storage.googleapis.com/LATEST_RELEASE_${CHROME_VERSION}")
        wget -O /tmp/chromedriver.zip "https://chromedriver.storage.googleapis.com/${CHROMEDRIVER_VERSION}/chromedriver_linux64.zip"
        sudo unzip /tmp/chromedriver.zip -d /usr/local/bin/
        sudo chmod +x /usr/local/bin/chromedriver
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --no-restore
    
    - name: Start application
      run: dotnet run --project src/VirtualQueue.Api --no-build &
    
    - name: Wait for application
      run: sleep 30
    
    - name: Run UI tests
      run: dotnet test --no-build --verbosity normal --filter "Category=UI"

  performance-tests:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --no-restore
    
    - name: Start application
      run: dotnet run --project src/VirtualQueue.Api --no-build &
    
    - name: Wait for application
      run: sleep 30
    
    - name: Run performance tests
      run: dotnet test --no-build --verbosity normal --filter "Category=Performance"

  security-tests:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Run security tests
      run: dotnet test --no-build --verbosity normal --filter "Category=Security"
    
    - name: Run dependency check
      run: dotnet list package --vulnerable
```

### **Test Execution Automation**

#### **Automated Test Runner**
```bash
#!/bin/bash
# automated-test-runner.sh

echo "Automated Test Runner"
echo "===================="

TEST_TYPE=$1
ENVIRONMENT=$2

if [ -z "$TEST_TYPE" ]; then
    echo "Usage: $0 <test_type> [environment]"
    echo "Test types: unit, integration, api, ui, performance, security, all"
    exit 1
fi

ENVIRONMENT=${ENVIRONMENT:-"testing"}

# Set environment variables
export ASPNETCORE_ENVIRONMENT=$ENVIRONMENT
export ConnectionStrings__DefaultConnection="Host=localhost;Database=VirtualQueueTest;Username=postgres;Password=postgres"
export Redis__ConnectionString="localhost:6379"

# Function to run unit tests
run_unit_tests() {
    echo "Running unit tests..."
    dotnet test --filter "Category=Unit" --logger "console;verbosity=normal" --collect:"XPlat Code Coverage"
    return $?
}

# Function to run integration tests
run_integration_tests() {
    echo "Running integration tests..."
    dotnet test --filter "Category=Integration" --logger "console;verbosity=normal"
    return $?
}

# Function to run API tests
run_api_tests() {
    echo "Running API tests..."
    # Start application
    dotnet run --project src/VirtualQueue.Api --no-build &
    APP_PID=$!
    
    # Wait for application to start
    sleep 30
    
    # Run API tests
    dotnet test --filter "Category=API" --logger "console;verbosity=normal"
    TEST_RESULT=$?
    
    # Stop application
    kill $APP_PID
    
    return $TEST_RESULT
}

# Function to run UI tests
run_ui_tests() {
    echo "Running UI tests..."
    # Start application
    dotnet run --project src/VirtualQueue.Api --no-build &
    APP_PID=$!
    
    # Wait for application to start
    sleep 30
    
    # Run UI tests
    dotnet test --filter "Category=UI" --logger "console;verbosity=normal"
    TEST_RESULT=$?
    
    # Stop application
    kill $APP_PID
    
    return $TEST_RESULT
}

# Function to run performance tests
run_performance_tests() {
    echo "Running performance tests..."
    # Start application
    dotnet run --project src/VirtualQueue.Api --no-build &
    APP_PID=$!
    
    # Wait for application to start
    sleep 30
    
    # Run performance tests
    dotnet test --filter "Category=Performance" --logger "console;verbosity=normal"
    TEST_RESULT=$?
    
    # Stop application
    kill $APP_PID
    
    return $TEST_RESULT
}

# Function to run security tests
run_security_tests() {
    echo "Running security tests..."
    dotnet test --filter "Category=Security" --logger "console;verbosity=normal"
    return $?
}

# Main execution
case $TEST_TYPE in
    "unit")
        run_unit_tests
        ;;
    "integration")
        run_integration_tests
        ;;
    "api")
        run_api_tests
        ;;
    "ui")
        run_ui_tests
        ;;
    "performance")
        run_performance_tests
        ;;
    "security")
        run_security_tests
        ;;
    "all")
        echo "Running all tests..."
        run_unit_tests && \
        run_integration_tests && \
        run_api_tests && \
        run_ui_tests && \
        run_performance_tests && \
        run_security_tests
        ;;
    *)
        echo "Invalid test type: $TEST_TYPE"
        exit 1
        ;;
esac

TEST_RESULT=$?

if [ $TEST_RESULT -eq 0 ]; then
    echo "✅ All tests passed"
else
    echo "❌ Some tests failed"
fi

exit $TEST_RESULT
```

## Test Data Management

### **Test Data Automation**

#### **Test Data Generator**
```csharp
// TestDataGenerator.cs
public class TestDataGenerator
{
    private readonly VirtualQueueDbContext _context;
    private readonly Random _random;

    public TestDataGenerator(VirtualQueueDbContext context)
    {
        _context = context;
        _random = new Random();
    }

    public async Task<Tenant> CreateTestTenantAsync()
    {
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = $"Test Tenant {_random.Next(1000, 9999)}",
            Slug = $"test-tenant-{_random.Next(1000, 9999)}",
            CreatedAt = DateTime.UtcNow
        };

        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync();
        return tenant;
    }

    public async Task<Queue> CreateTestQueueAsync(Guid tenantId)
    {
        var queue = new Queue
        {
            Id = Guid.NewGuid(),
            Name = $"Test Queue {_random.Next(1000, 9999)}",
            Description = "Test queue description",
            Capacity = _random.Next(50, 500),
            TenantId = tenantId,
            Status = QueueStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        _context.Queues.Add(queue);
        await _context.SaveChangesAsync();
        return queue;
    }

    public async Task<User> CreateTestUserAsync(Guid tenantId)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = $"testuser{_random.Next(1000, 9999)}@example.com",
            FirstName = "Test",
            LastName = "User",
            TenantId = tenantId,
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<UserSession> CreateTestSessionAsync(Guid queueId, Guid userId)
    {
        var session = new UserSession
        {
            Id = Guid.NewGuid(),
            QueueId = queueId,
            UserId = userId,
            Status = SessionStatus.Waiting,
            Position = _random.Next(1, 100),
            CreatedAt = DateTime.UtcNow
        };

        _context.UserSessions.Add(session);
        await _context.SaveChangesAsync();
        return session;
    }

    public async Task CleanupTestDataAsync()
    {
        _context.UserSessions.RemoveRange(_context.UserSessions);
        _context.Queues.RemoveRange(_context.Queues);
        _context.Users.RemoveRange(_context.Users);
        _context.Tenants.RemoveRange(_context.Tenants);
        await _context.SaveChangesAsync();
    }
}
```

### **Test Environment Management**

#### **Test Environment Setup**
```bash
#!/bin/bash
# test-environment-setup.sh

echo "Test Environment Setup"
echo "====================="

ENVIRONMENT=$1
CLEANUP=$2

if [ -z "$ENVIRONMENT" ]; then
    echo "Usage: $0 <environment> [cleanup]"
    echo "Environments: unit, integration, api, ui, performance"
    exit 1
fi

# Function to setup unit test environment
setup_unit_environment() {
    echo "Setting up unit test environment..."
    # Unit tests use in-memory database, no setup needed
    echo "✅ Unit test environment ready"
}

# Function to setup integration test environment
setup_integration_environment() {
    echo "Setting up integration test environment..."
    
    # Start PostgreSQL
    docker run -d --name postgres-test \
        -e POSTGRES_DB=VirtualQueueTest \
        -e POSTGRES_USER=postgres \
        -e POSTGRES_PASSWORD=postgres \
        -p 5433:5432 \
        postgres:15
    
    # Start Redis
    docker run -d --name redis-test \
        -p 6380:6379 \
        redis:7-alpine
    
    # Wait for services to be ready
    sleep 10
    
    # Run database migrations
    dotnet ef database update --project src/VirtualQueue.Infrastructure --startup-project src/VirtualQueue.Api --environment Testing
    
    echo "✅ Integration test environment ready"
}

# Function to setup API test environment
setup_api_environment() {
    echo "Setting up API test environment..."
    
    # Start all services
    docker-compose -f docker-compose.test.yml up -d
    
    # Wait for services to be ready
    sleep 30
    
    # Run database migrations
    dotnet ef database update --project src/VirtualQueue.Infrastructure --startup-project src/VirtualQueue.Api --environment Testing
    
    # Seed test data
    dotnet run --project src/VirtualQueue.Api --environment Testing --seed-test-data
    
    echo "✅ API test environment ready"
}

# Function to setup UI test environment
setup_ui_environment() {
    echo "Setting up UI test environment..."
    
    # Start all services
    docker-compose -f docker-compose.test.yml up -d
    
    # Wait for services to be ready
    sleep 30
    
    # Run database migrations
    dotnet ef database update --project src/VirtualQueue.Infrastructure --startup-project src/VirtualQueue.Api --environment Testing
    
    # Seed test data
    dotnet run --project src/VirtualQueue.Api --environment Testing --seed-test-data
    
    # Start frontend
    cd src/VirtualQueue.Web
    npm install
    npm run build
    npm start &
    FRONTEND_PID=$!
    echo $FRONTEND_PID > /tmp/frontend.pid
    
    echo "✅ UI test environment ready"
}

# Function to setup performance test environment
setup_performance_environment() {
    echo "Setting up performance test environment..."
    
    # Start all services with performance configuration
    docker-compose -f docker-compose.performance.yml up -d
    
    # Wait for services to be ready
    sleep 30
    
    # Run database migrations
    dotnet ef database update --project src/VirtualQueue.Infrastructure --startup-project src/VirtualQueue.Api --environment Performance
    
    # Seed performance test data
    dotnet run --project src/VirtualQueue.Api --environment Performance --seed-performance-data
    
    echo "✅ Performance test environment ready"
}

# Function to cleanup test environment
cleanup_environment() {
    echo "Cleaning up test environment..."
    
    # Stop and remove containers
    docker-compose -f docker-compose.test.yml down -v
    docker-compose -f docker-compose.performance.yml down -v
    
    # Remove test containers
    docker rm -f postgres-test redis-test 2>/dev/null || true
    
    # Stop frontend if running
    if [ -f /tmp/frontend.pid ]; then
        FRONTEND_PID=$(cat /tmp/frontend.pid)
        kill $FRONTEND_PID 2>/dev/null || true
        rm /tmp/frontend.pid
    fi
    
    echo "✅ Test environment cleaned up"
}

# Main execution
case $ENVIRONMENT in
    "unit")
        setup_unit_environment
        ;;
    "integration")
        setup_integration_environment
        ;;
    "api")
        setup_api_environment
        ;;
    "ui")
        setup_ui_environment
        ;;
    "performance")
        setup_performance_environment
        ;;
    *)
        echo "Invalid environment: $ENVIRONMENT"
        exit 1
        ;;
esac

# Cleanup if requested
if [ "$CLEANUP" = "cleanup" ]; then
    cleanup_environment
fi
```

## Test Reporting

### **Automated Test Reporting**

#### **Test Report Generator**
```bash
#!/bin/bash
# test-report-generator.sh

echo "Test Report Generator"
echo "===================="

REPORT_TYPE=$1
OUTPUT_DIR=${2:-"/var/reports/tests"}

if [ -z "$REPORT_TYPE" ]; then
    echo "Usage: $0 <report_type> [output_dir]"
    echo "Report types: summary, detailed, coverage, performance"
    exit 1
fi

mkdir -p "$OUTPUT_DIR"

# Function to generate summary report
generate_summary_report() {
    echo "Generating summary report..."
    
    cat > "$OUTPUT_DIR/test-summary-$(date +%Y%m%d_%H%M%S).html" << EOF
<!DOCTYPE html>
<html>
<head>
    <title>Test Summary Report</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 20px; }
        .header { background-color: #f0f0f0; padding: 20px; border-radius: 5px; }
        .summary { margin: 20px 0; }
        .test-results { margin: 20px 0; }
        .passed { color: green; }
        .failed { color: red; }
        .skipped { color: orange; }
    </style>
</head>
<body>
    <div class="header">
        <h1>Test Summary Report</h1>
        <p>Generated: $(date)</p>
    </div>
    
    <div class="summary">
        <h2>Test Summary</h2>
        <p>Total Tests: $(grep -c "Test:" /var/log/tests/test-results.log)</p>
        <p class="passed">Passed: $(grep -c "PASSED" /var/log/tests/test-results.log)</p>
        <p class="failed">Failed: $(grep -c "FAILED" /var/log/tests/test-results.log)</p>
        <p class="skipped">Skipped: $(grep -c "SKIPPED" /var/log/tests/test-results.log)</p>
    </div>
    
    <div class="test-results">
        <h2>Test Results</h2>
        <pre>$(tail -100 /var/log/tests/test-results.log)</pre>
    </div>
</body>
</html>
EOF

    echo "✅ Summary report generated"
}

# Function to generate detailed report
generate_detailed_report() {
    echo "Generating detailed report..."
    
    # Generate detailed test report using test results
    dotnet test --logger "html;LogFileName=$OUTPUT_DIR/detailed-report-$(date +%Y%m%d_%H%M%S).html" --collect:"XPlat Code Coverage"
    
    echo "✅ Detailed report generated"
}

# Function to generate coverage report
generate_coverage_report() {
    echo "Generating coverage report..."
    
    # Generate coverage report
    dotnet test --collect:"XPlat Code Coverage" --results-directory "$OUTPUT_DIR"
    
    # Generate HTML coverage report
    reportgenerator -reports:"$OUTPUT_DIR/**/coverage.cobertura.xml" -targetdir:"$OUTPUT_DIR/coverage-report-$(date +%Y%m%d_%H%M%S)" -reporttypes:"Html"
    
    echo "✅ Coverage report generated"
}

# Function to generate performance report
generate_performance_report() {
    echo "Generating performance report..."
    
    cat > "$OUTPUT_DIR/performance-report-$(date +%Y%m%d_%H%M%S).html" << EOF
<!DOCTYPE html>
<html>
<head>
    <title>Performance Test Report</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 20px; }
        .header { background-color: #f0f0f0; padding: 20px; border-radius: 5px; }
        .metrics { margin: 20px 0; }
        .chart { margin: 20px 0; }
    </style>
</head>
<body>
    <div class="header">
        <h1>Performance Test Report</h1>
        <p>Generated: $(date)</p>
    </div>
    
    <div class="metrics">
        <h2>Performance Metrics</h2>
        <p>Average Response Time: $(grep "Average Response Time" /var/log/tests/performance-results.log | tail -1 | cut -d: -f2)</p>
        <p>Throughput: $(grep "Throughput" /var/log/tests/performance-results.log | tail -1 | cut -d: -f2)</p>
        <p>Error Rate: $(grep "Error Rate" /var/log/tests/performance-results.log | tail -1 | cut -d: -f2)</p>
    </div>
    
    <div class="chart">
        <h2>Performance Trends</h2>
        <pre>$(tail -50 /var/log/tests/performance-results.log)</pre>
    </div>
</body>
</html>
EOF

    echo "✅ Performance report generated"
}

# Main execution
case $REPORT_TYPE in
    "summary")
        generate_summary_report
        ;;
    "detailed")
        generate_detailed_report
        ;;
    "coverage")
        generate_coverage_report
        ;;
    "performance")
        generate_performance_report
        ;;
    *)
        echo "Invalid report type: $REPORT_TYPE"
        exit 1
        ;;
esac

echo "Test report generation completed"
```

## Test Maintenance

### **Test Maintenance Procedures**

#### **Test Maintenance Script**
```bash
#!/bin/bash
# test-maintenance.sh

echo "Test Maintenance"
echo "==============="

# 1. Update Test Dependencies
echo "1. Updating test dependencies..."
dotnet restore
npm install

# 2. Update Test Data
echo "2. Updating test data..."
dotnet run --project src/VirtualQueue.Api --environment Testing --update-test-data

# 3. Cleanup Old Test Results
echo "3. Cleaning up old test results..."
find /var/log/tests -name "*.log" -mtime +30 -delete
find /var/reports/tests -name "*.html" -mtime +30 -delete

# 4. Update Test Configuration
echo "4. Updating test configuration..."
# Update test configuration files

# 5. Validate Test Environment
echo "5. Validating test environment..."
./test-environment-setup.sh integration

# 6. Run Test Validation
echo "6. Running test validation..."
dotnet test --filter "Category=Unit" --logger "console;verbosity=normal"

# 7. Update Test Documentation
echo "7. Updating test documentation..."
# Update test documentation

echo "Test maintenance completed"
```

## Approval and Sign-off

### **Automated Testing Approval**
- **QA Engineer**: [Name] - [Date]
- **Technical Lead**: [Name] - [Date]
- **DevOps Lead**: [Name] - [Date]
- **Test Manager**: [Name] - [Date]

### **Document Control**
- **Version**: 1.0
- **Last Updated**: January 15, 2024
- **Next Review**: February 15, 2024
- **Distribution**: QA Team, Development Team, DevOps Team

---

**Document Status**: Draft  
**Next Phase**: Environment Monitoring  
**Dependencies**: Test automation setup, CI/CD pipeline configuration
