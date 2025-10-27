# Database Design - Virtual Queue Management System

**Document Version:** 1.0  
**Date:** January 15, 2024  
**Author:** Database Architect  
**Status:** Draft  
**Phase:** 03 - Architecture  
**Priority:** 🔴 Critical  

---

## Executive Summary

This document provides comprehensive database design specifications for the Virtual Queue Management System. It covers database architecture, schema design, data modeling, indexing strategies, and performance optimization techniques. The design follows Domain-Driven Design principles and supports multi-tenancy, scalability, and high-performance requirements.

## Database Architecture Overview

### **Database Technology Stack**
- **Primary Database**: PostgreSQL 15+
- **Caching Layer**: Redis 7+
- **Search Engine**: Elasticsearch 8+ (for analytics)
- **Message Queue**: Redis Streams
- **Backup Solution**: PostgreSQL WAL-E/WAL-G

### **Architecture Principles**
- **Multi-Tenancy**: Shared schema with tenant isolation
- **Scalability**: Horizontal and vertical scaling support
- **Performance**: Optimized for high-throughput operations
- **Reliability**: ACID compliance and data consistency
- **Security**: Data encryption and access control

## Database Schema Design

### **Core Domain Entities**

#### **Tenant Entity**
```sql
CREATE TABLE tenants (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(255) NOT NULL,
    slug VARCHAR(100) UNIQUE NOT NULL,
    description TEXT,
    settings JSONB,
    status VARCHAR(20) DEFAULT 'active' CHECK (status IN ('active', 'inactive', 'suspended')),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    created_by UUID,
    updated_by UUID
);

-- Indexes
CREATE INDEX idx_tenants_slug ON tenants(slug);
CREATE INDEX idx_tenants_status ON tenants(status);
CREATE INDEX idx_tenants_created_at ON tenants(created_at);
```

#### **User Entity**
```sql
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE CASCADE,
    email VARCHAR(255) NOT NULL,
    username VARCHAR(100),
    first_name VARCHAR(100),
    last_name VARCHAR(100),
    phone VARCHAR(20),
    password_hash VARCHAR(255),
    salt VARCHAR(255),
    role VARCHAR(50) DEFAULT 'user' CHECK (role IN ('admin', 'manager', 'staff', 'user')),
    status VARCHAR(20) DEFAULT 'active' CHECK (status IN ('active', 'inactive', 'suspended', 'pending')),
    email_verified BOOLEAN DEFAULT FALSE,
    phone_verified BOOLEAN DEFAULT FALSE,
    two_factor_enabled BOOLEAN DEFAULT FALSE,
    two_factor_secret VARCHAR(255),
    last_login_at TIMESTAMP WITH TIME ZONE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    created_by UUID,
    updated_by UUID,
    
    CONSTRAINT uk_users_tenant_email UNIQUE (tenant_id, email),
    CONSTRAINT uk_users_tenant_username UNIQUE (tenant_id, username)
);

-- Indexes
CREATE INDEX idx_users_tenant_id ON users(tenant_id);
CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_users_username ON users(username);
CREATE INDEX idx_users_role ON users(role);
CREATE INDEX idx_users_status ON users(status);
CREATE INDEX idx_users_created_at ON users(created_at);
```

#### **Queue Entity**
```sql
CREATE TABLE queues (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE CASCADE,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    slug VARCHAR(100) NOT NULL,
    capacity INTEGER DEFAULT 1000,
    priority INTEGER DEFAULT 0,
    status VARCHAR(20) DEFAULT 'active' CHECK (status IN ('active', 'inactive', 'maintenance')),
    settings JSONB,
    operating_hours JSONB,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    created_by UUID REFERENCES users(id),
    updated_by UUID REFERENCES users(id),
    
    CONSTRAINT uk_queues_tenant_slug UNIQUE (tenant_id, slug),
    CONSTRAINT chk_queues_capacity CHECK (capacity > 0)
);

-- Indexes
CREATE INDEX idx_queues_tenant_id ON queues(tenant_id);
CREATE INDEX idx_queues_slug ON queues(slug);
CREATE INDEX idx_queues_status ON queues(status);
CREATE INDEX idx_queues_priority ON queues(priority);
CREATE INDEX idx_queues_created_at ON queues(created_at);
```

