using SmartCommunityApi.DTOs;

namespace SmartCommunityApi.Services;

/// <summary>
/// 通知服務（目前為 Stub 實作，待未來整合推播或 Email）
/// </summary>
public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(ILogger<NotificationService> logger)
    {
        _logger = logger;
    }

    public Task SendNotificationAsync(int userId, string message)
    {
        _logger.LogInformation("[Notification] UserId={UserId} Message={Message}", userId, message);
        return Task.CompletedTask;
    }
}
