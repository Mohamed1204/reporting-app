using Microsoft.EntityFrameworkCore;
using ReportingApi1.Entities;

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
            entity.Property(e => e.Country).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Amount).IsRequired().HasPrecision(18, 2);
            entity.Property(e => e.VatRate).IsRequired().HasPrecision(5, 2);
            entity.Property(e => e.RowVersion).IsRowVersion();
            
            entity.HasOne(e => e.VatReport)
                .WithMany(vr => vr.SalesEntries)
                .HasForeignKey(e => e.VatReportId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
