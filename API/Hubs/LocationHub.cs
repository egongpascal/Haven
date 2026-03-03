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
            // Get userId from claims (assumes authentication is set up)
            var userIdClaim = Context.User?.FindFirst("id")?.Value ?? Context.UserIdentifier;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return;
            // Persist location to MongoDB
            var location = new LocationData
            {
                UserId = userId,
                GroupId = groupId,
                Latitude = latitude,
                Longitude = longitude,
                Timestamp = DateTime.UtcNow
            };
            await _locationRepository.SaveLocationAsync(location);

            // Geofence breach check
            await _geofenceService.CheckMemberGeofenceBreach(Guid.Parse(groupId), userId);

            await Clients.Group(groupId).SendAsync("ReceiveLocation", latitude, longitude);
        }

        public async Task JoinGroup(string groupId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupId);
            // Set privacy enabled when joining group
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
            // Set privacy disabled when leaving group
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
