
using Alert_Server.Models;
using Alert_Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using Newtonsoft.Json.Linq;
using System.Reflection.Emit;
using System.Security.Claims;


namespace Alert_Server.Controllers
{

    [Route("alerts")]
    [ApiController]
    public class AlertController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly TCUContext tcuContext;
        private readonly IFCMService _service;
        private readonly IMqttClient _mqttClient;
        string? _description;
        public AlertController(UserManager<IdentityUser> userManager, IFCMService fCMService, TCUContext tcuContext, IMqttClient mqttClient)
        {
            _userManager = userManager;
            _service = fCMService;
            this.tcuContext = tcuContext;
            _mqttClient = mqttClient;
        }

        [HttpPost("receive")]
        [Authorize(Policy = "TCUOnly")]
        public async Task<IActionResult> ReceiveMalfunctionAlert([FromBody] JObject alert)
        {
            
            string? obdCode = alert["code"]?.Value<string>();
            if (obdCode == null)
                return BadRequest();

            string? description = alert["description"]?.Value<string>();
            if (description == null)
                return BadRequest();

            string? state = alert["state"]?.Value<string>();
            if (state == null)
                return BadRequest();

           
            // Get the TCU claims from the current TCU principal
            var claimsIdentity = User.Identity as ClaimsIdentity;
            // TCU is authorized, extract the TCU identifier (Mac address)
            string? tcuMAC = claimsIdentity?.Name;

            if (tcuMAC == null)
                return Unauthorized();
            Tcu? tcu = (from _tcu in tcuContext.Tcus
                        where _tcu.Mac == tcuMAC
                        select _tcu).FirstOrDefault();

            if (tcu == null)
                return Unauthorized();
            if (string.IsNullOrEmpty(obdCode))
                return BadRequest("OBD code is required");
            

            if (tcu.DevicesTcus != null)
            {
                List<Device> devices = (from _device in tcuContext.Devices
                                        join _deviceTCU in tcuContext.DevicesTcus
                                        on _device.DeviceId equals _deviceTCU.DeviceId
                                        where _deviceTCU.TcuId == tcu.TcuId
                                        select _device).ToList();
                
                Alert _alert = new()
                {
                    TcuId = tcu.TcuId,
                    ObdCode = obdCode,
                    LogTimeStamp = DateTime.Now,
                    Description = description,
                    Status = state
                };
                tcuContext.Alerts.Add(_alert);
                await tcuContext.SaveChangesAsync();

                if (state == "FAULTY")
                     _description = "You need to check the " + description;
                else if (state == "Okay")
                    _description = description + " in good condition";


                foreach (Device device in devices)
                {
                    var deviceNotificationToken = device.NotificationToken;
                    if (deviceNotificationToken == null)
                        return NotFound("Notification token not found to device" + device.DeviceId);
                    // send notification
                    var messageId = await _service.SendNotificationAsync(new NotificationModel { title = description, message = _description, notificationToken = device.NotificationToken });
                    if (messageId == null)
                        return BadRequest("can't send notification");

                }
            }
            return Ok();
        }

       
        [HttpPost]
        [Authorize(Policy = "MobileOnly")]
        public async Task<IActionResult> WakeUpTCU()
        {
            Console.WriteLine("hello");
            if (User.Identity == null)
                return Unauthorized();

            string? deviceId = (from _claim in User.Claims
                                where _claim.Type == "deviceId"
                                select _claim.Value).FirstOrDefault();

            if (deviceId == null)
                return Unauthorized();

            var device = (from _device in tcuContext.Devices
                          where _device.DeviceId == deviceId
                          select _device).FirstOrDefault();

            if (device == null)
                return Unauthorized();

            string? userId = (from _claim in User.Claims
                              where _claim.Type == ClaimTypes.NameIdentifier
                              select _claim.Value).FirstOrDefault();

            if (userId == null)
                return Unauthorized();

            IdentityUser user = await _userManager.FindByIdAsync(userId);

            var tcu = (from _tcu in tcuContext.Tcus
                       where _tcu.UserId == user.Id
                       select _tcu).FirstOrDefault();

            if (tcu == null)
                return Forbid();

            var wakeUpRequest = new MqttApplicationMessageBuilder()
                    .WithTopic("TCU-" + tcu.TcuId.ToString() + "/WakeUp")
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtMostOnce)
                    .WithRetainFlag(false)
                    .Build();
            MqttClientPublishResult result = await _mqttClient.PublishAsync(wakeUpRequest);

            return Ok(new
            {
                resultStatus = result.ReasonString
            });
        }

    }
}
