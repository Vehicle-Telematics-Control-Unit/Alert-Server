
using Alert_Server.Models;
using Alert_Server.Notification_service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


namespace Alert_Server.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class AlertController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly TCUContext tcuContext;
        private readonly IFCMService _service;

        public AlertController(UserManager<IdentityUser> userManager, IFCMService fCMService, TCUContext tcuContext)
        {
            _userManager = userManager;
            _service = fCMService;
            this.tcuContext = tcuContext;
        }

        [HttpPost("receive")]
        [Authorize(Policy = "TCUOnly")]
        public async Task<IActionResult> ReceiveMalfunctionAlert()
        {
            Alert alert = new Alert();
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


            return Ok();


        }

        [HttpPost("send")]
        public async Task<IActionResult> SendNotification([FromBody] string deviceId)
        {
            try
            {
                var device = (from _device in tcuContext.Devices where _device.DeviceId == deviceId select _device).FirstOrDefault();
                if (device == null)
                {
                    return NotFound("device not found");
                }
                if (device.NotificationToken == null)
                {
                    return NotFound("notification token is null");
                }

                var messageId = await _service.SendNotificationAsync(device.NotificationToken);
                if (messageId != null)
                {
                    return Ok(messageId);
                }
                return BadRequest("can't send notification message");

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message.ToString());
            }

        }

        [HttpGet("token")]
        [Authorize] // authorize the user 
        public async Task<IActionResult> GetDeviceToken([FromBody] string token)
        {
            try
            {
                // Get the user claims from the current user principal
                var claimsIdentity = User.Identity as ClaimsIdentity;
                // Extract the information you need from the claims
                string? email = claimsIdentity?.FindFirst(ClaimTypes.Email)?.Value;
                var userName = claimsIdentity?.FindFirst(ClaimTypes.Name)?.Value;

                var user = await _userManager.FindByNameAsync(userName);

                user ??= await _userManager.FindByEmailAsync(userName);
                if (user == null)
                {
                    return NotFound();
                }

                // add the token to the database

                return Ok();

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message.ToString());
            }
        }



    }
}
