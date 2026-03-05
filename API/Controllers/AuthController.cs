using Haven.API.Services;
using Haven.Application;
using Haven.Domain;
using Haven.Domain.DTO;
using Haven.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Haven.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly AuthService _authService;

        public AuthController(IUserService userService, IConfiguration config)
        {
            _userService = userService;
            _authService = new AuthService(config);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var user = await _userService.RegisterAsync(request.Username, request.Email, request.Password, request.FirstName, request.LastName);
            var token = _authService.GenerateJwtToken(user);
            var refreshToken = _authService.GenerateRefreshToken();
            _authService.StoreRefreshToken(user.Id, refreshToken);
            return Ok(new { 
                AccessToken = token, 
                RefreshToken = refreshToken,
                User = user,
                ExpiresIn = 3600 // 1 hour in seconds
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _userService.AuthenticateAsync(request.Username, request.Password);
            if (user == null)
            {
                return Unauthorized(new { Message = "Invalid username or password." });
            }
            var token = _authService.GenerateJwtToken(user);
            var refreshToken = _authService.GenerateRefreshToken();
            _authService.StoreRefreshToken(user.Id, refreshToken);
            return Ok(new { 
                AccessToken = token, 
                RefreshToken = refreshToken,
                User = user,
                ExpiresIn = 3600 // 1 hour in seconds
            });
        }
        
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return BadRequest(new { Message = "Refresh token is required." });
            }

            try
            {
                var userId = _authService.GetUserIdByRefreshToken(request.RefreshToken);
                if (userId == null)
                {
                    return Unauthorized(new { Message = "Invalid refresh token." });
                }

                var user = await _userService.GetByIdAsync(userId.Value);
                if (user == null)
                {
                    return Unauthorized(new { Message = "User not found." });
                }

                var newToken = _authService.GenerateJwtToken(user);
                var newRefreshToken = _authService.GenerateRefreshToken();
                _authService.StoreRefreshToken(user.Id, newRefreshToken);

                return Ok(new
                {
                    AccessToken = newToken,
                    RefreshToken = newRefreshToken,
                    User = user,
                    ExpiresIn = 3600 // 1 hour in seconds
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Failed to refresh token.", Error = ex.Message });
            }
        }

        [HttpPost("logout")]
        public IActionResult Logout([FromHeader(Name = "Authorization")] string authHeader)
        {
            if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer "))
                return BadRequest("Missing or invalid Authorization header.");
            var token = authHeader.Substring("Bearer ".Length).Trim();
            return Ok(new { Message = "Logged out" });
        }
    }

}
