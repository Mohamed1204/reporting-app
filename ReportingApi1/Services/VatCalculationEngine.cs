using ReportingApi1.Entities;
using ReportingApi1.Repositories;

namespace ReportingApi1.Services;

public interface IVatCalculator
{
    TaxBreakdown Calculate(SalesEntry salesEntry);
}

public class VatCalculationEngine : IVatCalculator
{
    private readonly IVatRateRepository _vatRateRepository;

    public VatCalculationEngine(IVatRateRepository vatRateRepository)
    {
        _vatRateRepository = vatRateRepository;
    }

    public TaxBreakdown Calculate(SalesEntry salesEntry)
    {
        var rate = _vatRateRepository.GetRate(
            salesEntry.BuyerCountry,
            salesEntry.productCategory,
            salesEntry.SaleDate);

        var vatAmount = decimal.Round(salesEntry.Amount * rate / 100m, 2, MidpointRounding.AwayFromZero);

        return new TaxBreakdown(vatAmount, rate, VatScheme.Oss);
    }
}
