using Haven.API.Hubs;
using Haven.Domain.DTO;
using Haven.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace Haven.API.Controllers
{
    [ApiController]
    [Route("api/emergency")]
    [Authorize]
    public class EmergencyController : ControllerBase
    {
        private readonly IHubContext<EmergencyHub> _emergencyHubContext;
        private readonly IEmergencyRepository _emergencyRepository;
        private readonly IMusterPointRepository _musterPointRepository;
        private readonly INotificationService _notificationService;

        public EmergencyController(
            IHubContext<EmergencyHub> emergencyHubContext,
            IEmergencyRepository emergencyRepository,
            IMusterPointRepository musterPointRepository,
            INotificationService notificationService)
        {
            _emergencyHubContext = emergencyHubContext;
            _emergencyRepository = emergencyRepository;
            _musterPointRepository = musterPointRepository;
            _notificationService = notificationService;
        }

        [HttpPost]
        public async Task<IActionResult> TriggerSOS([FromBody] EmergencyRequest request)
        {
            var userIdClaim = User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();
            request.UserId = userIdClaim;
            request.Id = Guid.NewGuid().ToString();
            request.EmergencyType = request.EmergencyType ?? "SOS";
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
            return Ok(new
            {
                id = request.Id,
                type = request.EmergencyType ?? "SOS",
                triggeredBy = request.UserId,
                groupId = request.GroupId,
                startedAt = request.Timestamp,
                resolvedAt = (DateTime?)null,
                status = request.Status,
            });
        }

        [HttpGet("incidents/group/{groupId}")]
        public async Task<IActionResult> GetGroupIncidents(string groupId)
        {
            var incidents = await _emergencyRepository.GetByGroupAsync(groupId);
            var result = incidents.Select(e => new
            {
                id = e.Id,
                type = e.EmergencyType ?? "SOS",
                triggeredBy = e.UserId,
                groupId = e.GroupId,
                startedAt = e.Timestamp,
                resolvedAt = e.Status == "Resolved" ? (DateTime?)e.Timestamp : null,
                status = e.Status,
            }).ToList();
            return Ok(result);
        }

        [HttpGet("incidents/{id}")]
        public async Task<IActionResult> GetIncident(string id)
        {
            var emergency = await _emergencyRepository.GetByIdAsync(id);
            if (emergency == null) return NotFound();
            return Ok(new
            {
                id = emergency.Id,
                type = emergency.EmergencyType ?? "SOS",
                triggeredBy = emergency.UserId,
                groupId = emergency.GroupId,
                startedAt = emergency.Timestamp,
                resolvedAt = emergency.Status == "Resolved" ? (DateTime?)emergency.Timestamp : null,
                status = emergency.Status,
            });
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

        [HttpPost("incidents/{incidentId}/muster-point")]
        public async Task<IActionResult> CreateMusterPoint(string incidentId, [FromBody] CreateMusterPointRequest request)
        {
            var userIdClaim = User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();
            var incident = await _emergencyRepository.GetByIdAsync(incidentId);
            if (incident == null) return NotFound("Incident not found");
            var mp = new Haven.Domain.DTO.MusterPointRequest
            {
                Id = Guid.NewGuid().ToString(),
                IncidentId = incidentId,
                Name = request.Name,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                RadiusMetres = request.RadiusMetres,
                CreatedBy = userIdClaim,
                CreatedAt = DateTime.UtcNow,
                ResolvedAt = null,
                TotalMembers = request.TotalMembers,
                ArrivedCount = 0,
            };
            await _musterPointRepository.SaveMusterPointAsync(mp);
            return Ok(new
            {
                id = mp.Id,
                incidentId = mp.IncidentId,
                name = mp.Name,
                latitude = mp.Latitude,
                longitude = mp.Longitude,
                radiusMetres = mp.RadiusMetres,
                createdBy = mp.CreatedBy,
                createdAt = mp.CreatedAt,
                resolvedAt = (DateTime?)null,
                totalMembers = mp.TotalMembers,
                arrivedCount = mp.ArrivedCount,
                isComplete = false,
            });
        }

        [HttpGet("incidents/{incidentId}/muster-point")]
        public async Task<IActionResult> GetMusterPoint(string incidentId)
        {
            var mp = await _musterPointRepository.GetByIncidentIdAsync(incidentId);
            if (mp == null) return NotFound();
            return Ok(new
            {
                id = mp.Id,
                incidentId = mp.IncidentId,
                name = mp.Name,
                latitude = mp.Latitude,
                longitude = mp.Longitude,
                radiusMetres = mp.RadiusMetres,
                createdBy = mp.CreatedBy,
                createdAt = mp.CreatedAt,
                resolvedAt = mp.ResolvedAt,
                totalMembers = mp.TotalMembers,
                arrivedCount = mp.ArrivedCount,
                isComplete = mp.ArrivedCount >= mp.TotalMembers && mp.TotalMembers > 0,
            });
        }

    }

}
