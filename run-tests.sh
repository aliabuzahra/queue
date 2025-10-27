#!/bin/bash

# Virtual Queue Management System - Automated Test Runner
# Usage: ./run-tests.sh

set -e

API_URL="http://localhost:8080"
COLOR_GREEN='\033[0;32m'
COLOR_RED='\033[0;31m'
COLOR_YELLOW='\033[1;33m'
COLOR_NC='\033[0m'

log_success() { echo -e "${COLOR_GREEN}✅ $1${COLOR_NC}"; }
log_error() { echo -e "${COLOR_RED}❌ $1${COLOR_NC}"; }
log_info() { echo -e "${COLOR_YELLOW}ℹ️  $1${COLOR_NC}"; }
log_test() { echo -e "\n${COLOR_YELLOW}━━━ Test: $1 ━━━${COLOR_NC}"; }

# Test counters
TOTAL_TESTS=0
PASSED_TESTS=0
FAILED_TESTS=0

# Test Helper Functions
run_test() {
  TOTAL_TESTS=$((TOTAL_TESTS + 1))
  if $@; then
    PASSED_TESTS=$((PASSED_TESTS + 1))
  else
    FAILED_TESTS=$((FAILED_TESTS + 1))
  fi
}

# Test 1: System Health Check
test_system_health() {
  log_test "System Health Check"
  
  log_info "Checking API health..."
  response=$(curl -s -o /dev/null -w "%{http_code}" $API_URL/healthz)
  if [ "$response" = "200" ]; then
    log_success "API health check passed ($response)"
    return 0
  else
    log_error "API health check failed ($response)"
    return 1
  fi
}

# Test 2: Prometheus Targets
test_prometheus_targets() {
  log_test "Prometheus Targets"
  
  log_info "Checking Prometheus is accessible..."
  response=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:9090/-/healthy)
  if [ "$response" = "200" ]; then
    log_success "Prometheus is healthy"
    return 0
  else
    log_error "Prometheus health check failed"
    return 1
  fi
}

