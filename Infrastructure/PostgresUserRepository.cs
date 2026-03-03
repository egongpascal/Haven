using Haven.Domain.Models;
using Npgsql;


namespace Haven.Infrastructure
{
    public class PostgresUserRepository : IUserRepository
    {
        private readonly string _connectionString;

        public PostgresUserRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<User> CreateAsync(User user)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand(@"
                INSERT INTO users (
                    id, username, email, password_hash, created_at, updated_at,
                    first_name, last_name
                ) VALUES (
                    @id, @username, @email, @password_hash, @created_at, @updated_at,
                    @first_name, @last_name
                );", conn);

            cmd.Parameters.AddWithValue("id", user.Id);
            cmd.Parameters.AddWithValue("username", user.Username);
            cmd.Parameters.AddWithValue("email", user.Email);
            cmd.Parameters.AddWithValue("password_hash", user.PasswordHash);
            cmd.Parameters.AddWithValue("created_at", user.CreatedAt);
            cmd.Parameters.AddWithValue("updated_at", user.UpdatedAt);
            cmd.Parameters.AddWithValue("first_name", user.FirstName);
            cmd.Parameters.AddWithValue("last_name", user.LastName);

            await cmd.ExecuteNonQueryAsync();

            return user;
        }

        public async Task<User> GetByIdAsync(Guid id)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            var cmd = new NpgsqlCommand("SELECT id, username, email, password_hash, first_name, last_name, profile_image_url, is_verified, created_at, updated_at, privacy_enabled FROM users WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("id", id);
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new User
                {
                    Id = reader.GetGuid(0),
                    Username = reader.GetString(1),
                    Email = reader.GetString(2),
                    PasswordHash = reader.GetString(3),
                    FirstName = reader.GetString(4),
                    LastName = reader.GetString(5),
                    ProfileImageUrl = reader.IsDBNull(6) ? null : reader.GetString(6),
                    IsVerified = reader.GetBoolean(7),
                    CreatedAt = reader.GetDateTime(8),
                    UpdatedAt = reader.GetDateTime(9),
                    PrivacyEnabled = reader.GetBoolean(10)

                };
            }
            return null;
        }

        public async Task<User> GetByUsernameAsync(string username)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            var cmd = new NpgsqlCommand("SELECT id, username, email, password_hash, first_name, last_name, profile_image_url, is_verified, created_at, updated_at, privacy_enabled FROM users WHERE username = @username", conn);
            cmd.Parameters.AddWithValue("username", username);
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new User
                {
                    Id = reader.GetGuid(0),
                    Username = reader.GetString(1),
                    Email = reader.GetString(2),
                    PasswordHash = reader.GetString(3),
                    FirstName = reader.GetString(4),
                    LastName = reader.GetString(5),
                    ProfileImageUrl = reader.IsDBNull(6) ? null : reader.GetString(6),
                    IsVerified = reader.GetBoolean(7),
                    CreatedAt = reader.GetDateTime(8),
                    UpdatedAt = reader.GetDateTime(9),
                    PrivacyEnabled = reader.GetBoolean(10)

                };
            }
            return null;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            var users = new List<User>();
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            var cmd = new NpgsqlCommand("SELECT id, username, email, passwordhash FROM users", conn);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                users.Add(new User
                {
                    Id = reader.GetGuid(0),
                    Username = reader.GetString(1),
                    Email = reader.GetString(2),
                    PasswordHash = reader.GetString(3)
                });
            }
            return users;
        }

        public async Task<User> UpdateAsync(User user)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            var cmd = new NpgsqlCommand("UPDATE users SET username = @username, email = @email, first_name = @first_name, last_name = @last_name, profile_image_url = @profile_image_url, updated_at = @updated_at, is_verified = @is_verified WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("id", user.Id);
            cmd.Parameters.AddWithValue("username", user.Username);
            cmd.Parameters.AddWithValue("email", user.Email);
            cmd.Parameters.AddWithValue("password_hash", user.PasswordHash);
            cmd.Parameters.AddWithValue("updated_at", DateTime.UtcNow);
            cmd.Parameters.AddWithValue("first_name", user.FirstName);
            cmd.Parameters.AddWithValue("last_name", user.LastName);
            cmd.Parameters.AddWithValue("privacy_enabled", user.PrivacyEnabled);
            cmd.Parameters.AddWithValue("profile_image_url", (object)user.ProfileImageUrl ?? DBNull.Value);
            cmd.Parameters.AddWithValue("is_verified", user.IsVerified);
            
            await cmd.ExecuteNonQueryAsync();
            return user;
        }
    }
}
