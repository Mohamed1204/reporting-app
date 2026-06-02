namespace ReportingApi1.Entities
{
    public class Payment
    {
        public decimal Amount { get; set; }
        public int Id { get; set; }
        public VatReport VatReport { get; set; } = null!;
        public int VatReportId { get; set; }
        public string Currency { get; set; } = null!;
        public string ExternalReference { get; set; } = null!;
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
