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

        if (!db.ReportingPeriods.Any())
        {
            db.ReportingPeriods.AddRange(
                new ReportingPeriod { StartDate = new DateTime(2025, 10, 1), EndDate = new DateTime(2025, 12, 31), Status = PeriodStatus.Closed },
                new ReportingPeriod { StartDate = new DateTime(2026, 1, 1), EndDate = new DateTime(2026, 3, 31), Status = PeriodStatus.Open },
                new ReportingPeriod { StartDate = new DateTime(2026, 4, 1), EndDate = new DateTime(2026, 6, 30), Status = PeriodStatus.Open });
            db.SaveChanges();
        }

        if (!db.VatReports.Any())
        {
            var companyId = db.Companies.First().Id;
            var periods = db.ReportingPeriods.OrderBy(p => p.StartDate).ToList();

            db.VatReports.AddRange(
                new VatReport
                {
                    CompanyId = companyId,
                    ReportingPeriodId = periods[0].Id,
                    SubmittedAt = new DateTime(2026, 1, 15),
                    Status = ReportStatus.Approved,
                    AmountDue = 1500.00m,
                    SettlementCurrency = "EUR",
                    DueDate = new DateTime(2026, 1, 31),
                    PaymentStatus = PaymentStatus.Paid
                },
                new VatReport
                {
                    CompanyId = companyId,
                    ReportingPeriodId = periods[1].Id,
                    SubmittedAt = new DateTime(2026, 4, 10),
                    Status = ReportStatus.Submitted,
                    AmountDue = 2300.00m,
                    SettlementCurrency = "EUR",
                    DueDate = new DateTime(2026, 4, 30),
                    PaymentStatus = PaymentStatus.Unpaid
                },
                new VatReport
                {
                    CompanyId = companyId,
                    ReportingPeriodId = periods[2].Id,
                    SubmittedAt = new DateTime(2026, 6, 15),
                    Status = ReportStatus.Draft,
                    PaymentStatus = PaymentStatus.Unpaid
                });
            db.SaveChanges();
        }
    }
}
