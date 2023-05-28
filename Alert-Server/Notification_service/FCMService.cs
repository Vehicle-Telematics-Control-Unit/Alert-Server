using Alert_Server.Notification_service;
using FirebaseAdmin.Messaging;
using System.Configuration;

namespace Alert_Server.Notification_service
{
    public class FCMService : IFCMService
    {
        public async Task<dynamic> SendNotificationAsync(string notificationToken)
        {
            var _message = new Message()
            {
                Notification = new Notification
                {
                    Title = "Test Notification",
                    Body = "This is a test notification"
                },
                Token = notificationToken
            };
            try
            {
                var messageId = await FirebaseMessaging.DefaultInstance.SendAsync(_message);
                return messageId;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }


        }
    }
}
