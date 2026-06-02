namespace ReportingApi1.Entities;

public class SalesEntry
{
    public int Id { get; set; }

    public int VatReportId { get; set; }
    public VatReport VatReport { get; set; } = null!;
    public string SellerCountry { get; set; } = string.Empty;
    public string BuyerCountry { get; set; } = string.Empty;
    public decimal Amount { get; set; }

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public bool BuyerHasValidVatNumber { get; set; }

    public ProductCategory productCategory { get; set; }

    public Currency Currency { get; set; }

    public DateOnly SaleDate { get; set; }

    public BuyerType BuyerType { get; set; }

    public TaxBreakdown? Breakdown { get; set; }
}
public enum ProductCategory
{
    Standard, Food, Books, Medicine, FinancialServices, Education
}

public enum Currency { EUR }
public enum BuyerType { B2B, B2C }
