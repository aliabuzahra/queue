using VirtualQueue.Domain.Common;
using VirtualQueue.Domain.Events;

namespace VirtualQueue.Domain.Entities;

public class QueueTemplate : BaseEntity
{
    public Guid TenantId { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string TemplateType { get; private set; }
    public int MaxConcurrentUsers { get; private set; }
    public int ReleaseRatePerMinute { get; private set; }
    public string? ScheduleJson { get; private set; }
    public string? BusinessRulesJson { get; private set; }
    public string? NotificationSettingsJson { get; private set; }
    public bool IsPublic { get; private set; }
    public bool IsActive { get; private set; }
    public int UsageCount { get; private set; }
    public Dictionary<string, string> Metadata { get; private set; }

    private QueueTemplate() { } // EF Core constructor

    public QueueTemplate(
        Guid tenantId,
        string name,
        string description,
        string templateType,
        int maxConcurrentUsers,
        int releaseRatePerMinute,
        string? scheduleJson = null,
        string? businessRulesJson = null,
        string? notificationSettingsJson = null,
        bool isPublic = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty", nameof(name));
        
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be null or empty", nameof(description));
        
        if (string.IsNullOrWhiteSpace(templateType))
            throw new ArgumentException("Template type cannot be null or empty", nameof(templateType));

        TenantId = tenantId;
        Name = name;
        Description = description;
        TemplateType = templateType;
        MaxConcurrentUsers = maxConcurrentUsers;
        ReleaseRatePerMinute = releaseRatePerMinute;
        ScheduleJson = scheduleJson;
        BusinessRulesJson = businessRulesJson;
        NotificationSettingsJson = notificationSettingsJson;
        IsPublic = isPublic;
        IsActive = true;
        UsageCount = 0;
        Metadata = new Dictionary<string, string>();

        AddDomainEvent(new QueueTemplateCreatedEvent(Id, TenantId, Name, TemplateType));
    }

    public void UpdateTemplate(
        string name,
        string description,
        int maxConcurrentUsers,
        int releaseRatePerMinute,
        string? scheduleJson = null,
        string? businessRulesJson = null,
        string? notificationSettingsJson = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty", nameof(name));
        
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be null or empty", nameof(description));

        Name = name;
        Description = description;
        MaxConcurrentUsers = maxConcurrentUsers;
        ReleaseRatePerMinute = releaseRatePerMinute;
        ScheduleJson = scheduleJson;
        BusinessRulesJson = businessRulesJson;
        NotificationSettingsJson = notificationSettingsJson;

        AddDomainEvent(new QueueTemplateUpdatedEvent(Id, TenantId, Name));
    }

    public void SetPublic(bool isPublic)
    {
        if (IsPublic == isPublic) return;

        IsPublic = isPublic;
        AddDomainEvent(new QueueTemplateVisibilityChangedEvent(Id, TenantId, Name, isPublic));
    }

    public void Activate()
    {
        if (IsActive) return;

        IsActive = true;
        AddDomainEvent(new QueueTemplateActivatedEvent(Id, TenantId, Name));
    }

    public void Deactivate()
    {
        if (!IsActive) return;

        IsActive = false;
        AddDomainEvent(new QueueTemplateDeactivatedEvent(Id, TenantId, Name));
    }

    public void IncrementUsage()
    {
        UsageCount++;
        AddDomainEvent(new QueueTemplateUsedEvent(Id, TenantId, Name, UsageCount));
    }

    public void UpdateMetadata(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Metadata key cannot be null or empty", nameof(key));

        Metadata[key] = value;
    }

    public void RemoveMetadata(string key)
    {
        if (Metadata.ContainsKey(key))
        {
            Metadata.Remove(key);
        }
    }
}
