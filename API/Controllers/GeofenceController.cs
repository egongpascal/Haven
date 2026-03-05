using Haven.API.Services;
using Haven.Domain.Models;
using Haven.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Haven.API.Controllers
{
    [ApiController]
    [Route("api/geofence")]
    [Authorize]
    public class GeofenceController : ControllerBase
    {
        private readonly IGeofenceRepository _geofenceRepository;
        private readonly IGroupRepository _groupRepository;

        public GeofenceController(IGeofenceRepository geofenceRepository, IGroupRepository groupRepository)
        {
            _geofenceRepository = geofenceRepository;
            _groupRepository = groupRepository;
        }

        // Create or update geofence for a group
        [HttpPost("{groupId}")]
        public async Task<IActionResult> SetGeofence(Guid groupId, [FromBody] GeofenceRequest request)
        {
            var group = await _groupRepository.GetByIdAsync(groupId);
            if (group == null) return NotFound();
            // Only allow group admin to set geofence
            var userId = Guid.Parse(User.FindFirst("sub").Value);
            if (group.CreatedBy != userId) return Forbid();
            // Store only radius in MongoDB; center is always creator's current location
            await _geofenceRepository.SetGeofenceAsync(groupId, request.RadiusMeters);
            return Ok();
        }

        // Get geofence for a group
        [HttpGet("{groupId}")]
        public async Task<IActionResult> GetGeofence(Guid groupId)
        {
            var geofence = await _geofenceRepository.GetGeofenceAsync(groupId);
            if (geofence == null) return NotFound();
            return Ok(new {
                RadiusMeters = geofence.RadiusMeters
            });
        }
    }

    public class GeofenceRequest
    {
        public double RadiusMeters { get; set; }
        // Center is not stored; it is always the creator's current location
    }
}
