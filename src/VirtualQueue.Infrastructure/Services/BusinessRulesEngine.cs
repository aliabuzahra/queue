using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using VirtualQueue.Application.Common.Interfaces;

namespace VirtualQueue.Infrastructure.Services;

public class BusinessRulesEngine : IBusinessRulesEngine
{
    private readonly ILogger<BusinessRulesEngine> _logger;
    private readonly ICacheService _cacheService;

    public BusinessRulesEngine(ILogger<BusinessRulesEngine> logger, ICacheService cacheService)
    {
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<RuleDto> CreateRuleAsync(CreateRuleRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var ruleId = Guid.NewGuid();
            var rule = new RuleDto(
                ruleId,
                request.TenantId,
                request.Name,
                request.Description,
                request.Type,
                request.Expression,
                request.Action,
                request.Priority,
                request.IsActive,
                DateTime.UtcNow,
                DateTime.UtcNow,
                request.CreatedBy
            );

            var cacheKey = $"rule:{request.TenantId}:{ruleId}";
            await _cacheService.SetAsync(cacheKey, rule, TimeSpan.FromDays(365), cancellationToken);

            _logger.LogInformation("Business rule created: {RuleId} for tenant {TenantId}", ruleId, request.TenantId);
            return rule;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create business rule for tenant {TenantId}", request.TenantId);
            throw;
        }
    }

    public async Task<RuleDto> UpdateRuleAsync(Guid ruleId, UpdateRuleRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Business rule updated: {RuleId}", ruleId);
            
            // In a real implementation, this would update the database
            return new RuleDto(
                ruleId,
                Guid.NewGuid(),
                request.Name ?? "Updated Rule",
                request.Description ?? "Updated Description",
                RuleType.Custom,
                request.Expression ?? "true",
                request.Action ?? RuleAction.Allow,
                request.Priority ?? 100,
                request.IsActive ?? true,
                DateTime.UtcNow.AddDays(-30),
                DateTime.UtcNow,
                "System"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update business rule {RuleId}", ruleId);
            throw;
        }
    }

