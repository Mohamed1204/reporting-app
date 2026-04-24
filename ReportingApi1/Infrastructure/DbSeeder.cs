using ReportingApi1.Data;
using ReportingApi1.Entities;

namespace ReportingApi1.Infrastructure;

public static class DbSeeder
{
    public static void SeedDevelopmentData(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<VatReportingContext>();

        if (!db.Companies.Any())
        {
            db.Companies.Add(new Company { Name = "Test Company", Country = "DK" });
            db.SaveChanges();
        }

        if (!db.Users.Any(u => u.UserName == "admin"))
        {
            var companyId = db.Companies.First().Id;
            db.Users.Add(new User
            {
                UserName = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin", workFactor: 12),
                CompanyId = companyId,
                Role = UserRole.Admin
            });
            db.SaveChanges();
        }
    }
}
