using Microsoft.EntityFrameworkCore;
using VirtualQueue.Domain.Entities;
using VirtualQueue.Domain.Enums;
using VirtualQueue.Domain.ValueObjects;

namespace VirtualQueue.Infrastructure.Data;

public class VirtualQueueDbContext : DbContext
{
    public VirtualQueueDbContext(DbContextOptions<VirtualQueueDbContext> options) : base(options)
    {
    }

    public DbSet<Tenant> Tenants { get; set; } = null!;
    public DbSet<Queue> Queues { get; set; } = null!;
    public DbSet<UserSession> UserSessions { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<QueueEvent> QueueEvents { get; set; } = null!;
    public DbSet<NotificationTemplate> NotificationTemplates { get; set; } = null!;
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;
    public DbSet<ApiKey> ApiKeys { get; set; } = null!;
    public DbSet<Integration> Integrations { get; set; } = null!;
    public DbSet<Webhook> Webhooks { get; set; } = null!;
    public DbSet<Alert> Alerts { get; set; } = null!;
    public DbSet<Backup> Backups { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Tenant configuration
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Domain).IsRequired().HasMaxLength(255);
            entity.Property(e => e.ApiKey).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.Domain).IsUnique();
            entity.HasIndex(e => e.ApiKey).IsUnique();
        });

        // Queue configuration
        modelBuilder.Entity<Queue>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.TenantId).IsRequired();
            entity.Property(e => e.MaxConcurrentUsers).IsRequired();
            entity.Property(e => e.ReleaseRatePerMinute).IsRequired();
            entity.Property(e => e.LastReleaseAt);
            
            // Configure Schedule as JSON
            entity.OwnsOne(e => e.Schedule, schedule =>
            {
                schedule.ToJson();
                schedule.OwnsOne(s => s.BusinessHours, bh =>
                {
                    bh.Property(b => b.StartTime).HasConversion(
                        v => v.ToString(@"hh\:mm\:ss"),
                        v => TimeSpan.Parse(v));
                    bh.Property(b => b.EndTime).HasConversion(
                        v => v.ToString(@"hh\:mm\:ss"),
                        v => TimeSpan.Parse(v));
                    bh.Property(b => b.TimeZone).HasMaxLength(100);
                });
            });

            entity.HasOne<Tenant>()
                .WithMany(t => t.Queues)
                .HasForeignKey(q => q.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // UserSession configuration
        modelBuilder.Entity<UserSession>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Priority).HasConversion<int>();
            entity.Property(e => e.UserIdentifier).IsRequired().HasMaxLength(255);
            entity.Property(e => e.QueueId).IsRequired();
            entity.Property(e => e.Status).IsRequired().HasConversion<string>();
            entity.Property(e => e.EnqueuedAt).IsRequired();
            entity.Property(e => e.ReleasedAt);
            entity.Property(e => e.ServedAt);
            entity.Property(e => e.Position).IsRequired();
            entity.Property(e => e.Metadata).HasMaxLength(1000);

            entity.HasOne<Queue>()
                .WithMany(q => q.Users)
                .HasForeignKey(us => us.QueueId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.QueueId, e.UserIdentifier }).IsUnique();
        });

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(255);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Role).IsRequired().HasConversion<string>();
            entity.Property(e => e.Status).IsRequired().HasConversion<string>();
            entity.Property(e => e.LastLoginAt);
            entity.Property(e => e.EmailVerifiedAt);
            entity.Property(e => e.PhoneVerifiedAt);
            entity.Property(e => e.TwoFactorSecret).HasMaxLength(255);
            entity.Property(e => e.IsTwoFactorEnabled).IsRequired();
            entity.Property(e => e.RefreshToken).HasMaxLength(255);
            entity.Property(e => e.RefreshTokenExpiresAt);
            entity.Property(e => e.ProfileImageUrl).HasMaxLength(500);
            entity.Property(e => e.Metadata).HasMaxLength(2000);
            entity.Property(e => e.TenantId).IsRequired();

            entity.HasOne<Tenant>()
                .WithMany()
                .HasForeignKey(u => u.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.TenantId, e.Username }).IsUnique();
            entity.HasIndex(e => new { e.TenantId, e.Email }).IsUnique();
        });

        // QueueEvent configuration
        modelBuilder.Entity<QueueEvent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TenantId).IsRequired();
            entity.Property(e => e.QueueId).IsRequired();
            entity.Property(e => e.EventType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
            entity.Property(e => e.EventTimestamp).IsRequired();
            entity.Property(e => e.UserIdentifier).HasMaxLength(255);
            entity.Property(e => e.Metadata).HasMaxLength(2000);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(500);

            entity.HasOne<Queue>()
                .WithMany()
                .HasForeignKey(e => e.QueueId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.TenantId, e.EventTimestamp });
            entity.HasIndex(e => new { e.QueueId, e.EventType });
        });

        // NotificationTemplate configuration
        modelBuilder.Entity<NotificationTemplate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TenantId).IsRequired();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Type).IsRequired().HasConversion<string>();
            entity.Property(e => e.Subject).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Body).IsRequired().HasMaxLength(5000);
            entity.Property(e => e.Language).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Variables).HasMaxLength(1000);
            entity.Property(e => e.Category).HasMaxLength(50);
            entity.Property(e => e.Tags).HasMaxLength(200);

            entity.HasIndex(e => new { e.TenantId, e.Name }).IsUnique();
            entity.HasIndex(e => new { e.TenantId, e.Type, e.IsActive });
        });

        // AuditLog configuration
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TenantId).IsRequired();
            entity.Property(e => e.UserIdentifier).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
            entity.Property(e => e.EntityType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.EntityId).IsRequired();
            entity.Property(e => e.ActionTimestamp).IsRequired();
            entity.Property(e => e.OldValues).HasMaxLength(5000);
            entity.Property(e => e.NewValues).HasMaxLength(5000);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.Metadata).HasMaxLength(2000);
            entity.Property(e => e.Result).IsRequired().HasConversion<string>();
            entity.Property(e => e.ErrorMessage).HasMaxLength(1000);

            entity.HasIndex(e => new { e.TenantId, e.ActionTimestamp });
            entity.HasIndex(e => new { e.EntityType, e.EntityId });
            entity.HasIndex(e => e.UserIdentifier);
        });

        // ApiKey configuration
        modelBuilder.Entity<ApiKey>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TenantId).IsRequired();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Key).IsRequired().HasMaxLength(100);
            entity.Property(e => e.KeyHash).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Permissions).HasMaxLength(2000);
            entity.Property(e => e.IpRestrictions).HasMaxLength(1000);
            entity.Property(e => e.CreatedBy).HasMaxLength(255);

            entity.HasOne<Tenant>()
                .WithMany()
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.TenantId, e.Name }).IsUnique();
            entity.HasIndex(e => e.KeyHash).IsUnique();
        });

        // Integration configuration
        modelBuilder.Entity<Integration>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TenantId).IsRequired();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Type).IsRequired().HasConversion<string>();
            entity.Property(e => e.Provider).IsRequired().HasConversion<string>();
            entity.Property(e => e.Configuration).IsRequired().HasMaxLength(5000);
            entity.Property(e => e.Status).IsRequired().HasConversion<string>();
            entity.Property(e => e.LastTestResult).HasConversion<string>();
            entity.Property(e => e.CreatedBy).HasMaxLength(255);
            entity.Property(e => e.Metadata).HasMaxLength(2000);

            entity.HasOne<Tenant>()
                .WithMany()
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.TenantId, e.Name }).IsUnique();
            entity.HasIndex(e => new { e.TenantId, e.Type, e.IsActive });
        });

        // Webhook configuration
        modelBuilder.Entity<Webhook>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TenantId).IsRequired();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Url).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Events).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Headers).HasMaxLength(2000);
            entity.Property(e => e.SecretKey).HasMaxLength(255);
            entity.Property(e => e.LastDeliveryStatus).HasConversion<string>();
            entity.Property(e => e.CreatedBy).HasMaxLength(255);

            entity.HasOne<Tenant>()
                .WithMany()
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.TenantId, e.Name }).IsUnique();
            entity.HasIndex(e => new { e.TenantId, e.IsActive });
        });

        // Alert configuration
        modelBuilder.Entity<Alert>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TenantId).IsRequired();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Type).IsRequired().HasConversion<string>();
            entity.Property(e => e.Severity).IsRequired().HasConversion<string>();
            entity.Property(e => e.Condition).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Message).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.NotificationChannels).HasMaxLength(500);
            entity.Property(e => e.CreatedBy).HasMaxLength(255);
            entity.Property(e => e.Metadata).HasMaxLength(2000);

            entity.HasOne<Tenant>()
                .WithMany()
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.TenantId, e.Name }).IsUnique();
            entity.HasIndex(e => new { e.TenantId, e.Type, e.IsActive });
        });

        // Backup configuration
        modelBuilder.Entity<Backup>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Type).IsRequired().HasConversion<string>();
            entity.Property(e => e.Status).IsRequired().HasConversion<string>();
            entity.Property(e => e.Location).HasMaxLength(500);
            entity.Property(e => e.ErrorMessage).HasMaxLength(1000);
            entity.Property(e => e.CreatedBy).HasMaxLength(255);
            entity.Property(e => e.Metadata).HasMaxLength(2000);
            entity.Property(e => e.Checksum).HasMaxLength(255);

            entity.HasOne<Tenant>()
                .WithMany()
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => new { e.TenantId, e.Type, e.Status });
            entity.HasIndex(e => e.CreatedAt);
        });

        // Global query filters for soft delete
        modelBuilder.Entity<Tenant>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Queue>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<UserSession>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<User>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<QueueEvent>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<NotificationTemplate>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<AuditLog>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ApiKey>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Integration>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Webhook>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Alert>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Backup>().HasQueryFilter(e => !e.IsDeleted);
    }
}
