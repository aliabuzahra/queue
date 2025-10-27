# Development Environment Setup - Virtual Queue Management System

**Document Version:** 1.0  
**Date:** January 15, 2024  
**Author:** Technical Lead  
**Status:** Draft  
**Phase:** 4 - Implementation  
**Priority:** 🔴 Critical  

---

## Executive Summary

This document provides comprehensive guidance for setting up development environments for the Virtual Queue Management System. It covers local development setup, required tools, IDE configuration, debugging procedures, and troubleshooting guidelines to ensure consistent development experiences across the team.

## System Requirements

### **Hardware Requirements**

#### **Minimum Requirements**
- **CPU**: Intel i5 or AMD Ryzen 5 (4 cores)
- **RAM**: 16 GB RAM
- **Storage**: 100 GB available disk space
- **Network**: Stable internet connection
- **OS**: Windows 10/11, macOS 10.15+, or Ubuntu 20.04+

#### **Recommended Requirements**
- **CPU**: Intel i7 or AMD Ryzen 7 (8 cores)
- **RAM**: 32 GB RAM
- **Storage**: 500 GB SSD
- **Network**: High-speed internet connection
- **OS**: Latest stable version

### **Software Requirements**

#### **Core Development Tools**
- **.NET 8 SDK**: Latest .NET 8 SDK
- **Visual Studio 2022**: Latest version with .NET workload
- **Visual Studio Code**: Latest version with C# extension
- **Git**: Latest version
- **Docker Desktop**: Latest version
- **PostgreSQL**: Latest version

#### **Additional Tools**
- **Redis**: Latest version
- **Node.js**: LTS version
- **npm/yarn**: Package managers
- **Postman**: API testing
- **DBeaver**: Database management
- **RedisInsight**: Redis management

## Development Environment Setup

### **Local Development Setup**

#### **Step 1: Install Core Tools**
```bash
# Install .NET 8 SDK
# Download from: https://dotnet.microsoft.com/download/dotnet/8.0

# Install Git
# Download from: https://git-scm.com/downloads

# Install Docker Desktop
# Download from: https://www.docker.com/products/docker-desktop

# Install PostgreSQL
# Download from: https://www.postgresql.org/download/

# Install Redis
# Download from: https://redis.io/download
```

#### **Step 2: Clone Repository**
```bash
# Clone the repository
git clone https://github.com/your-org/virtual-queue-management-system.git
cd virtual-queue-management-system

# Checkout main branch
git checkout main

# Pull latest changes
git pull origin main
```

#### **Step 3: Configure Environment**
```bash
# Copy environment template
cp .env.template .env

# Edit environment variables
# Update database connection strings
# Update Redis connection strings
# Update API keys and secrets
```

#### **Step 4: Install Dependencies**
```bash
# Restore NuGet packages
dotnet restore

# Install npm packages (if applicable)
npm install

# Build the solution
dotnet build
```

### **Database Setup**

#### **PostgreSQL Configuration**
```sql
-- Create database
CREATE DATABASE virtual_queue_dev;

-- Create user
CREATE USER vq_dev_user WITH PASSWORD 'dev_password';

-- Grant permissions
GRANT ALL PRIVILEGES ON DATABASE virtual_queue_dev TO vq_dev_user;

-- Connect to database
\c virtual_queue_dev;

-- Grant schema permissions
GRANT ALL ON SCHEMA public TO vq_dev_user;
```

#### **Database Migration**
```bash
# Run database migrations
dotnet ef database update --project src/VirtualQueue.Infrastructure --startup-project src/VirtualQueue.Api

# Seed development data
dotnet run --project src/VirtualQueue.Api --seed-data
```

### **Redis Setup**

#### **Redis Configuration**
```bash
# Start Redis server
redis-server

# Test Redis connection
redis-cli ping

# Configure Redis for development
redis-cli config set save ""
redis-cli config set appendonly no
```

#### **Redis Management**
```bash
# Install RedisInsight for GUI management
# Download from: https://redis.com/redis-enterprise/redis-insight/

# Connect to Redis instance
# Host: localhost
# Port: 6379
# Password: (if configured)
```

## IDE Configuration

### **Visual Studio 2022 Setup**

#### **Required Extensions**
- **C# Dev Kit**: Microsoft C# extension
- **GitHub Copilot**: AI-powered code completion
- **SonarLint**: Code quality analysis
- **ReSharper**: Code analysis and refactoring
- **Postman**: API testing integration

#### **Project Configuration**
```xml
<!-- .csproj file configuration -->
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <WarningsAsErrors />
  </PropertyGroup>
</Project>
```

