namespace Haven.Domain.DTO
{
    public class CreateGroupRequest
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public Guid CreatedBy { get; set; }
        public double GeofenceRadius { get; set; } = 100.0;
    }

    public class UpdateGroupRequest
    {
        public required string Name { get; set; }
    }


    public class JoinGroupRequest
    {
        public required string InviteCode { get; set; }
        public Guid UserId { get; set; }
    }

    public class ResolveEmergencyRequest
    {
        public required string GroupId { get; set; }
    }


    public class RegisterRequest
    {
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
    }

    public class UpdateUserRequest
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public string? ProfileImageUrl { get; set; }
    }

    public class ChangePasswordRequest
    {
        public required string CurrentPassword { get; set; }
        public required string NewPassword { get; set; }
    }

    public class RefreshTokenRequest
    {
        public required string RefreshToken { get; set; }
    }

    public class RefreshTokenResponse
    {
        public required string AccessToken { get; set; }
        public required string RefreshToken { get; set; }
        public int ExpiresIn { get; set; }
    }

    public class GroupMemberWithUserDetails
    {
        public Guid MembershipId { get; set; }
        public Guid GroupId { get; set; }
        public Guid UserId { get; set; }
        public required string Role { get; set; }
        public DateTime JoinedAt { get; set; }
        public bool IsActive { get; set; }

        // User details
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
    }
}