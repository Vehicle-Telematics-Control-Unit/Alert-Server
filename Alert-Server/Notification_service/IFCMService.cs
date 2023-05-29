using Alert_Server.Models;

namespace Alert_Server.Notification_service
{
    public interface IFCMService
    {
        Task<dynamic> SendNotificationAsync(NotificationModel notificationModel);
    }
}
