namespace VirtualQueue.Application.Common.Interfaces;

public interface IBusinessRulesEngine
{
    // Methods expected by controllers
    Task<RuleDto> CreateRuleAsync(Guid tenantId, string name, string description, string ruleType, string expression, string action, int priority, bool isActive, CancellationToken cancellationToken = default);
    Task<RuleDto?> GetRuleAsync(Guid ruleId, CancellationToken cancellationToken = default);
    Task<List<RuleDto>> GetAllRulesAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<List<RuleDto>> GetRulesByTypeAsync(string ruleType, CancellationToken cancellationToken = default);
    Task<RuleDto> UpdateRuleAsync(Guid ruleId, string name, string description, string ruleType, string expression, string action, int priority, bool isActive, CancellationToken cancellationToken = default);
    Task<bool> DeleteRuleAsync(Guid ruleId, CancellationToken cancellationToken = default);
    Task<bool> ValidateRuleAsync(string ruleExpression, CancellationToken cancellationToken = default);
    
    // Original methods
    Task<RuleDto> CreateRuleAsync(CreateRuleRequest request, CancellationToken cancellationToken = default);
    Task<RuleDto> UpdateRuleAsync(Guid ruleId, UpdateRuleRequest request, CancellationToken cancellationToken = default);
    Task<List<RuleDto>> GetRulesAsync(Guid tenantId, RuleType? type = null, CancellationToken cancellationToken = default);
    Task<RuleExecutionResult> ExecuteRuleAsync(Guid ruleId, RuleExecutionContext context, CancellationToken cancellationToken = default);
    Task<List<RuleExecutionResult>> ExecuteRulesAsync(Guid tenantId, RuleExecutionContext context, CancellationToken cancellationToken = default);
    Task<RuleTestResult> TestRuleAsync(Guid ruleId, RuleTestData testData, CancellationToken cancellationToken = default);
    Task<List<RuleDto>> GetActiveRulesAsync(Guid tenantId, CancellationToken cancellationToken = default);
}

public record RuleDto(
    Guid Id,
    Guid TenantId,
    string Name,
    string Description,
    RuleType Type,
    string Expression,
    RuleAction Action,
    int Priority,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string? CreatedBy
);

public record CreateRuleRequest(
    Guid TenantId,
    string Name,
    string Description,
    RuleType Type,
    string Expression,
    RuleAction Action,
    int Priority = 100,
    bool IsActive = true,
    string? CreatedBy = null
);

public record UpdateRuleRequest(
    string? Name,
    string? Description,
    string? Expression,
    RuleAction? Action,
    int? Priority,
    bool? IsActive
);

public record RuleExecutionContext(
    Guid TenantId,
    Guid? QueueId,
    string? UserIdentifier,
    Dictionary<string, object> Variables,
    DateTime Timestamp
);

public record RuleExecutionResult(
    Guid RuleId,
    string RuleName,
    bool Executed,
    bool ConditionMet,
    RuleAction? Action,
    object? Result,
    string? ErrorMessage,
    DateTime ExecutedAt
);

public record RuleTestResult(
    Guid RuleId,
    bool TestPassed,
    bool ConditionMet,
    RuleAction? Action,
    object? Result,
    string? ErrorMessage,
    DateTime TestedAt
);

public record RuleTestData(
    Dictionary<string, object> Variables,
    Guid? QueueId,
    string? UserIdentifier
);

public enum RuleType
{
    QueueAccess,
    UserPriority,
    Notification,
    Scheduling,
    Capacity,
    Custom
}

public enum RuleAction
{
    Allow,
    Deny,
    SetPriority,
    SendNotification,
    ScheduleAction,
    ModifyCapacity,
    CustomAction
}
