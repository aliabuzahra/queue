#!/bin/bash

# Virtual Queue Management System - Docker Desktop Startup Script
# This script helps you quickly start the production system on Docker Desktop

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

# Check if Docker Desktop is running
check_docker() {
    log "Checking Docker Desktop status..."
    if ! docker info > /dev/null 2>&1; then
        error "Docker Desktop is not running. Please start Docker Desktop and try again."
    fi
    log "Docker Desktop is running ✓"
}

# Check if Docker Compose is available
check_docker_compose() {
    log "Checking Docker Compose..."
    if ! docker-compose --version > /dev/null 2>&1; then
        error "Docker Compose is not available. Please install Docker Compose."
    fi
    log "Docker Compose is available ✓"
}

# Create environment file if it doesn't exist
create_env_file() {
    if [ ! -f ".env" ]; then
        log "Creating .env file from template..."
        cp env.desktop.example .env
        warn "Please edit .env file with your configuration before continuing."
        warn "Press Enter to continue after editing .env file..."
        read -r
    else
        log ".env file already exists ✓"
    fi
}

# Check if required files exist
check_files() {
    log "Checking required files..."
    
    local required_files=(
        "docker-compose.desktop.yml"
        "Dockerfile.production"
        "scripts/init-db.sql"
        "monitoring/prometheus.yml"
        "monitoring/alertmanager-enhanced.yml"
    )
    
    for file in "${required_files[@]}"; do
        if [ ! -f "$file" ]; then
            error "Required file $file not found. Please ensure all files are present."
        fi
    done
    
    log "All required files present ✓"
}

# Start the application
start_application() {
    log "Starting Virtual Queue Management System..."
    
    # Pull latest images
    log "Pulling latest Docker images..."
    docker-compose -f docker-compose.desktop.yml pull
    
    # Build the application
    log "Building Virtual Queue API..."
    docker-compose -f docker-compose.desktop.yml build virtualqueue-api
    
    # Start all services
    log "Starting all services..."
    docker-compose -f docker-compose.desktop.yml up -d
    
    log "Services started successfully ✓"
}

# Wait for services to be ready
wait_for_services() {
    log "Waiting for services to be ready..."
    
    # Wait for PostgreSQL
    log "Waiting for PostgreSQL..."
    timeout=60
    while [ $timeout -gt 0 ]; do
        if docker exec virtualqueue-postgres pg_isready -U virtualqueue_user -d virtualqueue_prod > /dev/null 2>&1; then
            log "PostgreSQL is ready ✓"
            break
        fi
        sleep 2
        timeout=$((timeout - 2))
    done
    
    if [ $timeout -le 0 ]; then
        error "PostgreSQL failed to start within 60 seconds"
    fi
    
    # Wait for Redis
    log "Waiting for Redis..."
    timeout=60
    while [ $timeout -gt 0 ]; do
        if docker exec virtualqueue-redis redis-cli ping > /dev/null 2>&1; then
            log "Redis is ready ✓"
            break
        fi
        sleep 2
        timeout=$((timeout - 2))
    done
    
    if [ $timeout -le 0 ]; then
        error "Redis failed to start within 60 seconds"
    fi
    
    # Wait for API
    log "Waiting for Virtual Queue API..."
    timeout=120
    while [ $timeout -gt 0 ]; do
        if curl -f http://localhost:8080/healthz > /dev/null 2>&1; then
            log "Virtual Queue API is ready ✓"
            break
        fi
        sleep 5
        timeout=$((timeout - 5))
    done
    
    if [ $timeout -le 0 ]; then
        error "Virtual Queue API failed to start within 120 seconds"
    fi
}

# Check service health
check_health() {
    log "Checking service health..."
    
    # Check API health
    if curl -f http://localhost:8080/healthz > /dev/null 2>&1; then
        log "API health check passed ✓"
    else
        warn "API health check failed"
    fi
    
    # Check database health
    if curl -f http://localhost:8080/healthz/db > /dev/null 2>&1; then
        log "Database health check passed ✓"
    else
        warn "Database health check failed"
    fi
    
    # Check Redis health
    if curl -f http://localhost:8080/healthz/redis > /dev/null 2>&1; then
        log "Redis health check passed ✓"
    else
        warn "Redis health check failed"
    fi
    
    # Check Prometheus
    if curl -f http://localhost:9090/-/healthy > /dev/null 2>&1; then
        log "Prometheus health check passed ✓"
    else
        warn "Prometheus health check failed"
    fi
    
    # Check Grafana
    if curl -f http://localhost:3000/api/health > /dev/null 2>&1; then
        log "Grafana health check passed ✓"
    else
        warn "Grafana health check failed"
    fi
}

# Display service information
show_service_info() {
    log "Virtual Queue Management System is now running!"
    echo ""
    info "🌐 Access Points:"
    echo "  • Virtual Queue API: http://localhost:8080"
    echo "  • API Documentation: http://localhost:8080/swagger"
    echo "  • Health Check: http://localhost:8080/healthz"
    echo ""
    info "📊 Monitoring:"
    echo "  • Prometheus: http://localhost:9090"
    echo "  • Grafana: http://localhost:3000 (admin / admin123)"
    echo "  • Alertmanager: http://localhost:9093"
    echo ""
    info "🗄️ Database:"
    echo "  • PostgreSQL: localhost:5432"
    echo "  • Redis: localhost:6379"
    echo ""
    info "🔧 Management Commands:"
    echo "  • View logs: docker-compose -f docker-compose.desktop.yml logs -f"
    echo "  • Stop services: docker-compose -f docker-compose.desktop.yml down"
    echo "  • Restart services: docker-compose -f docker-compose.desktop.yml restart"
    echo "  • Check status: docker-compose -f docker-compose.desktop.yml ps"
    echo ""
}

# Main execution
main() {
    echo "🐳 Virtual Queue Management System - Docker Desktop Startup"
    echo "=========================================================="
    echo ""
    
    check_docker
    check_docker_compose
    create_env_file
    check_files
    start_application
    wait_for_services
    check_health
    show_service_info
    
    log "Setup completed successfully! 🚀"
}

# Run main function
main "$@"
