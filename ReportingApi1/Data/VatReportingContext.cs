using Microsoft.EntityFrameworkCore;
using ReportingApi1.Entities;
using System.Diagnostics.Metrics;
using System.Text.RegularExpressions;

namespace ReportingApi1.Data;

public class VatReportingContext : DbContext
{
    public VatReportingContext(DbContextOptions<VatReportingContext> options)
        : base(options)
    {
    }

    public DbSet<Company> Companies { get; set; }
    public DbSet<ReportingPeriod> ReportingPeriods { get; set; }
    public DbSet<VatReport> VatReports { get; set; }
    public DbSet<SalesEntry> SalesEntries { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Payment> Payments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Company
        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Country).IsRequired().HasMaxLength(100);
            entity.Property(e => e.RowVersion).IsRowVersion();

            entity.HasIndex(e => e.Name);
        });

        // Configure ReportingPeriod
        modelBuilder.Entity<ReportingPeriod>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.StartDate).IsRequired();
            entity.Property(e => e.EndDate).IsRequired();
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.RowVersion).IsRowVersion();

            entity.HasIndex(e => new { e.StartDate, e.EndDate });
        });

        // Configure VatReport
        modelBuilder.Entity<VatReport>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SubmittedAt).IsRequired();
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.AmountDue).HasPrecision(18, 2);
            entity.Property(e => e.SettlementCurrency).HasMaxLength(3);
            entity.Property(e => e.PaymentStatus).IsRequired();
            entity.Property(e => e.RowVersion).IsRowVersion();

            entity.HasOne(e => e.Company)
                .WithMany(c => c.VatReports)
                .HasForeignKey(e => e.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ReportingPeriod)
                .WithMany(rp => rp.VatReports)
                .HasForeignKey(e => e.ReportingPeriodId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.CompanyId, e.ReportingPeriodId }).IsUnique();
        });

        // Configure SalesEntry
        modelBuilder.Entity<SalesEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BuyerCountry).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Amount).IsRequired().HasPrecision(18, 2);
            entity.Property(e => e.RowVersion).IsRowVersion();

            entity.OwnsOne(e => e.Breakdown, b =>
            {
                b.Property(x => x.VatAmount).HasPrecision(18, 2);
                b.Property(x => x.VatRate).HasPrecision(5, 2);
                b.Property(x => x.Scheme).HasConversion<string>().HasMaxLength(20);
            });

            entity.HasOne(e => e.VatReport)
                .WithMany(vr => vr.SalesEntries)
                .HasForeignKey(e => e.VatReportId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.VatReportId, e.BuyerCountry }).IsUnique();
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Category).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Price).IsRequired().HasPrecision(18, 2);

            entity.HasIndex(e => e.Name).IsUnique();

            entity.HasData(
                new Product { Id = 1, Name = "Laptop", Category = "Electronics", Price = 999.99m },
                new Product { Id = 2, Name = "Desk Chair", Category = "Furniture", Price = 249.99m },
                new Product { Id = 3, Name = "Notebook", Category = "Stationery", Price = 4.99m },
                new Product { Id = 4, Name = "Monitor", Category = "Electronics", Price = 399.99m },
                new Product { Id = 5, Name = "Standing Desk", Category = "Furniture", Price = 599.99m }
            );
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Role).IsRequired().HasDefaultValue(UserRole.User);
            entity.HasIndex(e => e.UserName).IsUnique();
            entity.HasOne(e => e.Company)
                .WithMany()
                .HasForeignKey(e => e.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TokenHash).IsRequired().HasMaxLength(128);
            entity.HasIndex(e => e.TokenHash).IsUnique();

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Payment>(entity=>{
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).IsRequired().HasPrecision(18, 2);
            entity.Property(e => e.Currency).IsRequired().HasMaxLength(3);
            entity.Property(e => e.ExternalReference).IsRequired().HasMaxLength(100);
            entity.Property(e => e.RowVersion).IsRowVersion();
            entity.HasOne(e => e.VatReport).WithMany(vr => vr.Payments).HasForeignKey(e => e.VatReportId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.VatReportId, e.ExternalReference }).IsUnique();
        });
    }
}