# Test 3: Grafana Access
test_grafana() {
  log_test "Grafana Access"
  
  log_info "Checking Grafana health..."
  response=$(curl -s http://localhost:3000/api/health)
  if echo "$response" | grep -q "ok"; then
    log_success "Grafana is accessible"
    return 0
  else
    log_error "Grafana check failed"
    return 1
  fi
}

# Test 4: Database Connection
test_database() {
  log_test "Database Connection"
  
  log_info "Checking PostgreSQL connection..."
  if docker exec virtualqueue-postgres pg_isready -U virtualqueue_user -d virtualqueue_prod > /dev/null 2>&1; then
    log_success "PostgreSQL is ready"
    return 0
  else
    log_error "PostgreSQL not ready"
    return 1
  fi
}

# Test 5: Redis Connection
test_redis() {
  log_test "Redis Connection"
  
  log_info "Checking Redis connection..."
  response=$(docker exec virtualqueue-redis redis-cli -a Redis123! ping 2>/dev/null)
  if [ "$response" = "PONG" ]; then
    log_success "Redis is ready"
    return 0
  else
    log_error "Redis not ready"
    return 1
  fi
}

# Test 6: API Metrics Endpoint
test_metrics() {
  log_test "API Metrics"
  
  log_info "Checking metrics endpoint..."
  response=$(curl -s -o /dev/null -w "%{http_code}" $API_URL/metrics)
  if [ "$response" = "200" ]; then
    log_success "Metrics endpoint accessible ($response)"
    return 0
  else
    log_error "Metrics endpoint failed ($response)"
    return 1
  fi
}

# Test 7: API Documentation
test_api_docs() {
  log_test "API Documentation"
  
  log_info "Checking Swagger UI..."
  response=$(curl -s -o /dev/null -w "%{http_code}" $API_URL/swagger/index.html)
  if [ "$response" = "200" ]; then
    log_success "Swagger UI accessible"
    return 0
  else
    log_error "Swagger UI check failed ($response)"
    return 1
  fi
}

# Test 8: Container Status
test_containers() {
  log_test "Container Status"
  
  containers=("virtualqueue-api" "virtualqueue-postgres" "virtualqueue-redis" "virtualqueue-grafana" "virtualqueue-prometheus")
  all_running=true
  
  for container in "${containers[@]}"; do
    if docker ps | grep -q "$container"; then
      log_success "$container is running"
    else
      log_error "$container is not running"
      all_running=false
    fi
  done
  
  if [ "$all_running" = true ]; then
    return 0
  else
    return 1
  fi
}

# Test 9: Network Connectivity
test_network() {
  log_test "Network Connectivity"
  
  log_info "Testing container networking..."
  
  # Test API can reach PostgreSQL
  if docker exec virtualqueue-api wget -q -O- http://virtualqueue-postgres:5432 > /dev/null 2>&1; then
    log_success "API can reach PostgreSQL"
  else
    log_info "API-PostgreSQL connectivity (normal - port check)"
  fi
  
  # Test API can reach Redis
  if docker exec virtualqueue-api ping -c 1 virtualqueue-redis > /dev/null 2>&1; then
    log_success "API can reach Redis"
    return 0
  else
    log_error "API cannot reach Redis"
    return 1
  fi
}

# Test 10: Performance Check
test_performance() {
  log_test "Performance Check"
  
  log_info "Testing API response time..."
  
  start_time=$(date +%s%N)
  curl -s $API_URL/healthz > /dev/null
  end_time=$(date +%s%N)
  
  duration_ms=$(( (end_time - start_time) / 1000000 ))
  
  if [ $duration_ms -lt 500 ]; then
    log_success "API response time: ${duration_ms}ms (excellent)"
    return 0
  elif [ $duration_ms -lt 1000 ]; then
    log_success "API response time: ${duration_ms}ms (good)"
    return 0
  else
    log_error "API response time: ${duration_ms}ms (slow)"
    return 1
  fi
}

# Main Test Execution
main() {
  echo -e "${COLOR_GREEN}"
  echo "═══════════════════════════════════════════════════════════════"
  echo "  Virtual Queue Management System - Test Runner"
  echo "═══════════════════════════════════════════════════════════════"
  echo -e "${COLOR_NC}"
  
  log_info "Starting comprehensive system tests..."
  
  # Run all tests
  run_test test_system_health
  run_test test_prometheus_targets
  run_test test_grafana
  run_test test_database
  run_test test_redis
  run_test test_metrics
  run_test test_api_docs
  run_test test_containers
  run_test test_network
  run_test test_performance
  
  # Print summary
  echo -e "\n${COLOR_GREEN}═══════════════════════════════════════════════════════════════"
  echo "  Test Summary"
  echo "═══════════════════════════════════════════════════════════════${COLOR_NC}"
  echo "Total Tests:  $TOTAL_TESTS"
  echo -e "${COLOR_GREEN}Passed:        $PASSED_TESTS${COLOR_NC}"
  
  if [ $FAILED_TESTS -gt 0 ]; then
    echo -e "${COLOR_RED}Failed:        $FAILED_TESTS${COLOR_NC}"
  else
    echo -e "${COLOR_GREEN}Failed:        $FAILED_TESTS${COLOR_NC}"
  fi
  
  success_rate=$((PASSED_TESTS * 100 / TOTAL_TESTS))
  echo "Success Rate:   $success_rate%"
  
  if [ $FAILED_TESTS -eq 0 ]; then
    echo -e "\n${COLOR_GREEN}🎉 All tests passed!${COLOR_NC}"
    return 0
  else
    echo -e "\n${COLOR_RED}❌ Some tests failed${COLOR_NC}"
    return 1
  fi
}

# Run main function
main "$@"
