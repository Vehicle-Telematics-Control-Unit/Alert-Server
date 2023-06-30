using Alert_Server.Models;

namespace Alert_Server.Services
{
    public interface IFCMService
    {
        Task<dynamic> SendNotificationAsync(NotificationModel notificationModel);
    }
}
