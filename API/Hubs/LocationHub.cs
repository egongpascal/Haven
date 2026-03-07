using Haven.API.Services;
using Haven.Infrastructure;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Haven.API.Hubs
{
    public class LocationHub : Hub
    {
        private readonly ILocationRepository _locationRepository;
        private readonly GeofenceService _geofenceService;
        private readonly IUserRepository _userRepository;

        public LocationHub(ILocationRepository locationRepository, GeofenceService geofenceService, IUserRepository userRepository)
        {
            _locationRepository = locationRepository;
            _geofenceService = geofenceService;
            _userRepository = userRepository;
        }

        public async Task SendLocation(string groupId, double latitude, double longitude)
        {
            var userIdClaim = Context.User?.FindFirst("id")?.Value ?? Context.UserIdentifier;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return;

            var location = new LocationData
            {
                UserId = userId,
                GroupId = groupId,
                Latitude = latitude,
                Longitude = longitude,
                Timestamp = DateTime.UtcNow
            };
            await _locationRepository.SaveLocationAsync(location);

            // Broadcast LocationUpdated with the shape the frontend expects
            await Clients.Group(groupId).SendAsync("LocationUpdated", new
            {
                userId = userId.ToString(),
                latitude,
                longitude,
                accuracy = 0,
                timestamp = location.Timestamp.ToString("o"),
                privacyLevel = "Exact"
            });

            // Geofence breach check (broadcasts GeofenceCrossing via GeofenceHub)
            await _geofenceService.CheckMemberGeofenceBreach(Guid.Parse(groupId), userId);
        }

        public async Task JoinGroup(string groupId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupId);

            var userIdClaim = Context.User?.FindFirst("id")?.Value ?? Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var userId))
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user != null)
                {
                    user.PrivacyEnabled = true;
                    await _userRepository.UpdateAsync(user);
                }
            }
        }

        public async Task LeaveGroup(string groupId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId);

            var userIdClaim = Context.User?.FindFirst("id")?.Value ?? Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var userId))
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user != null)
                {
                    user.PrivacyEnabled = false;
                    await _userRepository.UpdateAsync(user);
                }
            }
        }
    }
}
