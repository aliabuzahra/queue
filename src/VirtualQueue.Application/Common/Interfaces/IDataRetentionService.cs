namespace VirtualQueue.Application.Common.Interfaces;

public interface IDataRetentionService
{
    // Methods expected by controllers
    Task<RetentionPolicyDto> CreateRetentionPolicyAsync(Guid tenantId, string name, string description, CancellationToken cancellationToken = default);
    Task<RetentionPolicyDto?> GetRetentionPolicyByEntityTypeAsync(string entityType, CancellationToken cancellationToken = default);
    Task<List<RetentionPolicyDto>> GetAllRetentionPoliciesAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<RetentionPolicyDto> UpdateRetentionPolicyAsync(Guid policyId, string name, string description, CancellationToken cancellationToken = default);
    Task<bool> DeleteRetentionPolicyAsync(Guid policyId, CancellationToken cancellationToken = default);
    Task<bool> ApplyRetentionPolicyAsync(Guid policyId, CancellationToken cancellationToken = default);
    Task<bool> ApplyAllRetentionPoliciesAsync(Guid tenantId, CancellationToken cancellationToken = default);
    
    // Original methods
    Task<RetentionPolicyDto> CreateRetentionPolicyAsync(CreateRetentionPolicyRequest request, CancellationToken cancellationToken = default);
    Task<RetentionPolicyDto> UpdateRetentionPolicyAsync(Guid policyId, UpdateRetentionPolicyRequest request, CancellationToken cancellationToken = default);
    Task<RetentionPolicyDto?> GetRetentionPolicyAsync(Guid policyId, CancellationToken cancellationToken = default);
    Task<List<RetentionPolicyDto>> GetRetentionPoliciesAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<RetentionExecutionResult> ExecuteRetentionPolicyAsync(Guid policyId, CancellationToken cancellationToken = default);
    Task<RetentionExecutionResult> ExecuteAllRetentionPoliciesAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<RetentionStatistics> GetRetentionStatisticsAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<bool> ScheduleRetentionPolicyAsync(Guid policyId, RetentionSchedule schedule, CancellationToken cancellationToken = default);
    Task<List<RetentionExecution>> GetRetentionExecutionsAsync(Guid policyId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
}

public record RetentionPolicyDto(
    Guid Id,
    Guid TenantId,
    string Name,
    string Description,
    RetentionEntityType EntityType,
    TimeSpan RetentionPeriod,
    RetentionAction Action,
    Dictionary<string, object>? Criteria,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string? CreatedBy
);

public record CreateRetentionPolicyRequest(
    Guid TenantId,
    string Name,
    string Description,
    RetentionEntityType EntityType,
    TimeSpan RetentionPeriod,
    RetentionAction Action,
    Dictionary<string, object>? Criteria = null,
    bool IsActive = true,
    string? CreatedBy = null
);

public record UpdateRetentionPolicyRequest(
    string? Name,
    string? Description,
    TimeSpan? RetentionPeriod,
    RetentionAction? Action,
    Dictionary<string, object>? Criteria,
    bool? IsActive
);

public record RetentionExecutionResult(
    Guid PolicyId,
    bool Success,
    int RecordsProcessed,
    int RecordsDeleted,
    int RecordsArchived,
    TimeSpan Duration,
    DateTime ExecutedAt,
    string? ErrorMessage = null
);

public record RetentionStatistics(
    Guid TenantId,
    int TotalPolicies,
    int ActivePolicies,
    int TotalRecords,
    int RecordsToBeDeleted,
    int RecordsToBeArchived,
    DateTime LastExecution,
    Dictionary<string, int> RecordsByType
);

public record RetentionSchedule(
    Guid PolicyId,
    TimeSpan Interval,
    DateTime? NextExecution,
    bool IsActive
);

public record RetentionExecution(
    Guid Id,
    Guid PolicyId,
    RetentionExecutionResult Result,
    DateTime ExecutedAt,
    string? ExecutedBy = null
);

public enum RetentionEntityType
{
    UserSessions,
    AuditLogs,
    Analytics,
    Notifications,
    Webhooks,
    Alerts
}

public enum RetentionAction
{
    Delete,
    Archive,
    Compress
}