#### **Debug Configuration**
```json
// launchSettings.json
{
  "profiles": {
    "VirtualQueue.Api": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "launchUrl": "swagger",
      "applicationUrl": "https://localhost:7001;http://localhost:5001",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

### **Visual Studio Code Setup**

#### **Required Extensions**
- **C# Dev Kit**: Microsoft C# extension
- **C#**: Microsoft C# extension
- **GitLens**: Git integration
- **REST Client**: API testing
- **Thunder Client**: API testing
- **Postman**: API testing

#### **Settings Configuration**
```json
// settings.json
{
  "dotnet.defaultSolution": "VirtualQueue.sln",
  "dotnet.completion.showCompletionItemsFromUnimportedNamespaces": true,
  "dotnet.inlayHints.enableInlayHintsForParameters": true,
  "dotnet.inlayHints.enableInlayHintsForLiteralParameters": true,
  "dotnet.inlayHints.enableInlayHintsForIndexerParameters": true,
  "dotnet.inlayHints.enableInlayHintsForObjectCreationParameters": true,
  "dotnet.inlayHints.enableInlayHintsForOtherParameters": true,
  "dotnet.inlayHints.suppressInlayHintsForParametersThatDifferOnlyBySuffix": true,
  "dotnet.inlayHints.suppressInlayHintsForParametersThatMatchMethodIntent": true,
  "dotnet.inlayHints.suppressInlayHintsForParametersThatMatchArgumentName": true
}
```

#### **Tasks Configuration**
```json
// tasks.json
{
  "version": "2.0.0",
  "tasks": [
    {
      "label": "build",
      "command": "dotnet",
      "type": "process",
      "args": ["build"],
      "problemMatcher": "$msCompile"
    },
    {
      "label": "test",
      "command": "dotnet",
      "type": "process",
      "args": ["test"],
      "problemMatcher": "$msCompile"
    },
    {
      "label": "run",
      "command": "dotnet",
      "type": "process",
      "args": ["run", "--project", "src/VirtualQueue.Api"],
      "problemMatcher": "$msCompile"
    }
  ]
}
```

## Docker Development Environment

### **Docker Compose Setup**

#### **Development Docker Compose**
```yaml
# docker-compose.dev.yml
version: '3.8'

services:
  api:
    build:
      context: .
      dockerfile: Dockerfile.dev
    ports:
      - "5001:80"
      - "7001:443"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Host=postgres;Database=virtual_queue_dev;Username=vq_dev_user;Password=dev_password
      - Redis__ConnectionString=redis:6379
    depends_on:
      - postgres
      - redis
    volumes:
      - .:/app
      - /app/bin
      - /app/obj

  postgres:
    image: postgres:15
    environment:
      - POSTGRES_DB=virtual_queue_dev
      - POSTGRES_USER=vq_dev_user
      - POSTGRES_PASSWORD=dev_password
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
    volumes:
      - redis_data:/data

volumes:
  postgres_data:
  redis_data:
```

#### **Development Dockerfile**
```dockerfile
# Dockerfile.dev
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy project files
COPY *.sln ./
COPY src/ ./src/
COPY tests/ ./tests/

# Restore dependencies
RUN dotnet restore

# Build the application
RUN dotnet build --configuration Release --no-restore

# Run the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/src/VirtualQueue.Api/bin/Release/net8.0/publish/ .
ENTRYPOINT ["dotnet", "VirtualQueue.Api.dll"]
```

### **Docker Commands**

#### **Development Commands**
```bash
# Start development environment
docker-compose -f docker-compose.dev.yml up -d

# View logs
docker-compose -f docker-compose.dev.yml logs -f

# Stop development environment
docker-compose -f docker-compose.dev.yml down

# Rebuild and start
docker-compose -f docker-compose.dev.yml up --build -d

# Execute commands in container
docker-compose -f docker-compose.dev.yml exec api dotnet test
```

## Debugging Configuration

### **Debugging Setup**

#### **Visual Studio Debugging**
```json
// launchSettings.json
{
  "profiles": {
    "VirtualQueue.Api": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "launchUrl": "swagger",
      "applicationUrl": "https://localhost:7001;http://localhost:5001",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development",
        "ASPNETCORE_LOGGING__LOGLEVEL__DEFAULT": "Debug",
        "ASPNETCORE_LOGGING__LOGLEVEL__MICROSOFT": "Information",
        "ASPNETCORE_LOGGING__LOGLEVEL__MICROSOFT.HOSTING.LIFETIME": "Information"
      }
    }
  }
}
```

#### **Visual Studio Code Debugging**
```json
// launch.json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": "Launch API",
      "type": "coreclr",
      "request": "launch",
      "preLaunchTask": "build",
      "program": "${workspaceFolder}/src/VirtualQueue.Api/bin/Debug/net8.0/VirtualQueue.Api.dll",
      "args": [],
      "cwd": "${workspaceFolder}/src/VirtualQueue.Api",
      "stopAtEntry": false,
      "serverReadyAction": {
        "action": "openExternally",
        "pattern": "\\bNow listening on:\\s+(https?://\\S+)",
        "uriFormat": "%s/swagger"
      },
      "env": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      },
      "sourceFileMap": {
        "/Views": "${workspaceFolder}/Views"
      }
    }
  ]
}
```

### **Debugging Tools**

#### **Debugging Extensions**
- **C# Dev Kit**: Microsoft C# debugging
- **Debugger for Chrome**: Chrome debugging
- **REST Client**: API debugging
- **Thunder Client**: API testing
- **Postman**: API testing

#### **Debugging Techniques**
- **Breakpoints**: Set breakpoints in code
- **Watch Variables**: Monitor variable values
- **Call Stack**: Examine call stack
- **Immediate Window**: Execute code in debug mode
- **Output Window**: View debug output

## Testing Environment

### **Test Configuration**

#### **Test Project Setup**
```xml
<!-- Test project configuration -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
    <PackageReference Include="xunit" Version="2.6.1" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.5.3" />
    <PackageReference Include="Moq" Version="4.20.69" />
    <PackageReference Include="FluentAssertions" Version="6.12.0" />
    <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="8.0.0" />
  </ItemGroup>