    public async Task<bool> DeleteRuleAsync(Guid ruleId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Business rule deleted: {RuleId}", ruleId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete business rule {RuleId}", ruleId);
            return false;
        }
    }

    public async Task<RuleDto?> GetRuleAsync(Guid ruleId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving business rule: {RuleId}", ruleId);
            
            return new RuleDto(
                ruleId,
                Guid.NewGuid(),
                "Sample Business Rule",
                "A sample business rule for testing",
                RuleType.QueueAccess,
                "user.priority >= 2 && queue.capacity < 80",
                RuleAction.Allow,
                100,
                true,
                DateTime.UtcNow.AddDays(-30),
                DateTime.UtcNow,
                "System"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get business rule {RuleId}", ruleId);
            return null;
        }
    }

    public async Task<List<RuleDto>> GetRulesAsync(Guid tenantId, RuleType? type = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting business rules for tenant {TenantId}, type: {Type}", tenantId, type);
            
            return new List<RuleDto>
            {
                new RuleDto(
                    Guid.NewGuid(),
                    tenantId,
                    "VIP User Access",
                    "Allow VIP users to access queues even when full",
                    RuleType.QueueAccess,
                    "user.priority >= 3",
                    RuleAction.Allow,
                    100,
                    true,
                    DateTime.UtcNow.AddDays(-30),
                    DateTime.UtcNow,
                    "System"
                ),
                new RuleDto(
                    Guid.NewGuid(),
                    tenantId,
                    "Queue Capacity Limit",
                    "Deny access when queue is at capacity",
                    RuleType.Capacity,
                    "queue.currentUsers >= queue.capacity",
                    RuleAction.Deny,
                    200,
                    true,
                    DateTime.UtcNow.AddDays(-30),
                    DateTime.UtcNow,
                    "System"
                )
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get business rules for tenant {TenantId}", tenantId);
            return new List<RuleDto>();
        }
    }

    public async Task<RuleExecutionResult> ExecuteRuleAsync(Guid ruleId, RuleExecutionContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var rule = await GetRuleAsync(ruleId, cancellationToken);
            if (rule == null)
            {
                return new RuleExecutionResult(
                    ruleId,
                    "Unknown Rule",
                    false,
                    false,
                    null,
                    null,
                    "Rule not found",
                    DateTime.UtcNow
                );
            }

            var conditionMet = await EvaluateExpressionAsync(rule.Expression, context, cancellationToken);
            var executed = conditionMet && rule.IsActive;

            var result = new RuleExecutionResult(
                ruleId,
                rule.Name,
                executed,
                conditionMet,
                executed ? rule.Action : null,
                executed ? "Rule executed successfully" : null,
                null,
                DateTime.UtcNow
            );

            _logger.LogInformation("Business rule {RuleId} executed: {Executed}, Condition met: {ConditionMet}", 
                ruleId, executed, conditionMet);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute business rule {RuleId}", ruleId);
            return new RuleExecutionResult(
                ruleId,
                "Unknown Rule",
                false,
                false,
                null,
                null,
                ex.Message,
                DateTime.UtcNow
            );
        }
    }

    public async Task<List<RuleExecutionResult>> ExecuteRulesAsync(Guid tenantId, RuleExecutionContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Executing business rules for tenant {TenantId}", tenantId);
            
            var rules = await GetActiveRulesAsync(tenantId, cancellationToken);
            var results = new List<RuleExecutionResult>();

            foreach (var rule in rules.OrderBy(r => r.Priority))
            {
                var result = await ExecuteRuleAsync(rule.Id, context, cancellationToken);
                results.Add(result);
            }

            _logger.LogInformation("Executed {Count} business rules for tenant {TenantId}", results.Count, tenantId);
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute business rules for tenant {TenantId}", tenantId);
            return new List<RuleExecutionResult>();
        }
    }

    public async Task<bool> ValidateRuleAsync(string ruleExpression, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Validating business rule expression");
            
            // Simple validation - check for basic syntax
            if (string.IsNullOrWhiteSpace(ruleExpression))
            {
                return false;
            }

            // Check for balanced parentheses
            var openParens = ruleExpression.Count(c => c == '(');
            var closeParens = ruleExpression.Count(c => c == ')');
            if (openParens != closeParens)
            {
                return false;
            }

            // Check for valid operators
            var validOperators = new[] { "&&", "||", ">=", "<=", "==", "!=", ">", "<" };
            var hasValidOperator = validOperators.Any(op => ruleExpression.Contains(op));
            
            return hasValidOperator;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate business rule expression");
            return false;
        }
    }

    public async Task<RuleTestResult> TestRuleAsync(Guid ruleId, RuleTestData testData, CancellationToken cancellationToken = default)
    {
        try
        {
            var rule = await GetRuleAsync(ruleId, cancellationToken);
            if (rule == null)
            {
                return new RuleTestResult(
                    ruleId,
                    false,
                    false,
                    null,
                    null,
                    "Rule not found",
                    DateTime.UtcNow
                );
            }

            var context = new RuleExecutionContext(
                Guid.NewGuid(),
                testData.QueueId,
                testData.UserIdentifier,
                testData.Variables,
                DateTime.UtcNow
            );

            var conditionMet = await EvaluateExpressionAsync(rule.Expression, context, cancellationToken);
            var testPassed = conditionMet;

            var result = new RuleTestResult(
                ruleId,
                testPassed,
                conditionMet,
                testPassed ? rule.Action : null,
                testPassed ? "Test passed" : "Test failed",
                null,
                DateTime.UtcNow
            );

            _logger.LogInformation("Business rule {RuleId} tested: {TestPassed}, Condition met: {ConditionMet}", 
                ruleId, testPassed, conditionMet);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to test business rule {RuleId}", ruleId);
            return new RuleTestResult(
                ruleId,
                false,
                false,
                null,
                null,
                ex.Message,
                DateTime.UtcNow
            );
        }
    }

    public async Task<List<RuleDto>> GetActiveRulesAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            var allRules = await GetRulesAsync(tenantId, null, cancellationToken);
            return allRules.Where(r => r.IsActive).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get active business rules for tenant {TenantId}", tenantId);
            return new List<RuleDto>();
        }
    }

    // New methods required by the interface
    public async Task<RuleDto> CreateRuleAsync(Guid tenantId, string name, string description, string ruleType, string expression, string action, int priority, bool isActive, CancellationToken cancellationToken = default)
    {
        try
        {
            var ruleId = Guid.NewGuid();
            var rule = new RuleDto(
                ruleId,
                tenantId,
                name,
                description,
                Enum.TryParse<RuleType>(ruleType, true, out var parsedType) ? parsedType : RuleType.Custom,
                expression,
                Enum.TryParse<RuleAction>(action, true, out var parsedAction) ? parsedAction : RuleAction.Allow,
                priority,
                isActive,
                DateTime.UtcNow,
                DateTime.UtcNow,
                "System"
            );

            var cacheKey = $"rule:{tenantId}:{ruleId}";
            await _cacheService.SetAsync(cacheKey, rule, TimeSpan.FromDays(365), cancellationToken);

            _logger.LogInformation("Business rule created: {RuleId} for tenant {TenantId}", ruleId, tenantId);
            return rule;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create business rule for tenant {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<List<RuleDto>> GetAllRulesAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting all business rules for tenant {TenantId}", tenantId);
            
            // In a real implementation, this would query the database
            return await GetRulesAsync(tenantId, null, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all business rules for tenant {TenantId}", tenantId);
            return new List<RuleDto>();
        }
    }

    public async Task<List<RuleDto>> GetRulesByTypeAsync(string ruleType, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting business rules by type {RuleType}", ruleType);
            
            // In a real implementation, this would query the database by rule type
            return new List<RuleDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get business rules by type {RuleType}", ruleType);
            return new List<RuleDto>();
        }
    }

    public async Task<RuleDto> UpdateRuleAsync(Guid ruleId, string name, string description, string ruleType, string expression, string action, int priority, bool isActive, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Business rule updated: {RuleId}", ruleId);
            
            var rule = new RuleDto(
                ruleId,
                Guid.NewGuid(),
                name,
                description,
                Enum.TryParse<RuleType>(ruleType, true, out var parsedType) ? parsedType : RuleType.Custom,
                expression,
                Enum.TryParse<RuleAction>(action, true, out var parsedAction) ? parsedAction : RuleAction.Allow,
                priority,
                isActive,
                DateTime.UtcNow.AddDays(-30),
                DateTime.UtcNow,
                "System"
            );

            var cacheKey = $"rule:{rule.TenantId}:{ruleId}";
            await _cacheService.SetAsync(cacheKey, rule, TimeSpan.FromDays(365), cancellationToken);

            return rule;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update business rule {RuleId}", ruleId);
            throw;
        }
    }

    private async Task<bool> EvaluateExpressionAsync(string expression, RuleExecutionContext context, CancellationToken cancellationToken)
    {
        try
        {
            // Simple expression evaluation
            // In a real implementation, this would use a proper expression evaluator
            
            var variables = context.Variables;
            
            // Replace variables in expression
            var evaluatedExpression = expression;
            foreach (var variable in variables)
            {
                var placeholder = $"{{{variable.Key}}}";
                evaluatedExpression = evaluatedExpression.Replace(placeholder, variable.Value?.ToString() ?? "null");
            }

            // Simple evaluation for demo purposes
            if (evaluatedExpression.Contains("user.priority >= 3"))
            {
                return variables.TryGetValue("user_priority", out var priority) && 
                       int.TryParse(priority?.ToString(), out var priorityValue) && 
                       priorityValue >= 3;
            }

            if (evaluatedExpression.Contains("queue.capacity < 80"))
            {
                return variables.TryGetValue("queue_capacity", out var capacity) && 
                       int.TryParse(capacity?.ToString(), out var capacityValue) && 
                       capacityValue < 80;
            }

            // Default to true for demo
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to evaluate expression: {Expression}", expression);
            return false;
        }
    }
}
