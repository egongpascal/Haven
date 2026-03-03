using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Haven.Infrastructure
{
    public class LocationData
    {
        public string GroupId { get; set; }
        public Guid UserId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public interface ILocationRepository
    {
        Task SaveLocationAsync(LocationData location);
        Task<IEnumerable<LocationData>> GetLocationsByGroupAsync(string groupId);
        Task<LocationData?> GetLatestLocationAsync(Guid userId);
    }
}
