namespace VirtualQueue.Application.DTOs;

public record MobileQueueStatusDto(
    Guid QueueId,
    string QueueName,
    int Position,
    int TotalUsers,
    TimeSpan EstimatedWaitTime,
    string Status,
    DateTime EnqueuedAt,
    MobileQueueSettings Settings);

public record MobileQueueSettings(
    bool EnableNotifications,
    bool EnableLocationTracking,
    string NotificationFrequency,
    bool AllowOfflineMode,
    Dictionary<string, string> CustomSettings);

public record MobileUserSessionDto(
    Guid SessionId,
    string UserIdentifier,
    Guid QueueId,
    string QueueName,
    int Position,
    string Status,
    DateTime EnqueuedAt,
    TimeSpan? EstimatedWaitTime,
    MobileUserPreferences Preferences);

public record MobileUserPreferences(
    string Language,
    string TimeZone,
    bool EnablePushNotifications,
    bool EnableSMSNotifications,
    bool EnableEmailNotifications,
    string NotificationSound,
    Dictionary<string, string> CustomPreferences);

public record MobileNotificationDto(
    Guid Id,
    string Title,
    string Message,
    string Type,
    string Priority,
    DateTime CreatedAt,
    bool IsRead,
    string? ActionUrl = null,
    Dictionary<string, string>? Metadata = null);

public record MobileOfflineDataDto(
    Guid TenantId,
    List<MobileQueueStatusDto> Queues,
    List<MobileUserSessionDto> UserSessions,
    DateTime LastSyncAt,
    bool HasOfflineChanges);

public record MobileSyncRequest(
    Guid TenantId,
    DateTime? LastSyncAt = null,
    List<string>? DataTypes = null);

public record MobileSyncResponse(
    bool Success,
    DateTime SyncTime,
    List<MobileQueueStatusDto> UpdatedQueues,
    List<MobileUserSessionDto> UpdatedSessions,
    List<MobileNotificationDto> NewNotifications,
    bool HasMoreData);

public record MobileLocationUpdateDto(
    Guid TenantId,
    double Latitude,
    double Longitude,
    double? Accuracy = null,
    DateTime? Timestamp = null);

public record MobileGeofenceDto(
    Guid Id,
    string Name,
    double Latitude,
    double Longitude,
    double Radius,
    bool IsActive,
    Dictionary<string, string>? Metadata = null);

public record MobilePushTokenDto(
    string Token,
    string Platform,
    string DeviceId,
    bool IsActive,
    DateTime LastUpdated);

public record MobileDeviceInfoDto(
    string DeviceId,
    string Platform,
    string AppVersion,
    string OSVersion,
    string DeviceModel,
    Dictionary<string, string>? Capabilities = null);

