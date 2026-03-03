using Haven.Domain.Models;
using Microsoft.AspNetCore.SignalR;
using Haven.API.Hubs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Haven.Infrastructure;

namespace Haven.API.Services
{
    public class GeofenceService
    {
        private readonly IHubContext<LocationHub> _locationHubContext;
        private readonly ILocationRepository _locationRepository;
        private readonly IGroupRepository _groupRepository;

        public GeofenceService(IHubContext<LocationHub> locationHubContext, ILocationRepository locationRepository, IGroupRepository groupRepository)
        {
            _locationHubContext = locationHubContext;
            _locationRepository = locationRepository;
            _groupRepository = groupRepository;
        }

        // Checks a single member's location against the creator's current location and radius
        // NOTE: memberLat/memberLng and creatorLat/creatorLng should be fetched from MongoDB
        public async Task CheckMemberGeofenceBreach(Guid groupId, Guid memberId)
        {
            // Get group info (including creatorId and geofence radius)
            var group = await _groupRepository.GetByIdAsync(groupId);
            if (group == null) return;

            var creatorId = group.CreatedBy;
            var radiusMeters = group.GeofenceRadius;

            // Get latest locations from MongoDB
            var memberLocation = await _locationRepository.GetLatestLocationAsync(memberId);
            var creatorLocation = await _locationRepository.GetLatestLocationAsync(creatorId);

            if (memberLocation == null || creatorLocation == null) return;

            double memberLat = memberLocation.Latitude;
            double memberLng = memberLocation.Longitude;
            double creatorLat = creatorLocation.Latitude;
            double creatorLng = creatorLocation.Longitude;

            double distance = GetDistanceMeters(creatorLat, creatorLng, memberLat, memberLng);
            if (distance > radiusMeters)
            {
                // Member breached geofence, send alert via SignalR
                await _locationHubContext.Clients.Group(groupId.ToString())
                    .SendAsync("GeofenceBreached", new {
                        UserId = memberId,
                        Distance = distance,
                        Radius = radiusMeters
                    });
            }
        }

        // Haversine formula for distance between two lat/lng points
        private double GetDistanceMeters(double lat1, double lng1, double lat2, double lng2)
        {
            var R = 6371000; // Earth radius in meters
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