#### **UserSession Entity**
```sql
CREATE TABLE user_sessions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE CASCADE,
    queue_id UUID NOT NULL REFERENCES queues(id) ON DELETE CASCADE,
    user_id UUID REFERENCES users(id) ON DELETE SET NULL,
    session_token VARCHAR(255) UNIQUE NOT NULL,
    position INTEGER NOT NULL,
    status VARCHAR(20) DEFAULT 'waiting' CHECK (status IN ('waiting', 'serving', 'completed', 'cancelled', 'no_show')),
    estimated_wait_time INTEGER, -- in minutes
    actual_wait_time INTEGER, -- in minutes
    service_start_time TIMESTAMP WITH TIME ZONE,
    service_end_time TIMESTAMP WITH TIME ZONE,
    metadata JSONB,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    
    CONSTRAINT chk_user_sessions_position CHECK (position > 0),
    CONSTRAINT chk_user_sessions_wait_time CHECK (estimated_wait_time >= 0)
);

-- Indexes
CREATE INDEX idx_user_sessions_tenant_id ON user_sessions(tenant_id);
CREATE INDEX idx_user_sessions_queue_id ON user_sessions(queue_id);
CREATE INDEX idx_user_sessions_user_id ON user_sessions(user_id);
CREATE INDEX idx_user_sessions_session_token ON user_sessions(session_token);
CREATE INDEX idx_user_sessions_status ON user_sessions(status);
CREATE INDEX idx_user_sessions_position ON user_sessions(position);
CREATE INDEX idx_user_sessions_created_at ON user_sessions(created_at);
```

### **Supporting Entities**

#### **QueueEvent Entity**
```sql
CREATE TABLE queue_events (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE CASCADE,
    queue_id UUID NOT NULL REFERENCES queues(id) ON DELETE CASCADE,
    user_session_id UUID REFERENCES user_sessions(id) ON DELETE CASCADE,
    event_type VARCHAR(50) NOT NULL,
    event_data JSONB,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    created_by UUID REFERENCES users(id)
);

-- Indexes
CREATE INDEX idx_queue_events_tenant_id ON queue_events(tenant_id);
CREATE INDEX idx_queue_events_queue_id ON queue_events(queue_id);
CREATE INDEX idx_queue_events_user_session_id ON queue_events(user_session_id);
CREATE INDEX idx_queue_events_event_type ON queue_events(event_type);
CREATE INDEX idx_queue_events_created_at ON queue_events(created_at);
```

#### **Notification Entity**
```sql
CREATE TABLE notifications (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE CASCADE,
    user_id UUID REFERENCES users(id) ON DELETE CASCADE,
    user_session_id UUID REFERENCES user_sessions(id) ON DELETE CASCADE,
    type VARCHAR(50) NOT NULL,
    channel VARCHAR(20) NOT NULL CHECK (channel IN ('email', 'sms', 'push', 'webhook')),
    recipient VARCHAR(255) NOT NULL,
    subject VARCHAR(255),
    content TEXT NOT NULL,
    status VARCHAR(20) DEFAULT 'pending' CHECK (status IN ('pending', 'sent', 'delivered', 'failed')),
    sent_at TIMESTAMP WITH TIME ZONE,
    delivered_at TIMESTAMP WITH TIME ZONE,
    error_message TEXT,
    metadata JSONB,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Indexes
CREATE INDEX idx_notifications_tenant_id ON notifications(tenant_id);
CREATE INDEX idx_notifications_user_id ON notifications(user_id);
CREATE INDEX idx_notifications_user_session_id ON notifications(user_session_id);
CREATE INDEX idx_notifications_type ON notifications(type);
CREATE INDEX idx_notifications_channel ON notifications(channel);
CREATE INDEX idx_notifications_status ON notifications(status);
CREATE INDEX idx_notifications_created_at ON notifications(created_at);
```

#### **AuditLog Entity**
```sql
CREATE TABLE audit_logs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE CASCADE,
    user_id UUID REFERENCES users(id) ON DELETE SET NULL,
    entity_type VARCHAR(100) NOT NULL,
    entity_id UUID NOT NULL,
    action VARCHAR(50) NOT NULL,
    old_values JSONB,
    new_values JSONB,
    ip_address INET,
    user_agent TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Indexes
CREATE INDEX idx_audit_logs_tenant_id ON audit_logs(tenant_id);
CREATE INDEX idx_audit_logs_user_id ON audit_logs(user_id);
CREATE INDEX idx_audit_logs_entity_type ON audit_logs(entity_type);
CREATE INDEX idx_audit_logs_entity_id ON audit_logs(entity_id);
CREATE INDEX idx_audit_logs_action ON audit_logs(action);
CREATE INDEX idx_audit_logs_created_at ON audit_logs(created_at);
```

## Data Modeling Principles

### **Domain-Driven Design (DDD) Implementation**

#### **Aggregate Roots**
- **Tenant**: Root aggregate for tenant management
- **User**: Root aggregate for user management
- **Queue**: Root aggregate for queue management
- **UserSession**: Root aggregate for session management

