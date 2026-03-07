using Haven.Domain.Models;
using Haven.API.Hubs;
using Microsoft.AspNetCore.SignalR;
using Haven.Infrastructure;

namespace Haven.API.Services
{
    public class GeofenceService
    {
        private readonly IHubContext<GeofenceHub> _geofenceHubContext;
        private readonly ILocationRepository _locationRepository;
        private readonly IGroupRepository _groupRepository;
        private readonly IUserRepository _userRepository;

        public GeofenceService(
            IHubContext<GeofenceHub> geofenceHubContext,
            ILocationRepository locationRepository,
            IGroupRepository groupRepository,
            IUserRepository userRepository)
        {
            _geofenceHubContext = geofenceHubContext;
            _locationRepository = locationRepository;
            _groupRepository = groupRepository;
            _userRepository = userRepository;
        }

        public async Task CheckMemberGeofenceBreach(Guid groupId, Guid memberId)
        {
            var group = await _groupRepository.GetByIdAsync(groupId);
            if (group == null) return;

            var creatorId = group.CreatedBy;
            var radiusMeters = group.GeofenceRadius;

            var memberLocation = await _locationRepository.GetLatestLocationAsync(memberId);
            var creatorLocation = await _locationRepository.GetLatestLocationAsync(creatorId);

            if (memberLocation == null || creatorLocation == null) return;

            double distance = GetDistanceMeters(
                creatorLocation.Latitude, creatorLocation.Longitude,
                memberLocation.Latitude, memberLocation.Longitude);

            if (distance > radiusMeters)
            {
                string displayName = memberId.ToString();
                try
                {
                    var member = await _userRepository.GetByIdAsync(memberId);
                    if (member != null)
                    {
                        var name = $"{member.FirstName} {member.LastName}".Trim();
                        displayName = string.IsNullOrEmpty(name) ? member.Username : name;
                    }
                }
                catch { /* fall back to userId string */ }

                // Send GeofenceCrossing with the shape the frontend expects
                await _geofenceHubContext.Clients.Group(groupId.ToString())
                    .SendAsync("GeofenceCrossing", new
                    {
                        geofenceId = $"geofence-{groupId}",
                        userId = memberId.ToString(),
                        displayName,
                        eventType = "Exit",
                        timestamp = DateTime.UtcNow.ToString("o")
                    });
            }
        }

        private double GetDistanceMeters(double lat1, double lng1, double lat2, double lng2)
        {
            const double R = 6371000;
            var dLat = ToRadians(lat2 - lat1);
            var dLng = ToRadians(lng2 - lng1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private double ToRadians(double deg) => deg * (Math.PI / 180);
    }
}
