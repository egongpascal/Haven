using Haven.API.Hubs;
using Haven.Domain.DTO;
using Haven.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace Haven.API.Controllers
{
    [ApiController]
    [Route("api/emergency")]
    public class EmergencyController : ControllerBase
    {
        private readonly IHubContext<EmergencyHub> _emergencyHubContext;
        private readonly IEmergencyRepository _emergencyRepository;
        private readonly INotificationService _notificationService;

        public EmergencyController(IHubContext<EmergencyHub> emergencyHubContext, IEmergencyRepository emergencyRepository, INotificationService notificationService)
        {
            _emergencyHubContext = emergencyHubContext;
            _emergencyRepository = emergencyRepository;
            _notificationService = notificationService;
        }

        [HttpPost]
        public async Task<IActionResult> TriggerSOS([FromBody] EmergencyRequest request)
        {
            // Save emergency to database
            await _emergencyRepository.SaveEmergencyAsync(request);
            // Send notifications (SMS/email)
            await _notificationService.SendEmergencyNotificationAsync(request);
            await _emergencyHubContext.Clients.Group(request.GroupId)
                .SendAsync("SOSAlert", new {
                    UserId = request.UserId,
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    EmergencyType = request.EmergencyType
                });
            return Ok(new { Message = "SOS triggered, persisted, and notifications sent." });
        }

        [HttpPost("{id}/resolve")]
        public async Task<IActionResult> ResolveSOS(string id, [FromBody] ResolveEmergencyRequest request)
        {
            // Update emergency status in database
            await _emergencyRepository.UpdateEmergencyStatusAsync(id, "Resolved");
            // Send notifications (SMS/email)
            await _notificationService.SendEmergencyResolvedNotificationAsync(id, request);
            await _emergencyHubContext.Clients.Group(request.GroupId)
                .SendAsync("SOSResolved", id);
            return Ok(new { Message = "Emergency resolved, persisted, and notifications sent." });
        }

    }

}