#### **Value Objects**
```sql
-- Email value object
CREATE DOMAIN email_address AS VARCHAR(255) 
CHECK (VALUE ~* '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$');

-- Phone number value object
CREATE DOMAIN phone_number AS VARCHAR(20) 
CHECK (VALUE ~* '^\+?[1-9]\d{1,14}$');

-- Status enums
CREATE TYPE user_status AS ENUM ('active', 'inactive', 'suspended', 'pending');
CREATE TYPE queue_status AS ENUM ('active', 'inactive', 'maintenance');
CREATE TYPE session_status AS ENUM ('waiting', 'serving', 'completed', 'cancelled', 'no_show');
```

#### **Domain Events**
```sql
CREATE TABLE domain_events (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE CASCADE,
    aggregate_id UUID NOT NULL,
    aggregate_type VARCHAR(100) NOT NULL,
    event_type VARCHAR(100) NOT NULL,
    event_data JSONB NOT NULL,
    version INTEGER NOT NULL,
    occurred_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    processed_at TIMESTAMP WITH TIME ZONE
);

-- Indexes
CREATE INDEX idx_domain_events_tenant_id ON domain_events(tenant_id);
CREATE INDEX idx_domain_events_aggregate_id ON domain_events(aggregate_id);
CREATE INDEX idx_domain_events_aggregate_type ON domain_events(aggregate_type);
CREATE INDEX idx_domain_events_event_type ON domain_events(event_type);
CREATE INDEX idx_domain_events_occurred_at ON domain_events(occurred_at);
CREATE INDEX idx_domain_events_processed_at ON domain_events(processed_at);
```

### **Multi-Tenancy Implementation**

#### **Tenant Isolation Strategy**
- **Shared Schema**: All tenants share the same database schema
- **Tenant ID**: Every table includes `tenant_id` for data isolation
- **Row-Level Security**: PostgreSQL RLS for additional security
- **Data Partitioning**: Partition large tables by tenant_id

#### **Row-Level Security (RLS)**
```sql
-- Enable RLS on all tenant-specific tables
ALTER TABLE users ENABLE ROW LEVEL SECURITY;
ALTER TABLE queues ENABLE ROW LEVEL SECURITY;
ALTER TABLE user_sessions ENABLE ROW LEVEL SECURITY;
ALTER TABLE queue_events ENABLE ROW LEVEL SECURITY;
ALTER TABLE notifications ENABLE ROW LEVEL SECURITY;
ALTER TABLE audit_logs ENABLE ROW LEVEL SECURITY;

-- Create RLS policies
CREATE POLICY tenant_isolation_users ON users
    FOR ALL TO application_role
    USING (tenant_id = current_setting('app.current_tenant_id')::UUID);

CREATE POLICY tenant_isolation_queues ON queues
    FOR ALL TO application_role
    USING (tenant_id = current_setting('app.current_tenant_id')::UUID);

-- Similar policies for other tables...
```

## Performance Optimization

### **Indexing Strategy**

#### **Primary Indexes**
- **Primary Keys**: UUID primary keys with clustered indexes
- **Foreign Keys**: Indexed for join performance
- **Unique Constraints**: Indexed for uniqueness enforcement
- **Tenant ID**: Indexed on all tenant-specific tables

#### **Composite Indexes**
```sql
-- User sessions by queue and status
CREATE INDEX idx_user_sessions_queue_status ON user_sessions(queue_id, status);

-- Queue events by queue and type
CREATE INDEX idx_queue_events_queue_type ON queue_events(queue_id, event_type);

-- Notifications by user and status
CREATE INDEX idx_notifications_user_status ON notifications(user_id, status);

-- Audit logs by entity and action
CREATE INDEX idx_audit_logs_entity_action ON audit_logs(entity_type, entity_id, action);
```

#### **Partial Indexes**
```sql
-- Active users only
CREATE INDEX idx_users_active ON users(tenant_id, email) 
WHERE status = 'active';

-- Waiting sessions only
CREATE INDEX idx_user_sessions_waiting ON user_sessions(queue_id, position) 
WHERE status = 'waiting';

-- Pending notifications only
CREATE INDEX idx_notifications_pending ON notifications(created_at) 
WHERE status = 'pending';
```

### **Query Optimization**

