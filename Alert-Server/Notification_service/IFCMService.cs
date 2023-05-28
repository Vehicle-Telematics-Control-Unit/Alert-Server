namespace Alert_Server.Notification_service
{
    public interface IFCMService
    {
        Task<dynamic> SendNotificationAsync(string notificationToken);
    }
}
