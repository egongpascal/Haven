using System;

namespace Haven.Domain.DTO
{
    public class EmergencyRequest
    {
        public string Id { get; set; }
        public string GroupId { get; set; }
        public string UserId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string EmergencyType { get; set; }
        public DateTime Timestamp { get; set; }
        public string Status { get; set; }
    }
}
