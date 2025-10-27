# 🔧 Docker Build Troubleshooting Guide
## Virtual Queue Management System - Fixing Build Issues

**Issue**: `failed to solve: process "/bin/sh -c dotnet build \"VirtualQueue.Api.csproj\" -c Release -o /app/build" did not complete successfully: exit code: 1`

---

## 🚀 **Quick Fix**

### **Option 1: Use the Troubleshooting Script**
```bash
# Run the automated troubleshooting script
chmod +x scripts/troubleshoot-docker-build.sh
./scripts/troubleshoot-docker-build.sh
```

### **Option 2: Manual Fix**
```bash
# Clean Docker cache
docker system prune -f

# Build with verbose output
docker build -f Dockerfile.simple -t virtualqueue-api:latest . --progress=plain --no-cache
```

---

## 🔍 **Common Causes & Solutions**

### **1. Missing Solution File**
**Problem**: Docker can't find the solution file
**Solution**: 
```bash
# Ensure VirtualQueue.sln exists
ls -la VirtualQueue.sln
```

### **2. Project Reference Issues**
**Problem**: Project references are broken
**Solution**:
```bash
# Check project files exist
ls -la src/VirtualQueue.Api/VirtualQueue.Api.csproj
ls -la src/VirtualQueue.Application/VirtualQueue.Application.csproj
ls -la src/VirtualQueue.Domain/VirtualQueue.Domain.csproj
ls -la src/VirtualQueue.Infrastructure/VirtualQueue.Infrastructure.csproj
```

### **3. Package Restore Issues**
**Problem**: NuGet packages can't be restored
**Solution**:
```bash
# Test local restore
dotnet restore VirtualQueue.sln

# Check for package conflicts
dotnet list package --outdated
```

### **4. Build Context Issues**
**Problem**: Docker build context doesn't include all files
**Solution**:
```bash
# Check .dockerignore file
cat .dockerignore

# Ensure all source files are included
find src/ -name "*.cs" | head -10
```

### **5. Memory Issues**
**Problem**: Docker doesn't have enough memory
**Solution**:
- Increase Docker Desktop memory limit to 8GB+
- Close other applications
- Restart Docker Desktop

---

## 🛠️ **Step-by-Step Troubleshooting**

### **Step 1: Check Prerequisites**
```bash
# Check Docker is running
docker --version
docker-compose --version

# Check .NET SDK (optional)
dotnet --version
```

### **Step 2: Verify Project Structure**
```bash
# Check all required files exist
ls -la VirtualQueue.sln
ls -la src/VirtualQueue.Api/VirtualQueue.Api.csproj
ls -la src/VirtualQueue.Application/VirtualQueue.Application.csproj
ls -la src/VirtualQueue.Domain/VirtualQueue.Domain.csproj
ls -la src/VirtualQueue.Infrastructure/VirtualQueue.Infrastructure.csproj
```

### **Step 3: Test Local Build**
```bash
# Restore packages
dotnet restore VirtualQueue.sln

# Build solution
dotnet build VirtualQueue.sln -c Release
```

### **Step 4: Clean Docker Environment**
```bash
# Remove all containers
docker rm -f $(docker ps -aq)

# Remove all images
docker rmi -f $(docker images -q)

# Clean system
docker system prune -af
```

### **Step 5: Build with Debug Output**
```bash
# Build with verbose output
docker build -f Dockerfile.simple -t virtualqueue-api:latest . --progress=plain --no-cache
```

### **Step 6: Check Build Logs**
```bash
# If build fails, check the logs
docker logs $(docker ps -q --filter "ancestor=virtualqueue-api:latest" | head -1)
```

---

## 🔧 **Alternative Dockerfiles**

### **Option 1: Use the Simple Dockerfile**
The project now includes `Dockerfile.simple` which should work better:
```bash
docker build -f Dockerfile.simple -t virtualqueue-api:latest .
```

### **Option 2: Use the Original Dockerfile**
If you prefer the original:
```bash
docker build -f Dockerfile.production -t virtualqueue-api:latest .
```

### **Option 3: Create a Minimal Dockerfile**
If all else fails, create a minimal Dockerfile:
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy everything
COPY . .

