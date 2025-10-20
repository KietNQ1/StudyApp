using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using myapp.Data;
using myapp.Models;
using myapp.Models.DTOs;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Auth; // Add this using directive

namespace myapp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(RegisterDto registerDto)
        {
            // ... (existing code)
            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
            {
                return BadRequest("Email already exists.");
            }

            var user = new User
            {
                Email = registerDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                FullName = registerDto.FullName,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Register), new { id = user.Id }, user);
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(LoginDto loginDto)
        {
            // ... (existing code)
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                return Unauthorized("Invalid credentials.");
            }

            var token = GenerateJwtToken(user);
            return Ok(token);
        }
        
        [HttpPost("google-login")]
        public async Task<ActionResult<string>> GoogleLogin([FromBody] GoogleLoginRequestDto request)
        {
            try
            {
                var validationSettings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _configuration["Google:ClientId"] }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(request.Credential, validationSettings);

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == payload.Email);

                if (user == null)
                {
                    // If user doesn't exist, create a new one
                    user = new User
                    {
                        Email = payload.Email,
                        FullName = payload.Name,
                        AvatarUrl = payload.Picture,
                        PasswordHash = "GOOGLE_USER", // Indicates a user who signed up with Google
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                    };
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();
                }
                
                // Generate and return our app's JWT token
                var token = GenerateJwtToken(user);
                return Ok(token);
            }
            catch (InvalidJwtException ex)
            {
                return Unauthorized($"Invalid Google token: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        private string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(30),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
