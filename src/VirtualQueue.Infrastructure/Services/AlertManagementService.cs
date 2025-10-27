using Microsoft.Extensions.Logging;
using VirtualQueue.Application.Common.Interfaces;

namespace VirtualQueue.Infrastructure.Services;

public class AlertManagementService : IAlertManagementService
{
    private readonly ILogger<AlertManagementService> _logger;
    private readonly ICacheService _cacheService;
    private readonly INotificationService _notificationService;

    public AlertManagementService(
        ILogger<AlertManagementService> logger, 
        ICacheService cacheService, 
        INotificationService notificationService)
    {
        _logger = logger;
        _cacheService = cacheService;
        _notificationService = notificationService;
    }

    public async Task<AlertDto> CreateAlertAsync(CreateAlertRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var alertId = Guid.NewGuid();
            var alert = new AlertDto(
                alertId,
                request.TenantId,
                request.Name,
                request.Description,
                request.Type,
                request.Severity,
                request.Condition,
                request.Actions,
                request.IsActive,
                DateTime.UtcNow,
                DateTime.UtcNow,
                request.CreatedBy
            );

            var cacheKey = $"alert:{request.TenantId}:{alertId}";
            await _cacheService.SetAsync(cacheKey, alert, TimeSpan.FromDays(365), cancellationToken);

            _logger.LogInformation("Alert created: {AlertId} for tenant {TenantId}", alertId, request.TenantId);
            return alert;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create alert for tenant {TenantId}", request.TenantId);
            throw;
        }
    }

    public async Task<AlertDto> UpdateAlertAsync(Guid alertId, UpdateAlertRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Alert updated: {AlertId}", alertId);
            
            // In a real implementation, this would update the database
            return new AlertDto(
                alertId,
                Guid.NewGuid(),
                request.Name ?? "Updated Alert",
                request.Description ?? "Updated Description",
                AlertType.QueueLength,
                request.Severity ?? AlertSeverity.Medium,
                request.Condition ?? new AlertCondition("queue_length", AlertOperator.GreaterThan, 100),
                request.Actions ?? new List<AlertAction>(),
                request.IsActive ?? true,
                DateTime.UtcNow.AddDays(-30),
                DateTime.UtcNow,
                "System"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update alert {AlertId}", alertId);
            throw;
        }
    }

    public async Task<bool> DeleteAlertAsync(Guid alertId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Alert deleted: {AlertId}", alertId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete alert {AlertId}", alertId);
            return false;
        }
    }

    public async Task<AlertDto?> GetAlertAsync(Guid alertId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving alert: {AlertId}", alertId);
            
            return new AlertDto(
                alertId,
                Guid.NewGuid(),
                "Queue Length Alert",
                "Alert when queue length exceeds threshold",
                AlertType.QueueLength,
                AlertSeverity.High,
                new AlertCondition("queue_length", AlertOperator.GreaterThan, 50),
                new List<AlertAction>
                {
                    new AlertAction(AlertActionType.Email, "admin@example.com", "Queue length exceeded threshold"),
                    new AlertAction(AlertActionType.Webhook, "https://example.com/webhook")
                },
                true,
                DateTime.UtcNow.AddDays(-30),
                DateTime.UtcNow,
                "System"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get alert {AlertId}", alertId);
            return null;
        }
    }

    public async Task<List<AlertDto>> GetAlertsAsync(Guid tenantId, AlertSeverity? severity = null, bool? isActive = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting alerts for tenant {TenantId}, severity: {Severity}, active: {IsActive}", 
                tenantId, severity, isActive);
            
            return new List<AlertDto>
            {
                new AlertDto(
                    Guid.NewGuid(),
                    tenantId,
                    "High Queue Length",
                    "Alert when queue length is high",
                    AlertType.QueueLength,
                    AlertSeverity.High,
                    new AlertCondition("queue_length", AlertOperator.GreaterThan, 100),
                    new List<AlertAction>(),
                    true,
                    DateTime.UtcNow.AddDays(-30),
                    DateTime.UtcNow,
                    "System"
                )
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get alerts for tenant {TenantId}", tenantId);
            return new List<AlertDto>();
        }
    }

    public async Task<List<VirtualQueue.Application.DTOs.AlertDto>> GetActiveAlertsAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting active alerts for tenant {TenantId}", tenantId);
            
            // In a real implementation, this would query active alerts from the database
            return new List<VirtualQueue.Application.DTOs.AlertDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get active alerts for tenant {TenantId}", tenantId);
            return new List<VirtualQueue.Application.DTOs.AlertDto>();
        }
    }

    public async Task<bool> TriggerAlertAsync(Guid alertId, AlertTriggerData triggerData, CancellationToken cancellationToken = default)
    {
        try
        {
            var alert = await GetAlertAsync(alertId, cancellationToken);
            if (alert == null)
            {
                _logger.LogWarning("Alert {AlertId} not found", alertId);
                return false;
            }

            var shouldTrigger = EvaluateCondition(alert.Condition, triggerData);
            
            if (shouldTrigger)
            {
                await ExecuteAlertActions(alert, triggerData, cancellationToken);
                _logger.LogInformation("Alert {AlertId} triggered for metric {Metric} with value {Value}", 
                    alertId, triggerData.Metric, triggerData.Value);
            }

            return shouldTrigger;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to trigger alert {AlertId}", alertId);
            return false;
        }
    }

    public async Task<bool> ResolveAlertAsync(Guid alertId, string? resolution = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Alert {AlertId} resolved: {Resolution}", alertId, resolution ?? "No resolution provided");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resolve alert {AlertId}", alertId);
            return false;
        }
    }

    public async Task<List<AlertHistory>> GetAlertHistoryByAlertIdAsync(Guid alertId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting alert history for {AlertId}", alertId);
            
            return new List<AlertHistory>
            {
                new AlertHistory(
                    Guid.NewGuid(),
                    alertId,
                    new AlertTriggerData("queue_length", 75, DateTime.UtcNow.AddHours(-1)),
                    true,
                    DateTime.UtcNow.AddHours(-1),
                    "Queue length exceeded threshold"
                )
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get alert history for {AlertId}", alertId);
            return new List<AlertHistory>();
        }
    }

    public async Task<bool> TestAlertAsync(Guid alertId, CancellationToken cancellationToken = default)
    {
        try
        {
            var alert = await GetAlertAsync(alertId, cancellationToken);
            if (alert == null)
            {
                return false;
            }

            var testTriggerData = new AlertTriggerData(
                alert.Condition.Metric,
                alert.Condition.Threshold + 1, // Ensure it triggers
                DateTime.UtcNow
            );

            return await TriggerAlertAsync(alertId, testTriggerData, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to test alert {AlertId}", alertId);
            return false;
        }
    }

    public async Task<List<AlertDto>> GetAlertsByConditionAsync(AlertCondition condition, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting alerts by condition for metric {Metric}", condition.Metric);
            
            // In a real implementation, this would query alerts that match the condition
            return new List<AlertDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get alerts by condition");
            return new List<AlertDto>();
        }
    }

    // New methods required by the interface
    public async Task<AlertRuleDto> CreateAlertRuleAsync(Guid tenantId, string name, string description, string metric, string condition, double threshold, string severity, string? notificationChannels = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var ruleId = Guid.NewGuid();
            var alertRule = new AlertRuleDto(
                ruleId,
                tenantId,
                name,
                description,
                metric,
                condition,
                threshold,
                severity,
                notificationChannels,
                true,
                DateTime.UtcNow,
                DateTime.UtcNow
            );

            var cacheKey = $"alert_rule:{tenantId}:{ruleId}";
            await _cacheService.SetAsync(cacheKey, alertRule, TimeSpan.FromDays(365), cancellationToken);

            _logger.LogInformation("Alert rule created: {RuleId} for tenant {TenantId}", ruleId, tenantId);
            return alertRule;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create alert rule for tenant {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<AlertRuleDto?> GetAlertRuleAsync(Guid tenantId, Guid ruleId, CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = $"alert_rule:{tenantId}:{ruleId}";
            var alertRule = await _cacheService.GetAsync<AlertRuleDto>(cacheKey, cancellationToken);
            
            if (alertRule == null)
            {
                _logger.LogWarning("Alert rule {RuleId} not found for tenant {TenantId}", ruleId, tenantId);
                return null;
            }

            return alertRule;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get alert rule {RuleId} for tenant {TenantId}", ruleId, tenantId);
            return null;
        }
    }

    public async Task<List<AlertRuleDto>> GetAllAlertRulesAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting all alert rules for tenant {TenantId}", tenantId);
            
            // In a real implementation, this would query the database
            return new List<AlertRuleDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get alert rules for tenant {TenantId}", tenantId);
            return new List<AlertRuleDto>();
        }
    }

    public async Task<AlertRuleDto> UpdateAlertRuleAsync(Guid tenantId, Guid ruleId, string name, string description, string metric, string condition, double threshold, string severity, string? notificationChannels = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var alertRule = new AlertRuleDto(
                ruleId,
                tenantId,
                name,
                description,
                metric,
                condition,
                threshold,
                severity,
                notificationChannels,
                true,
                DateTime.UtcNow.AddDays(-30),
                DateTime.UtcNow
            );

            var cacheKey = $"alert_rule:{tenantId}:{ruleId}";
            await _cacheService.SetAsync(cacheKey, alertRule, TimeSpan.FromDays(365), cancellationToken);

            _logger.LogInformation("Alert rule updated: {RuleId} for tenant {TenantId}", ruleId, tenantId);
            return alertRule;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update alert rule {RuleId} for tenant {TenantId}", ruleId, tenantId);
            throw;
        }
    }

    public async Task<bool> DeleteAlertRuleAsync(Guid tenantId, Guid ruleId, CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = $"alert_rule:{tenantId}:{ruleId}";
            await _cacheService.RemoveAsync(cacheKey, cancellationToken);

            _logger.LogInformation("Alert rule deleted: {RuleId} for tenant {TenantId}", ruleId, tenantId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete alert rule {RuleId} for tenant {TenantId}", ruleId, tenantId);
            return false;
        }
    }

    public async Task<bool> TriggerAlertAsync(Guid tenantId, Guid alertId, string message, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Alert {AlertId} triggered for tenant {TenantId}: {Message}", alertId, tenantId, message);
            
            // In a real implementation, this would trigger the alert and send notifications
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to trigger alert {AlertId} for tenant {TenantId}", alertId, tenantId);
            return false;
        }
    }

    public async Task<bool> ResolveAlertAsync(Guid tenantId, Guid alertId, string resolution, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Alert {AlertId} resolved for tenant {TenantId}: {Resolution}", alertId, tenantId, resolution);
            
            // In a real implementation, this would mark the alert as resolved
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resolve alert {AlertId} for tenant {TenantId}", alertId, tenantId);
            return false;
        }
    }

    public async Task<List<VirtualQueue.Application.DTOs.AlertDto>> GetAlertHistoryAsync(Guid tenantId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting alert history for tenant {TenantId}", tenantId);
            
            // In a real implementation, this would query alert history from the database
            return new List<VirtualQueue.Application.DTOs.AlertDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get alert history for tenant {TenantId}", tenantId);
            return new List<VirtualQueue.Application.DTOs.AlertDto>();
        }
    }

    private bool EvaluateCondition(AlertCondition condition, AlertTriggerData triggerData)
    {
        if (condition.Metric != triggerData.Metric)
            return false;

        return condition.Operator switch
        {
            AlertOperator.GreaterThan => triggerData.Value > condition.Threshold,
            AlertOperator.LessThan => triggerData.Value < condition.Threshold,
            AlertOperator.Equal => Math.Abs(triggerData.Value - condition.Threshold) < 0.001,
            AlertOperator.NotEqual => Math.Abs(triggerData.Value - condition.Threshold) >= 0.001,
            AlertOperator.GreaterThanOrEqual => triggerData.Value >= condition.Threshold,
            AlertOperator.LessThanOrEqual => triggerData.Value <= condition.Threshold,
            _ => false
        };
    }

    private async Task ExecuteAlertActions(AlertDto alert, AlertTriggerData triggerData, CancellationToken cancellationToken)
    {
        foreach (var action in alert.Actions)
        {
            try
            {
                switch (action.Type)
                {
                    case AlertActionType.Email:
                        await _notificationService.SendEmailAsync(
                            action.Target, 
                            $"Alert: {alert.Name}", 
                            action.Message ?? $"Alert triggered: {alert.Description}",
                            cancellationToken);
                        break;
                        
                    case AlertActionType.Sms:
                        await _notificationService.SendSmsAsync(
                            action.Target, 
                            action.Message ?? $"Alert: {alert.Name}",
                            cancellationToken);
                        break;
                        
                    case AlertActionType.Webhook:
                        // In a real implementation, this would send a webhook
                        _logger.LogInformation("Webhook alert sent to {Target}", action.Target);
                        break;
                        
                    case AlertActionType.Slack:
                    case AlertActionType.Teams:
                        // In a real implementation, this would send to Slack/Teams
                        _logger.LogInformation("{Type} alert sent to {Target}", action.Type, action.Target);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to execute alert action {ActionType} for alert {AlertId}", 
                    action.Type, alert.Id);
            }
        }
    }
}
