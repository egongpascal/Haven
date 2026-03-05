using Haven.Domain.DTO;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Haven.Infrastructure
{
    /// <summary>
    /// PostgreSQL implementation of IEmergencyRepository.
    /// Stores SOS incidents in the same database as groups.
    /// Requires running scripts/001_create_emergencies_table.sql.
    /// </summary>
    public class PostgresEmergencyRepository : IEmergencyRepository
    {
        private readonly string _connectionString;

        public PostgresEmergencyRepository(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public async Task SaveEmergencyAsync(EmergencyRequest request)
        {
            request.Timestamp = DateTime.UtcNow;
            request.Status = "Active";

            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand(@"
                INSERT INTO emergencies (id, group_id, user_id, latitude, longitude, emergency_type, timestamp, status)
                VALUES (@id, @groupId::uuid, @userId, @latitude, @longitude, @emergencyType, @timestamp, @status)",
                conn);
            cmd.Parameters.AddWithValue("id", request.Id);
            cmd.Parameters.AddWithValue("groupId", request.GroupId);
            cmd.Parameters.AddWithValue("userId", request.UserId);
            cmd.Parameters.AddWithValue("latitude", request.Latitude);
            cmd.Parameters.AddWithValue("longitude", request.Longitude);
            cmd.Parameters.AddWithValue("emergencyType", request.EmergencyType ?? "SOS");
            cmd.Parameters.AddWithValue("timestamp", request.Timestamp);
            cmd.Parameters.AddWithValue("status", request.Status);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task UpdateEmergencyStatusAsync(string id, string status)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand(@"
                UPDATE emergencies
                SET status = @status, resolved_at = CASE WHEN @status = 'Resolved' THEN NOW() ELSE resolved_at END, updated_at = NOW()
                WHERE id = @id",
                conn);
            cmd.Parameters.AddWithValue("id", id);
            cmd.Parameters.AddWithValue("status", status);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<EmergencyRequest>> GetByGroupAsync(string groupId)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand(@"
                SELECT id, group_id::text, user_id, latitude, longitude, emergency_type, timestamp, status
                FROM emergencies
                WHERE group_id = @groupId::uuid
                ORDER BY timestamp DESC
                LIMIT 50",
                conn);
            cmd.Parameters.AddWithValue("groupId", groupId);

            var list = new List<EmergencyRequest>();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new EmergencyRequest
                {
                    Id = reader.GetString(0),
                    GroupId = reader.GetString(1),
                    UserId = reader.GetString(2),
                    Latitude = reader.GetDouble(3),
                    Longitude = reader.GetDouble(4),
                    EmergencyType = reader.IsDBNull(5) ? "SOS" : reader.GetString(5),
                    Timestamp = reader.GetDateTime(6),
                    Status = reader.GetString(7),
                });
            }
            return list;
        }

        public async Task<EmergencyRequest> GetByIdAsync(string id)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand(@"
                SELECT id, group_id::text, user_id, latitude, longitude, emergency_type, timestamp, status
                FROM emergencies
                WHERE id = @id",
                conn);
            cmd.Parameters.AddWithValue("id", id);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new EmergencyRequest
                {
                    Id = reader.GetString(0),
                    GroupId = reader.GetString(1),
                    UserId = reader.GetString(2),
                    Latitude = reader.GetDouble(3),
                    Longitude = reader.GetDouble(4),
                    EmergencyType = reader.IsDBNull(5) ? "SOS" : reader.GetString(5),
                    Timestamp = reader.GetDateTime(6),
                    Status = reader.GetString(7),
                };
            }
            return null;
        }
    }
}
