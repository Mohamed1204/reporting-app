using Microsoft.EntityFrameworkCore;
using ReportingApi1.Data;
using ReportingApi1.DTOs;
using ReportingApi1.Entities;
using BCrypt.Net;

namespace ReportingApi1.Services
{
    public interface IAuthService
    {
        Task<bool> RegisterAsync(RegisterUserDto dto);
        Task<User?> LoginAsync(LoginUserDto dto);
    }

    public class AuthService : IAuthService
    {
        private readonly VatReportingContext _context;

        public AuthService(VatReportingContext context)
        {
            _context = context;
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

        public async Task<User?> LoginAsync(LoginUserDto dto)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.UserName == dto.UserName);
            if (user == null) return null;

            if (!VerifyPassword(user.PasswordHash, dto.Password)) return null;

            return user;
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
