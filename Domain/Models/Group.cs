using System;
using System.Collections.Generic;

namespace Haven.Domain.Models
{
    public class Group
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public double GeofenceRadius { get; set; } 
        public bool IsActive { get; set; } = true;
        public string InviteCode { get; set; }
        public int MemberCount { get; set; } = 0; 
    }
}