</Project>
```

#### **Test Environment Variables**
```bash
# Test environment variables
export ASPNETCORE_ENVIRONMENT=Testing
export ConnectionStrings__DefaultConnection="Host=localhost;Database=virtual_queue_test;Username=vq_test_user;Password=test_password"
export Redis__ConnectionString="localhost:6379"
```

### **Test Execution**

#### **Test Commands**
```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test tests/VirtualQueue.UnitTests/

# Run tests with specific filter
dotnet test --filter "Category=Integration"

# Run tests in watch mode
dotnet watch test
```

## Troubleshooting

### **Common Issues**

#### **Build Issues**
```bash
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build

# Clear NuGet cache
dotnet nuget locals all --clear

# Update packages
dotnet list package --outdated
dotnet add package <package-name> --version <version>
```

#### **Database Issues**
```bash
# Reset database
dotnet ef database drop --project src/VirtualQueue.Infrastructure --startup-project src/VirtualQueue.Api
dotnet ef database update --project src/VirtualQueue.Infrastructure --startup-project src/VirtualQueue.Api

# Check database connection
dotnet ef database update --project src/VirtualQueue.Infrastructure --startup-project src/VirtualQueue.Api --verbose
```

#### **Redis Issues**
```bash
# Check Redis connection
redis-cli ping

# Clear Redis cache
redis-cli flushall

# Check Redis logs
redis-cli monitor
```

### **Performance Issues**

#### **Performance Optimization**
```bash
# Enable performance profiling
export DOTNET_EnableEventPipe=1
export DOTNET_EnableDiagnostics=1

# Run with performance counters
dotnet run --project src/VirtualQueue.Api --environment Development --performance-counters
```

#### **Memory Issues**
```bash
# Check memory usage
dotnet-dump collect --process-id <pid>

# Analyze memory dump
dotnet-dump analyze <dump-file>
```

## Environment Validation

### **Validation Checklist**

#### **Pre-Development Checklist**
- [ ] .NET 8 SDK installed and working
- [ ] Git configured and repository cloned
- [ ] Database server running and accessible
- [ ] Redis server running and accessible
- [ ] IDE configured with required extensions
- [ ] Environment variables configured
- [ ] Solution builds without errors
- [ ] Tests run successfully
- [ ] API starts and responds to requests

#### **Post-Setup Validation**
```bash
# Validate .NET installation
dotnet --version

# Validate Git configuration
git --version
git config --list

# Validate database connection
dotnet ef database update --project src/VirtualQueue.Infrastructure --startup-project src/VirtualQueue.Api

# Validate Redis connection
redis-cli ping

# Validate API startup
dotnet run --project src/VirtualQueue.Api
curl https://localhost:7001/health
```

## Team Onboarding

### **New Developer Setup**

#### **Onboarding Checklist**
1. **Hardware Setup**: Verify hardware meets requirements
2. **Software Installation**: Install required software
3. **Repository Access**: Grant repository access
4. **Environment Setup**: Follow environment setup guide
5. **Validation**: Validate environment setup
6. **Training**: Provide development training
7. **Mentoring**: Assign mentor for first week

#### **Onboarding Timeline**
- **Day 1**: Hardware and software setup
- **Day 2**: Environment configuration
- **Day 3**: First code contribution
- **Week 1**: Complete onboarding checklist
- **Week 2**: Independent development

### **Environment Maintenance**

#### **Regular Maintenance**
- **Weekly**: Update dependencies
- **Monthly**: Update development tools
- **Quarterly**: Review and update setup guide
- **As Needed**: Fix environment issues

#### **Maintenance Tasks**
```bash
# Update .NET SDK
dotnet --version
# Download latest from Microsoft

# Update NuGet packages
dotnet list package --outdated
dotnet add package <package-name> --version <version>

# Update Docker images
docker-compose pull
docker-compose up --build

# Update IDE extensions
# Check for updates in Visual Studio or VS Code
```

## Approval and Sign-off

### **Development Environment Setup Approval**
- **Technical Lead**: [Name] - [Date]
- **Development Team**: [Name] - [Date]
- **DevOps Team**: [Name] - [Date]
- **Infrastructure Team**: [Name] - [Date]

### **Document Control**
- **Version**: 1.0
- **Last Updated**: January 15, 2024
- **Next Review**: February 15, 2024
- **Distribution**: Development Team, DevOps Team, Infrastructure Team

---

**Document Status**: Draft  
**Next Phase**: Operations Documentation  
**Dependencies**: Development guidelines approval, tool selection, environment validation
