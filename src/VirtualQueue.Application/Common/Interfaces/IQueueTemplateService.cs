using VirtualQueue.Application.DTOs;

namespace VirtualQueue.Application.Common.Interfaces;

public interface IQueueTemplateService
{
    // Template Management
    Task<QueueTemplateDto> CreateTemplateAsync(CreateQueueTemplateRequest request, CancellationToken cancellationToken = default);
    Task<QueueTemplateDto?> GetTemplateAsync(Guid templateId, CancellationToken cancellationToken = default);
    Task<List<QueueTemplateDto>> GetTemplatesAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<List<QueueTemplateDto>> GetPublicTemplatesAsync(CancellationToken cancellationToken = default);
    Task<List<QueueTemplateDto>> SearchTemplatesAsync(QueueTemplateSearchRequest request, CancellationToken cancellationToken = default);
    Task UpdateTemplateAsync(Guid templateId, UpdateQueueTemplateRequest request, CancellationToken cancellationToken = default);
    Task DeleteTemplateAsync(Guid templateId, CancellationToken cancellationToken = default);
    
    // Template Operations
    Task<QueueDto> CreateQueueFromTemplateAsync(Guid tenantId, QueueTemplateUsageRequest request, CancellationToken cancellationToken = default);
    Task<QueueTemplateDto> CloneTemplateAsync(Guid templateId, string newName, CancellationToken cancellationToken = default);
    Task SetTemplateVisibilityAsync(Guid templateId, bool isPublic, CancellationToken cancellationToken = default);
    Task ActivateTemplateAsync(Guid templateId, CancellationToken cancellationToken = default);
    Task DeactivateTemplateAsync(Guid templateId, CancellationToken cancellationToken = default);
    
    // Template Analytics
    Task<Dictionary<string, int>> GetTemplateStatisticsAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<List<QueueTemplateDto>> GetMostUsedTemplatesAsync(Guid tenantId, int count = 10, CancellationToken cancellationToken = default);
    Task<List<QueueTemplateDto>> GetRecentlyCreatedTemplatesAsync(Guid tenantId, int count = 10, CancellationToken cancellationToken = default);
}

