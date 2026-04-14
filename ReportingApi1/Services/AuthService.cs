using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ReportingApi1.Data;
using ReportingApi1.DTOs;
using ReportingApi1.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;

namespace ReportingApi1.Services
{
    public interface IAuthService
    {
        Task<bool> RegisterAsync(RegisterUserDto dto);
        Task<String?> LoginAsync(LoginUserDto dto);
    }

    public class AuthService : IAuthService
    {
        private readonly VatReportingContext _context;
        private readonly IConfiguration _config;

        public AuthService(VatReportingContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<bool> RegisterAsync(RegisterUserDto dto)
        {
            // username must be unique
            if (await _context.Users.AnyAsync(u => u.UserName == dto.UserName))
                return false;

            // company must exist
            var company = await _context.Companies.FindAsync(dto.CompanyId);
            if (company == null) return false;

            var user = new User
            {
                UserName = dto.UserName,
                PasswordHash = HashPassword(dto.Password),
                CompanyId = dto.CompanyId
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<String?> LoginAsync(LoginUserDto dto)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.UserName == dto.UserName);
            if (user == null) return null;

            if (!VerifyPassword(user.PasswordHash, dto.Password)) return null;

            return GenerateToken(user);
        }

        private string GenerateToken(User user)
        {
            var secret = _config["Jwt:Secret"]!;
            var issuer = _config["Jwt:Issuer"];
            var audience = _config["Jwt:Audience"];
            var expiry = int.Parse(_config["Jwt:ExpiryMinutes"]!);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("userId", user.Id.ToString()),
                new Claim("userName", user.UserName),
                new Claim("companyId", user.CompanyId.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiry),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string HashPassword(string password)
        {
            // work factor 12 is a reasonable default; adjust if needed
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        }

        private static bool VerifyPassword(string stored, string password)
        {
            if (string.IsNullOrEmpty(stored)) return false;
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, stored);
            }
            catch
            {
                return false;
            }
        }
    }
}