#### **Query Patterns**
```sql
-- Get active queues for tenant
SELECT * FROM queues 
WHERE tenant_id = $1 AND status = 'active' 
ORDER BY priority DESC, created_at ASC;

-- Get waiting sessions for queue
SELECT * FROM user_sessions 
WHERE queue_id = $1 AND status = 'waiting' 
ORDER BY position ASC;

-- Get user session statistics
SELECT 
    COUNT(*) as total_sessions,
    AVG(actual_wait_time) as avg_wait_time,
    COUNT(*) FILTER (WHERE status = 'completed') as completed_sessions
FROM user_sessions 
WHERE queue_id = $1 AND created_at >= $2;
```

#### **Query Performance Optimization**
- **EXPLAIN ANALYZE**: Regular query analysis and optimization
- **Query Caching**: Redis caching for frequently accessed data
- **Connection Pooling**: PgBouncer for connection management
- **Read Replicas**: Read-only replicas for analytics queries

### **Data Partitioning**

#### **Time-Based Partitioning**
```sql
-- Partition audit_logs by month
CREATE TABLE audit_logs_y2024m01 PARTITION OF audit_logs
FOR VALUES FROM ('2024-01-01') TO ('2024-02-01');

CREATE TABLE audit_logs_y2024m02 PARTITION OF audit_logs
FOR VALUES FROM ('2024-02-01') TO ('2024-03-01');

-- Partition queue_events by month
CREATE TABLE queue_events_y2024m01 PARTITION OF queue_events
FOR VALUES FROM ('2024-01-01') TO ('2024-02-01');
```

#### **Hash Partitioning**
```sql
-- Partition user_sessions by tenant_id
CREATE TABLE user_sessions_part_0 PARTITION OF user_sessions
FOR VALUES WITH (MODULUS 4, REMAINDER 0);

CREATE TABLE user_sessions_part_1 PARTITION OF user_sessions
FOR VALUES WITH (MODULUS 4, REMAINDER 1);
```

## Data Migration and Versioning

### **Migration Strategy**
```sql
-- Migration versioning table
CREATE TABLE schema_migrations (
    version VARCHAR(255) PRIMARY KEY,
    applied_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    checksum VARCHAR(255)
);

-- Example migration
-- V001__Create_initial_tables.sql
-- V002__Add_user_roles.sql
-- V003__Add_queue_priorities.sql
```

### **Data Seeding**
```sql
-- Seed data for development
INSERT INTO tenants (id, name, slug, description) VALUES
('00000000-0000-0000-0000-000000000001', 'Demo Tenant', 'demo', 'Demo tenant for development');

INSERT INTO users (id, tenant_id, email, username, first_name, last_name, role) VALUES
('00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001', 'admin@demo.com', 'admin', 'Admin', 'User', 'admin');
```

## Backup and Recovery

### **Backup Strategy**
- **Full Backups**: Weekly full database backups
- **Incremental Backups**: Daily incremental backups using WAL
- **Point-in-Time Recovery**: WAL archiving for PITR
- **Cross-Region Replication**: Multi-region backup storage

### **Recovery Procedures**
- **RTO**: Recovery Time Objective < 15 minutes
- **RPO**: Recovery Point Objective < 5 minutes
- **Testing**: Monthly recovery testing
- **Documentation**: Comprehensive recovery procedures

## Security Considerations

### **Data Encryption**
- **At Rest**: AES-256 encryption for sensitive data
- **In Transit**: TLS 1.3 for all connections
- **Key Management**: AWS KMS or Azure Key Vault
- **Field-Level Encryption**: Sensitive fields encrypted individually

### **Access Control**
- **Database Users**: Least privilege principle
- **Application Roles**: Role-based access control
- **Network Security**: VPC and firewall rules
- **Audit Logging**: Comprehensive audit trail

## Monitoring and Maintenance

### **Performance Monitoring**
- **Query Performance**: Slow query identification and optimization
- **Index Usage**: Index utilization monitoring
- **Connection Monitoring**: Connection pool monitoring
- **Storage Monitoring**: Disk space and growth monitoring

### **Maintenance Tasks**
- **VACUUM**: Regular vacuum and analyze operations
- **REINDEX**: Periodic index rebuilding
- **Statistics Update**: Regular statistics updates
- **Log Rotation**: Log file management and rotation

## Approval and Sign-off

### **Database Design Approval**
- **Database Architect**: [Name] - [Date]
- **Technical Lead**: [Name] - [Date]
- **Security Lead**: [Name] - [Date]
- **Performance Lead**: [Name] - [Date]

### **Document Control**
- **Version**: 1.0
- **Last Updated**: January 15, 2024
- **Next Review**: February 15, 2024
- **Distribution**: Development Team, Database Team, Operations Team

---

**Document Status**: Draft  
**Next Phase**: API Architecture Design  
**Dependencies**: Requirements approval, technology stack confirmation
