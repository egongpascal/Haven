using Haven.Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace Haven.API.Services
{
    public class AuthService
    {
        private readonly IConfiguration _config;
        private static readonly Dictionary<Guid, string> RefreshTokens = new();

        public AuthService(IConfiguration config)
        {
            _config = config;
        }

        public string GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim("id", user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(nameof(User.FirstName), user.FirstName ?? string.Empty),
                new Claim(nameof(User.LastName), user.LastName ?? string.Empty),
                new Claim(nameof(User.IsVerified), user.IsVerified.ToString()),
                new Claim(nameof(User.PrivacyEnabled), user.PrivacyEnabled.ToString()),
                new Claim(nameof(User.CreatedAt), user.CreatedAt.ToString("O")),
                new Claim(nameof(User.UpdatedAt), user.UpdatedAt.ToString("O")),
                new Claim(nameof(User.ProfileImageUrl), user.ProfileImageUrl ?? string.Empty)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetValue<string>("Jwt:Key")));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config.GetValue<string>("Jwt:Issuer"),
                audience: _config.GetValue<string>("Jwt:Audience"),
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            Console.WriteLine("Generated JWT Token: " + tokenString); // Debug log to inspect the token
            return tokenString;
        }

        public string GenerateRefreshToken()
        {
            var bytes = new byte[32];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            return Convert.ToBase64String(bytes);
        }

        public void StoreRefreshToken(Guid userId, string refreshToken)
        {
            RefreshTokens[userId] = refreshToken;
        }

        public string GetRefreshToken(Guid userId)
        {
            return RefreshTokens.TryGetValue(userId, out var token) ? token : null;
        }

        public Guid? GetUserIdByRefreshToken(string refreshToken)
        {
            var entry = RefreshTokens.FirstOrDefault(x => x.Value == refreshToken);
            return entry.Key != Guid.Empty ? entry.Key : (Guid?)null;
        }

        public void RemoveRefreshToken(Guid userId)
        {
            RefreshTokens.Remove(userId);
        }
    }
}
