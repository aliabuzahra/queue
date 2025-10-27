#!/bin/bash

# Virtual Queue Management System - Docker Build Troubleshooting Script
# This script helps troubleshoot Docker build issues

set -euo pipefail

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Functions
log() {
    echo -e "${GREEN}[$(date '+%Y-%m-%d %H:%M:%S')]${NC} $1"
}

warn() {
    echo -e "${YELLOW}[$(date '+%Y-%m-%d %H:%M:%S')] WARNING:${NC} $1"
}

error() {
    echo -e "${RED}[$(date '+%Y-%m-%d %H:%M:%S')] ERROR:${NC} $1"
    exit 1
}

info() {
    echo -e "${BLUE}[$(date '+%Y-%m-%d %H:%M:%S')] INFO:${NC} $1"
}

# Check if Docker is running
check_docker() {
    log "Checking Docker status..."
    if ! docker info > /dev/null 2>&1; then
        error "Docker is not running. Please start Docker Desktop and try again."
    fi
    log "Docker is running ✓"
}

# Check project structure
check_project_structure() {
    log "Checking project structure..."
    
    local required_dirs=(
        "src/VirtualQueue.Api"
        "src/VirtualQueue.Application"
        "src/VirtualQueue.Domain"
        "src/VirtualQueue.Infrastructure"
    )
    
    for dir in "${required_dirs[@]}"; do
        if [ ! -d "$dir" ]; then
            error "Required directory $dir not found"
        fi
    done
    
    local required_files=(
        "src/VirtualQueue.Api/VirtualQueue.Api.csproj"
        "src/VirtualQueue.Application/VirtualQueue.Application.csproj"
        "src/VirtualQueue.Domain/VirtualQueue.Domain.csproj"
        "src/VirtualQueue.Infrastructure/VirtualQueue.Infrastructure.csproj"
        "VirtualQueue.sln"
    )
    
    for file in "${required_files[@]}"; do
        if [ ! -f "$file" ]; then
            error "Required file $file not found"
        fi
    done
    
    log "Project structure is correct ✓"
}

# Test local build
test_local_build() {
    log "Testing local .NET build..."
    
    if ! command -v dotnet &> /dev/null; then
        warn ".NET SDK not found locally, skipping local build test"
        return
    fi
    
    # Restore packages
    log "Restoring packages..."
    if ! dotnet restore VirtualQueue.sln; then
        error "Package restore failed"
    fi
    
    # Build solution
    log "Building solution..."
    if ! dotnet build VirtualQueue.sln -c Release --no-restore; then
        error "Local build failed"
    fi
    
    log "Local build successful ✓"
}

# Clean Docker cache
clean_docker_cache() {
    log "Cleaning Docker cache..."
    
    # Remove unused images
    docker image prune -f
    
    # Remove unused containers
    docker container prune -f
    
    # Remove unused volumes
    docker volume prune -f
    
    # Remove unused networks
    docker network prune -f
    
    log "Docker cache cleaned ✓"
}

# Build with verbose output
build_with_verbose() {
    log "Building Docker image with verbose output..."
    
    # Build the image with verbose output
    docker build -f Dockerfile.simple -t virtualqueue-api:latest . --progress=plain --no-cache
    
    if [ $? -eq 0 ]; then
        log "Docker build successful ✓"
    else
        error "Docker build failed"
    fi
}

# Test the built image
test_built_image() {
    log "Testing built image..."
    
    # Run the container
    docker run -d --name test-virtualqueue-api -p 8081:8080 virtualqueue-api:latest
    
    # Wait for container to start
    sleep 10
    
    # Check if container is running
    if docker ps | grep -q test-virtualqueue-api; then
        log "Container is running ✓"
        
        # Test health endpoint
        if curl -f http://localhost:8081/healthz > /dev/null 2>&1; then
            log "Health check passed ✓"
        else
            warn "Health check failed"
        fi
        
        # Clean up test container
        docker stop test-virtualqueue-api
        docker rm test-virtualqueue-api
    else
        error "Container failed to start"
    fi
}

# Show build logs
show_build_logs() {
    log "Showing recent Docker build logs..."
    
    # Get the last build log
    docker logs $(docker ps -q --filter "ancestor=virtualqueue-api:latest" | head -1) 2>&1 | tail -50
}

# Main troubleshooting function
main() {
    echo "🔧 Virtual Queue Management System - Docker Build Troubleshooting"
    echo "================================================================="
    echo ""
    
    check_docker
    check_project_structure
    test_local_build
    clean_docker_cache
    
    echo ""
    info "Starting Docker build with verbose output..."
    build_with_verbose
    
    echo ""
    info "Testing built image..."
    test_built_image
    
    log "Troubleshooting completed successfully! 🚀"
}

# Run main function
main "$@"
