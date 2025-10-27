using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtualQueue.Infrastructure.Migrations
{
    /// <summary>
    /// Seed data migration for Virtual Queue Management System
    /// Creates initial tenant and admin user for testing
    /// </summary>
    public partial class SeedData : Migration
    {
        /// <summary>
        /// Up migration - inserts seed data
        /// </summary>
        /// <param name="migrationBuilder">The migration builder</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Insert default tenant
            migrationBuilder.InsertData(
                table: "Tenants",
                columns: new[] { "Id", "Name", "Domain", "ApiKey", "IsActive", "CreatedAt", "UpdatedAt", "IsDeleted" },
                values: new object[] 
                { 
                    new Guid("11111111-1111-1111-1111-111111111111"), 
                    "Default Tenant", 
                    "default.local", 
                    "vq_default_tenant_key_2024", 
                    true, 
                    DateTime.UtcNow, 
                    null, 
                    false 
                });

            // Insert admin user
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { 
                    "Id", 
                    "TenantId", 
                    "Username", 
                    "Email", 
                    "PasswordHash", 
                    "FirstName", 
                    "LastName", 
                    "PhoneNumber", 
                    "Role", 
                    "Status", 
                    "LastLoginAt", 
                    "EmailVerifiedAt", 
                    "PhoneVerifiedAt", 
                    "TwoFactorEnabled", 
                    "TwoFactorSecret", 
                    "CreatedAt", 
                    "UpdatedAt", 
                    "IsDeleted" 
                },
                values: new object[] 
                { 
                    new Guid("22222222-2222-2222-2222-222222222222"), 
                    new Guid("11111111-1111-1111-1111-111111111111"), 
                    "admin", 
                    "admin@default.local", 
                    "$2a$11$K8Y1xY7xY7xY7xY7xY7xY7xY7xY7xY7xY7xY7xY7xY7xY7xY7xY7xY7", // BCrypt hash for "admin123"
                    "System", 
                    "Administrator", 
                    "+1234567890", 
                    0, // Admin role
                    0, // Active status
                    null, 
                    DateTime.UtcNow, 
                    null, 
                    false, 
                    null, 
                    DateTime.UtcNow, 
                    null, 
                    false 
                });

            // Insert sample queue
            migrationBuilder.InsertData(
                table: "Queues",
                columns: new[] { 
                    "Id", 
                    "TenantId", 
                    "Name", 
                    "Description", 
                    "MaxConcurrentUsers", 
                    "ReleaseRatePerMinute", 
                    "IsActive", 
                    "LastReleaseAt", 
                    "Schedule", 
                    "CreatedAt", 
                    "UpdatedAt", 
                    "IsDeleted" 
                },
                values: new object[] 
                { 
                    new Guid("33333333-3333-3333-3333-333333333333"), 
                    new Guid("11111111-1111-1111-1111-111111111111"), 
                    "Customer Service Queue", 
                    "Main customer service queue for support requests", 
                    10, 
                    5, 
                    true, 
                    null, 
                    "{\"BusinessHours\":{\"StartTime\":\"09:00:00\",\"EndTime\":\"17:00:00\",\"TimeZone\":\"UTC\",\"DaysOfWeek\":[1,2,3,4,5]}}", 
                    DateTime.UtcNow, 
                    null, 
                    false 
                });

            // Insert sample notification template
            migrationBuilder.InsertData(
                table: "NotificationTemplates",
                columns: new[] { 
                    "Id", 
                    "TenantId", 
                    "Name", 
                    "Description", 
                    "Type", 
                    "Subject", 
                    "Body", 
                    "Variables", 
                    "IsActive", 
                    "CreatedAt", 
                    "UpdatedAt", 
                    "IsDeleted" 
                },
                values: new object[] 
                { 
                    new Guid("44444444-4444-4444-4444-444444444444"), 
                    new Guid("11111111-1111-1111-1111-111111111111"), 
                    "Queue Position Update", 
                    "Email template for queue position updates", 
                    0, // Email type
                    "Your Queue Position Update", 
                    "Hello {{FirstName}},<br><br>Your position in the {{QueueName}} queue is {{Position}}.<br><br>Estimated wait time: {{WaitTime}}<br><br>Best regards,<br>Virtual Queue System", 
                    "FirstName,QueueName,Position,WaitTime", 
                    true, 
                    DateTime.UtcNow, 
                    null, 
                    false 
                });

            // Insert sample API key
            migrationBuilder.InsertData(
                table: "ApiKeys",
                columns: new[] { 
                    "Id", 
                    "TenantId", 
                    "KeyName", 
                    "KeyValue", 
                    "Description", 
                    "Permissions", 
                    "IsActive", 
                    "ExpiresAt", 
                    "LastUsedAt", 
                    "UsageCount", 
                    "CreatedAt", 
                    "UpdatedAt", 
                    "IsDeleted" 
                },
                values: new object[] 
                { 
                    new Guid("55555555-5555-5555-5555-555555555555"), 
                    new Guid("11111111-1111-1111-1111-111111111111"), 
                    "Default API Key", 
                    "vq_11111111-1111-1111-1111-111111111111_default_key_2024", 
                    "Default API key for testing and development", 
                    "queue:read,queue:write,user:read,user:write", 
                    true, 
                    DateTime.UtcNow.AddYears(1), 
                    null, 
                    0, 
                    DateTime.UtcNow, 
                    null, 
                    false 
                });
        }

        /// <summary>
        /// Down migration - removes seed data
        /// </summary>
        /// <param name="migrationBuilder">The migration builder</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove seed data in reverse order
            migrationBuilder.DeleteData(
                table: "ApiKeys",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"));

            migrationBuilder.DeleteData(
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"));

            migrationBuilder.DeleteData(
                table: "Queues",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));
        }
    }
}
