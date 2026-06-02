using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ReportingApi1.Data;
using ReportingApi1.DTOs;
using ReportingApi1.Entities;
using ReportingApi1.Exceptions;
using ReportingApi1.Infrastructure;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ReportingApi1.Services
{
    public record AuthResult(AuthResponseDto Response, string RefreshToken, DateTime RefreshExpiresAt);

    public interface IAuthService
    {
        Task RegisterAsync(RegisterUserDto dto);
        Task<AuthResult?> LoginAsync(LoginUserDto dto);
        Task<AuthResult?> RefreshAsync(string rawRefreshToken);
        Task LogoutAsync(string rawRefreshToken);
    }

    public class AuthService : IAuthService
    {
        private readonly VatReportingContext _context;
        private readonly JwtSettings _jwt;
        private readonly ILogger<AuthService> _logger;

        public AuthService(VatReportingContext context, IOptions<JwtSettings> jwt, ILogger<AuthService> logger)
        {
            _context = context;
            _jwt = jwt.Value;
            _logger = logger;
        }

        public async Task RegisterAsync(RegisterUserDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.UserName == dto.UserName))
            {
                _logger.LogInformation("Registration failed: Username {UserName} already exists", dto.UserName);
                throw new BadRequestException("Registration Failed");
            }

            var company = await _context.Companies.FindAsync(dto.CompanyId);
            if (company == null)
            {
                _logger.LogInformation("Registration failed: Company with ID {CompanyId} not found", dto.CompanyId);
                throw new BadRequestException("Registration Failed");
            }

            var user = new User
            {
                UserName = dto.UserName,
                PasswordHash = HashPassword(dto.Password),
                CompanyId = dto.CompanyId
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Register succeeded: user {UserName} created for company {CompanyId}", dto.UserName, dto.CompanyId);
        }

        public async Task<AuthResult?> LoginAsync(LoginUserDto dto)
        {
            var user = await _context.Users
                .Include(u => u.Company)
                .SingleOrDefaultAsync(u => u.UserName == dto.UserName);
            if (user == null) return null;
            if (!VerifyPassword(user.PasswordHash, dto.Password)) return null;

            var rawRefresh = GenerateRefreshToken();
            var expiresAt = DateTime.UtcNow.AddDays(_jwt.RefreshExpiryDays);

            _context.RefreshTokens.Add(new RefreshToken
            {
                UserId = user.Id,
                TokenHash = HashToken(rawRefresh),
                ExpiresAt = expiresAt,
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            return new AuthResult(BuildResponse(user), rawRefresh, expiresAt);
        }

        public async Task<AuthResult?> RefreshAsync(string rawRefreshToken)
        {
            var hash = HashToken(rawRefreshToken);
            var existing = await _context.RefreshTokens
                .Include(rt => rt.User).ThenInclude(u => u.Company)
                .FirstOrDefaultAsync(rt => rt.TokenHash == hash);

            if (existing == null) return null;

            if (existing.RevokedAt != null)
            {
                _logger.LogWarning("Refresh token reuse detected for user {UserId}", existing.UserId);
                var active = await _context.RefreshTokens
                    .Where(rt => rt.UserId == existing.UserId && rt.RevokedAt == null)
                    .ToListAsync();
                foreach (var t in active) t.RevokedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return null;
            }

            if (existing.ExpiresAt < DateTime.UtcNow) return null;

            var newRaw = GenerateRefreshToken();
            var newExpiresAt = DateTime.UtcNow.AddDays(_jwt.RefreshExpiryDays);
            var newToken = new RefreshToken
            {
                UserId = existing.UserId,
                TokenHash = HashToken(newRaw),
                ExpiresAt = newExpiresAt,
                CreatedAt = DateTime.UtcNow
            };
            _context.RefreshTokens.Add(newToken);
            await _context.SaveChangesAsync();

            existing.RevokedAt = DateTime.UtcNow;
            existing.ReplacedByTokenId = newToken.Id;
            await _context.SaveChangesAsync();

            return new AuthResult(BuildResponse(existing.User), newRaw, newExpiresAt);
        }

        public async Task LogoutAsync(string rawRefreshToken)
        {
            var hash = HashToken(rawRefreshToken);
            var existing = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.TokenHash == hash);

            if (existing == null || existing.RevokedAt != null) return;

            existing.RevokedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        private AuthResponseDto BuildResponse(User user) => new()
        {
            Token = GenerateAccessToken(user),
            Role = user.Role.ToString(),
            UserName = user.UserName,
            CompanyName = user.Company?.Name ?? string.Empty
        };

        private string GenerateAccessToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("userId", user.Id.ToString()),
                new Claim("userName", user.UserName),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim("companyId", user.CompanyId.ToString()),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwt.ExpiryMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string GenerateRefreshToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(32);
            return Convert.ToBase64String(bytes);
        }

        private static string HashToken(string token)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(bytes);
        }

        private static string HashPassword(string password)
        {
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
