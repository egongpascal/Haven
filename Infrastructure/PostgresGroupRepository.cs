using Haven.Domain.DTO;
using Haven.Domain.Models;
using Npgsql;

namespace Haven.Infrastructure
{
    public class PostgresGroupRepository : IGroupRepository
    {
        private readonly string _connectionString;

        public PostgresGroupRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<Group> GetUserActiveGroupAsync(Guid userId)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            var cmd = new NpgsqlCommand(@"
                SELECT g.id, g.name, g.description, g.created_by, g.created_at, g.updated_at, g.is_active, g.invite_code, g.geofenceradius,
                       (SELECT COUNT(*) FROM groupmembers gm2 WHERE gm2.group_id = g.id AND gm2.is_active = TRUE) as member_count
                FROM groups g
                JOIN groupmembers gm ON g.id = gm.group_id
                WHERE gm.user_id = @userId AND gm.is_active = TRUE AND g.is_active = TRUE
                LIMIT 1", conn);
            cmd.Parameters.AddWithValue("userId", userId);
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Group
                {
                    Id = reader.GetGuid(0),
                    Name = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                    CreatedBy = reader.GetGuid(3),
                    CreatedAt = reader.GetDateTime(4),
                    UpdatedAt = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5),
                    IsActive = reader.GetBoolean(6),
                    InviteCode = reader.GetString(7),
                    GeofenceRadius = reader.GetDouble(8),
                    MemberCount = reader.GetInt32(9)
                };
            }
            return null;
        }

        public async Task<List<Group>> GetUserGroupsAsync(Guid userId)
        {
            var groups = new List<Group>();
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            var cmd = new NpgsqlCommand(@"
                SELECT g.id, g.name, g.description, g.created_by, g.created_at, g.updated_at, g.is_active, g.invite_code, g.geofenceradius,
                       (SELECT COUNT(*) FROM groupmembers gm WHERE gm.group_id = g.id AND gm.is_active = TRUE) as member_count
                FROM groups g
                JOIN groupmembers gm ON g.id = gm.group_id
                WHERE gm.user_id = @userId", conn);
            cmd.Parameters.AddWithValue("userId", userId);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                groups.Add(new Group
                {
                    Id = reader.GetGuid(0),
                    Name = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                    CreatedBy = reader.GetGuid(3),
                    CreatedAt = reader.GetDateTime(4),
                    UpdatedAt = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5),
                    IsActive = reader.GetBoolean(6),
                    InviteCode = reader.GetString(7),
                    GeofenceRadius = reader.GetDouble(8),
                    MemberCount = reader.GetInt32(9)
                });
            }
            return groups;
        }

        public async Task<Group> CreateAsync(Group group)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            var cmd = new NpgsqlCommand(@"INSERT INTO groups (id, name, description, created_by, created_at, updated_at, is_active, geofenceradius, invite_code) 
                VALUES (@id, @name, @description, @created_by, @created_at, @updated_at, @is_active, @geofenceradius, @invite_code)", conn);
            cmd.Parameters.AddWithValue("id", group.Id);
            cmd.Parameters.AddWithValue("name", group.Name);
            cmd.Parameters.AddWithValue("description", group.Description ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("created_by", group.CreatedBy);
            cmd.Parameters.AddWithValue("created_at", group.CreatedAt);
            cmd.Parameters.AddWithValue("updated_at", group.UpdatedAt ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("is_active", group.IsActive);
            cmd.Parameters.AddWithValue("geofenceradius", group.GeofenceRadius);
            cmd.Parameters.AddWithValue("invite_code", group.InviteCode);
            await cmd.ExecuteNonQueryAsync();
            return group;
        }
        public async Task<Group> GetByInviteCodeAsync(string inviteCode)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            var cmd = new NpgsqlCommand(@"
                SELECT g.id, g.name, g.description, g.created_by, g.created_at, g.updated_at, g.is_active, g.invite_code, g.geofenceradius,
                       (SELECT COUNT(*) FROM groupmembers gm WHERE gm.group_id = g.id AND gm.is_active = TRUE) as member_count
                FROM groups g WHERE g.invite_code = @invite_code", conn);
            cmd.Parameters.AddWithValue("invite_code", inviteCode);
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Group
                {
                    Id = reader.GetGuid(0),
                    Name = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                    CreatedBy = reader.GetGuid(3),
                    CreatedAt = reader.GetDateTime(4),
                    UpdatedAt = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5),
                    IsActive = reader.GetBoolean(6),
                    InviteCode = reader.GetString(7),
                    GeofenceRadius = reader.GetDouble(8),
                    MemberCount = reader.GetInt32(9)
                };
            }
            return null;
        }
        public async Task<Group> GetByIdAsync(Guid id)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            var cmd = new NpgsqlCommand(@"
                SELECT g.id, g.name, g.description, g.created_by, g.created_at, g.updated_at, g.is_active, g.invite_code, g.geofenceradius,
                       (SELECT COUNT(*) FROM groupmembers gm WHERE gm.group_id = g.id AND gm.is_active = TRUE) as member_count
                FROM groups g WHERE g.id = @id", conn);
            cmd.Parameters.AddWithValue("id", id);
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Group
                {
                    Id = reader.GetGuid(0),
                    Name = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                    CreatedBy = reader.GetGuid(3),
                    CreatedAt = reader.GetDateTime(4),
                    UpdatedAt = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5),
                    IsActive = reader.GetBoolean(6),
                    InviteCode = reader.GetString(7),
                    GeofenceRadius = reader.GetDouble(8),
                    MemberCount = reader.GetInt32(9)
                };
            }
            return null;
        }

        public async Task<Group> UpdateAsync(Group group)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            var cmd = new NpgsqlCommand(@"UPDATE groups SET name = @name, description = @description, updated_at = @updated_at, is_active = @is_active WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("id", group.Id);
            cmd.Parameters.AddWithValue("name", group.Name);
            cmd.Parameters.AddWithValue("description", group.Description ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("updated_at", group.UpdatedAt ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("is_active", group.IsActive);
            await cmd.ExecuteNonQueryAsync();
            return group;
        }

        public async Task<bool> AddMemberAsync(Guid groupId, Guid userId, string role)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            var cmd = new NpgsqlCommand(@"
                INSERT INTO groupmembers (id, group_id, user_id, role, joined_at, is_active)
                VALUES (@id, @groupId, @userId, @role, @joinedAt, @isActive)", conn);
            cmd.Parameters.AddWithValue("id", Guid.NewGuid());
            cmd.Parameters.AddWithValue("groupId", groupId);
            cmd.Parameters.AddWithValue("userId", userId);
            cmd.Parameters.AddWithValue("role", role);
            cmd.Parameters.AddWithValue("joinedAt", DateTime.UtcNow);
            cmd.Parameters.AddWithValue("isActive", true);
            try
            {
                await cmd.ExecuteNonQueryAsync();
                return true;
            }
            catch (NpgsqlException ex) when (ex.SqlState == "23505") // Unique violation
            {
                return false;
            }
        }

        public async Task<bool> JoinGroupByInviteCodeAsync(JoinGroupRequest request)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            // 1. Get the group by invite code
            var getGroupCmd = new NpgsqlCommand("SELECT id FROM groups WHERE invite_code = @inviteCode", conn);
            getGroupCmd.Parameters.AddWithValue("inviteCode", request.InviteCode);
            var groupIdObj = await getGroupCmd.ExecuteScalarAsync();

            if (groupIdObj == null)
                return false; // Group not found

            var groupId = (Guid)groupIdObj;

            // 2. Get the user by ID
            var getUserCmd = new NpgsqlCommand("SELECT id FROM users WHERE id = @userId", conn);
            getUserCmd.Parameters.AddWithValue("userId", request.UserId);
            var userIdObj = await getUserCmd.ExecuteScalarAsync();

            if (userIdObj == null)
                return false; // User not found

            var userId = (Guid)userIdObj;

            // 3. Check if the user is already a member
            var checkMemberCmd = new NpgsqlCommand(
                "SELECT 1 FROM groupmembers WHERE group_id = @groupId AND user_id = @userId", conn);
            checkMemberCmd.Parameters.AddWithValue("groupId", groupId);
            checkMemberCmd.Parameters.AddWithValue("userId", userId);
            var exists = await checkMemberCmd.ExecuteScalarAsync();

            if (exists != null)
                return true; // Already a member, or you can return false if you prefer

            // 4. Insert new membership
            var insertCmd = new NpgsqlCommand(@"
                INSERT INTO groupmembers (id, group_id, user_id, role, joined_at, is_active)
                VALUES (@id, @groupId, @userId, @role, @joinedAt, @isActive)", conn);

            insertCmd.Parameters.AddWithValue("id", Guid.NewGuid());
            insertCmd.Parameters.AddWithValue("groupId", groupId);
            insertCmd.Parameters.AddWithValue("userId", userId);
            insertCmd.Parameters.AddWithValue("role", "Member");
            insertCmd.Parameters.AddWithValue("joinedAt", DateTime.UtcNow);
            insertCmd.Parameters.AddWithValue("isActive", true);
            insertCmd.Parameters.AddWithValue("lastActive", DateTime.UtcNow);

            await insertCmd.ExecuteNonQueryAsync();

            return true;
        }

        public async Task<IEnumerable<Group>> GetAllAsync()
        {
            var groups = new List<Group>();
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            var cmd = new NpgsqlCommand("SELECT id, name, description, created_by, created_at, updated_at, is_active FROM groups", conn);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                groups.Add(new Group
                {
                    Id = reader.GetGuid(0),
                    Name = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                    CreatedBy = reader.GetGuid(3),
                    CreatedAt = reader.GetDateTime(4),
                    UpdatedAt = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5),
                    IsActive = reader.GetBoolean(6)
                });
            }
            return groups;
        }   

        public async Task<List<GroupMemberWithUserDetails>> GetGroupMembersWithDetailsAsync(Guid groupId)
        {
            var result = new List<GroupMemberWithUserDetails>();

            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand(@"
                SELECT 
                    gm.id AS membership_id,
                    gm.group_id,
                    gm.user_id,
                    gm.role,
                    gm.joined_at,
                    gm.is_active,
                    u.id AS user_id,
                    u.username,
                    u.email,
                    u.first_name,
                    u.last_name
                FROM 
                    groupmembers gm
                JOIN 
                    users u ON gm.user_id = u.id
                WHERE 
                    gm.group_id = @groupId;", conn);

            cmd.Parameters.AddWithValue("groupId", groupId);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var member = new GroupMemberWithUserDetails
                {
                    MembershipId = reader.GetGuid(reader.GetOrdinal("membership_id")),
                    GroupId = reader.GetGuid(reader.GetOrdinal("group_id")),
                    UserId = reader.GetGuid(reader.GetOrdinal("user_id")),
                    Role = reader.GetString(reader.GetOrdinal("role")),
                    JoinedAt = reader.GetDateTime(reader.GetOrdinal("joined_at")),
                    IsActive = reader.GetBoolean(reader.GetOrdinal("is_active")),

                    // User details
                    Username = reader.GetString(reader.GetOrdinal("username")),
                    Email = reader.GetString(reader.GetOrdinal("email")),
                    FirstName = reader.GetString(reader.GetOrdinal("first_name")),
                    LastName = reader.GetString(reader.GetOrdinal("last_name"))
                };

                result.Add(member);
            }
            return result;
        }  

        public async Task RemoveMemberAsync(Guid groupId, Guid userId)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            var cmd = new NpgsqlCommand(@"
                UPDATE groupmembers SET is_active = FALSE
                WHERE group_id = @groupId AND user_id = @userId", conn);
            cmd.Parameters.AddWithValue("groupId", groupId);
            cmd.Parameters.AddWithValue("userId", userId);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<IEnumerable<Group>> GetGroupsByUserIdAsync(Guid userId)
        {
            var groups = new List<Group>();
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            var cmd = new NpgsqlCommand(@"
                SELECT g.id, g.name, g.description, g.created_by, g.created_at, g.updated_at, g.is_active
                FROM groups g
                JOIN groupmembers gm ON g.id = gm.group_id
                WHERE gm.user_id = @userId", conn);
            cmd.Parameters.AddWithValue("userId", userId);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                groups.Add(new Group
                {
                    Id = reader.GetGuid(0),
                    Name = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                    CreatedBy = reader.GetGuid(3),
                    CreatedAt = reader.GetDateTime(4),
                    UpdatedAt = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5),
                    IsActive = reader.GetBoolean(6)
                });
            }
            return groups;
        }
    }
}
