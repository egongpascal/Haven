using System;
using System.Text.Json.Serialization;

namespace Haven.Domain.DTO
{
    public class EmergencyRequest
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("groupId")]
        public string GroupId { get; set; }
        [JsonPropertyName("userId")]
        public string UserId { get; set; }
        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }
        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }
        [JsonPropertyName("emergencyType")]
        public string EmergencyType { get; set; }
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }
        [JsonPropertyName("status")]
        public string Status { get; set; }
    }
}
