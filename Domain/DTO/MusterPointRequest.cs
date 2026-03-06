using System;

namespace Haven.Domain.DTO
{
    public class MusterPointRequest
    {
        public string Id { get; set; }
        public string IncidentId { get; set; }
        public string Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double RadiusMetres { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public int TotalMembers { get; set; }
        public int ArrivedCount { get; set; }
    }
}
