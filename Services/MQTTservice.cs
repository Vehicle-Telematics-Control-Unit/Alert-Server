using MQTTnet;
using MQTTnet.Client;
using Newtonsoft.Json.Linq;
using System.Text;
using Alert_Server.Models;
using Microsoft.EntityFrameworkCore;

namespace Alert_Server.Services
{
    public class MQTTservice : BackgroundService
    {
        private readonly IMqttClient _mqttClient;
        private readonly IFCMService _service;
        private readonly TCUContext _tcuContext;
        private readonly IConfiguration _config;
        public MQTTservice(IMqttClient mqttClient, IFCMService fCMService, IConfiguration config)
        {
            _mqttClient = mqttClient;
            _config = config;
            var connectionString = config.GetConnectionString("TcuServerConnection");
            var options = new DbContextOptionsBuilder<TCUContext>()
               .UseNpgsql(connectionString)
               .Options;
            _tcuContext = new TCUContext(options);
            _service = fCMService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var server = _config.GetSection("MQTT:server").Value ?? "127.0.0.1";
            int port = int.Parse(_config.GetSection("MQTT:port").Value ?? "1883");
            var options = new MqttClientOptionsBuilder();
            options.WithTcpServer(server, port); // Replace with your MQTT broker details
            await _mqttClient.ConnectAsync(options.Build(), stoppingToken);

            _mqttClient.ApplicationMessageReceivedAsync += e => { 
                return HandleReceivedMessage(e); 
            };

            await SubscribeToTopics(stoppingToken);
        }

        private async Task SubscribeToTopics(CancellationToken stoppingToken)
        {
            MqttFactory mqttFactory = new();
            var mqttSubscribeOptionsBuilder = mqttFactory.CreateSubscribeOptionsBuilder()
            .WithTopicFilter(
                f =>
                {
                    f.WithTopic("Server-TCU/WakeUp");
                });
            // Subscribe to topics and handle received messages
            await _mqttClient.SubscribeAsync(mqttSubscribeOptionsBuilder.Build(), stoppingToken);
        }

        private async Task<bool> HandleReceivedMessage(MqttApplicationMessageReceivedEventArgs eventArgs)
        {
            // Process the received message based on the topic
            MqttApplicationMessage appMessage = eventArgs.ApplicationMessage;
            var topic = appMessage.Topic;
            var payload = Encoding.UTF8.GetString(appMessage.PayloadSegment);
            JObject jsonPayload = JObject.Parse(payload);

            switch (topic)
            {
                case "Server-TCU/CarCrash":
                    long? tcuId = jsonPayload["id"]?.Value<long>();
                    return await WakeUp(tcuId);
                default:
                    Console.WriteLine("Wrong topic");
                    return false;
            }
        }

        private async Task<bool> WakeUp(long? tcuId)
        {
            bool result = true;
            Tcu? tcu = (from _tcu in _tcuContext.Tcus
                        where _tcu.TcuId == tcuId
                        select _tcu).FirstOrDefault();

            if (tcu == null)
                return false;

            List<Device> devices = (from _device in _tcuContext.Devices
                                    join _deviceTCU in _tcuContext.DevicesTcus
                                    on _device.DeviceId equals _deviceTCU.DeviceId
                                    where _deviceTCU.TcuId == tcu.TcuId
                                    select _device).ToList();

            string description = "Something Hit your car";
            Alert _alert = new()
            {
                TcuId = tcu.TcuId,
                ObdCode = "XXXX",
                LogTimeStamp = DateTime.Now,
                Description = description,
                Status = "CRITICAL"
            };

            _tcuContext.Alerts.Add(_alert);
            _tcuContext.SaveChanges();
            foreach (Device device in devices)
            {
                var deviceNotificationToken = device.NotificationToken;
                if (deviceNotificationToken == null)
                    continue;
                // send notification
                var messageId = await _service.SendNotificationAsync(
                    new NotificationModel { 
                        title = "alert", 
                        message = description, 
                        notificationToken = device.NotificationToken 
                    });
                result &= messageId != null;
            }
            return result;
        }
    }
}
