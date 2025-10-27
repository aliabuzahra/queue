# Virtual Queue Management System - Database Initialization Script
-- This script initializes the database with required extensions and initial data

-- Create extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";

-- Create initial tenant
INSERT INTO "Tenants" ("Id", "Name", "Description", "IsActive", "CreatedAt", "UpdatedAt", "Settings")
VALUES (
    uuid_generate_v4(),
    'Default Tenant',
    'Default tenant for Virtual Queue Management System',
    true,
    NOW(),
    NOW(),
    '{"maxQueues": 10, "maxUsers": 1000, "features": ["notifications", "webhooks", "analytics"]}'
) ON CONFLICT DO NOTHING;

-- Create initial admin user
INSERT INTO "Users" ("Id", "TenantId", "Username", "Email", "FirstName", "LastName", "Role", "Status", "IsEmailVerified", "IsPhoneVerified", "CreatedAt", "UpdatedAt")
SELECT 
    uuid_generate_v4(),
    t."Id",
    'admin',
    'admin@virtualqueue.com',
    'System',
    'Administrator',
    0, -- Admin role
    0, -- Active status
    true,
    false,
    NOW(),
    NOW()
FROM "Tenants" t
WHERE t."Name" = 'Default Tenant'
ON CONFLICT DO NOTHING;

-- Create sample queue
INSERT INTO "Queues" ("Id", "TenantId", "Name", "Description", "IsActive", "MaxCapacity", "EstimatedWaitTimeMinutes", "CreatedAt", "UpdatedAt")
SELECT 
    uuid_generate_v4(),
    t."Id",
    'General Queue',
    'General purpose queue for customer service',
    true,
    100,
    15,
    NOW(),
    NOW()
FROM "Tenants" t
WHERE t."Name" = 'Default Tenant'
ON CONFLICT DO NOTHING;

-- Create notification templates
INSERT INTO "NotificationTemplates" ("Id", "TenantId", "Name", "Type", "Subject", "Body", "IsActive", "CreatedAt", "UpdatedAt")
SELECT 
    uuid_generate_v4(),
    t."Id",
    'Queue Position Update',
    'Email',
    'Your Queue Position Update',
    'Hello {{FirstName}}, your current position in {{QueueName}} is {{Position}}. Estimated wait time: {{WaitTime}} minutes.',
    true,
    NOW(),
    NOW()
FROM "Tenants" t
WHERE t."Name" = 'Default Tenant'
ON CONFLICT DO NOTHING;

INSERT INTO "NotificationTemplates" ("Id", "TenantId", "Name", "Type", "Subject", "Body", "IsActive", "CreatedAt", "UpdatedAt")
SELECT 
    uuid_generate_v4(),
    t."Id",
    'Queue Ready',
    'SMS',
    '',
    'Your turn is up! Please proceed to {{QueueName}}. Position: {{Position}}',
    true,
    NOW(),
    NOW()
FROM "Tenants" t
WHERE t."Name" = 'Default Tenant'
ON CONFLICT DO NOTHING;

-- Create sample webhook
INSERT INTO "Webhooks" ("Id", "TenantId", "Name", "Url", "Events", "IsActive", "CreatedAt", "UpdatedAt")
SELECT 
    uuid_generate_v4(),
    t."Id",
    'Queue Events Webhook',
    'https://webhook.site/your-webhook-url',
    '["UserEnqueued", "UserDequeued", "QueuePositionUpdated"]',
    true,
    NOW(),
    NOW()
FROM "Tenants" t
WHERE t."Name" = 'Default Tenant'
ON CONFLICT DO NOTHING;

-- Create sample alert
INSERT INTO "Alerts" ("Id", "TenantId", "Name", "Condition", "Severity", "Message", "IsActive", "CreatedAt", "UpdatedAt")
SELECT 
    uuid_generate_v4(),
    t."Id",
    'High Queue Length',
    'queue_length > 50',
    1, -- Warning
    'Queue length has exceeded 50 users',
    true,
    NOW(),
    NOW()
FROM "Tenants" t
WHERE t."Name" = 'Default Tenant'
ON CONFLICT DO NOTHING;

-- Create sample integration
INSERT INTO "Integrations" ("Id", "TenantId", "Name", "Type", "Provider", "Configuration", "IsActive", "CreatedAt", "UpdatedAt")
SELECT 
    uuid_generate_v4(),
    t."Id",
    'Twilio SMS Integration',
    'SMS',
    'Twilio',
    '{"accountSid": "your-account-sid", "authToken": "your-auth-token", "fromNumber": "+1234567890"}',
    true,
    NOW(),
    NOW()
FROM "Tenants" t
WHERE t."Name" = 'Default Tenant'
ON CONFLICT DO NOTHING;

-- Create indexes for better performance
CREATE INDEX IF NOT EXISTS "IX_Users_TenantId" ON "Users" ("TenantId");
CREATE INDEX IF NOT EXISTS "IX_Users_Username" ON "Users" ("Username");
CREATE INDEX IF NOT EXISTS "IX_Users_Email" ON "Users" ("Email");
CREATE INDEX IF NOT EXISTS "IX_Queues_TenantId" ON "Queues" ("TenantId");
CREATE INDEX IF NOT EXISTS "IX_UserSessions_TenantId" ON "UserSessions" ("TenantId");
CREATE INDEX IF NOT EXISTS "IX_UserSessions_QueueId" ON "UserSessions" ("QueueId");
CREATE INDEX IF NOT EXISTS "IX_QueueEvents_TenantId" ON "QueueEvents" ("TenantId");
CREATE INDEX IF NOT EXISTS "IX_QueueEvents_QueueId" ON "QueueEvents" ("QueueId");
CREATE INDEX IF NOT EXISTS "IX_QueueEvents_EventTimestamp" ON "QueueEvents" ("EventTimestamp");

-- Grant permissions
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO virtualqueue_user;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO virtualqueue_user;
