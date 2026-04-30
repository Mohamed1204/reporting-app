using Microsoft.EntityFrameworkCore;
using ReportingApi1.Data;
using ReportingApi1.DTOs;
using ReportingApi1.Entities;
using ReportingApi1.Exceptions;
using ReportingApi1.Services;

namespace ReportingApi1.Tests.Services;

public class CompanyServiceTests
{
    private static VatReportingContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<VatReportingContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new VatReportingContext(options);
    }

    [Fact]
    public async Task GetByIdAsync_returns_null_when_company_not_found()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new CompanyService(context);

        // Act
        var result = await service.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_returns_dto_when_company_exists()
    {
        // Arrange
        var context = CreateInMemoryContext();
        context.Companies.Add(new Company { Id = 1, Name = "Acme", Country = "DK" });
        await context.SaveChangesAsync();

        var service = new CompanyService(context);

        // Act
        var result = await service.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Acme", result!.Name);
        Assert.Equal("DK", result.Country);
    }

    [Fact]
    public async Task UpdateAsync_throws_NotFoundException_when_company_missing()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new CompanyService(context);

        // Act + Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.UpdateAsync(999, new UpdateCompanyDto
            {
                Name = "New",
                Country = "DK",
                RowVersion = Array.Empty<byte>()
            }));
    }
}
