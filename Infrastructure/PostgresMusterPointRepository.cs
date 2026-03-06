using Haven.Domain.DTO;
using Npgsql;
using System;
using System.Threading.Tasks;

namespace Haven.Infrastructure
{
    public class PostgresMusterPointRepository : IMusterPointRepository
    {
        private readonly string _connectionString;

        public PostgresMusterPointRepository(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public async Task SaveMusterPointAsync(MusterPointRequest request)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand(@"
                INSERT INTO muster_points (id, incident_id, name, latitude, longitude, radius_metres, created_by, created_at, total_members, arrived_count)
                VALUES (@id, @incidentId, @name, @latitude, @longitude, @radiusMetres, @createdBy, @createdAt, @totalMembers, @arrivedCount)",
                conn);
            cmd.Parameters.AddWithValue("id", request.Id);
            cmd.Parameters.AddWithValue("incidentId", request.IncidentId);
            cmd.Parameters.AddWithValue("name", request.Name);
            cmd.Parameters.AddWithValue("latitude", request.Latitude);
            cmd.Parameters.AddWithValue("longitude", request.Longitude);
            cmd.Parameters.AddWithValue("radiusMetres", request.RadiusMetres);
            cmd.Parameters.AddWithValue("createdBy", request.CreatedBy);
            cmd.Parameters.AddWithValue("createdAt", request.CreatedAt);
            cmd.Parameters.AddWithValue("totalMembers", request.TotalMembers);
            cmd.Parameters.AddWithValue("arrivedCount", request.ArrivedCount);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<MusterPointRequest> GetByIncidentIdAsync(string incidentId)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand(@"
                SELECT id, incident_id, name, latitude, longitude, radius_metres, created_by, created_at, resolved_at, total_members, arrived_count
                FROM muster_points
                WHERE incident_id = @incidentId
                ORDER BY created_at DESC
                LIMIT 1",
                conn);
            cmd.Parameters.AddWithValue("incidentId", incidentId);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new MusterPointRequest
                {
                    Id = reader.GetString(0),
                    IncidentId = reader.GetString(1),
                    Name = reader.GetString(2),
                    Latitude = reader.GetDouble(3),
                    Longitude = reader.GetDouble(4),
                    RadiusMetres = reader.GetDouble(5),
                    CreatedBy = reader.GetString(6),
                    CreatedAt = reader.GetDateTime(7),
                    ResolvedAt = reader.IsDBNull(8) ? null : reader.GetDateTime(8),
                    TotalMembers = reader.GetInt32(9),
                    ArrivedCount = reader.GetInt32(10),
                };
            }
            return null;
        }
    }
}