# Restore and build
RUN dotnet restore VirtualQueue.sln
RUN dotnet build VirtualQueue.sln -c Release
RUN dotnet publish src/VirtualQueue.Api/VirtualQueue.Api.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "VirtualQueue.Api.dll"]
```

---

## 🐳 **Docker Compose Alternative**

If Docker build continues to fail, try using Docker Compose:

### **Step 1: Create Environment File**
```bash
cp env.desktop.example .env
```

### **Step 2: Start Services**
```bash
# Start only the database and cache first
docker-compose -f docker-compose.desktop.yml up -d postgres redis

# Wait for them to be ready
sleep 30

# Start the API
docker-compose -f docker-compose.desktop.yml up -d virtualqueue-api
```

### **Step 3: Check Logs**
```bash
# Check API logs
docker-compose -f docker-compose.desktop.yml logs virtualqueue-api

# Check all logs
docker-compose -f docker-compose.desktop.yml logs
```

---

## 📊 **Debugging Commands**

### **Check Container Status**
```bash
# List all containers
docker ps -a

# Check container logs
docker logs <container_id>

# Inspect container
docker inspect <container_id>
```

### **Check Image Status**
```bash
# List all images
docker images

# Inspect image
docker inspect virtualqueue-api:latest

# Check image layers
docker history virtualqueue-api:latest
```

### **Check Build Context**
```bash
# Check what files are being sent to Docker
docker build --no-cache -f Dockerfile.simple . 2>&1 | grep "Sending build context"
```

---

## 🚨 **Emergency Solutions**

### **Solution 1: Use Pre-built Image**
If you have access to a pre-built image:
```bash
# Pull from registry
docker pull your-registry/virtualqueue-api:latest

# Or use a different base image
docker build -f Dockerfile.simple -t virtualqueue-api:latest . --build-arg BASE_IMAGE=mcr.microsoft.com/dotnet/aspnet:8.0
```

### **Solution 2: Build Without Cache**
```bash
# Build without any cache
docker build --no-cache -f Dockerfile.simple -t virtualqueue-api:latest .
```

### **Solution 3: Use Different Build Context**
```bash
# Build from src directory
cd src
docker build -f ../Dockerfile.simple -t virtualqueue-api:latest ..
```

### **Solution 4: Manual Build Steps**
```bash
# Build each project separately
docker run --rm -v $(pwd):/workspace -w /workspace mcr.microsoft.com/dotnet/sdk:8.0 dotnet restore VirtualQueue.sln
docker run --rm -v $(pwd):/workspace -w /workspace mcr.microsoft.com/dotnet/sdk:8.0 dotnet build VirtualQueue.sln -c Release
docker run --rm -v $(pwd):/workspace -w /workspace mcr.microsoft.com/dotnet/sdk:8.0 dotnet publish src/VirtualQueue.Api/VirtualQueue.Api.csproj -c Release -o /workspace/publish
```

---

## 📞 **Getting Help**

### **Check System Resources**
```bash
# Check Docker Desktop resources
docker system df

# Check system memory
free -h

# Check disk space
df -h
```

### **Docker Desktop Settings**
1. Open Docker Desktop
2. Go to Settings → Resources
3. Increase Memory to 8GB+
4. Increase CPUs to 4+
5. Restart Docker Desktop

### **Common Error Messages**

#### **"No such file or directory"**
- Check file paths in Dockerfile
- Ensure all files exist in build context

#### **"Package restore failed"**
- Check internet connection
- Clear NuGet cache: `dotnet nuget locals all --clear`

#### **"Build failed"**
- Check for compilation errors
- Ensure all dependencies are available

#### **"Out of memory"**
- Increase Docker Desktop memory
- Close other applications
- Use multi-stage builds

---

## ✅ **Success Checklist**

- [ ] Docker Desktop is running
- [ ] All project files exist
- [ ] Solution file is present
- [ ] Local build works
- [ ] Docker cache is clean
- [ ] Build context is correct
- [ ] Memory is sufficient
- [ ] Network is available

---

## 🎯 **Final Steps**

Once the build succeeds:

1. **Test the Image**:
   ```bash
   docker run -d --name test-api -p 8080:8080 virtualqueue-api:latest
   curl http://localhost:8080/healthz
   ```

2. **Start Full Stack**:
   ```bash
   docker-compose -f docker-compose.desktop.yml up -d
   ```

3. **Verify Services**:
   ```bash
   docker-compose -f docker-compose.desktop.yml ps
   ```

**Your Virtual Queue Management System should now be running successfully! 🚀**
